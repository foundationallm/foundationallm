#! /usr/bin/pwsh
<#
.SYNOPSIS
    Destructive script that purges Azure AD applications based on a predefined list of names.

.DESCRIPTION
    This script uses the Azure CLI to delete all Azure AD applications that match any of the names in a hardcoded list.

#>

Param(
	[parameter(Mandatory=$false)][bool]$interactiveMode = $true
)

# Predefined list of application names to delete
$AppNames = @("FoundationaLLM-E2E", "FoundationaLLM-Authorization-E2E", "FoundationaLLM-Management-E2E-Client", "FoundationaLLM-Management-E2E", "FoundationaLLM-E2E-Client")

# Function to filter and delete Azure AD applications based on display name
function Delete-AppByName {
	param(
		[string]$Name,
		[bool]$interactiveMode
	)

	# Find applications matching the name
	$appIds = az ad app list --all --query "[].{appId:appId,displayName:displayName}[?displayName=='$Name'].appId" --output tsv

	if (-not $appIds) {
		Write-Host "No applications found with name: $Name"
		return
	}

	# Display warning message and ask for confirmation
	Write-Host "Applications to be deleted with name '$Name':"
	az ad app list --all --query "[].{displayName:displayName}[?displayName=='$Name'].displayName" --output table

	if ($interactiveMode) {
		$confirmation = Read-Host "Are you sure you want to delete these applications? (yes/no)"
	} else {
		$confirmation = "yes"
	}

	# Delete applications if confirmed
	if ($confirmation -eq "yes") {
		foreach ($appId in $appIds.Split("`n")) {
			Write-Host "Deleting Azure AD application with ID: $appId"
			az ad app delete --id $appId --only-show-errors
		}
	}
 else {
		Write-Host "Deletion cancelled for applications with name: $Name"
	}
}

# Process each application name provided in the list
foreach ($appName in $AppNames) {
	Delete-AppByName -Name $appName -interactiveMode $interactiveMode
}
