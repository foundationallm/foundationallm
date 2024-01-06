#! /usr/bin/pwsh

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $false)][string]$openAiName,
    [parameter(Mandatory = $false)][string]$openAiRg,
    # [parameter(Mandatory = $false)][string]$openAiDeployment,
    [parameter(Mandatory = $false)][string[]]$outputFile = $null,
    [parameter(Mandatory = $false)][string[]]$gvaluesTemplate = "..,gvalues.template.yml",
    [parameter(Mandatory = $false)][string]$ingressClass = "addon-http-application-routing",
    [parameter(Mandatory = $false)][string]$domain,
    [parameter(Mandatory = $true)][bool]$deployAks
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

# Check the rg
$rg = $(az group show -n $resourceGroup -o json | ConvertFrom-Json)

if (-not $rg) {
    Write-Host "Fatal: Resource group not found" -ForegroundColor Red
    exit 1
}

### Getting Resources
$tokens = @{}

## Getting storage info
# $storage=$(az storage account list -g $resourceGroup --query "[].{name: name, blob: primaryEndpoints.blob}" -o json | ConvertFrom-Json)
# $storage=EnsureAndReturnFirstItem $storage "Storage Account"
# Write-Host "Storage Account: $($storage.name)" -ForegroundColor Yellow

$resourcePrefix = $(az deployment group show -n foundationallm-azuredeploy -g $resourceGroup --query "properties.outputs.resourcePrefix.value" -o json | ConvertFrom-Json)
Write-Host "Resource Prefix: $resourcePrefix" -ForegroundColor Yellow

## Getting API URL domain
if ($deployAks) {
    Write-Host "Getting AKS info" -ForegroundColor Yellow
    if ([String]::IsNullOrEmpty($domain)) {
        $domain = $(az aks show --name $aksName -g $resourceGroup -o json --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName | ConvertFrom-Json)
        if (-not $domain) {
            $domain = $(az aks show -n $aksName -g $resourceGroup -o json --query addonProfiles.httpapplicationrouting.config.HTTPApplicationRoutingZoneName | ConvertFrom-Json)
        }
    }
    
    $apiUrl = "https://$domain"
    Write-Host "API URL: $apiUrl" -ForegroundColor Yellow
    $tokens.apiUrl = $apiUrl
}

$appConfigInstances = @(az appconfig show -n "$($resourcePrefix)-appconfig" -g $resourceGroup -o json | ConvertFrom-Json)
if ($appConfigInstances.Length -lt 1) {
    Write-Host "Error getting app config" -ForegroundColor Red
    exit 1
}
$appConfig = $appConfigInstances.name
Write-Host "App Config: $appConfig" -ForegroundColor Yellow

$appConfigEndpoint = $(az appconfig show -g $resourceGroup -n $appConfig --query 'endpoint' -o json | ConvertFrom-Json)
$appConfigConnectionString = $(az appconfig credential list -n $appConfig -g $resourceGroup --query "[?name=='Primary Read Only'].{connectionString: connectionString}" -o json | ConvertFrom-Json).connectionString

## Getting CosmosDb info
$docdb = $(az cosmosdb list -g $resourceGroup --query "[?kind=='GlobalDocumentDB'].{name: name, kind:kind, documentEndpoint:documentEndpoint}" -o json | ConvertFrom-Json)
$docdb = EnsureAndReturnFirstItem $docdb "CosmosDB (Document Db)"
$docdbKey = $(az cosmosdb keys list -g $resourceGroup -n $docdb.name -o json --query primaryMasterKey | ConvertFrom-Json)
Write-Host "Document Db Account: $($docdb.name)" -ForegroundColor Yellow

if ($deployAks) {
    $agentFactoryApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-agent-factory-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting agent factory mi"
    $agentFactoryApiMiClientId = $agentFactoryApiMi.clientId
    Write-Host "Agent Factory MI Client Id: $agentFactoryApiMiClientId" -ForegroundColor Yellow

    $agentHubApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-agent-hub-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting agent hub mi"
    $agentHubApiMiClientId = $agentHubApiMi.clientId
    Write-Host "Agent Hub MI Client Id: $agentHubApiMiClientId" -ForegroundColor Yellow 

    $chatUiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-chat-ui-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting chat ui mi"
    $chatUiMiClientId = $chatUiMi.clientId
    Write-Host "Chat UI MI Client Id: $chatUiMiClientId" -ForegroundColor Yellow

    $coreApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-core-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting core mi"
    $coreApiMiClientId = $coreApiMi.clientId
    Write-Host "Core MI Client Id: $coreApiMiClientId" -ForegroundColor Yellow

    $coreJobMi = $(az identity show -g $resourceGroup -n $resourcePrefix-core-job-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting core job mi"
    $coreJobMiClientId = $coreJobMi.clientId
    Write-Host "Core Job MI Client Id: $coreJobMiClientId" -ForegroundColor Yellow

    $dataSourceHubApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-data-source-hub-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting data source hub mi"
    $dataSourceHubApiMiClientId = $dataSourceHubApiMi.clientId
    Write-Host "Data Source Hub MI Client Id: $dataSourceHubApiMiClientId" -ForegroundColor Yellow

    $gatekeeperApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-gatekeeper-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting gatekeeper mi"
    $gatekeeperApiMiClientId = $gatekeeperApiMi.clientId
    Write-Host "Gatekeeper MI Client Id: $gatekeeperApiMiClientId" -ForegroundColor Yellow

    $gatekeeperIntegrationApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-gatekeeper-int-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting gatekeeper integration mi"
    $gatekeeperIntegrationApiMiClientId = $gatekeeperIntegrationApiMi.clientId
    Write-Host "Gatekeeper Integration MI Client Id: $gatekeeperIntegrationApiMiClientId" -ForegroundColor Yellow

    $langChainApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-langchain-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting langchain mi"
    $langChainApiMiClientId = $langChainApiMi.clientId
    Write-Host "LangChain MI Client Id: $langChainApiMiClientId" -ForegroundColor Yellow

    $promptHubApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-prompt-hub-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting prompt hub mi"
    $promptHubApiMiClientId = $promptHubApiMi.clientId
    Write-Host "Prompt Hub MI Client Id: $promptHubApiMiClientId" -ForegroundColor Yellow

    $semanticKernelApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-semantic-kernel-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting semantic kernel mi"
    $semanticKernelApiMiClientId = $semanticKernelApiMi.clientId
    Write-Host "Semantic Kernel MI Client Id: $semanticKernelApiMiClientId" -ForegroundColor Yellow

    $vectorizationApiMi = $(az identity show -g $resourceGroup -n $resourcePrefix-vectorization-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting vectorization mi"
    $vectorizationApiMiClientId = $vectorizationApiMi.clientId
    Write-Host "Vectorization MI Client Id: $vectorizationApiMiClientId" -ForegroundColor Yellow

    $vectorizationJobMi = $(az identity show -g $resourceGroup -n $resourcePrefix-vectorization-job-mi -o json | ConvertFrom-Json)
    EnsureSuccess "Error getting vectorization job mi"
    $vectorizationJobMiClientId = $vectorizationJobMi.clientId
    Write-Host "Vectorization Job MI Client Id: $vectorizationJobMiClientId" -ForegroundColor Yellow
}

$tenantId = $(az account show --query homeTenantId --output tsv)

# Setting tokens
$tokens.cosmosConnectionString = "AccountEndpoint=$($docdb.documentEndpoint);AccountKey=$docdbKey"
$tokens.cosmosEndpoint = $docdb.documentEndpoint
$tokens.cosmosKey = $docdbKey

if ($deployAks) {
    $tokens.agentFactoryApiMiClientId = $agentFactoryApiMiClientId
    $tokens.agentHubApiMiClientId = $agentHubApiMiClientId
    $tokens.chatUiMiClientId = $chatUiMiClientId
    $tokens.coreApiMiClientId = $coreApiMiClientId
    $tokens.coreJobMiClientId = $coreJobMiClientId
    $tokens.dataSourceHubApiMiClientId = $dataSourceHubApiMiClientId
    $tokens.gatekeeperApiMiClientId = $gatekeeperApiMiClientId
    $tokens.gatekeeperIntegrationApiMiClientId = $gatekeeperIntegrationApiMiClientId
    $tokens.langChainApiMiClientId = $langChainApiMiClientId
    $tokens.promptHubApiMiClientId = $promptHubApiMiClientId
    $tokens.semanticKernelApiMiClientId = $semanticKernelApiMiClientId
    $tokens.vectorizationApiMiClientId = $vectorizationApiMiClientId
    $tokens.vectorizationJobMiClientId = $vectorizationJobMiClientId
}

$tokens.tenantId = $tenantId
$tokens.appConfigEndpoint = $appConfigEndpoint
$tokens.appConfigConnectionString = $appConfigConnectionString

# Standard fixed tokens
$tokens.ingressclass = $ingressClass
$tokens.ingressrewritepath = "(/|$)(.*)"
$tokens.ingressrewritetarget = "`$2"

if ($ingressClass -eq "nginx") {
    $tokens.ingressrewritepath = "(/|$)(.*)" 
    $tokens.ingressrewritetarget = "`$2"
}

## Showing Values that will be used
Write-Host "===========================================================" -ForegroundColor Yellow
Write-Host "gvalues file will be generated with values:"
Write-Host ($tokens | ConvertTo-Json) -ForegroundColor Yellow
Write-Host "===========================================================" -ForegroundColor Yellow

if ($deployAks)
{
    Write-Host "Generating gvalues file..." -ForegroundColor Yellow
    Push-Location $($MyInvocation.InvocationName | Split-Path)
    $gvaluesTemplatePath = $(./Join-Path-Recursively -pathParts $gvaluesTemplate.Split(","))
    $outputFilePath = $(./Join-Path-Recursively -pathParts $outputFile.Split(","))
    & ./Token-Replace.ps1 -inputFile $gvaluesTemplatePath -outputFile $outputFilePath -tokens $tokens
    Pop-Location
}
