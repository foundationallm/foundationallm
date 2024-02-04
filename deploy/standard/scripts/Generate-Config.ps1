#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][object]$instanceId,
    [parameter(Mandatory = $true)][object]$entraClientIds,
    [parameter(Mandatory = $true)][object]$resourceGroups,
    [parameter(Mandatory = $true)][string]$resourceSuffix,
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

function GetAksPrivateIPMapping($aksInstance, $peInstances, $nicInstances) {
    $peInstance = $peInstances | Where-Object { $_.aksId -eq $aksInstance.aksId }
    $nicInstance = $nicInstances | Where-Object { $_.peId -eq $peInstance.peId }

    return @{
        privateIPAddress = $nicInstance.privateIpAddress
        fqdn             = $aksInstance.privateFqdn
        groupId          = $peInstance.groupId
    }
}

function GetPrivateIPMapping($privateEndpointId) {
    $networkInterface = $(
        az network private-endpoint show `
            --id $privateEndpointId `
            --query '{networkInterfaceId:networkInterfaces[0].id, groupId:privateLinkServiceConnections[0].groupIds[0]}' `
            --output json | `
            ConvertFrom-Json
    )
    EnsureSuccess "Error getting private endpoint network interface id!"

    $privateIpMapping = $(
        az network nic show `
            --ids $networkInterface.networkInterfaceId `
            --query '{privateIPAddress:ipConfigurations[0].privateIPAddress,fqdn:ipConfigurations[0].privateLinkConnectionProperties.fqdns[0]}' `
            --output json | `
            ConvertFrom-Json
    )
    EnsureSuccess "Error getting private endpoint network interface info!"

    return @{
        privateIPAddress = $privateIpMapping.privateIPAddress
        fqdn             = $privateIpMapping.fqdn
        groupId          = $networkInterface.groupId
    }
}

function PopulateTemplate($tokens, $template, $output) {
    Push-Location $($MyInvocation.InvocationName | Split-Path)
    $templatePath = $(../../common/scripts/Join-Path-Recursively -pathParts $template.Split(","))
    $outputFilePath = $(../../common/scripts/Join-Path-Recursively -pathParts $output.Split(","))
    Write-Host "Generating $outputFilePath file..." -ForegroundColor Yellow
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

$tenantId = $(az account show --query homeTenantId --output tsv)

# Getting Vectorization Config
$vectorizationConfig = $(
    Get-Content -Raw -Path "../config/vectorization.json" | `
        ConvertFrom-Json | `
        ConvertTo-Json -Compress
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

$appConfigEndpoint = $appConfigProperties.endpoint
$appConfigConnectionString = $(
    az appconfig credential list `
        --name $appConfig `
        --resource-group $($resourceGroups.ops) `
        --query "[?name=='Primary Read Only'].{connectionString: connectionString}" `
        --output json | `
        ConvertFrom-Json
).connectionString

$appConfigPrivateIpMapping = GetPrivateIPMapping $appConfigProperties.privateEndpointId

## Getting CosmosDb info
$docdb = $(
    az cosmosdb list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint, privateEndpointId:privateEndpointConnections[0].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$docdb = EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
$docdbPrivateIpMapping = GetPrivateIPMapping $docdb.privateEndpointId
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
$contentSafetyPrivateIpMapping = GetPrivateIPMapping $contentSafety.privateEndpointId
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
$cogSearchUri = "http://$($cogSearch.name).search.windows.net"
$cogSearchPrivateIpMapping = GetPrivateIPMapping $cogSearch.privateEndpointId

Write-Host "Getting OpenAI Accounts"
$openAiAccounts = $(
    az cognitiveservices account list `
        --resource-group $($resourceGroups.oai) `
        --query "[?kind=='OpenAI'].{name:name, uri: properties.endpoint, privateEndpointId:properties.privateEndpointConnections[0].properties.privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
Write-Host "Found $($openAiAccounts.Length) OpenAI Accounts" -ForegroundColor Yellow
for ($i = 0; $i -lt $openAiAccounts.Length; $i++) {
    $account = $openAiAccounts[$i]
    $accountPrivateIpMapping = GetPrivateIPMapping $account.privateEndpointId

    Write-Host "OpenAI Account $($i): $($account.name)" -ForegroundColor Blue

    $tokens.Add("openAiAccountFqdn$($i)", $accountPrivateIpMapping.fqdn)
    $tokens.Add("openAiAccountPrivateIp$($i)", $accountPrivateIpMapping.privateIPAddress)
}

Write-Host "Getting OpenAI Key Vault"
$openAiKeyVault = $(
    az keyvault list `
        --resource-group $($resourceGroups.oai) `
        --query "[].{name:name, privateEndpointId:properties.privateEndpointConnections[0].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$openAiKeyVault = EnsureAndReturnFirstItem $openAiKeyVault "OpenAI Key Vault"
Write-Host "OpenAI Key Vault: $($openAiKeyVault.name)" -ForegroundColor Blue
$openAiKeyVaultPrivateIpMapping = GetPrivateIPMapping $openAiKeyVault.privateEndpointId

Write-Host "Getting OPS Key Vault"
$opsKeyVault = $(
    az keyvault list `
        --resource-group $($resourceGroups.ops) `
        --query "[].{name:name, privateEndpointId:properties.privateEndpointConnections[0].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$opsKeyVault = EnsureAndReturnFirstItem $opsKeyVault "OPS Key Vault"
Write-Host "OPS Key Vault: $($opsKeyVault.name)" -ForegroundColor Blue
$opsKeyVaultPrivateIpMapping = GetPrivateIPMapping $opsKeyVault.privateEndpointId

Write-Host "Geting AMPLS"
# az monitor private-link-scope list --resource-group "EBTICP-D-NA24-AIOps-RGRP" --query "[].{name:name, privateEndpointId:privateEndpointConnections[0].privateEndpoint.id}"
$ampls = $(
    az monitor private-link-scope list `
        --resource-group $($resourceGroups.ops) `
        --query "[].{name:name, privateEndpointId:privateEndpointConnections[0].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)
$ampls = EnsureAndReturnFirstItem $ampls "AMPLS"
Write-Host "AMPLS: $($ampls.name)" -ForegroundColor Blue
$amplsPrivateIpMapping = GetPrivateIPMapping $ampls.privateEndpointId

Write-Host "Getting OPS Storage Account"
$storageAccountOps = $(
    az storage account list `
        --resource-group $($resourceGroups.ops) `
        --query "[?kind=='StorageV2'].{name:name, privateEndpointIds:privateEndpointConnections[].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)

$storageAccountOps = EnsureAndReturnFirstItem $storageAccountOps "Storage Account"
Write-Host "Storage Account: $($storageAccountOps.name)" -ForegroundColor Blue
$storageAccountOpsPrivateIpMapping = @{}
foreach ($privateEndpointId in $storageAccountOps.privateEndpointIds) {
    $privateIpMapping = GetPrivateIPMapping $privateEndpointId
    $storageAccountOpsPrivateIpMapping.Add($privateIpMapping.groupId, $privateIpMapping)
}

Write-Host "Getting ADLS Storage Account"
$storageAccountAdls = $(
    az storage account list `
        --resource-group $($resourceGroups.storage) `
        --query "[?kind=='StorageV2'].{name:name, privateEndpointIds:privateEndpointConnections[].privateEndpoint.id}" `
        --output json | `
        ConvertFrom-Json
)

$storageAccountAdls = EnsureAndReturnFirstItem $storageAccountAdls "Storage Account"
Write-Host "Storage Account: $($storageAccountAdls.name)" -ForegroundColor Blue
$storageAccountAdlsPrivateIpMapping = @{}
foreach ($privateEndpointId in $storageAccountAdls.privateEndpointIds) {
    $privateIpMapping = GetPrivateIPMapping $privateEndpointId
    $storageAccountAdlsPrivateIpMapping.Add($privateIpMapping.groupId, $privateIpMapping)
}

Write-Host "Getting AKS Instances, Private Endpoints and NICs"
$aksInstances = $(
    az aks list `
        --resource-group $resourceGroups.app `
        --query "[].{aksName:name,privateFqdn:privateFqdn,aksId:id}" | `
        ConvertFrom-Json
)
$peInstances = $(
    az network private-endpoint list `
        --resource-group $resourceGroups.app `
        --query "[].{groupId:privateLinkServiceConnections[0].groupIds[0],peName:name,aksId:privateLinkServiceConnections[0].privateLinkServiceId,nicId:networkInterfaces[0].id,peId:id}" | `
        ConvertFrom-Json
)

$nicInstances = $(
    az network nic list `
        --resource-group $resourceGroups.app `
        --query "[].{nicId:id, nicName:name, privateIpAddress:ipConfigurations[0].privateIPAddress,peId:privateEndpoint.id}" | `
        ConvertFrom-Json
)

Write-Host "Found $($aksInstances.Length) AKS Instances" -ForegroundColor Yellow
for ($i = 0; $i -lt $aksInstances.Length; $i++) {
    $aksInstance = $aksInstances[$i]
    Write-Host "AKS Instance $($i): $($aksInstance.aksName)" -ForegroundColor Blue
    $privateIpMapping = GetAksPrivateIPMapping $aksInstance $peInstances $nicInstances
    $tokens.Add("aksFqdn$($i)", $aksInstance.privateFqdn)
    $tokens.Add("aksPrivateIp$($i)", $privateIpMapping.privateIPAddress)
}

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
$tokens.contentSafetyFqdn = $contentSafetyPrivateIpMapping.fqdn
$tokens.contentSafetyPrivateIp = $contentSafetyPrivateIpMapping.privateIPAddress

$tokens.openAiEndpointUri = $apim.uri
$tokens.apimFqdn = $apim.fqdn
$tokens.apimPrivateIp = $apim.privateIPAddress

$tokens.cognitiveSearchEndpointUri = $cogSearchUri
$tokens.cognitiveSearchFqdn = $cogSearchPrivateIpMapping.fqdn
$tokens.cognitiveSearchPrivateIp = $cogSearchPrivateIpMapping.privateIPAddress

$tokens.cosmosEndpoint = $docdb.documentEndpoint
$tokens.cosmosFqdn = $docdbPrivateIpMapping.fqdn
$tokens.cosmosPrivateIp = $docdbPrivateIpMapping.privateIPAddress

$tokens.openAiKeyVaultFqdn = $openAiKeyVaultPrivateIpMapping.fqdn
$tokens.openAiKeyVaultPrivateIp = $openAiKeyVaultPrivateIpMapping.privateIPAddress

$tokens.opsKeyVaultFqdn = $opsKeyVaultPrivateIpMapping.fqdn
$tokens.opsKeyVaultPrivateIp = $opsKeyVaultPrivateIpMapping.privateIPAddress

$tokens.amplsFqdn = $amplsPrivateIpMapping.fqdn
$tokens.amplsPrivateIp = $amplsPrivateIpMapping.privateIPAddress

$tokens.storageAccountOpsBlobFqdn = $storageAccountOpsPrivateIpMapping.blob.fqdn
$tokens.storageAccountOpsBlobPrivateIp = $storageAccountOpsPrivateIpMapping.blob.privateIPAddress
$tokens.storageAccountOpsDfsFqdn = $storageAccountOpsPrivateIpMapping.dfs.fqdn
$tokens.storageAccountOpsDfsPrivateIp = $storageAccountOpsPrivateIpMapping.dfs.privateIPAddress
$tokens.storageAccountOpsFileFqdn = $storageAccountOpsPrivateIpMapping.file.fqdn
$tokens.storageAccountOpsFilePrivateIp = $storageAccountOpsPrivateIpMapping.file.privateIPAddress
$tokens.storageAccountOpsQueueFqdn = $storageAccountOpsPrivateIpMapping.queue.fqdn
$tokens.storageAccountOpsQueuePrivateIp = $storageAccountOpsPrivateIpMapping.queue.privateIPAddress
$tokens.storageAccountOpsTableFqdn = $storageAccountOpsPrivateIpMapping.table.fqdn
$tokens.storageAccountOpsTablePrivateIp = $storageAccountOpsPrivateIpMapping.table.privateIPAddress

$tokens.storageAccountAdlsBlobFqdn = $storageAccountAdlsPrivateIpMapping.blob.fqdn
$tokens.storageAccountAdlsBlobPrivateIp = $storageAccountAdlsPrivateIpMapping.blob.privateIPAddress
$tokens.storageAccountAdlsDfsFqdn = $storageAccountAdlsPrivateIpMapping.dfs.fqdn
$tokens.storageAccountAdlsDfsPrivateIp = $storageAccountAdlsPrivateIpMapping.dfs.privateIPAddress
$tokens.storageAccountAdlsFileFqdn = $storageAccountAdlsPrivateIpMapping.file.fqdn
$tokens.storageAccountAdlsFilePrivateIp = $storageAccountAdlsPrivateIpMapping.file.privateIPAddress
$tokens.storageAccountAdlsQueueFqdn = $storageAccountAdlsPrivateIpMapping.queue.fqdn
$tokens.storageAccountAdlsQueuePrivateIp = $storageAccountAdlsPrivateIpMapping.queue.privateIPAddress
$tokens.storageAccountAdlsTableFqdn = $storageAccountAdlsPrivateIpMapping.table.fqdn
$tokens.storageAccountAdlsTablePrivateIp = $storageAccountAdlsPrivateIpMapping.table.privateIPAddress

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
$tokens.appConfigEndpoint = $appConfigEndpoint
$tokens.appConfigConnectionString = $appConfigConnectionString
$tokens.appConfigFqdn = $appConfigPrivateIpMapping.fqdn
$tokens.appConfigPrivateIp = $appConfigPrivateIpMapping.privateIPAddress

## Showing Values that will be used
Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "appconfig.json file will be generated with values:"
Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

PopulateTemplate $tokens "..,config,appconfig.template.json" "..,config,appconfig.json"
PopulateTemplate $tokens "..,config,hosts.template" "..,config,hosts"
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