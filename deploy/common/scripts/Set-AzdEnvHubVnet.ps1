#! /usr/bin/pwsh
<#
.SYNOPSIS
    Sets environment values for VNET peering to a Hub Network in Azure.

.DESCRIPTION
    This script retrieves the VNet ID for a specified VNet in Azure and sets environment values for VNET peering. 
    It uses the Azure CLI to retrieve the VNet ID and sets the environment values using the Azure Developer CLI (azd).

.PARAMETER hubResourceGroupName
    The name of the resource group containing the hub VNet.

.PARAMETER hubSubscriptionId
    The subscription ID under which the hub VNet is managed.

.PARAMETER hubTenantId
    The tenant ID of the Entra directory  where the hub VNet resides.

.PARAMETER hubVnetName
    The name of the hub Virtual Network (VNet).

.EXAMPLE
	Standard deployment:
	from ./foundationallm/deploy/standard run:
    ../common/scripts/Set-AzdEnvHubVnet.ps1 -hubResourceGroupName "rg-fllm-hub-eastus2-net" -hubSubscriptionId "12345678-1234-1234-1234-1234567890ab" -hubVnetName "vnet-fllm-hub-eastus2-net" -hubTenantId "12345678-1234-1234-1234-1234567890ab"
.NOTES
	This script must be run from the ./deploy/standard directory after creating the azd environment using the azd env create command.
	It will populate the .env file located in the hidden .azure directory in the root of the project.
	This script requires the Azure CLI and the Azure Developer CLI (azd) to be installed.
	These values must be set prior to running the Standard deployment and will connect the FLLM network to the hub network.
#>
param (
	[Parameter(Mandatory = $true)][string]$hubTenantId,
	[Parameter(Mandatory = $true)][string]$hubVnetName,
	[Parameter(Mandatory = $true)][string]$hubResourceGroupName,
	[Parameter(Mandatory = $true)][string]$hubSubscriptionId
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Retrieve the VNet ID
$hubVnetId = az network vnet show --resource-group $hubResourceGroupName --name $hubVnetName --query id --output tsv

# Set the environment values
$values = @(
	"FOUNDATIONALLM_HUB_RESOURCE_GROUP=$hubResourceGroupName",
	"FOUNDATIONALLM_HUB_SUBSCRIPTION_ID=$hubSubscriptionId",
	"FOUNDATIONALLM_HUB_TENANT_ID=$hubTenantId",
	"FOUNDATIONALLM_HUB_VNET_ID=$hubVnetId",
	"FOUNDATIONALLM_HUB_VNET_NAME=$hubVnetName"
)

# Show azd environments
Write-Host -ForegroundColor Blue "Your azd environments are listed. Environment values updated for default environment file located ./deploy/standard/.azure/[environment_name]/.env file."
azd env list

# Write AZD environment values
Write-Host -ForegroundColor Yellow "Setting azd environment values for the VNET Peering to your Hub Network: $hubVNetName."
foreach ($value in $values) {
	$key, $val = $value -split '=', 2
	Write-Host -ForegroundColor Yellow  "Setting $key to $val"
	azd env set $key $val
}
Write-Host -ForegroundColor Blue "Environment values updated for default environment file located ./deploy/standard/.azure/[environment_name]/.env"
Write-Host -ForegroundColor Blue "Here are your current environment values:"
azd env get-values
