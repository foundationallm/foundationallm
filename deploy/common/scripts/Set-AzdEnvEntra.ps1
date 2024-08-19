#!/usr/bin/pwsh
<#
.SYNOPSIS
    Generates azd environment values to be used with the FLLM deployment. 
    This script should be run after running: 
    ./deploy/common/scripts/Create-FllmEntraApps.ps1 script to create your FLLM EntraID apps.
    ./deploy/common/scripts/Create-FllmAdminGroup.ps1 script to create your FLLM-Admins Group.

.DESCRIPTION
    This script generates a set of azd environment values required for the deployment.
    It retrieves the values of the Application IDs of the EntraID Apps required for the FLLM application and assigns them
    using the azd env command.

    For more information on setting up the FLLM EntraID apps: https://docs.foundationallm.ai/deployment/authentication/index.html
    
.PARAMETER tenantID
   The Azure EntraID tenant ID.
   
.PARAMETER adminGroupName
   The name of the admin group. Default is 'FLLM-Admins'.
   
.PARAMETER authAppName
   The name of the Authorization API app. Default is 'FoundationaLLM-Authorization-API'.
   
.PARAMETER coreAppName
   The name of the Core API app. Default is 'FoundationaLLM-Core-API'.
   
.PARAMETER coreClientAppName
   The name of the Core Portal app. Default is 'FoundationaLLM-Core-Portal'.
   
.PARAMETER mgmtAppName
   The name of the Management API app. Default is 'FoundationaLLM-Management-API'.
   
.PARAMETER mgmtClientAppName
   The name of the Management Portal app. Default is 'FoundationaLLM-Management-Portal'.

.EXAMPLE
QuickStart deployment:
from the ./foundationallm/deploy/quickstart directory:
    ../common/scripts/Set-AzdEnvEntra.ps1 -tenantID "12345678-1234-1234-1234-1234567890ab"

Standard deployment:
from the ./foundationallm/deploy/standard directory:
    ../common/scripts/Set-AzdEnvEntra.ps1 -tenantID "12345678-1234-1234-1234-1234567890ab"

.NOTES
    Set the Azure CLI context to the appropriate subscription before running this script.
    You can set the subscription using the 'az account set --subscription' command.

    This script must be run from the ./deploy/quikstart or the .deploy/standard directory after
    created the azd environment using the azd env create command. The values will be populated in the .env file
    located in the hidden .azure directory in the root of the project.
    
    Example location:
    ./deploy/standard/.azure/[environment_name]/.env
#>

Param(
	[parameter(Mandatory = $true)][string]$tenantID, # Azure EntraID Tenant
	[string]$adminGroupName = 'FLLM-Admins',
	[string]$authAppName = 'FoundationaLLM-Authorization-API',
	[string]$coreAppName = 'FoundationaLLM-Core-API',
	[string]$coreClientAppName = 'FoundationaLLM-Core-Portal',
	[string]$mgmtAppName = 'FoundationaLLM-Management-API',
	[string]$mgmtClientAppName = 'FoundationaLLM-Management-Portal'
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Set the environment values
Write-Host -ForegroundColor Blue "Please wait while gathering azd environment values for the ${tenantID} EntraID Tenant."

$adminGroupId = (az ad group list --filter "displayName eq '$adminGroupName'" --query "[0].id" --output tsv)
$authAppId = (az ad app list --display-name "$authAppName" --query '[].appId' --output tsv) 
$coreAppId = (az ad app list --display-name "$coreAppName" --query '[].appId' --output tsv)
$coreClientAppId = (az ad app list --display-name "$coreClientAppName" --query '[].appId' --output tsv) 
$mgmtAppId = (az ad app list --display-name "$mgmtAppName" --query '[].appId' --output tsv) 
$mgmtClientAppId = (az ad app list --display-name "$mgmtClientAppName" --query '[].appId' --output tsv)

$values = @(
   "ADMIN_GROUP_OBJECT_ID=$adminGroupId"	
   "ENTRA_AUTH_API_CLIENT_ID=$authAppId",
	"ENTRA_AUTH_API_INSTANCE=https://login.microsoftonline.com/",
	"ENTRA_AUTH_API_SCOPES=api://FoundationaLLM-Authorization",
	"ENTRA_CHAT_UI_CLIENT_ID=$coreClientAppId",
	"ENTRA_CHAT_UI_SCOPES=api://FoundationaLLM-Core/Data.Read",
	"ENTRA_CORE_API_CLIENT_ID=$coreAppId",
	"ENTRA_CORE_API_SCOPES=Data.Read",
	"ENTRA_MANAGEMENT_API_CLIENT_ID=$mgmtAppId",
	"ENTRA_MANAGEMENT_API_SCOPES=Data.Manage",
	"ENTRA_MANAGEMENT_UI_CLIENT_ID=$mgmtClientAppId",
	"ENTRA_MANAGEMENT_UI_SCOPES=api://FoundationaLLM-Management/Data.Manage"
)

# Show azd environments
Write-Host -ForegroundColor Blue "Your azd environments are listed. Environment values updated for default environment file located ./deploy/[FLLM-Deployment-Type]/.azure/[environment_name]/.env file."
azd env list

# Write AZD environment values
Write-Host -ForegroundColor Yellow "Setting azd environment values for the ${tenantID} EntraID Tenant."
foreach ($value in $values) {
	$key, $val = $value -split '=', 2
	Write-Host -ForegroundColor Yellow  "Setting $key to $val"
	azd env set $key $val
}
Write-Host -ForegroundColor Green "Environment values updated for default environment file located ./deploy/[FLLM-Deployment-Type]/.azure/[environment_name]/.env"
Write-Host -ForegroundColor Blue "Here are your current environment values:"
azd env get-values