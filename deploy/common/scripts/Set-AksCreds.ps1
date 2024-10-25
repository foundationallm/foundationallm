#! /usr/bin/pwsh
<#
.SYNOPSIS
    Sets the AKS credentials for the specified cluster.

.DESCRIPTION
    This script sets the Azure Kubernetes Service (AKS) credentials for either the front-end or back-end cluster based on the provided parameters.
    It requires the Azure CLI to be installed and authenticated.

.PARAMETER ClusterType
    Specifies whether to set credentials for the 'frontend' or 'backend' cluster. Valid values are 'frontend' and 'backend'.

.PARAMETER SubscriptionName
    The name of the Azure subscription where the AKS clusters are located.

.PARAMETER ResourceGroupName
    The name of the resource group that contains the AKS clusters.

.EXAMPLE
    ./Set-AksCreds.ps1 -ClusterType "frontend" -SubscriptionName "FoundationaLLM" -ResourceGroupName "rg-fllm-eastus-app"
    This example sets the AKS credentials for the front-end cluster.

.EXAMPLE
    ./Set-AksCreds.ps1 -ClusterType "backend" -SubscriptionName "FLLM" -ResourceGroupName "rg-fllm-eastus-app"
    This example sets the AKS credentials for the back-end cluster.

.NOTES
	This script can be used to easily work with multiple AKS clusters in the FLLM environment. Make sure to have the Azure CLI installed and authenticated.
	using 
#>

param (
	[Parameter(Mandatory = $true)]
	[ValidateSet("frontend", "backend")]
	[string]$ClusterType,

	[Parameter(Mandatory = $true)]
	[string]$SubscriptionName,

	[Parameter(Mandatory = $true)]
	[string]$ResourceGroupName
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Function to set the AKS credentials
function Set-AksCreds {
	param (
		[string]$ClusterName,
		[string]$SubscriptionName,
		[string]$ResourceGroupName
	)

	az account set -s "$SubscriptionName"
	az aks get-credentials `
		--name $ClusterName `
		--resource-group $ResourceGroupName `
		--subscription $SubscriptionName `
		--overwrite-existing
}

# Get the list of AKS clusters
$clusters = az aks list --resource-group $ResourceGroupName --subscription $SubscriptionName --query "[?contains(name, '$ClusterType')].{name:name}" -o tsv | Out-String
$clusters = $clusters.Trim() -split "`n"

# Output the list of clusters retrieved
Write-Host -ForegroundColor Blue  "Clusters found: $($clusters -join ', ')"

if ($clusters.Length -eq 0) {
	Write-Error -ForegroundColor Red "No clusters found with type '$ClusterType' in the resource group '$ResourceGroupName'."
	exit
}

# Select the first matching cluster
$ClusterName = $clusters[0]

# Output the selected cluster name & Set the AKS credentials
Write-Host -ForegroundColor Yellow "Selected Cluster: $ClusterName"

Set-AksCreds -ClusterName $ClusterName -SubscriptionName $SubscriptionName -ResourceGroupName $ResourceGroupName 