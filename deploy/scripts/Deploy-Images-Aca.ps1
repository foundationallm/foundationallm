#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][string]$name = "ms-openai-cosmos-db",
    [parameter(Mandatory = $false)][string]$resourceGroup,
    [parameter(Mandatory = $false)][string]$acrName,
    [parameter(Mandatory = $false)][string]$acrResourceGroup = $resourceGroup,
    [parameter(Mandatory = $false)][string]$tag = "latest"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function validate {
    $valid = $true

    if ([string]::IsNullOrEmpty($resourceGroup)) {
        Write-Host "No resource group. Use -resourceGroup to specify resource group." -ForegroundColor Red
        $valid = $false
    }

    if ([string]::IsNullOrEmpty($acrLogin)) {
        Write-Host "ACR login server can't be found. Are you using right ACR ($acrName) and RG ($resourceGroup)?" -ForegroundColor Red
        $valid = $false
    }

    if ($valid -eq $false) {
        exit 1
    }
}

filter timestamp {"$(Get-Date -Format o): $_"}

$message = @"
--------------------------------------------------------
 Deploying images on Aca

 Additional parameters are:
 Images tag: $tag
 --------------------------------------------------------
"@
Write-Host $message -ForegroundColor Yellow

if ($acrName -ne "bydtochatgptcr") {
    $acrLogin = $(az acr show -n $acrName -g $acrResourceGroup -o json | ConvertFrom-Json).loginServer
    Write-Host "acr login server is $acrLogin" -ForegroundColor Yellow
}
else {
    $acrLogin = "bydtochatgptcr.azurecr.io"
}

$deploymentOutputs = $(az deployment group show -g $resourceGroup -n foundationallm-azuredeploy -o json --query properties.outputs | ConvertFrom-Json)

validate

Push-Location $($MyInvocation.InvocationName | Split-Path)

Write-Host "Deploying images..." -ForegroundColor Yellow

Write-Host "AgentFactoryAPI deployment - agent-factory-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.agentfactoryAcaName.value) --resource-group $resourceGroup --image $acrLogin/agent-factory-api:$tag"
Invoke-Expression "$command"

Write-Host "AgentHubAPI deployment - agent-hub-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.agenthubAcaName.value) --resource-group $resourceGroup --image $acrLogin/agent-hub-api:$tag"
Invoke-Expression "$command"

Write-Host "CoreAPI deployment - core-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.coreAcaName.value) --resource-group $resourceGroup --image $acrLogin/core-api:$tag"
Invoke-Expression "$command"

Write-Host "CoreWorker deployment - core-job" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.coreJobAcaName.value) --resource-group $resourceGroup --image $acrLogin/core-job:$tag"
Invoke-Expression "$command"

Write-Host "DataSourceHubAPI deployment - data-source-hub-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.datasourcehubAcaName.value) --resource-group $resourceGroup --image $acrLogin/data-source-hub-api:$tag"
Invoke-Expression "$command"

Write-Host "GatekeeperAPI deployment - gatekeeper-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.gatekeeperAcaName.value) --resource-group $resourceGroup --image $acrLogin/gatekeeper-api:$tag"
Invoke-Expression "$command"

Write-Host "LangchainAPI deployment - langchain-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.langchainAcaName.value) --resource-group $resourceGroup --image $acrLogin/langchain-api:$tag"
Invoke-Expression "$command"

Write-Host "PromptHubAPI deployment - prompt-hub-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.prompthubAcaName.value) --resource-group $resourceGroup --image $acrLogin/prompt-hub-api:$tag"
Invoke-Expression "$command"

Write-Host "SemanticKernelAPI deployment - semantic-kernel-api" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.semantickernelAcaName.value) --resource-group $resourceGroup --image $acrLogin/semantic-kernel-api:$tag"
Invoke-Expression "$command"

Write-Host "ChatUI deployment - chat-ui" -ForegroundColor Yellow
$command = "az containerapp update --name $($deploymentOutputs.chatuiAcaName.value) --resource-group $resourceGroup --image $acrLogin/chat-ui:$tag"
Invoke-Expression "$command"

# $message = @"
# --------------------------------------------------------
#  Entering holding pattern to wait for proper backend API initialization 
#  Attempting to retrieve status from https://$($deploymentOutputs.apiFqdn.value)/status every 20 seconds with 50 retries
#  --------------------------------------------------------
# "@
# Write-Host $message
# $apiStatus = "initializing"
# $retriesLeft = 50
# while (($apiStatus.ToString() -ne "ready") -and ($retriesLeft -gt 0)) {
#     $message = @"
# --------------------------------------------------------
# API Status: $apiStatus
# Waiting for backend API to enter ready state. Retries left: $retriesLeft
# --------------------------------------------------------
# "@
#     Write-Host $message
#     write-host "Sleeping for 20 seconds..." -ForegroundColor Yellow | timestamp

#     Start-Sleep -Seconds 20
#     try {
#         $apiStatus = Invoke-RestMethod -Uri "https://$($deploymentOutputs.apiFqdn.value)/status" -Method GET
#     }
#     catch {
#         Write-Host "The attempt to invoke the API endpoint failed. Will retry."
#     }
#     finally {
#         Write-Host "Last known API endpoint status: $($apiStatus)"
#     }
    
#     $retriesLeft -= 1
# } 

# if ($apiStatus.ToString() -ne "ready") {
#     throw "The backend API did not enter the ready state."
# }

Pop-Location

Write-Host "MS OpenAI Chat deployed to ACA" -ForegroundColor Yellow