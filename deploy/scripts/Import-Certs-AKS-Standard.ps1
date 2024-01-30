#! /usr/bin/pwsh

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
			--key-vault-secret-id "https://${keyVaultName}.vault.azure.net/secrets/${cert})" `
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
'vectormanagementapi'

Import-Certs

# Import WWW Certificates into WWW AppGateway
$appGatewayName = $appGatewayNameWww
$certs = 
'chatui',
'managementui'

Import-Certs


