#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$stepDeployCerts = $false,
    [parameter(Mandatory = $false)][bool]$stepDeployImages = $false,
    [parameter(Mandatory = $false)][bool]$init = $true,
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load the Invoke-AndRequireSuccess function
. ./utility/Invoke-AndRequireSuccess.ps1

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Write-Host "Loading Deployment Manifest ../${manifestName}" -ForegroundColor Blue
    $manifest = $(Get-Content -Raw -Path ../${manifestName} | ConvertFrom-Json)

    # Convert the manifest resource groups to a hashtable for easier access
    $resourceGroup = @{}
    $manifest.resourceGroups.PSObject.Properties | ForEach-Object { $resourceGroup[$_.Name] = $_.Value }

    Invoke-AndRequireSuccess "Generate Host File" {
        ./Generate-Hosts.ps1 `
            -resourceGroup $resourceGroup `
            -subscription $manifest.subscription
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
