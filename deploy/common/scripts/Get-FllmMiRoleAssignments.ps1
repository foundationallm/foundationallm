# Disable command echo and set strict mode
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# Variables - Set these according to your environment
$SubscriptionId = "6356d509-cdce-4a30-922d-ff7346a15a65"

# Resource Groups map: Define the resource groups as key-value pairs where
# the key is the resource group name and the value is the specific scope for that resource group.
$ResourceGroups = @{
	"rg-yale-eastus2-app-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-app-test"
	"rg-yale-eastus2-auth-test" = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-auth-test"
	"rg-yale-eastus2-data-test" = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-data-test"
	"rg-yale-eastus2-jbx-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-jbx-test"
	"rg-yale-eastus2-net-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-net-test"
	"rg-yale-eastus2-oai-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-oai-test"
	"rg-yale-eastus2-ops-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-ops-test"
	"rg-yale-eastus2-storage-test" = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-storage-test"
	"rg-yale-eastus2-vec-test"  = "/subscriptions/$SubscriptionId/resourceGroups/rg-yale-eastus2-vec-test"
	# Add more resource groups and their specific scopes as needed
}

# Set the subscription context
az account set --subscription $SubscriptionId

# Function to get role assignments at different scopes
function Get-RoleAssignments {
	param (
		[string]$PrincipalId,
		[string]$Scope
	)
	$RoleAssignments = az role assignment list --assignee $PrincipalId --scope $Scope --output json | ConvertFrom-Json
	return $RoleAssignments
}

# Loop through each resource group in the map
foreach ($ResourceGroup in $ResourceGroups.Keys) {
	$Scope = $ResourceGroups[$ResourceGroup]
	Write-Host "Processing resource group: $ResourceGroup with scope: $Scope"

	# Get all Managed Identities in the current resource group
	$ManagedIdentities = az identity list --resource-group $ResourceGroup --output json | ConvertFrom-Json

	if (-not $ManagedIdentities) {
		Write-Host "No Managed Identities found in resource group '$ResourceGroup'."
		Write-Host "----------------------------"
		continue
	}

	# Loop through each Managed Identity and summarize their role assignments for different scopes
	foreach ($ManagedIdentity in $ManagedIdentities) {
		Write-Host "  Managed Identity: $($ManagedIdentity.Name)"
        
		# Get role assignments at the subscription level
		$SubscriptionRoles = Get-RoleAssignments -PrincipalId $ManagedIdentity.PrincipalId -Scope "/subscriptions/$SubscriptionId"
        
		# Get role assignments at the resource group level
		$ResourceGroupRoles = Get-RoleAssignments -PrincipalId $ManagedIdentity.PrincipalId -Scope $Scope
        
		# Combine all role assignments
		$AllRoles = @($SubscriptionRoles + $ResourceGroupRoles)
        
		if (-not $AllRoles) {
			Write-Host "    No role assignments found at the subscription or resource group scope."
		}
		else {
			Write-Host "    Summary of Role Assignments:"
			foreach ($RoleAssignment in $AllRoles) {
				Write-Host "      - Role: $($RoleAssignment.roleDefinitionName), Scope: $($RoleAssignment.scope)"
			}
		}

		Write-Host "---"
	}
}

Write-Host "Script completed."