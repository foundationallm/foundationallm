#!/usr/bin/env pwsh

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

function envsubst {
    param([Parameter(ValueFromPipeline)][string]$InputObject)

    $ExecutionContext.InvokeCommand.ExpandString($InputObject)
}

Invoke-AndRequireSuccess "Setting Azure Subscription" {
    az account set -s $env:AZURE_SUBSCRIPTION_ID
}

Invoke-AndRequireSuccess "Loading storage-preview extension" {
    az extension add --name storage-preview --allow-preview true --yes 
    az extension update --name storage-preview --allow-preview true
}

$env:VECTORIZATION_WORKER_CONFIG = Get-Content ./config/vectorization.json
cat ./config/agent-factory-api-event-profile.template.json | envsubst > ./config/agent-factory-api-event-profile.json
$env:FOUNDATIONALLM_AGENT_FACTORY_API_EVENT_GRID_PROFILE = Get-Content ./config/agent-factory-api-event-profile.json
cat ./config/core-api-event-profile.template.json | envsubst > ./config/core-api-event-profile.json
$env:FOUNDATIONALLM_CORE_API_EVENT_GRID_PROFILE = Get-Content ./config/core-api-event-profile.json
$env:FOUNDATIONALLM_MANAGEMENT_API_EVENT_GRID_PROFILE = Get-Content ./config/management-api-event-profile.json
cat ./config/vectorization-api-event-profile.template.json | envsubst > ./config/vectorization-api-event-profile.json
$env:FOUNDATIONALLM_VECTORIZATION_API_EVENT_GRID_PROFILE = Get-Content ./config/vectorization-api-event-profile.json
cat ./config/vectorization-worker-event-profile.template.json | envsubst > ./config/vectorization-worker-event-profile.json
$env:FOUNDATIONALLM_VECTORIZATION_WORKER_EVENT_GRID_PROFILE = Get-Content ./config/vectorization-worker-event-profile.json

$appConfigJson = cat ./config/appconfig.template.json
echo "" > ./config/appconfig.json
ForEach ($line in $appConfigJson) {
    envsubst $line >> ./config/appconfig.json
}

Invoke-AndRequireSuccess "Loading AppConfig Values" {
    az appconfig kv import `
        --profile appconfig/kvset `
        --name $env:AZURE_APP_CONFIG_NAME `
        --source file `
        --path ./config/appconfig.json `
        --format json `
        --yes `
        --output none
}

Invoke-AndRequireSuccess "Uploading Agents" {
    az storage azcopy blob upload `
        -c agents `
        --account-name $env:AZURE_STORAGE_ACCOUNT_NAME `
        -s "../common/data/agents/*" `
        --recursive `
        --only-show-errors `
        --auth-mode key `
        --output none
}

Invoke-AndRequireSuccess "Uploading Data Sources" {
    az storage azcopy blob upload `
        -c data-sources `
        --account-name $env:AZURE_STORAGE_ACCOUNT_NAME `
        -s "../common/data/data-sources/*" `
        --recursive `
        --only-show-errors `
        --auth-mode key `
        --output none
}

Invoke-AndRequireSuccess "Uploading Foundationallm Source" {
    az storage azcopy blob upload `
        -c foundationallm-source `
        --account-name $env:AZURE_STORAGE_ACCOUNT_NAME `
        -s "../common/data/foundationallm-source/*" `
        --recursive `
        --only-show-errors `
        --auth-mode key `
        --output none
}

Invoke-AndRequireSuccess "Uploading Prompts" {
    az storage azcopy blob upload `
        -c prompts `
        --account-name $env:AZURE_STORAGE_ACCOUNT_NAME `
        -s "../common/data/prompts/*" `
        --recursive `
        --only-show-errors `
        --auth-mode key `
        --output none
}

Invoke-AndRequireSuccess "Uploading Resource Providers" {
    az storage azcopy blob upload `
        -c resource-provider `
        --account-name $env:AZURE_STORAGE_ACCOUNT_NAME `
        -s "../common/data/resource-provider/*" `
        --recursive `
        --only-show-errors `
        --auth-mode key `
        --output none   
}
