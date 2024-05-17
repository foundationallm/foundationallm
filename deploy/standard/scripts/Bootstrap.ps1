#! /usr/bin/pwsh
<#
.SYNOPSIS
    Downloads and sets up AzCopy tool based on the operating system.

.DESCRIPTION
    This script checks the operating system and downloads the corresponding version of AzCopy. 
	It handles the setup for Windows, macOS, and Linux. 
	It also checks if AzCopy is already installed and verifies the login status.

.PARAMETER AZCOPY_VERSION
    Specifies the version of AzCopy to download and use. This parameter is fixed in the script.

.EXAMPLE
    .\SetupAzCopy.ps1

.NOTES
    Ensure that the PowerShell session has sufficient permissions to download and execute files, and that internet connectivity is available.
#>

# Disable command echo, Set strict mode, and stop script execution on any errors
Set-PSDebug -Trace 0
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# AzCopy tool version to be downloaded
$AZCOPY_VERSION = "10.24.0"

# Determine OS and set the download URL and file extension accordingly
if ($IsWindows) {
	$url = "https://aka.ms/downloadazcopy-v10-windows"
	$os = "windows"
	$ext = "zip"
}elseif ($IsMacOS) {
	$url = "https://aka.ms/downloadazcopy-v10-mac"
	$os = "darwin"
	$ext = "zip"
}elseif ($IsLinux) {
	$url = "https://aka.ms/downloadazcopy-v10-linux"
	$os = "linux"
	$ext = "tar.gz"
}

# Define paths for download and extraction
$outputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy.${ext}")
$destinationPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools")
$toolPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../tools/azcopy_${os}_amd64_${AZCOPY_VERSION}/azcopy")

# Check if AzCopy already exists, download and extract if not
if (Test-Path -Path "../tools/azcopy_${os}_amd64_${AZCOPY_VERSION}") {
	Write-Host "azcopy_${os}_amd64_${AZCOPY_VERSION} already exists."
}
else {
	Invoke-WebRequest -Uri $url -OutFile $outputPath
	if ($IsLinux) {
		tar -xvzf $outputPath -C $destinationPath
	}
	else {
		Expand-Archive -Path $outputPath -DestinationPath $destinationPath
	}
}

# Check AzCopy login status and prompt for login if not logged in
Write-Host -ForegroundColor Blue "Checking AzCopy login status..."
$status = & ../tools/azcopy_${os}_amd64_${AZCOPY_VERSION}/azcopy login status
if (-not $status.contains("Your login session is still active")) {
	Write-Host -ForegroundColor Blue "Please Follow the instructions below to login to Azure using AzCopy."
	& ../tools/azcopy_${os}_amd64_${AZCOPY_VERSION}/azcopy login
}
else {
	Write-Host -ForegroundColor Blue "AzCopy login session is still active."
}