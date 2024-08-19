#! /usr/bin/pwsh
<#
.SYNOPSIS
    Tests connectivity to specified hostnames using TCP port 443 and verifies HTTPS endpoints.

.DESCRIPTION
    This script tests the connectivity to specified hostnames (userPortalHostName, managementPortalHostName, coreApiHostName, and managementApiHostName) using the `Test-Connection` cmdlet.
    It uses TCP port 443 and provides detailed output with a count of 2 attempts.
    The script also verifies the HTTP status of specified endpoints using `Invoke-WebRequest`.
    The script can be used to verify network connectivity to web portals or services.

.PARAMETER userPortalHostName
    The hostname of the User Portal to test connectivity.

.PARAMETER managementPortalHostName
    The hostname of the Management Portal to test connectivity.

.PARAMETER coreApiHostName
    The hostname of the Core API to verify the status.

.PARAMETER managementApiHostName
    The hostname of the Management API to verify the status.

.PARAMETER fllmInstanceId
    The instance ID for the FLLM service.

.PARAMETER fllmBaseDomain
    The base domain for the FLLM service.

.EXAMPLE
    ./Test-FllmConnection.ps1 -userPortalHostName "userportal" -managementPortalHostName "managementportal" -coreApiHostName "coreapi" -managementApiHostName "managementapi" -fllmInstanceId "your-instance-id" -fllmBaseDomain "example.com"
    This example shows how to run the script to test connectivity to the FLLM User Portal and Management Portal, and verify the Core API and Management API endpoints.
	
.NOTES
    This script requires PowerShell Core to be installed on the system.
    The script uses the `Test-Connection` cmdlet and `Invoke-WebRequest` cmdlet.
    Note that the script is looking for hostnames for connectivity tests and constructs URLs for HTTP status verification.
	The instance ID can be found in the .env file in the azd project root of the project.
	example: FOUNDATIONALLM_INSTANCE_ID="d00028c2-89e1-4e76-bca2-fdc64af89f4f"
#>

param (
	[Parameter(Mandatory = $false)]
	[string]$coreApiHostName = "ai-api",

	[Parameter(Mandatory = $false)]
	[string]$fllmBaseDomain = "yale.edu",

	[Parameter(Mandatory = $false)]
	[string]$fllmInstanceId = "747c1c88-fc26-4336-be46-2f454109e922",

	[Parameter(Mandatory = $false)]
	[string]$managementApiHostName = "ai-management-api",

	[Parameter(Mandatory = $false)]
	[string]$managementPortalHostName = "ai-management",

	[Parameter(Mandatory = $false)]
	[string]$userPortalHostName = "ai-chat"
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Function to test TCP connection
function Test-HostConnection {
	param (
		[string]$Hostname
	)
	Write-Host -ForegroundColor Blue "Testing connection to $Hostname..."
	$result = Test-Connection $Hostname -TcpPort 443 -Detailed -Count 2
	if ($result) {
		Write-Host -ForegroundColor Yellow "Connection to $Hostname successful."
	}
 else {
		Write-Host -ForegroundColor Red "Connection to $Hostname failed."
	}
	Write-Host -ForegroundColor Blue "----------------------------------------"
}

# Function to test HTTPS endpoint
function Test-HttpEndpoint {
	param (
		[string]$Url
	)
	Write-Host -ForegroundColor Blue "Testing HTTP endpoint $Url..."
	try {
		$response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10
		Write-Host -ForegroundColor Yellow "Status Code: $($response.StatusCode)"
	}
	catch [System.Net.WebException] {
		Write-Host -ForegroundColor Red "WebException: $($_.Exception.Message)"
	}
	catch {
		Write-Host -ForegroundColor Red "Failed to reach $Url. Error: $_"
	}
	Write-Host "----------------------------------------"
}

# Construct hostnames
$hostnames = @(
	"$userPortalHostName.$fllmBaseDomain",
	"$managementPortalHostName.$fllmBaseDomain"
)

# Construct URLs
$urls = @(
	"https://$coreApiHostName.$fllmBaseDomain/core/instances/$fllmInstanceId/status",
	"https://$managementApiHostName.$fllmBaseDomain/management/instances/$fllmInstanceId/status",
	"https://$userPortalHostName.$fllmBaseDomain/core/ai-chat",
	"https://$managementPortalHostName.$fllmBaseDomain/management/ai-management"
)

# Test TCP connection for each hostname
$hostnames | ForEach-Object { Test-HostConnection -Hostname $_ }

# Test HTTPS endpoints for each URL
$urls | ForEach-Object { Test-HttpEndpoint -Url $_ }