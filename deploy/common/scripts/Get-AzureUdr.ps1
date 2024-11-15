#! /usr/bin/pwsh
<#
.SYNOPSIS
    Lists all routes in an Azure Route Table using Azure CLI.

.DESCRIPTION
    This script connects to your Azure account using the Azure CLI and retrieves all the routes
    within a specified route table in a given resource group, along with the VNets and Subnets associated with it.

.PARAMETER resourceGroupName
    The name of the resource group containing the route table.

.PARAMETER routeTableName
    The name of the route table to list all routes.

.EXAMPLE
    ./Get-AzureUdr.ps1 -resourceGroupName "MyResourceGroup" -routeTableName "MyRouteTable"
    This example retrieves and lists all the routes within the specified route table in the given resource group.

.NOTES
    Set the Azure CLI context to the appropriate subscription before running this script.
    You can set the subscription using the 'az account set --subscription' command.
    
    This script can be used to gain insights into the network configuration used to connect to an FLLM
    Standard deployment using private networking.
#>

param (
    [Parameter(Mandatory = $true)]
    [string]$resourceGroupName,
        
    [Parameter(Mandatory = $true)]
    [string]$routeTableName
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Continue"

# Retrieve all routes in the route table
Write-Host -ForegroundColor Yellow "Retrieving all routes in Route Table '$routeTableName' within Resource Group '$resourceGroupName'..."
$command = "az network route-table route list --resource-group $resourceGroupName --route-table-name $routeTableName --output json"
$routes = Invoke-Expression $command

# Display the routes
if ($routes) {
    Write-Host -ForegroundColor BLue "Routes in Route Table '$routeTableName':"
    $routes | ConvertFrom-Json | Format-Table
    $routes
}
else {
    Write-Host -ForegroundColor Red "No routes found in Route Table '$routeTableName' within Resource Group '$resourceGroupName'."
}

# Retrieve VNets and Subnets associated with the route table
Write-Host -ForegroundColor Yellow "Retrieving VNets and Subnets associated with Route Table '$routeTableName' within Resource Group '$resourceGroupName'..."
$vnetCommand = "az network vnet list --resource-group $resourceGroupName --output json"
$vnets = Invoke-Expression $vnetCommand | ConvertFrom-Json

$associatedSubnets = @()

foreach ($vnet in $vnets) {
    foreach ($subnet in $vnet.subnets) {
        if ($subnet.routeTable.id -match $routeTableName) {
            $associatedSubnets += [PSCustomObject]@{
                VNetName      = $vnet.name
                SubnetName    = $subnet.name
                AddressPrefix = $subnet.addressPrefix
            }
        }
    }
}

# Display the associated VNets and Subnets
if ($associatedSubnets.Count -gt 0) {
    Write-Host -ForegroundColor Blue "VNets and Subnets associated with Route Table '$routeTableName':"
    $associatedSubnets | Format-Table
}
else {
    Write-Host -ForegroundColor Red "No VNets or Subnets found associated with Route Table '$routeTableName' within Resource Group '$resourceGroupName'."
}