#! /usr/bin/pwsh

Param(
	# Mandatory parameters
	[parameter(Mandatory = $true)][string]$resourceGroup,
	[parameter(Mandatory = $true)][string]$resourcePrefix,
	[parameter(Mandatory = $true)][string]$subscription,

	# Optional parameters
	[parameter(Mandatory = $false)][string]$containerPrefix = "solliancenet/foundationallm",
	[parameter(Mandatory = $false)][string]$containerTag = "latest",
	[parameter(Mandatory = $false)][string]$registry = "ghcr.io"
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

Write-Host "Updating container apps using these perameters:
			Resource group: ${resourceGroup}
			Subscription:   ${subscription} 
			Resource Prefix: ${resourcePrefix} 
			Container Registry: ${registry}
			Container Prefix: ${containerPrefix}
			with the tag of ${containerTag}"

foreach ($app in $containerApp.GetEnumerator()) {
	$name = "${resourcePrefix}$($app.Key)ca"

	Write-Host "Updating container app ${name} with image ${registry}/${containerPrefix}/$($app.Value):${containerTag}"
	az containerapp update `
		--image "${registry}/${containerPrefix}/$($app.Value):${containerTag}" `
		--name $name `
		--resource-group $resourceGroup `
		--subscription $subscription

	if ($LASTEXITCODE -ne 0) {
		Write-Error("Failed to update the container app ${name}")
		exit 1
	}
}