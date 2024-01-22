#! /usr/bin/pwsh

Param(
	# Mandatory parameters
	[parameter(Mandatory = $true)][string]$resourceGroup,
	[parameter(Mandatory = $true)][string]$resourcePrefix,
	[parameter(Mandatory = $true)][string]$subscription
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$containerApp = @{
	agenthub         = "agent-hub-api"
	agentfactory     = "agent-factory-api"
	core             = "core-api"
	corejob          = "core-job"
	chatui           = "chat-ui"
	datasourcehub    = "data-source-hub-api"
	gatekeeper       = "gatekeeper-api"
	gatekeeperint    = "gatekeeper-integration-api"
	langchain        = "langchain-api"
	prompthub        = "prompt-hub-api"
	management       = "management-api"
	managementui     = "management-ui"
	semantickernel   = "semantic-kernel-api"
	vectorization    = "vectorization-api"
	vectorizationjob = "vectorization-job"
}

Write-Host "Restarting container apps using these perameters:
			Resource group: ${resourceGroup}
			Subscription:   ${subscription} 
			Resource Prefix: ${resourcePrefix}"

foreach ($app in $containerApp.GetEnumerator()) {
	$name = "${resourcePrefix}$($app.Key)ca"
    $revision = (az containerapp show --name $name --resource-group $resourceGroup --subscription $subscription --query "properties.latestRevisionName" -o tsv)
	Write-Host "Restarting container app ${name}"
	az containerapp revision restart `
		--revision $revision `
		--name $name `
		--resource-group $resourceGroup `
		--subscription $subscription

	if ($LASTEXITCODE -ne 0) {
		Write-Error("Failed to restart the container app ${name}")
		exit 1
	}
}