#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][object]$instanceId,
    [parameter(Mandatory = $true)][object]$entraClientIds,
    [parameter(Mandatory = $true)][object]$resourceGroups,
    [parameter(Mandatory = $true)][string]$resourceSuffix,
    [parameter(Mandatory = $true)][string]$subscriptionId,
    [parameter(Mandatory = $true)][object]$ingress
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function EnsureAndReturnFirstItem($arr, $restype) {
    if (-not $arr -or $arr.Length -ne 1) {
        Write-Host "Fatal: No $restype found (or found more than one)" -ForegroundColor Red
        exit 1
    }

    return $arr[0]
}

function EnsureSuccess($message) {
    if ($LASTEXITCODE -ne 0) {
        Write-Host $message -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

function PopulateTemplate($tokens, $template, $output) {
    Push-Location $($MyInvocation.InvocationName | Split-Path)
    $templatePath = $(../../common/scripts/Join-Path-Recursively -pathParts $template.Split(","))
    $outputFilePath = $(../../common/scripts/Join-Path-Recursively -pathParts $output.Split(","))
    Write-Host "Generating $outputFilePath file..." -ForegroundColor Blue
    & ../../common/scripts/Token-Replace.ps1 -inputFile $templatePath -outputFile $outputFilePath -tokens $tokens
    Pop-Location
}

$svcResourceSuffix = "$project-$environment-$location-svc"

$services = @{
    agentfactoryapi          = @{
        miName         = "mi-agent-factory-api-$svcResourceSuffix"
        miConfigName   = "agentFactoryApiMiClientId"
        ingressEnabled = $false
    }
    agenthubapi              = @{
        miName         = "mi-agent-hub-api-$svcResourceSuffix"
        miConfigName   = "agentHubApiMiClientId"
        ingressEnabled = $false
    }
    chatui                   = @{
        miName         = "mi-chat-ui-$svcResourceSuffix"
        miConfigName   = "chatUiMiClientId"
        ingressEnabled = $true
        hostname       = "www.internal.foundationallm.ai"
    }
    coreapi                  = @{
        miName         = "mi-core-api-$svcResourceSuffix"
        miConfigName   = "coreApiMiClientId"
        ingressEnabled = $true
        hostname       = "api.internal.foundationallm.ai"
    }
    corejob                  = @{
        miName         = "mi-core-job-$svcResourceSuffix"
        miConfigName   = "coreJobMiClientId"
        ingressEnabled = $false
    }
    datasourcehubapi         = @{
        miName         = "mi-data-source-hub-api-$svcResourceSuffix"
        miConfigName   = "dataSourceHubApiMiClientId"
        ingressEnabled = $false
    }
    gatekeeperapi            = @{
        miName         = "mi-gatekeeper-api-$svcResourceSuffix"
        miConfigName   = "gatekeeperApiMiClientId"
        ingressEnabled = $false
    }
    gatekeeperintegrationapi = @{
        miName         = "mi-gatekeeper-integration-api-$svcResourceSuffix"
        miConfigName   = "gatekeeperIntegrationApiMiClientId"
        ingressEnabled = $false
    }
    managementapi            = @{
        miName         = "mi-management-api-$svcResourceSuffix"
        miConfigName   = "managementApiMiClientId"
        ingressEnabled = $true
        hostname       = "management-api.internal.foundationallm.ai"
    }
    managementui             = @{
        miName         = "mi-management-ui-$svcResourceSuffix"
        miConfigName   = "managementUiMiClientId"
        ingressEnabled = $true
        hostname       = "management.internal.foundationallm.ai"
    }
    langchainapi             = @{
        miName         = "mi-langchain-api-$svcResourceSuffix"
        miConfigName   = "langChainApiMiClientId"
        ingressEnabled = $false
    }
    prompthubapi             = @{
        miName         = "mi-prompt-hub-api-$svcResourceSuffix"
        miConfigName   = "promptHubApiMiClientId"
        ingressEnabled = $false
    }
    semantickernelapi        = @{
        miName         = "mi-semantic-kernel-api-$svcResourceSuffix"
        miConfigName   = "semanticKernelApiMiClientId"
        ingressEnabled = $false
    }
    vectorizationapi         = @{
        miName         = "mi-vectorization-api-$svcResourceSuffix"
        miConfigName   = "vectorizationApiMiClientId"
        ingressEnabled = $false
    }
    vectorizationjob         = @{
        miName         = "mi-vectorization-job-$svcResourceSuffix"
        miConfigName   = "vectorizationJobMiClientId"
        ingressEnabled = $false
    }
}

### Getting Resources
$tokens = @{}

$tokens.subscriptionId = $subscriptionId
$tokens.storageResourceGroup = $resourceGroups.storage
$tokens.opsResourceGroup = $resourceGroups.ops

$tenantId = $(az account show --query homeTenantId --output tsv)

# Getting Vectorization Config
$vectorizationConfig = $(
    Get-Content -Raw -Path "../config/vectorization.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')

$appConfigInstances = @(
    az appconfig show `
        --name "appconfig-$resourceSuffix-ops" `
        --resource-group $($resourceGroups.ops) `
        --output json | `
        ConvertFrom-Json
)
if ($appConfigInstances.Length -lt 1) {
    Write-Host "$($PSCommandPath): Error getting app config" -ForegroundColor Red
    exit 1
}

$appConfig = $appConfigInstances.name
Write-Host "App Config: $appConfig" -ForegroundColor Blue

$appConfigProperties = $(
    az appconfig show `
        --resource-group $($resourceGroups.ops) `
        --name $appConfig `
        --query '{endpoint:endpoint, privateEndpointId:privateEndpointConnections[0].privateEndpoint.id}' `
        --output json | `
        ConvertFrom-Json
)

$appConfigName = $appConfigProperties.name
$appConfigEndpoint = $appConfigProperties.endpoint
$appConfigConnectionString = $(
    az appconfig credential list `
        --name $appConfig `
        --resource-group $($resourceGroups.ops) `
        --query "[?name=='Primary Read Only'].{connectionString: connectionString}" `
        --output json | `
        ConvertFrom-Json
).connectionString

## Getting CosmosDb info
$docdb = $(
    az cosmosdb list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint, privateEndpointId:privateEndpointConnections[0].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$docdb = EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
Write-Host "Document Db Account: $($docdb.name)" -ForegroundColor Blue

## Getting Content Safety endpoint
$contentSafety = $(
    az cognitiveservices account list `
        --resource-group $($resourceGroups.oai) `
        --query "[?kind=='ContentSafety'].{name:name, uri: properties.endpoint, privateEndpointId:properties.privateEndpointConnections[0].properties.privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$contentSafety = EnsureAndReturnFirstItem $contentSafety "Content Safety"
Write-Host "Content Safety Account: $($contentSafety.name)" -ForegroundColor Blue

## Getting OpenAI endpoint
$apim = $(
    az apim list `
        --resource-group $($resourceGroups.oai) `
        --query "[].{name:name, uri: gatewayUrl, privateIPAddress:privateIpAddresses[0], fqdn:hostnameConfigurations[0].hostName}" `
        --output json | `
        ConvertFrom-Json
)
$apim = EnsureAndReturnFirstItem $apim "OpenAI Endpoint (APIM)"
Write-Host "OpenAI Frontend Endpoint: $($apim.name)" -ForegroundColor Blue

## Getting Cognitive search endpoint
$cogSearch = $(
    az search service list `
        --resource-group $resourceGroups.vec `
        --query "[].{name: name, privateEndpointId:privateEndpointConnections[0].properties.privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$cogSearch = EnsureAndReturnFirstItem $cogSearch "Cognitive Search"
Write-Host "Cognitive Search Service: $($cogSearch.name)" -ForegroundColor Blue
$cogSearchUri = "https://$($cogSearch.name).search.windows.net"
$tokens.cognitiveSearchEndpointUri = $cogSearchUri

Write-Host "Getting ADLS Storage Account"
$storageAccountAdls = $(
    az storage account list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='StorageV2'].{name:name, privateEndpointIds:privateEndpointConnections[].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)

$storageAccountAdls = EnsureAndReturnFirstItem $storageAccountAdls "Storage Account"

## Getting managed identities
foreach ($service in $services.GetEnumerator()) {
    $mi = $(
        az identity show `
            --resource-group $($resourceGroups.app) `
            --name $($service.Value.miName) `
            --output json | `
            ConvertFrom-Json
    )

    EnsureSuccess "Error getting $($service.Key) managed identity!"
    $service.Value.miClientId = $mi.clientId

    Write-Host "$($service.Key) MI Client Id: $($service.Value.miClientId)" -ForegroundColor Yellow
}

# Setting tokens
$tokens.instanceId = $instanceId

$tokens.chatEntraClientId = $entraClientIds.chat
$tokens.coreEntraClientId = $entraClientIds.core
$tokens.managementApiEntraClientId = $entraClientIds.managementapi
$tokens.managementEntraClientId = $entraClientIds.managementUi
$tokens.vectorizationApiEntraClientId = $entraClientIds.vectorizationapi

$tokens.coreApiHostname = $ingress.apiIngress.coreapi.host
$tokens.managementApiHostname = $ingress.apiIngress.managementapi.host
$tokens.vectorizationApiHostname = $ingress.apiIngress.vectorizationapi.host

$tokens.contentSafetyEndpointUri = $contentSafety.uri

$tokens.openAiEndpointUri = $apim.uri


$tokens.cosmosEndpoint = $docdb.documentEndpoint
$tokens.storageAccountAdlsName = $storageAccountAdls.name

$tokens.agentFactoryApiMiClientId = $services["agentfactoryapi"].miClientId
$tokens.agentHubApiMiClientId = $services["agenthubapi"].miClientId
$tokens.chatUiMiClientId = $services["chatui"].miClientId
$tokens.coreApiMiClientId = $services["coreapi"].miClientId
$tokens.coreJobMiClientId = $services["corejob"].miClientId
$tokens.dataSourceHubApiMiClientId = $services["datasourcehubapi"].miClientId
$tokens.gatekeeperApiMiClientId = $services["gatekeeperapi"].miClientId
$tokens.gatekeeperIntegrationApiMiClientId = $services["gatekeeperintegrationapi"].miClientId
$tokens.langChainApiMiClientId = $services["langchainapi"].miClientId
$tokens.managementApiMiClientId = $services["managementapi"].miClientId
$tokens.managementUiMiClientId = $services["managementui"].miClientId
$tokens.promptHubApiMiClientId = $services["prompthubapi"].miClientId
$tokens.semanticKernelApiMiClientId = $services["semantickernelapi"].miClientId
$tokens.vectorizationApiMiClientId = $services["vectorizationapi"].miClientId
$tokens.vectorizationJobMiClientId = $services["vectorizationjob"].miClientId
$tokens.vectorizationConfig = $vectorizationConfig

$tokens.tenantId = $tenantId
$tokens.appConfigName = $appConfigName
$tokens.appConfigEndpoint = $appConfigEndpoint
$tokens.appConfigConnectionString = $appConfigConnectionString

## Showing Values that will be used
Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "appconfig.json file will be generated with values:"
Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

PopulateTemplate $tokens "..,config,agent-factory-api-event-profile.template.json" "..,config,agent-factory-api-event-profile.json"
$tokens.agentFactoryApiEventGridProfile = $(
    Get-Content -Raw -Path "../config/agent-factory-api-event-profile.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')

PopulateTemplate $tokens "..,config,core-api-event-profile.template.json" "..,config,core-api-event-profile.json"
$tokens.coreApiEventGridProfile = $(
    Get-Content -Raw -Path "../config/core-api-event-profile.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')

$tokens.managementApiEventGridProfile = $(
    Get-Content -Raw -Path "../config/management-api-event-profile.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')

PopulateTemplate $tokens "..,config,vectorization-api-event-profile.template.json" "..,config,vectorization-api-event-profile.json"
$tokens.vectorizationApiEventGridProfile = $(
    Get-Content -Raw -Path "../config/vectorization-api-event-profile.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')

PopulateTemplate $tokens "..,config,vectorization-worker-event-profile.template.json" "..,config,vectorization-worker-event-profile.json"
$tokens.vectorizationWorkerEventGridProfile = $(
    Get-Content -Raw -Path "../config/vectorization-worker-event-profile.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
).Replace('"', '\"')


PopulateTemplate $tokens "..,config,appconfig.template.json" "..,config,appconfig.json"
PopulateTemplate $tokens "..,values,internal-service.template.yml" "..,values,microservice-values.yml"

$($ingress.apiIngress).PSObject.Properties | ForEach-Object {
    $tokens.serviceHostname = $_.Value.host
    $tokens.servicePath = $_.Value.path
    $tokens.servicePathType = $_.Value.pathType
    $tokens.serviceAgwSslCert = $_.Value.sslCert
    PopulateTemplate $tokens "..,values,exposed-service.template.yml" "..,values,$($_.Name)-values.yml"
}

$($ingress.frontendIngress).PSObject.Properties | ForEach-Object {
    $tokens.serviceHostname = $_.Value.host
    $tokens.servicePath = $_.Value.path
    $tokens.servicePathType = $_.Value.pathType
    $tokens.serviceAgwSslCert = $_.Value.sslCert
    PopulateTemplate $tokens "..,values,frontend-service.template.yml" "..,values,$($_.Name)-values.yml"
}

exit 0