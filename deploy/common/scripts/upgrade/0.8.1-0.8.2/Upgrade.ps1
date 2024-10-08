#!/usr/bin/env pwsh

Param(
    [Parameter(Mandatory = $true)][string]$opsResourceGroup,
    [Parameter(Mandatory = $true)][string]$storageResourceGroup
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    <#
    .SYNOPSIS
    Invokes a script block and requires it to execute successfully.

    .DESCRIPTION
    The Invoke-AndRequireSuccess function is used to invoke a script block and ensure that it executes successfully. It takes a message and a script block as parameters. The function will display the message in blue color, execute the script block, and check the exit code. If the exit code is non-zero, an exception will be thrown.

    .PARAMETER Message
    The message to be displayed before executing the script block.

    .PARAMETER ScriptBlock
    The script block to be executed.

    .EXAMPLE
    Invoke-AndRequireSuccess -Message "Running script" -ScriptBlock {
        # Your script code here
    }

    This example demonstrates how to use the Invoke-AndRequireSuccess function to run a script block and require it to execute successfully.

    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    # Get main storage account
    $sourceStorageAccountName = ""
    foreach ($resourceId in (az storage account list -g $storageResourceGroup --query "@[].id" --output tsv)) {
        if ((az tag list --resource-id $resourceId --query "contains(keys(@.properties.tags), 'azd-env-name')") -eq $true) {
            $sourceStorageAccountName = $(az resource show --ids $resourceId --query "@.name" --output tsv)
            Write-Host "Selecting $sourceStorageAccountName as the storage account"
            break;
        }
    }

    # # Create backups container
    # az storage container create -n backups --account-name $sourceStorageAccountName

    if ([string]::IsNullOrEmpty($sourceStorageAccountName)) {
        throw "Could not find any storage accounts with the azd-env-name tag in $storageResourceGroup."
    }

    Remove-Item ./containers -Recurse

    # Recursively copy storage account contents
    azcopy copy "https://$($sourceStorageAccountName).blob.core.windows.net/resource-provider/" ./containers/ --recursive

    # TODO Update file names and contents for resource-provider
    $providerFileMapping = @{
        Agent = @{
            Src = "_agent-references.json"
            Dest = "_resource-references.json"
            SrcProperty = "AgentReferences"
            DestProperty = "ResourceReferences"
        }
        AIModel = @{
            Src = "_ai-model-references.json"
            Dest = "_resource-references.json"
            SrcProperty = "ResourceReferences"
            DestProperty = "ResourceReferences"
        }
        Configuration = @{
            Src = "_api-endpoint-references.json"
            Dest = "_resource-references.json"
            SrcProperty = "ResourceReferences"
            DestProperty = "ResourceReferences"
        }
        DataSource = @{
            Src = "_data-source-references.json"
            Dest = "_resource-references.json"
            SrcProperty = "DataSourceReferences"
            DestProperty = "ResourceReferences"
        }
        Prompt = @{
            Src = "_prompt-references.json"
            Dest = "_resource-references.json"
            SrcProperty = "PromptReferences"
            DestProperty = "ResourceReferences"
        }
    }

    foreach ($providerFile in $providerFileMapping.GetEnumerator()) {
        $agentReferences = Get-Content ./containers/resource-provider/FoundationaLLM.$($providerFile.Name)/$($providerFile.Value.Src) | Out-String | ConvertFrom-Json
        $resourceReferences = $agentReferences
        $srcProperty = ($agentReferences | Select -ExpandProperty $($providerFile.Value.SrcProperty))
        $resourceReferences.PSObject.Properties.Remove($($providerFile.Value.SrcProperty))
        $resourceReferences | Add-Member -MemberType NoteProperty -Name $($providerFile.Value.DestProperty) -Value $srcProperty
        Set-Content -Path ./containers/resource-provider/FoundationaLLM.$($providerFile.Name)/$($providerFile.Value.Dest) -Value ($resourceReferences | ConvertTo-Json)
        Remove-Item ./containers/resource-provider/FoundationaLLM.$($providerFile.Name)/$($providerFile.Value.Src)
    }

    # Recursively copy storage account contents
    azcopy copy ./containers/resource-provider/* "https://$sourceStorageAccountName.blob.core.windows.net/resource-provider/" --recursive --overwrite=true

    $appConfigName = Invoke-AndRequireSuccess "Get AppConfig" {
        az appconfig list `
            --resource-group $opsResourceGroup `
            --query "[0].name" `
            --output tsv
    }

    $configurationFile = Resolve-Path "./appconfig.json"
    Invoke-AndRequireSuccess "Loading AppConfig Values" {
        az appconfig kv import `
            --profile appconfig/kvset `
            --name $appConfigName `
            --source file `
            --path $configurationFile `
            --format json `
            --yes `
            --output none
    }

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