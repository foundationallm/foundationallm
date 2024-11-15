#! /usr/bin/pwsh
<#
.SYNOPSIS
    Sets environment values for OpenAI resources in Azure.

.DESCRIPTION
    This script retrieves and sets environment values for OpenAI resources in Azure.
    It uses the Azure CLI to manage and set the environment values using the Azure Developer CLI (azd).

.PARAMETER openAiName
    The name of the OpenAI resource.

.PARAMETER openAiResourceGroup
    The name of the resource group containing the OpenAI resource.

.PARAMETER openAiSubscriptionId
    The subscription ID under which the OpenAI resource is managed.

.EXAMPLE
    Standard deployment:
    ./Set-AzdEnvOpenAI.ps1 -openAiName "myOpenAI" -openAiResourceGroup "rg-openai" -openAiSubscriptionId "12345678-1234-1234-1234-1234567890ab"

.NOTES
    This script should be run from the appropriate directory after creating the azd environment using the azd env create command.
    It will populate the .env file located in the hidden .azure directory in the root of the project.
    This script requires the Azure CLI and the Azure Developer CLI (azd) to be installed.
#>
param (
	[Parameter(Mandatory = $true)][string]$openAiName,
	[Parameter(Mandatory = $true)][string]$openAiResourceGroup,
	[Parameter(Mandatory = $true)][string]$openAiSubscriptionId
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Set the environment values
$values = @(
	"OPENAI_NAME=$openAiName",
	"OPENAI_RESOURCE_GROUP=$openAiResourceGroup",
	"OPENAI_SUBSCRIPTION_ID=$openAiSubscriptionId"
)

# Show azd environments
Write-Host -ForegroundColor Blue "Your azd environments are listed. Environment values updated for the default environment file located in the .azure directory."

azd env list

# Write AZD environment values
Write-Host -ForegroundColor Yellow "Setting azd environment values for OpenAI resource: $openAiName."
foreach ($value in $values) {
	$key, $val = $value -split '=', 2
	Write-Host -ForegroundColor Yellow "Setting $key to $val"
	azd env set $key $val
}

Write-Host -ForegroundColor Blue "Environment values updated for the default environment file located in the .azure directory."
Write-Host -ForegroundColor Blue "Here are your current environment values:"
azd env get-values