#! /usr/bin/pwsh
<#
.SYNOPSIS
    Updates environment variables in Azure Developer CLI (azd) based on certificate filenames in a specified folder structure.

.DESCRIPTION
    This script reads .pfx certificate filenames from specific directories and uses them to update environment variables in Azure Developer CLI (azd). 
    It assumes that each folder contains a single .pfx file, and the script will trim the ".pfx" extension from the filename to use as the hostname.
    It also outputs the current environment settings before and after the update.

.PARAMETER coreApiHostname
    The hostname for the Core API. This value is derived from the certificate file found in the "coreapi" directory.

.PARAMETER mgmtApiHostname
    The hostname for the Management API. This value is derived from the certificate file found in the "managementapi" directory.

.PARAMETER mgmtPortalHostname
    The hostname for the Management Portal. This value is derived from the certificate file found in the "managementapi" directory.

.PARAMETER userPortalHostname
    The hostname for the User Portal. This value is derived from the certificate file found in the "chatui" directory.

.EXAMPLE
    ./Set-AzdAksNet.ps1 -coreApiHostname "coreapi.example.com" -mgmtApiHostname "mgmtapi.example.com" -mgmtPortalHostname "portal.example.com" -userPortalHostname "userportal.example.com"
    This example shows how to run the script with specified hostnames for each component.

.NOTES
    The script assumes that the .pfx files are named according to the desired hostnames and are located in their respective folders under "./certs".
#>

Param(
	[parameter(Mandatory = $false)][string]$fllmAksServiceCidr = "10.100.0.0/16", # CIDR block for the VNet - e.g., 10.100.0.0/16
	[parameter(Mandatory = $false)][string]$fllmVnetCidr = "10.220.128.0/20", # CIDR block for the VNet - e.g., 10.220.128.0/20
	[parameter(Mandatory = $false)][string]$fllmAllowedExternalCidrs = "192.168.101.0/28" # CIDR block for NSGs to allow VPN or HUB VNet - e.g., 192.168.101.0/28,10.0.0.0/16 - comma separated - updates allow-vpn nsg rule
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Define the path to the certificates
$basePath = "./certs"

# Read and process the filenames
$coreApiHostname = (Get-ChildItem -Path "$basePath/coreapi" -Filter "*.pfx").Name -replace "\.pfx$"
$mgmtApiHostname = (Get-ChildItem -Path "$basePath/managementapi" -Filter "*.pfx").Name -replace "\.pfx$"
$mgmtPortalHostname = (Get-ChildItem -Path "$basePath/managementapi" -Filter "*.pfx").Name -replace "\.pfx$"
$userPortalHostname = (Get-ChildItem -Path "$basePath/chatui" -Filter "*.pfx").Name -replace "\.pfx$"

# Set the environment values
$values = @(
	"FLLM_ALLOWED_CIDR=$fllmAllowedExternalCidrs",
	"FLLM_CORE_API_HOSTNAME=$coreApiHostname",
	"FLLM_AKS_SERVICE_CIDR=$fllmAksServiceCidr",
	"FLLM_VNET_CIDR=$fllmVnetCidr",
	"FLLM_MGMT_API_HOSTNAME=$mgmtApiHostname",
	"FLLM_MGMT_PORTAL_HOSTNAME=$mgmtPortalHostname",
	"FLLM_USER_PORTAL_HOSTNAME=$userPortalHostname"
)

# Show azd environments
Write-Host -ForegroundColor Blue "Your azd environments are listed. Environment values updated for the default environment file located in the .azure directory."
azd env list

# Write AZD environment values
Write-Host -ForegroundColor Yellow "Setting azd environment values for Networking:"
Write-Host -ForegroundColor Yellow "-------------------------------------------"
Write-Host -ForegroundColor Yellow "FLLM Allowed External CIDRs: $fllmAllowedExternalCidrs"
Write-Host -ForegroundColor Yellow "FLLM Vnet CIDR Range: $fllmVnetCidr"
Write-Host -ForegroundColor Yellow "FLLM AKS Service CIDR Range: $fllmAksServiceCidr"
Write-Host -ForegroundColor Yellow "Core API Hostname: $coreApiHostname"
Write-Host -ForegroundColor Yellow "Management API Hostname: $mgmtApiHostname"
Write-Host -ForegroundColor Yellow "Management Portal Hostname: $userPortalHostname"
Write-Host -ForegroundColor Yellow "User Portal Hostname: $userPortalHostname"

foreach ($value in $values) {
	$key, $val = $value -split '=', 2
	Write-Host -ForegroundColor Yellow "Setting $key to $val"
	azd env set $key $val
}

Write-Host -ForegroundColor Blue "Environment values updated for the default environment file located in the .azure directory."
Write-Host -ForegroundColor Blue "Here are your current environment values:"
azd env get-values