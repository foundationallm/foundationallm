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

function Format-EnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)][string]$template,
        [Parameter(Mandatory = $true)][string]$render
    )

    $content = Get-Content $template
    $result = @()
    foreach ($line in $content) {
        $result += $line | envsubst
    }

    $result | Out-File $render -Force
}

Invoke-AndRequireSuccess "Setting Azure Subscription" {
    az account set -s $env:AZURE_SUBSCRIPTION_ID
}

Invoke-AndRequireSuccess "Loading storage-preview extension" {
    az extension add --name storage-preview --allow-preview true --yes
    az extension update --name storage-preview --allow-preview true
}

$env:DEPLOY_TIME = $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffffffZ'))
$env:GUID01 = $($(New-Guid).Guid)
$env:GUID02 = $($(New-Guid).Guid)
$env:GUID03 = $($(New-Guid).Guid)
$env:GUID04 = $($(New-Guid).Guid)
$env:GUID05 = $($(New-Guid).Guid)
$env:GUID06 = $($(New-Guid).Guid)

$env:FOUNDATIONALLM_MANAGEMENT_API_EVENT_GRID_PROFILE = Get-Content ./config/management-api-event-profile.json
$env:VECTORIZATION_WORKER_CONFIG = Get-Content ./config/vectorization.json

$envConfiguraitons = @{
    "agent-factory-api-event-profile"    = @{
        template     = './config/agent-factory-api-event-profile.template.json'
        render       = './config/agent-factory-api-event-profile.json'
        variableName = 'FOUNDATIONALLM_AGENT_FACTORY_API_EVENT_GRID_PROFILE'
    }
    "core-api-event-profile"             = @{
        template     = './config/core-api-event-profile.template.json'
        render       = './config/core-api-event-profile.json'
        variableName = 'FOUNDATIONALLM_CORE_API_EVENT_GRID_PROFILE'
    }
    "vectorization-api-event-profile"    = @{
        template     = './config/vectorization-api-event-profile.template.json'
        render       = './config/vectorization-api-event-profile.json'
        variableName = 'FOUNDATIONALLM_VECTORIZATION_API_EVENT_GRID_PROFILE'
    }
    "vectorization-worker-event-profile" = @{
        template     = './config/vectorization-worker-event-profile.template.json'
        render       = './config/vectorization-worker-event-profile.json'
        variableName = 'FOUNDATIONALLM_VECTORIZATION_WORKER_EVENT_GRID_PROFILE'
    }
}

foreach ($envConfiguraiton in $envConfiguraitons.GetEnumerator()) {
    Write-Host "Formatting $($envConfiguraiton.Key) environment variables" -ForegroundColor Blue
    $template = Resolve-Path $envConfiguraiton.Value.template
    $render = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($envConfiguraiton.Value.render)
    Format-EnvironmentVariables -template $template -render $render

    $name = $envConfiguraiton.Value.variableName
    $value = Get-Content $render
    Set-Content env:\$name $value
}

$configurations = @{
    "fllm-agent"       = @{
        template = './data/resource-provider/FoundationaLLM.Agent/FoundationaLLM.template.json'
        render   = '../common/data/resource-provider/FoundationaLLM.Agent/FoundationaLLM.json'
    }
    "fllm-prompt"      = @{
        template = './data/resource-provider/FoundationaLLM.Prompt/FoundationaLLM.template.json'
        render   = '../common/data/resource-provider/FoundationaLLM.Prompt/FoundationaLLM.json'
    }
    "appconfig"        = @{
        template = './config/appconfig.template.json'
        render   = './config/appconfig.json'
    }
    "role-assignments" = @{
        template = './data/role-assignments/DefaultRoleAssignments.template.json'
        render   = "./data/role-assignments/${env:FOUNDATIONALLM_INSTANCE_ID}.json"
    }
}

foreach ($configuration in $configurations.GetEnumerator()) {
    Write-Host "Formatting $($configuration.Key) environment variables" -ForegroundColor Blue
    $template = Resolve-Path $configuration.Value.template
    $render = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($configuration.Value.render)
    Format-EnvironmentVariables -template $template -render $render
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

if ($IsWindows) {
    $os = "windows"
} elseif ($IsMac) {
    $os = "mac"
}

$AZCOPY_VERSION = "10.24.0"

Push-Location ./tools/azcopy_${os}_amd64_${AZCOPY_VERSION}

Invoke-AndRequireSuccess "Uploading Agents" {
    ./azcopy.exe cp `
        ../../../common/data/agents/* `
        https://$env:AZURE_STORAGE_ACCOUNT_NAME.blob.core.windows.net/agents/ `
        --recursive=True
}

Invoke-AndRequireSuccess "Uploading Data Sources" {
    ./azcopy.exe cp `
        ../../../common/data/data-sources/* `
        https://$env:AZURE_STORAGE_ACCOUNT_NAME.blob.core.windows.net/data-sources/ `
        --recursive=True
}

Invoke-AndRequireSuccess "Uploading Foundationallm Source" {
    ./azcopy.exe cp `
        ../../../common/data/foundationallm-source/* `
        https://$env:AZURE_STORAGE_ACCOUNT_NAME.blob.core.windows.net/foundationallm-source/ `
        --recursive=True
}

Invoke-AndRequireSuccess "Uploading Prompts" {
    ./azcopy.exe cp `
        ../../../common/data/prompts/* `
        https://$env:AZURE_STORAGE_ACCOUNT_NAME.blob.core.windows.net/prompts/ `
        --recursive=True
}

Invoke-AndRequireSuccess "Uploading Resource Providers" {
    ./azcopy.exe cp `
        ../../../common/data/resource-provider/* `
        https://$env:AZURE_STORAGE_ACCOUNT_NAME.blob.core.windows.net/resource-provider/ `
        --exclude-pattern .git* `
        --recursive=True
}

Invoke-AndRequireSuccess "Uploading Default Role Assignments to Authorization Store" {
    ./azcopy.exe cp `
        ../.././data/role-assignments/$($env:FOUNDATIONALLM_INSTANCE_ID).json `
        https://$env:AZURE_AUTHORIZATION_STORAGE_ACCOUNT_NAME.blob.core.windows.net/role-assignments/ `
        --recursive=True
}

Pop-Location