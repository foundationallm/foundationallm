/** Inputs **/
@description('Administrator Object Id')
param administratorObjectId string

@description('Action Group to use for alerts.')
param actionGroupId string

@description('DNS resource group name')
param dnsResourceGroupName string

@description('The environment name token used in naming resources.')
param environmentName string

@description('Number of OpenAI instances to deploy.')
param instanceCount int = 2

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('OPS Resource Group name')
param opsResourceGroupName string

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

param capacity object = {
  completions: 10
  embeddings: 10
}

/** Locals **/
@description('KeyVault resource suffix')
var kvResourceSuffix = '${project}-${environmentName}-${location}-ops'

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'OpenAI'
}

@description('Workload Token used in naming resources.')
var workload = 'oai'

@description('Private DNS Zones for Azure API Management')
var zonesApim = filter(
  dnsZones.outputs.ids,
  (zone) => contains([ 'gateway_developer', 'gateway_management', 'gateway_portal', 'gateway_public', 'gateway_scm' ], zone.key)
)

/** Nested Modules **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

@description('API Management')
module apim 'modules/apim.bicep' = {
  name: 'apim-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    dnsResourceGroupName: dnsResourceGroupName
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: zonesApim
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags

    cognitiveAccounts: [for x in range(0, instanceCount): {
      name: openai[x].outputs.name
      endpoint: openai[x].outputs.endpoint
      keys: openai[x].outputs.keys
    }]
  }
  dependsOn: [ openai ]
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
  dependsOn: [ keyVault ]
}

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    administratorObjectId: administratorObjectId
    allowAzureServices: false
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
}

@description('OpenAI')
module openai './modules/openai.bicep' = [for x in range(0, instanceCount): {
  name: 'openai-${x}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    capacity: capacity
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    opsKvResourceSuffix: kvResourceSuffix
    opsResourceGroupName: opsResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'openai')
    resourceSuffix: '${resourceSuffix}-${x}'
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
    keyVaultName: keyVault.outputs.name
  }
  dependsOn: [ keyVault ]
}]
