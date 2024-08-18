#! /usr/bin/pwsh

Param(
	[parameter(Mandatory = $true)][string]$subscriptionId,
	[parameter(Mandatory = $true)][string]$azdEnvName
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

. ./Function-Library.ps1

# Set the subscription context
Invoke-CliCommand "Set the subscription context" {
	az account set --subscription $subscriptionId
}

# List all Resource Groups
$subscriptionRgs = $null
Invoke-CliCommand "List all resource groups in the subscription" {
	$script:subscriptionRgs = az group list `
		--subscription $subscriptionId `
		--output json | `
		ConvertFrom-Json -AsHashtable
}

# Filter for the resource groups where the tags property exists
$resourceGroups = @{}
foreach ($rg in $subscriptionRgs) {
	if (-not ($rg.tags -is [hashtable] -and $rg.tags.ContainsKey('azd-env-name'))) {
		continue
	}

	if ($rg.tags.'azd-env-name' -eq $azdEnvName) {
		$resourceGroups[$rg.name] += $rg.id
	}
}

# Loop through each resource group in the map
$report = @()
foreach ($resourceGroup in $resourceGroups.GetEnumerator()) {
	Write-Host "Processing resource group: $($resourceGroup.Key)"
	$report += "Resource Group: $($resourceGroup.Key)"

	# Get all Managed Identities in the current resource group
	$managedIdentities = @()
	Invoke-CliCommand "List all managed identities in the resource group" {
		$script:managedIdentities = az identity list `
			--resource-group $resourceGroup.Key `
			--output json | `
			ConvertFrom-Json -AsHashtable
	}

	# Check if we found any managed identities is it null or empty
	if (-not $managedIdentities -or $managedIdentities.Count -eq 0) {
		$report += "  No Managed Identities found in resource group '$($resourceGroup.Key)'."
		$report += "---"
		continue
	}

	# Loop through each Managed Identity and summarize their role assignments for different scopes
	foreach ($managedIdentity in $managedIdentities) {
		$report += "  Managed Identity: $($managedIdentity.name)"

		# Get role assignments for the identity
		$roleAssignments = $null
		Invoke-CliCommand "Get role assignments for the managed identity" {
			$script:roleAssignments = az role assignment list `
				--all `
				--assignee $managedIdentity.principalId `
				-o json | `
				ConvertFrom-Json -AsHashtable
		}

		# Check if we found any role assignments
		if (-not $roleAssignments -or $roleAssignments.Count -eq 0) {
			$report += "    No role assignments found for the Managed Identity."
			$report += "---"
			continue
		}

		$report += "    Summary of Role Assignments:"
		foreach ($roleAssignment in $roleAssignments) {
			$report += "      - Role: $($roleAssignment.roleDefinitionName), Scope: $($roleAssignment.scope)"
		}

		$report += "---"
	}
	Write-Host "---"
}

# Save the report to a file
$reportPath = "./role-assignments-report.txt"
$report | Out-File -FilePath $reportPath

Write-Host "Script completed."