#! /usr/bin/pwsh
<#
.SYNOPSIS
    Restarts all container apps in a specified Azure resource group.

.DESCRIPTION
    This script retrieves all container apps within a given Azure resource group and restarts each one. 
    It requires the Azure CLI to be installed and authenticated. 
    The script accepts two mandatory parameters: the resource group name and the subscription ID.

.PARAMETER resourceGroup
    The name of the Azure resource group containing the container apps to restart. This parameter is mandatory.

.PARAMETER subscriptionId
    The subscription ID associated with the Azure account. This parameter is mandatory.

.EXAMPLE
    ./Restart-Acas.ps1 -resourceGroup "myResourceGroup" -subscriptionId "12345678-1234-1234-1234-123456789abc"
    This example shows how to run the script to restart all container apps in the resource group "myResourceGroup" within the specified subscription.

.NOTES
    Set the Azure CLI context to the appropriate subscription before running this script.
    You can set the subscription using the 'az account set --subscription' command.
#>

Param(
	# Mandatory parameters
	[parameter(Mandatory = $true)][string]$resourceGroup,
	[parameter(Mandatory = $true)][string]$subscriptionId
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Get the list of container apps in the resource group
$containerApps = (az containerapp list -g $resourceGroup --query "[].name" --output tsv)

Write-Host -ForegroundColor Blue "Restarting container apps using these parameters: Resource group: ${resourceGroup} Subscription:   ${subscriptionId}"

foreach ($acaName in $containerApps) {
	try {
		$revision = (az containerapp show --name $acaName --resource-group $resourceGroup --subscription $subscriptionId --query "properties.latestRevisionName" -o tsv)
		Write-Host -ForegroundColor Yellow "Restarting container app ${acaName}"
		az containerapp revision restart `
			--revision $revision `
			--name $acaName `
			--resource-group $resourceGroup `
			--subscription $subscriptionId

		if ($LASTEXITCODE -ne 0) {
			Write-Error("Failed to restart the container app ${acaName}")
		}
	}
 catch {
		Write-Error("An error occurred while restarting the container app ${acaName}: $_")
	}
}