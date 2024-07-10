#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$init = $true,
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-CLICommand {
    <#
    .SYNOPSIS
    Invoke a CLI Command and allow all output to print to the terminal.  Does not check for return values or pass the output to the caller.
    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }
}


# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Write-Host "Loading Deployment Manifest ../../${manifestName}" -ForegroundColor Blue
    $manifest = $(Get-Content -Raw -Path ../../${manifestName} | ConvertFrom-Json)

    if ($init) {
        Invoke-CLICommand "Login to Azure" {
            az login
            az account set --subscription $manifest.subscription
            az account show
        }
    }

    # Convert the manifest resource groups to a hashtable for easier access
    $resourceGroup = @{}
    $manifest.externalResourceGroups.PSObject.Properties | ForEach-Object { $resourceGroup[$_.Name] = $_.Value }
    $vnetName = "vnet-$($manifest.project)-$($manifest.environment)-$($manifest.location)-pre"

    # Use the Azure CLI to create the "net" resource group
    Invoke-CLICommand "Create Resource Group" {
        az group create `
            --name $resourceGroup["net"] `
            --location $manifest.location
    }

    # Use the Azure CLI to create a virtual network in the resource group
    Invoke-CLICommand "Create Virtual Network" {
        az network vnet create `
            --name $vnetName `
            --resource-group $resourceGroup["net"] `
            --location $manifest.location `
            --address-prefixes '10.220.128.0/21'
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
