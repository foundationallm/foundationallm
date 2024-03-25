#!/usr/bin/pwsh

<#
.SYNOPSIS
    Uploads system prompts to Azure storage containers.

.DESCRIPTION
    This script uploads system prompts to Azure storage containers. It ensures that the required containers exist and uploads the data to each container.

.PARAMETER resourceGroup
    Specifies the name of the resource group where the storage account is located. This parameter is mandatory.

.PARAMETER location
    Specifies the location of the storage account. This parameter is mandatory.

.EXAMPLE
    UploadSystemPrompts.ps1 -resourceGroup "myResourceGroup" -location "westus"
#>

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location
)

Set-PSDebug -Trace  0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
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

$scriptRoot = $MyInvocation.InvocationName | Split-Path
$dataPath = Join-Path $scriptRoot .. .. .. "common" "data" | Resolve-Path
Push-Location $dataPath
try {
    $storageAccount = Invoke-AndRequireSuccess "Getting storage account name" {
        az storage account list `
            --resource-group $resourceGroup `
            --query '[0].name' `
            --output tsv
    }

    $data = @("agents", "data-sources", "foundationallm-source", "prompts", "resource-provider")
    foreach ($container in $data) {
        Invoke-AndRequireSuccess "Ensuring $($container) container exists" {
            $container = az storage container show `
                --account-name $storageAccount `
                --name $container `
                --auth-mode key `
                --query "name" `
                --output tsv `
                --only-show-errors

            if (-not $container) {
                az storage container create `
                    --account-name $storageAccount `
                    --name $container `
                    --only-show-errors
            }
            else {
                Write-Host "Container '$($container)' already exists" -ForegroundColor Yellow
            }
        }

        Invoke-AndRequireSuccess "Uploading $($container) data" {
            az storage azcopy blob upload `
                --container $container `
                --account-name $storageAccount `
                --source "./$($container)/*" `
                --recursive `
                --only-show-errors
        }
    }
}
finally {
    Pop-Location
}
