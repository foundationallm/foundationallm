#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][string]$resourceGroup
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function validate {
    $valid = $true

    if ([string]::IsNullOrEmpty($resourceGroup)) {
        Write-Host "No resource group. Use -resourceGroup to specify resource group." -ForegroundColor Red
        $valid = $false
    }

    if ($valid -eq $false) {
        exit 1
    }
}

filter timestamp {"$(Get-Date -Format o): $_"}

$message = @"
--------------------------------------------------------
 Restarting images on Aca
 --------------------------------------------------------
"@
Write-Host $message -ForegroundColor Yellow

$deploymentOutputs = $(az deployment group show -g $resourceGroup -n foundationallm-azuredeploy -o json --query properties.outputs | ConvertFrom-Json)

validate

Push-Location $($MyInvocation.InvocationName | Split-Path)

Write-Host "Restarting images..." -ForegroundColor Yellow

Write-Host "AgentFactoryAPI restart - agent-factory-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.agentfactoryAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.agentfactoryAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "AgentHubAPI restart - agent-hub-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.agenthubAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.agenthubAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "CoreAPI restart - core-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.coreAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.coreAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "CoreWorker restart - core-job" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.coreJobAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.coreJobAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "DataSourceHubAPI restart - data-source-hub-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.datasourcehubAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.datasourcehubAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "GatekeeperAPI restart - gatekeeper-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.gatekeeperAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.gatekeeperAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "LangchainAPI restart - langchain-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.langchainAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.langchainAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "PromptHubAPI restart - prompt-hub-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.prompthubAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.prompthubAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "SemanticKernelAPI restart - semantic-kernel-api" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.semantickernelAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.semantickernelAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "ChatUI restart - chat-ui" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.chatuiAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.chatuiAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Write-Host "jaeger restart - jaeger" -ForegroundColor Yellow
$revisions = $(az containerapp revision list -g $resourceGroup -n $($deploymentOutputs.jaegerAcaName.value) -o json | ConvertFrom-Json)
$command = "az containerapp revision restart --revision $($revisions[0].name) --name $($deploymentOutputs.jaegerAcaName.value) --resource-group $resourceGroup"
Invoke-Expression "$command"

Pop-Location

Write-Host "Images restarted." -ForegroundColor Yellow