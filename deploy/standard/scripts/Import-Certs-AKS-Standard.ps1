#! /usr/bin/pwsh

<#
.SYNOPSIS
	Imports SSL certificates into Azure Application Gateways.

.DESCRIPTION
	This script imports SSL certificates into Azure Application Gateways. It takes the following parameters:
	- appGatewayNameApi: The name of the API Application Gateway.
	- appGatewayNameWww: The name of the WWW Application Gateway.
	- keyVaultName: The name of the Key Vault where the certificates are stored.
	- resourceGroupAg: The resource group of the Application Gateways.

.PARAMETER appGatewayNameApi
	The name of the API Application Gateway.

.PARAMETER appGatewayNameWww
	The name of the WWW Application Gateway.

.PARAMETER keyVaultName
	The name of the Key Vault where the certificates are stored.

.PARAMETER resourceGroupAg
	The resource group of the Application Gateways.

.EXAMPLE
	Import-Certs-AKS-Standard.ps1 -appGatewayNameApi "apiGateway" -appGatewayNameWww "wwwGateway" -keyVaultName "myKeyVault" -resourceGroupAg "myResourceGroup"

.NOTES
	This script requires the Azure PowerShell module to be installed. Make sure you have the necessary permissions to access the Azure resources.
#>

Param(
	# Mandatory parameters
	[parameter(Mandatory = $true)][string]$appGatewayNameApi,
	[parameter(Mandatory = $true)][string]$appGatewayNameWww,
	[parameter(Mandatory = $true)][string]$keyVaultName,
	[parameter(Mandatory = $true)][string]$resourceGroupAg
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Import-Certs {
	foreach ($cert in $certs.GetEnumerator()) {

		Write-Host "Adding SSL Certificate $cert into AppGateway $appGatewayName"

		az network application-gateway ssl-cert create `
			--gateway-name  $appGatewayName `
			--key-vault-secret-id "https://${keyVaultName}.vault.azure.net/secrets/${cert}" `
			--name $cert `
			--resource-group $resourceGroupAg `
			--no-wait

		if ($LASTEXITCODE -ne 0) {
			Write-Error("Failed to create SSL Certificate $cert in AppGateway $appGatewayName")
			exit 1
		}
	}
}

# Import WWW Certificates into API AppGateway
$appGatewayName = $appGatewayNameApi
$certs =
'coreapi',
'managementapi',
'vectorizationapi'

Import-Certs

# Import WWW Certificates into WWW AppGateway
$appGatewayName = $appGatewayNameWww
$certs =
'chatui',
'managementui'

Import-Certs
