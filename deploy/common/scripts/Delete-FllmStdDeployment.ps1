#! /usr/bin/pwsh
<#
.SYNOPSIS
Deletes resource groups and associated resources in Azure.

.DESCRIPTION
This script is used to delete resource groups and their associated resources in Azure. It takes the environment name, project identifier, and Azure region/location as parameters. The script loops through a list of resource groups to be deleted and checks if each resource group exists before deleting the resources within it. The script also deletes and purges Key Vaults, OpenAI resources, and App Configurations within each resource group. After deleting the resources, the script deletes the resource group itself. It then checks the status of the deletion until all resource groups are confirmed to be deleted.

.PARAMETER env
The environment name. Default value is "prd".

.PARAMETER project
The project identifier or name. Default value is "fllm".

.PARAMETER location
The Azure region/location. Default value is "eastus2".

.EXAMPLE
Delete-FllmStdDeployment -env "dev" -project "fllm" -location "westeurope"
Deletes resource groups and associated resources in the "dev" environment, with the project identifier "fllm" and Azure region "westeurope".

.NOTES
This script requires the Azure CLI to be installed and authenticated.
#>

param (
	[string]$env = "foo", # The environment name (e.g., prd, dev, etc.)
	[string]$project = "test", # The project identifier or name
	[string]$location = "eastus2" # The Azure region/location (e.g., eastus2, westeurope, etc.)
)

# List of resource groups to delete
$resourceGroups = @(
	"rg-$env-$location-app-$project",
	"rg-$env-$location-auth-$project",
	"rg-$env-$location-data-$project",
	"rg-$env-$location-jbx-$project",
	"rg-$env-$location-net-$project",
	"rg-$env-$location-oai-$project",
	"rg-$env-$location-ops-$project",
	"rg-$env-$location-storage-$project",
	"rg-$env-$location-vec-$project"
)

# Function to delete and purge Key Vaults, OpenAI, and App Configurations
function Delete-And-Purge-Resources($rg) {
	# Delete Key Vaults in the specified resource group
	$keyVaults = az keyvault list --resource-group $rg --query "[].name" -o tsv
	foreach ($kv in $keyVaults) {
		az keyvault delete --name $kv --location $location
		az keyvault purge --name $kv --location $location
	}

	# Delete OpenAI resources in the specified resource group (assuming a method to list them)
	$openAIResources = az cognitiveservices account list --resource-group $rg --query "[?kind=='OpenAI'].name" -o tsv
	foreach ($oai in $openAIResources) {
		az cognitiveservices account delete --name $oai --resource-group $rg --location $location
		az cognitiveservices account purge --name $oai --resource-group $rg --location $location
	}

	# Delete App Configurations in the specified resource group
	$appConfigs = az appconfig list --resource-group $rg --query "[].name" -o tsv
	foreach ($ac in $appConfigs) {
		az appconfig delete --name $ac --resource-group $rg --yes
		az appconfig purge --name $ac --resource-group $rg --yes
	}
}

# Loop through each resource group, check if it exists, delete resources, and then delete the resource group
foreach ($rg in $resourceGroups) {
	# Check if the resource group exists before processing
	$exists = az group exists --name $rg
	if ($exists -eq "true") {
		Write-Host -ForegroundColor Blue "Processing resource group: $rg"
		Delete-And-Purge-Resources -rg $rg
		az group delete --name $rg --no-wait --yes
	}
 else {
		Write-Host "Resource group $rg does not exist. Skipping..."
	}
}

# Check the status of the deletion until all resource groups are confirmed deleted
while ($true) {
	$remainingGroups = @()
	foreach ($rg in $resourceGroups) {
		$exists = az group exists --name $rg
		if ($exists -eq "true") {
			$remainingGroups += $rg
		}
	}

	# If all resource groups have been deleted, exit the loop
	if ($remainingGroups.Count -eq 0) {
		Write-Host -ForegroundColor Green "All resource groups have been successfully deleted."
		break
	}
	else {
		Write-Host -ForegroundColor Blue "Waiting for the following resource groups to be deleted: $($remainingGroups -join ', ')"
		Start-Sleep -Seconds 30
	}
}