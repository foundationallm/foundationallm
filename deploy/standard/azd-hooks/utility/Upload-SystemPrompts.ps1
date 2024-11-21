#!/usr/bin/pwsh

<#
.SYNOPSIS
    Uploads system prompts to Azure storage containers.

.DESCRIPTION
    This script uploads system prompts to Azure storage containers.

.PARAMETER resourceGroup
    Specifies the name of the resource group where the storage account is located. This parameter is mandatory.

.PARAMETER location
    Specifies the location of the storage account. This parameter is mandatory.

.EXAMPLE
    UploadSystemPrompts.ps1 -resourceGroup "myResourceGroup" -location "westus"
#>

Param(
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

$storageAccount = Invoke-AndRequireSuccess "Getting storage account name" {
    az storage account list `
        --resource-group $resourceGroup `
        --query '[0].name' `
        --output tsv
}

$target = "https://$storageAccount.blob.core.windows.net/resource-provider/"
$azcopy = "../../common/tools/azcopy/azcopy" | Get-AbsolutePath
$sourcePath = '../../common/data/resource-provider' | Get-AbsolutePath

Invoke-AndRequireSuccess "Create New OpenAI Assistant" {
    $accountInfo = $(az resource show --ids $env:AZURE_OPENAI_ID --query "{resourceGroup:resourceGroup,name:name}" | ConvertFrom-Json)
    $oaiApiKey = $(az cognitiveservices account keys list --name $accountInfo.name --resource-group $accountInfo.resourceGroup --query "key1" --output tsv)
    $oaiAssistantId = $(curl "https://$($accountInfo.name).openai.azure.com/openai/assistants?api-version=2024-05-01-preview" `
        -H "api-key: $oaiApiKey" `
        -H "Content-Type: application/json" `
        -d '{
            "instructions": "\nYou are an analytic agent named Khalil that helps people find information about FoundationaLLM.\nProvide concise answers that are polite and professional.\n\nContext:\nFoundationaLLM simplifies and streamlines building knowledge management (e.g., question/answer agents) and analytic (e.g., self-service business intelligence) copilots over the data sources present across your enterprise.\n\nFoundationaLLM deploys a secure, comprehensive and highly configurable copilot platform to your Azure cloud environment:\n\n- Simplifies integration with enterprise data sources used by agent for in-context learning (e.g., enabling RAG, CoT, ReAct and inner monologue patterns).\n- Provides defense in depth with fine-grain security controls over data used by agent and pre/post completion filters that guard against attack.\n- Hardened solution attacked by an LLM red team from inception.\n- Scalable solution load balances across multiple LLM endpoints.\n- Extensible to new data sources, new LLM orchestrators and LLMs.\n\nYou can learn more about FoundationaLLM at https://foundationallm.ai\n",
            "name": "FoundationaLLM - FoundationaLLM",
            "tools": [{"type": "code_interpreter"}, {"type": "file_search"}],
            "model": "completions4o"
        }' | ConvertFrom-Json).id

    $agentPath = $('../../common/data/resource-provider/FoundationaLLM.Agent/FoundationaLLM.json' | Get-AbsolutePath)
    (Get-Content $agentPath).Replace('{{oaiAssistantId}}', $oaiAssistantId) | Set-Content $agentPath
}

Write-Host "$azcopy copy `"$($sourcePath)/*`" $target --exclude-pattern .git* --recursive=True --overwrite=True"
& $azcopy copy "$($sourcePath)/*" $target `
    --exclude-pattern .git* --recursive=True --overwrite=True
