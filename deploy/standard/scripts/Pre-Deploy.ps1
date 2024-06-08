#!/usr/bin/env pwsh

<#
.SYNOPSIS
    This script runs the pre-deployment tasks for the FoundationAllM solution.

.DESCRIPTION
    This script performs necessary pre-deployment tasks such as loading deployment manifests,
    generating certificates, and creating required Azure AD app registrations and admin groups.
    Depending on the customer setup, some tasks can be skipped.

.EXAMPLE
    Pre-Deploy.ps1 -manifestName "Deployment-Manifest.json" -skipCertificates $true
#>

param(
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json",
    [parameter(Mandatory = $false)][bool]$skipCertificates = $false,
    [parameter(Mandatory = $false)][bool]$skipEntraIdApps = $false,
    [parameter(Mandatory = $false)][bool]$skipEntraIdAdminGroup = $false,
    [parameter(Mandatory = $false)][bool]$skipBootstrap = $false
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

    if (-not $skipCertificates) {
        Invoke-AndRequireSuccess "Generate Certificates" {
            ./pre-provision/Get-LetsEncryptCertificates.ps1 `
                -baseDomain $manifest.baseDomain `
                -email $manifest.letsEncryptEmail `
                -subdomainPrefix $manifest.project
        }
    }
    else {
        Write-Host "Skipping certificate generation as per the skipCertificates parameter." -ForegroundColor Yellow
    }
    Push-Location ./pre-provision
    if (-not $skipEntraIdApps) {
        Invoke-AndRequireSuccess "Create FLLM EntraID App Registrations" {
            ./Create-FllmEntraIdApps.ps1
        }
    }
    else {
        Write-Host "Skipping EntraID app registration creation as per the skipEntraIdApps parameter." -ForegroundColor Yellow
    }
    if (-not $skipEntraIdAdminGroup) {
        Invoke-AndRequireSuccess "Create FLLM Admin Group" {
            ./Create-FllmAdminGroup.ps1
        }
    }
    else {
        Write-Host "Skipping EntraID admin group creation as per the skipEntraIdAdminGroup parameter." -ForegroundColor Yellow
    }
    if (-not $skipBootstrap) {
        Invoke-AndRequireSuccess "Bootstrap the FoundationAllM solution" {
            ./Bootstrap.ps1
        }
    }
    else {
        Write-Host "Skipping bootstrap as per the skipBootstrap parameter." -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}
