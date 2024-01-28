#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][object]$resourceGroups,
    [parameter(Mandatory = $true)][string]$resourceSuffix,
    [parameter(Mandatory = $true)][object]$domains
)

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

$svcResourceSuffix = "$project-$environment-$location-svc"

$services = @{
    agentfactoryapi            = @{ miName = "mi-agent-factory-api-$svcResourceSuffix"          
                                    miConfigName = "agentFactoryApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    agenthubapi                = @{ miName = "mi-agent-hub-api-$svcResourceSuffix"              
                                    miConfigName = "agentHubApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    chatui                     = @{ miName = "mi-chat-ui-$svcResourceSuffix"                    
                                    miConfigName = "chatUiMiClientId"
                                    ingressEnabled = $true
                                    hostname = "www.internal.foundationallm.ai"
                                                                                                }
    coreapi                    = @{ miName = "mi-core-api-$svcResourceSuffix"                   
                                    miConfigName = "coreApiMiClientId"
                                    ingressEnabled = $true
                                    hostname = "api.internal.foundationallm.ai"
                                                                                                }
    corejob                    = @{ miName = "mi-core-job-$svcResourceSuffix"                   
                                    miConfigName = "coreJobMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    datasourcehubapi           = @{ miName = "mi-data-source-hub-api-$svcResourceSuffix"        
                                    miConfigName = "dataSourceHubApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    gatekeeperapi              = @{ miName = "mi-gatekeeper-api-$svcResourceSuffix"             
                                    miConfigName = "gatekeeperApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    gatekeeperintegrationapi   = @{ miName = "mi-gatekeeper-integration-api-$svcResourceSuffix" 
                                    miConfigName = "gatekeeperIntegrationApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    managementapi              = @{ miName = "mi-management-api-$svcResourceSuffix"             
                                    miConfigName = "managementApiMiClientId"
                                    ingressEnabled = $true
                                    hostname = "management-api.internal.foundationallm.ai"
                                                                                                }
    managementui               = @{ miName = "mi-management-ui-$svcResourceSuffix"              
                                    miConfigName = "managementUiMiClientId"
                                    ingressEnabled = $true
                                    hostname = "management.internal.foundationallm.ai"
                                                                                                }
    langchainapi               = @{ miName = "mi-langchain-api-$svcResourceSuffix"              
                                    miConfigName = "langChainApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    prompthubapi               = @{ miName = "mi-prompt-hub-api-$svcResourceSuffix"             
                                    miConfigName = "promptHubApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    semantickernelapi          = @{ miName = "mi-semantic-kernel-api-$svcResourceSuffix"        
                                    miConfigName = "semanticKernelApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    vectorizationapi           = @{ miName = "mi-vectorization-api-$svcResourceSuffix"          
                                    miConfigName = "vectorizationApiMiClientId"
                                    ingressEnabled = $false
                                                                                                }
    vectorizationjob           = @{ miName = "mi-vectorization-job-$svcResourceSuffix"          
                                    miConfigName = "vectorizationJobMiClientId"
                                    ingressEnabled = $false
                                                                                                }
}

### Getting Resources
$tokens = @{}

$appConfigInstances = @(az appconfig show -n "appconfig-$resourceSuffix-ops" -g $($resourceGroups["ops"]) -o json | ConvertFrom-Json)
if ($appConfigInstances.Length -lt 1) {
    Write-Host "Error getting app config" -ForegroundColor Red
    exit 1
}
$appConfig = $appConfigInstances.name
Write-Host "App Config: $appConfig" -ForegroundColor Yellow

$appConfigEndpoint = $(az appconfig show -g $($resourceGroups["ops"]) -n $appConfig --query 'endpoint' -o json | ConvertFrom-Json)
$appConfigConnectionString = $(az appconfig credential list -n $appConfig -g $($resourceGroups["ops"]) --query "[?name=='Primary Read Only'].{connectionString: connectionString}" -o json | ConvertFrom-Json).connectionString

## Getting managed identities
foreach ($service in $services.GetEnumerator()) {
    $mi = $(az identity show -g $($resourceGroups["app"]) -n $($service.Value.miName) -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting $($service.Key) managed identity!"
    $service.Value.miClientId = $mi.clientId
    Write-Host "$($service.Key) MI Client Id: $($service.Value.miClientId)" -ForegroundColor Yellow
}

$tenantId = $(az account show --query homeTenantId --output tsv)

## Getting CosmosDb info
$docdb = $(az cosmosdb list -g $($resourceGroups["storage"]) --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint}" -o json | ConvertFrom-Json)
$docdb = EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
$docdbKey = $(az cosmosdb keys list -g $($resourceGroups["storage"]) -n $docdb.name -o json --query primaryMasterKey | ConvertFrom-Json)
Write-Host "Document Db Account: $($docdb.name)" -ForegroundColor Yellow

# Setting tokens
$tokens.coreApiHostname = "api.internal.foundationallm.ai"
$tokens.contentSafetyEndpointUri = "TODO"
$tokens.openAiEndpointUri = "TODO"
$tokens.chatEntraClientId = "TODO"
$tokens.coreEntraClientId = "TODO"
$tokens.cognitiveSearchEndpointUri = "TODO"

$tokens.cosmosConnectionString = "AccountEndpoint=$($docdb.documentEndpoint);AccountKey=$docdbKey"
$tokens.cosmosEndpoint = $docdb.documentEndpoint
$tokens.cosmosKey = $docdbKey

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

$tokens.tenantId = $tenantId
$tokens.appConfigEndpoint = $appConfigEndpoint
$tokens.appConfigConnectionString = $appConfigConnectionString

## Showing Values that will be used
Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "appconfig.json file will be generated with values:"
Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

Write-Host "Generating appconfig.json file..." -ForegroundColor Yellow
Push-Location $($MyInvocation.InvocationName | Split-Path)
$template = "..,config,appconfig.template.json"
$outputFile = "..,config,appconfig.json"
$templatePath = $(./Join-Path-Recursively -pathParts $template.Split(","))
$outputFilePath = $(./Join-Path-Recursively -pathParts $outputFile.Split(","))
& ./Token-Replace.ps1 -inputFile $templatePath -outputFile $outputFilePath -tokens $tokens
Pop-Location

