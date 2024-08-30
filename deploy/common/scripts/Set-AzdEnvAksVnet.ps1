#! /usr/bin/pwsh
<#
.SYNOPSIS
Sets the environment variables for Networking in the default environment file.

.DESCRIPTION
This script sets the environment variables for Networking in the default
environment file located in the .azure directory. It takes the following
parameters:
- $fllmAksServiceCidr: The CIDR block for the AKS Services. Default value is "10.100.0.0/16".
- $fllmVnetCidr: The CIDR block for the VNet. Default value is "10.220.128.0/20".
- $fllmAllowedExternalCidrs: The CIDR block for NSGs to allow VPN or HUB VNet. Default value is "192.168.101.0/28".

.PARAMETER fllmAksServiceCidr
The CIDR block for the AKS Services.

.PARAMETER fllmVnetCidr
The CIDR block for the VNet.

.PARAMETER fllmAllowedExternalCidrs
The CIDR block for NSGs to allow VPN or HUB VNet.

.EXAMPLE
Set-AzdEnvAksVnet.ps1 -fllmAksServiceCidr "10.100.0.0/16" -fllmVnetCidr "10.220.128.0/20" -fllmAllowedExternalCidrs "192.168.101.0/28"
Sets the environment variables for Networking with the specified CIDR blocks.

.OUTPUTS
None. The script sets the environment variables in the default environment file.

.NOTES
This script requires the Function-Library.ps1 script to be present in the same
directory.

#>

Param(
	[parameter(Mandatory = $false, HelpMessage = "CIDR block for the AKS Services - e.g., 10.100.0.0/16")][string]
	$fllmAksServiceCidr = "10.100.0.0/16",
	[parameter(Mandatory = $false, HelpMessage = "CIDR block for the VNet - e.g., 10.220.128.0/20")][string]
	$fllmVnetCidr = "10.220.128.0/20",
	[parameter(Mandatory = $false, HelpMessage = "CIDR block for NSGs to allow VPN or HUB VNet - e.g., 192.168.101.0/28,10.0.0.0/16 - comma separated - updates allow-vpn nsg rule")][string]
	$fllmAllowedExternalCidrs = "192.168.101.0/28"
)

$TranscriptName = $($MyInvocation.MyCommand.Name) -replace ".ps1", ".transcript.txt"
Start-Transcript -path .\$TranscriptName -Force

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path
. $ScriptDirectory/Function-Library.ps1

function Get-HostNameFromPfxName {
	param(
		[string]$directory
	)

	# Fail if basePath directory does not exist
	if (-not (Test-Path -Path "$directory")) {
		throw "The basePath $directory does not exist."
	}

	# Get the first pfx file in the directory
	$certificate = Get-ChildItem -Path "$directory" -Filter "*.pfx" | Select-Object -First 1

	# Fail if certificate does not exist
	if (-not $certificate) {
		throw "No pfx files found in the $directory directory."
	}

	$hostname = $certificate.Name -replace "\.pfx$"
	return $hostname
}

# Define the path to the certificates
$basePath = "./certs" | Get-AbsolutePath

# Fail if base path does not exist
if (-not (Test-Path -Path $basePath)) {
	throw "The base path $basePath does not exist."
}

$hosts = @(
	"coreapi",
	"managementapi",
	"managementui",
	"chatui"
)
$hostnames = @{}
foreach ($hostId in $hosts) {
	$hostnames[$hostId] = Get-HostNameFromPfxName -directory "$basePath/$hostId"
}

# Set the environment values
$envValues = @{
	"FLLM_ALLOWED_CIDR"         = $fllmAllowedExternalCidrs
	"FLLM_CORE_API_HOSTNAME"    = $hostnames["coreapi"]
	"FLLM_AKS_SERVICE_CIDR"     = $fllmAksServiceCidr
	"FLLM_VNET_CIDR"            = $fllmVnetCidr
	"FLLM_MGMT_API_HOSTNAME"    = $hostnames["managementapi"]
	"FLLM_MGMT_PORTAL_HOSTNAME" = $hostnames["managementui"]
	"FLLM_USER_PORTAL_HOSTNAME" = $hostnames["chatui"]
}

# Show azd environments
$message = @"
Your azd environments are listed. Environment values updated for the default
environment file located in the .azure directory.
"@
Write-Host -ForegroundColor Blue $message
Invoke-CliCommand "azd env list" {
	azd env list
}

# Write AZD environment values
$message = @"
Setting azd environment values for Networking:
-------------------------------------------
FLLM Allowed External CIDRs: $fllmAllowedExternalCidrs
FLLM Vnet CIDR Range: $fllmVnetCidr
FLLM AKS Service CIDR Range: $fllmAksServiceCidr
Core API Hostname: $($hostnames["coreapi"])
Management API Hostname: $($hostnames["managementapi"])
Management Portal Hostname: $($hostnames["managementui"])
User Portal Hostname: $($hostnames["chatui"])
-------------------------------------------
"@
Write-Host -ForegroundColor Yellow $message

foreach ($value in $envValues.GetEnumerator()) {
	Write-Host -ForegroundColor Yellow "Setting $($value.Name) to $($value.Value)"
	azd env set $value.Name $value.Value
}

$message = @"
Environment values updated for the default environment file located in the
.azure directory.
Here are your current environment values:
"@
Write-Host -ForegroundColor Blue $message
Invoke-CliCommand "azd env get-values" {
	azd env get-values
}