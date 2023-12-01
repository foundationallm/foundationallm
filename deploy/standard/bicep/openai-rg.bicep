/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('The environment name token used in naming resources.')
param environmentName string

@description('Number of OpenAI instances to deploy.')
param instanceCount int = 2

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

param capacity object = {
  completions: 1
  embeddings: 1
}

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'OpenAI'
}

@description('Workload Token used in naming resources.')
var workload = 'oai'

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMOpenAI'
    tags: tags
  }
}

module openai './modules/openai.bicep' = [for x in range(0, instanceCount): {
  name: 'ha-openai-openai-${timestamp}-${x}'
  params: {
    capacity: capacity
    environmentName: environmentName
    location: location
    project: project
    instanceId: x
  }
}]
