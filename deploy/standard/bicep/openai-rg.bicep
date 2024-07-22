/** Inputs **/
param actionGroupId string
param dnsResourceGroupName string
param environmentName string
param location string
param logAnalyticsWorkspaceId string
param opsResourceGroupName string
param opsKeyVaultName string
param project string
param timestamp string = utcNow()
param vnetId string

param existingOpenAiInstance object

/** Locals **/
var deployments = filter(deploymentConfigurations, (d) => contains(d.locations, location))
var kvResourceSuffix = '${project}-${environmentName}-${location}-ops'
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'
var workload = 'oai'
var deployOpenAi = empty(existingOpenAiInstance.name)
var azureOpenAiEndpoint = deployOpenAi ? openai.outputs.endpoint : customerOpenAi.properties.endpoint
var azureOpenAiId = deployOpenAi ? openai.outputs.id : customerOpenAi.id
var azureOpenAi = deployOpenAi ? openAiInstance : existingOpenAiInstance
var azureOpenAiName = deployOpenAi ? openai.outputs.name : existingOpenAiInstance.name
var azureOpenAiRg = deployOpenAi ? resourceGroup().name : existingOpenAiInstance.resourceGroup
var azureOpenAiSub = deployOpenAi ? subscription().subscriptionId : existingOpenAiInstance.subscriptionId
var openAiInstance = {
  name: azureOpenAiName
  resourceGroup: azureOpenAiRg
  subscriptionId: azureOpenAiSub
}

var deploymentConfigurations = [
  {
    name: 'completions'
    locations: [
      'eastus'
      'eastus2'
      'japaneast'
      'northcentralus'
      'switzerlandnorth'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'completions'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'francecentral'
      'southindia'
      'swedencentral'
      'uksouth'
      'westus'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '1106'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'completions4'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'eastus2'
      'francecentral'
      'norwayeast'
      'southindia'
      'swedencentral'
      'uksouth'
      'westus'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-4'
      version: '1106-Preview'
    }
    sku: {
      capacity: 40
      name: 'Standard'
    }
  }
  {
    name: 'completions4o'
    locations: [
      'eastus'
      'eastus2'
      'northcentralus'
      'southcentralus'
      'southindia'
      'westus'
      'westus3'
    ]
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-05-13'
    }
    sku: {
      capacity: 40
      name: 'Standard'
    }
  }
  {
    name: 'embeddings'
    locations: [
      'austrailiaeast'
      'canadaeast'
      'eastus'
      'eastus2'
      'francecentral'
      'japaneast'
      'northcentralus'
      'norwayeast'
      'southcentralus'
      'swedencentral'
      'switzerlandnorth'
      'uksouth'
      'westeurope'
      'westus'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'embeddings-3-large'
    locations: [
      'canadaeast'
      'eastus'
      'eastus2'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-large'
      version: ''
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
  {
    name: 'embeddings-3-small'
    locations: [
      'canadaeast'
      'eastus'
      'eastus2'
    ]
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-3-small'
      version: ''
    }
    sku: {
      capacity: 60
      name: 'Standard'
    }
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'OpenAI'
}

/** Nested Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

@description('Content Safety')
module contentSafety 'modules/contentSaftey.bicep' = {
  name: 'contentSafety-${timestamp}'
  params: {
    kvResourceSuffix: kvResourceSuffix
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'cognitiveservices')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
}

@description('OpenAI')
module openai './modules/openai.bicep' = if (deployOpenAi) {
  name: 'openai-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    deployments: deployments
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'openai')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
}

module openAiSecrets './modules/openai-secrets.bicep' = {
  name: 'openaiSecrets-${timestamp}'

  params: {
    keyvaultName: opsKeyVaultName
    openAiInstance: azureOpenAi
    tags: tags
  }

  scope: resourceGroup(opsResourceGroupName)
  dependsOn: deployOpenAi ? [ openai ] : []
}

resource customerOpenAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing =
  if (!deployOpenAi) {
    scope: subscription(existingOpenAiInstance.subscriptionId)
    name: existingOpenAiInstance.resourceGroup
  }

resource customerOpenAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing =
  if (!deployOpenAi) {
    name: existingOpenAiInstance.name
    scope: customerOpenAiResourceGroup
  }

output azureOpenAiEndpoint string = azureOpenAiEndpoint
output azureOpenAiId string = azureOpenAiId
output azureOpenAiResourceGroup string = openAiInstance.resourceGroup
  