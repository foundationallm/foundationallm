#!/usr/bin/env pwsh

# This script will run the pre-deployment tasks for the FoundationAllM solution.
# Depending on customer setup it might not be needed to run all tasks.

param(
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

    Invoke-AndRequireSuccess "Generate Certificates" {
        ./pre-provision/Get-LetsEncryptCertificates.ps1 `
            -baseDomain $manifest.baseDomain `
            -email $manifest.letsEncryptEmail `
            -subdomainPrefix $manifest.project
    }
}
finally {
    Pop-Location
}
