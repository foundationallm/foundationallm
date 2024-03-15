#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
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

$storageAccountAdls = Invoke-AndRequireSuccess "Get ADLS Auth Storage Account" {
    az storage account list `
        --resource-group $resourceGroup `
        --query "[?kind=='StorageV2'].name | [0]" `

        --output tsv

}

Invoke-AndRequireSuccess "Uploading Default Role Assignments to Authorization Store" {
    az storage azcopy blob upload `
        -c role-assignments `
        --account-name $storageAccountAdls.name `
        -s "../data/role-assignments/DefaultRoleAssignments.json" `
        --recursive `
        --only-show-errors `
        --output none
}

