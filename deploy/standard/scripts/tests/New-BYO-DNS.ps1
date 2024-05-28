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
    $dnsResourceGroup = $resourceGroup["dns"]
    $location = $manifest.location

    # Use the Azure CLI to create the "dns" resource group
    Invoke-CLICommand "Create Resource Group ${dnsResourceGroup} in ${location}..." {
        az group create `
            --name $dnsResourceGroup `
            --location $location
    }

    $privateDnsZone = @{
        agentsvc             = "privatelink.agentsvc.azure-automation.net"
        aks                  = "privatelink.$($location).azmk8s.io"
        blob                 = "privatelink.blob.core.windows.net"
        cognitiveservices    = "privatelink.cognitiveservices.azure.com"
        configuration_stores = "privatelink.azconfig.io"
        cosmosdb             = "privatelink.documents.azure.com"
        cr                   = "privatelink.azurecr.io"
        cr_region            = "${location}.privatelink.azurecr.io"
        dfs                  = "privatelink.dfs.core.windows.net"
        eventgrid            = "privatelink.eventgrid.azure.net"
        file                 = "privatelink.file.core.windows.net"
        monitor              = "privatelink.monitor.azure.com"
        ods                  = "privatelink.ods.opinsights.azure.com"
        oms                  = "privatelink.oms.opinsights.azure.com"
        openai               = "privatelink.openai.azure.com"
        queue                = "privatelink.queue.core.windows.net"
        search               = "privatelink.search.windows.net"
        sites                = "privatelink.azurewebsites.net"
        sql_server           = "privatelink.database.windows.net"
        table                = "privatelink.table.core.windows.net"
        vault                = "privatelink.vaultcore.azure.net"
    }

    # Iterate the list of private DNS zones and create them in the resource group using the Azure CLI
    $privateDnsZone.GetEnumerator() | ForEach-Object {
        $zoneName = $_.Value
        Invoke-CLICommand "Create Private DNS Zone $zoneName" {
            az network private-dns zone create `
                --name $zoneName `
                --resource-group $dnsResourceGroup `
        }
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
