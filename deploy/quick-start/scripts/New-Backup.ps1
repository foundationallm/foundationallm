#!/usr/bin/env pwsh

Param(
    [Parameter(Mandatory = $true)][string]$resourceGroup,
    [Parameter(Mandatory = $true)][string]$destinationStorageAccount
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Get-SearchServiceIndexes {
    param(
        [Parameter(Mandatory = $true)][string]$searchServiceName,
        [Parameter(Mandatory = $true)][string]$pathPrefix
    )

    $serviceUri = "https://$searchServiceName.search.windows.net"
    $uri = $serviceUri + "/indexes?api-version=2020-06-30&`$select=name"
    $token = $(az account get-access-token --query accessToken --output tsv --scope "https://search.azure.com/.default")
    $headers = @{"Authorization" = "Bearer $token"; "Accept" = "application/json"; "ContentType" = "application/json; charset=utf-8"}

    $indexes = $(Invoke-RestMethod -Uri $uri -Method GET -Headers $headers).value.name
    foreach ($index in $indexes) {
        $uri = $serviceUri `
            + "/indexes/$index/docs/`$count?api-version=2020-06-30"
        $req = [System.Net.WebRequest]::Create($uri)

        $req.ContentType = "application/json; charset=utf-8"
        $req.Accept = "application/json"
        $req.Headers["Authorization"] = "Bearer $token"

        $resp = $req.GetResponse()
        $reader = new-object System.IO.StreamReader($resp.GetResponseStream())
        $result = $reader.ReadToEnd()
        $documentCount = [int]$result

        $pageCount = [math]::ceiling($documentCount / 500) 

        $job = 1..$pageCount  | ForEach-Object -Parallel {
            $skip = ($_ - 1) * 500
            $uri = $using:serviceUri + "/indexes/$($using:index)/docs?api-version=2020-06-30&search=*&`$skip=$($skip)&`$top=500&searchMode=all"
            $outputPath = "$($using:index)_$($_).json"
            Invoke-RestMethod -Uri $uri -Method GET -Headers $using:headers -ContentType "application/json" |
                ConvertTo-Json -Depth 9 |
                Set-Content $outputPath
            "Output: $uri"
            azcopy copy $outputPath "$($using:pathPrefix)/$($using:index)/"
        } -ThrottleLimit 5 -AsJob
        $job | Receive-Job -Wait
    }
}

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    # Create backups container
    az storage container create -n backups --account-name $destinationStorageAccount

    # Get main storage account
    $sourceStorageAccountName = ""
    foreach ($resourceId in (az storage account list -g $resourceGroup --query "@[].id" --output tsv)) {
        if ((az tag list --resource-id $resourceId --query "contains(keys(@.properties.tags), 'azd-env-name')") -eq $true) {
            $sourceStorageAccountName = $(az resource show --ids $resourceId --query "@.name" --output tsv)
            Write-Host "Selecting $sourceStorageAccountName as the storage account"
            break;
        }
    }

    if ([string]::IsNullOrEmpty($sourceStorageAccountName)) {
        throw "Could not find any storage accounts with the azd-env-name tag in $resourceGroup."
    }

    # Recursively copy storage account contents
    foreach ($container in (az storage container list --account-name $sourceStorageAccountName --query "@[].name" --auth-mode login -o tsv)) {
        azcopy copy "https://$($sourceStorageAccountName).blob.core.windows.net/$container/" "https://$destinationStorageAccount.blob.core.windows.net/backups/$resourceGroup/" --recursive
    }

    $sourceSearchServiceName = ""
    foreach ($resourceId in (az search service list -g $resourceGroup --query "@[].id" --output tsv)) {
        if ((az tag list --resource-id $resourceId --query "contains(keys(@.properties.tags), 'azd-env-name')") -eq $true) {
            $sourceSearchServiceName = $(az resource show --ids $resourceId --query "@.name" --output tsv)
            Write-Host "Selecting $sourceSearchServiceName as the search service"
            break;
        }
    }
    
    if ([string]::IsNullOrEmpty($sourceSearchServiceName)) {
        throw "Could not find any search services with the azd-env-name tag in $resourceGroup."
    }

    # Save Search Indexes
    Get-SearchServiceIndexes -searchServiceName $sourceSearchServiceName -pathPrefix "https://$destinationStorageAccount.blob.core.windows.net/backups/$resourceGroup/data-sources/indexes"
}
catch {
    if (Test-Path -Path "$env:HOME/.azcopy") {
        $logFile = Get-ChildItem -Path "$env:HOME/.azcopy" -Filter "*.log" | `
            Where-Object { $_.Name -notlike "*-scanning*" } | `
            Sort-Object LastWriteTime -Descending | `
            Select-Object -First 1
        $logFileContent = Get-Content -Raw -Path $logFile.FullName
        Write-Host $logFileContent
    }
    Write-Host $_.Exception.Message
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}