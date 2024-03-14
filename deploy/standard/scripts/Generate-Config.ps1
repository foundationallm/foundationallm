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

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    $LASTEXITCODE = 0
    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

function PopulateTemplate {
    param(
        [parameter(Mandatory = $true, Position = 0)][object]$tokens,
        [parameter(Mandatory = $true, Position = 1)][string]$template,
        [parameter(Mandatory = $true, Position = 2)][string]$output
    )

    $templatePath = $(
        ../../common/scripts/Join-Path-Recursively `
            -pathParts $template.Split(",")
    ) | Resolve-Path
    Write-Host "Template: $templatePath" -ForegroundColor Blue

    $outputFilePath = $(
        ../../common/scripts/Join-Path-Recursively `
            -pathParts $output.Split(",")
    )
    # This works when output file doesn't exist
    $outputFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($outputFilePath)
    Write-Host "Output: $outputFilePath" -ForegroundColor Blue

    ../../common/scripts/Token-Replace.ps1 `
        -inputFile $templatePath `
        -outputFile $outputFilePath `
        -tokens $tokens
}

# function EnsureAndReturnFirstItem($arr, $restype) {
#     if (-not $arr -or $arr.Length -ne 1) {
#         Write-Host "Fatal: No $restype found (or found more than one)" -ForegroundColor Red
#         exit 1
#     }

#     return $arr[0]
# }

# function EnsureSuccess($message) {
#     if ($LASTEXITCODE -ne 0) {
#         Write-Host $message -ForegroundColor Red
#         exit $LASTEXITCODE
#     }
# }



$svcResourceSuffix = "${resourceSuffix}-svc"
$tokens = @{}

$authServices = @{
    authorizationapi              = @{
        miName         = "mi-authorization-api-$svcResourceSuffix"
        miConfigName   = "authorizationApiMiClientId"
        ingressEnabled = $false
    }
}

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

$tokens.deployTime = $((Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffffffZ'))
$tokens.contributorRoleAssignmentGuid = $(New-Guid).Guid
$tokens.userAccessAdminRoleAssignmentGuid = $(New-Guid).Guid
$tokens.subscriptionId = $subscriptionId
$tokens.storageResourceGroup = $resourceGroups.storage
$tokens.opsResourceGroup = $resourceGroups.ops

$tokens.adminGroupObjectId = $adminGroupObjectId

$tokens.chatEntraClientId = $entraClientIds.chat
$tokens.coreApiHostname = $ingress.apiIngress.coreapi.host
$tokens.coreEntraClientId = $entraClientIds.core
$tokens.instanceId = $instanceId
$tokens.managementApiEntraClientId = $entraClientIds.managementapi
$tokens.managementApiHostname = $ingress.apiIngress.managementapi.host
$tokens.managementEntraClientId = $entraClientIds.managementUi
$tokens.opsResourceGroup = $resourceGroups.ops
$tokens.storageResourceGroup = $resourceGroups.storage
$tokens.subscriptionId = $subscriptionId
$tokens.vectorizationApiEntraClientId = $entraClientIds.vectorizationapi
$tokens.vectorizationApiHostname = $ingress.apiIngress.vectorizationapi.host

$tenantId = Invoke-AndRequireSuccess "Get Tenant ID" {
    az account show --query homeTenantId --output tsv
}
$tokens.tenantId = $tenantId

$vectorizationConfig = Invoke-AndRequireSuccess "Get Vectorization Config" {
    $content = Get-Content -Raw -Path "../config/vectorization.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress -Depth 50
    return $content.Replace('"', '\"')
}
$tokens.vectorizationConfig = $vectorizationConfig

$apimUri = Invoke-AndRequireSuccess "Get OpenAI APIM endpoint" {
    az apim list `
        --resource-group $($resourceGroups.oai) `
        --query "[0].gatewayUrl" `
        --output tsv
}
$tokens.openAiEndpointUri = $apimUri

$appConfig = Invoke-AndRequireSuccess "Get AppConfig Instance" {
    az appconfig list `
        --resource-group $($resourceGroups.ops) `
        --query "[0].{name:name,endpoint:endpoint}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.appConfigName = $appConfig.name
$tokens.appConfigEndpoint = $appConfig.endpoint

$appConfigCredential = Invoke-AndRequireSuccess "Get AppConfig Credential" {
    az appconfig credential list `
        --name $appConfig.name `
        --resource-group $($resourceGroups.ops) `
        --query "[?name=='Primary Read Only'].{connectionString: connectionString}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.appConfigConnectionString = $appConfigCredential.connectionString

$cogSearchName = Invoke-AndRequireSuccess "Get Cognitive Search endpoint" {
    az search service list `
        --resource-group $resourceGroups.vec `
        --query "[0].name" `
        --output tsv
}
$tokens.cognitiveSearchEndpointUri = "https://$($cogSearchName).search.windows.net"

$contentSafetyUri = Invoke-AndRequireSuccess "Get Content Safety endpoint" {
    az cognitiveservices account list `
        --resource-group $($resourceGroups.oai) `
        --query "[?kind=='ContentSafety'].properties.endpoint" `
        --output tsv
}
$tokens.contentSafetyEndpointUri = $contentSafetyUri

$docDbEndpoint = Invoke-AndRequireSuccess "Get CosmosDB endpoint" {
    az cosmosdb list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='GlobalDocumentDB'].documentEndpoint" `
        --output tsv
}
$tokens.cosmosEndpoint = $docDbEndpoint

$eventGridNamespace = Invoke-AndRequireSuccess "Get Event Grid Namespace" {
    az eventgrid namespace list `
        --resource-group $($resourceGroups.app) `
        --query "[0].{hostname:topicsConfiguration.hostname, id:id}" `
        --output json | `
        ConvertFrom-Json
}
$tokens.eventGridNamespaceEndpoint = "https://$($eventGridNamespace.hostname)/"
$tokens.eventGridNamespaceId = $eventGridNamespace.id

$keyvaultUri = Invoke-AndRequireSuccess "Get Key Vault URI" {
    az keyvault list `
        --resource-group $($resourceGroups.ops) `
        --query "[0].properties.vaultUri" `
        --output tsv
}
$tokens.keyvaultUri = $keyvaultUri

$storageAccountAdlsName = Invoke-AndRequireSuccess "Get ADLS Storage Account" {
    az storage account list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='StorageV2'].name" `
        --output tsv
}
$tokens.storageAccountAdlsName = $storageAccountAdlsName

foreach ($service in $services.GetEnumerator()) {
    $miClientId = Invoke-AndRequireSuccess "Get $($service.Key) managed identity" {
        az identity show `
            --resource-group $($resourceGroups.app) `
            --name $($service.Value.miName) `
            --query "clientId" `
            --output tsv
    }

    $service.Value.miClientId = $miClientId
}

foreach ($service in $authServices.GetEnumerator()) {
    $miClientId = Invoke-AndRequireSuccess "Get $($service.Key) managed identity" {
        az identity show `
            --resource-group $($resourceGroups.auth) `
            --name $($service.Value.miName) `
            --query "clientId" `
            --output tsv
    }

    $service.Value.miClientId = $miClientId
}

$tokens.agentFactoryApiMiClientId = $services["agentfactoryapi"].miClientId
$tokens.agentHubApiMiClientId = $services["agenthubapi"].miClientId
$tokens.authorizationApiMiClientId = $authServices["authorizationapi"].miClientId
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

$eventGridProfiles = @{}
$eventGridProfileNames = @(
    "agent-factory-api-event-profile"
    "core-api-event-profile"
    "vectorization-api-event-profile"
    "vectorization-worker-event-profile"
    "management-api-event-profile"
)
foreach ($profileName in $eventGridProfileNames) {
    Write-Host "Populating $profileName..." -ForegroundColor Blue

    PopulateTemplate $tokens `
        "..,config,$($profileName).template.json" `
        "..,config,$($profileName).json"

    $eventGridProfiles[$profileName] = $(
        Get-Content -Raw -Path "../config/$($profileName).json" | `
            ConvertFrom-Json | `
            ConvertTo-Json -Compress -Depth 50
    ).Replace('"', '\"')
}

$tokens.agentFactoryApiEventGridProfile = $eventGridProfiles["agent-factory-api-event-profile"]
$tokens.coreApiEventGridProfile = $eventGridProfiles["core-api-event-profile"]
$tokens.vectorizationApiEventGridProfile = $eventGridProfiles["vectorization-api-event-profile"]
$tokens.vectorizationWorkerEventGridProfile = $eventGridProfiles["vectorization-worker-event-profile"]
$tokens.managementApiEventGridProfile = $eventGridProfiles["management-api-event-profile"]

PopulateTemplate $tokens "..,config,appconfig.template.json" "..,config,appconfig.json"
PopulateTemplate $tokens "..,values,internal-service.template.yml" "..,values,microservice-values.yml"
PopulateTemplate $tokens "..,data,resource-provider,FoundationaLLM.Agent,FoundationaLLM.template.json" "..,..,common,data,resource-provider,FoundationaLLM.Agent,FoundationaLLM.json"
PopulateTemplate $tokens "..,data,resource-provider,FoundationaLLM.Prompt,FoundationaLLM.template.json" "..,..,common,data,resource-provider,FoundationaLLM.Prompt,FoundationaLLM.json"

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

PopulateTemplate $tokens "..,data,role-assignments,DefaultRoleAssignments.template.json" "..,data,role-assignments,DefaultRoleAssignments.json"

exit 0