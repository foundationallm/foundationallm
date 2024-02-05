/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('Administrator Object Id')
param administratorObjectId string

@description('Application Gateway resource group name')
param agwResourceGroupName string

@description('Chat UI OIDC Client Secret')
@secure()
param chatUiClientSecret string

@description('Core API OIDC Client Secret')
@secure()
param coreApiClientSecret string

@description('DNS Resource Group Name')
param dnsResourceGroupName string

@description('The environment name token used in naming resources.')
param environmentName string

@description('AKS namespace')
param k8sNamespace string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Log Analytics Workspace Resource Id to use for diagnostics')
param logAnalyticsWorkspaceResourceId string

@description('Management UI OIDC Client Secret')
@secure()
param managementUiClientSecret string

@description('Management API OIDC Client Secret')
@secure()
param managementApiClientSecret string

@description('Networking Resource Group Name')
param networkingResourceGroupName string

@description('OPS Resource Group name')
param opsResourceGroupName string

@description('Project Name, used in naming resources.')
param project string

@description('Storage Resource Group name')
param storageResourceGroupName string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Vectorization API OIDC Client Secret')
@secure()
param vectorizationApiClientSecret string

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('KeyVault resource suffix')
var opsResourceSuffix = '${project}-${environmentName}-${location}-ops'

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Services'
}

var backendServices = {
  'agent-factory-api': { displayName: 'AgentFactoryAPI'}
  'agent-hub-api': { displayName: 'AgentHubAPI' }
  'core-job': { displayName: 'CoreWorker' }
  'data-source-hub-api': { displayName: 'DataSourceHubAPI' }
  'gatekeeper-api': { displayName: 'GatekeeperAPI' }
  'gatekeeper-integration-api': { displayName: 'GatekeeperIntegrationAPI' }
  'langchain-api': { displayName: 'LangChainAPI' }
  'prompt-hub-api': { displayName: 'PromptHubAPI' }
  'semantic-kernel-api': { displayName: 'SemanticKernelAPI' }
  'vectorization-job': { displayName: 'VectorizationWorker' }
}

var chatUiService = { 'chat-ui': { displayName: 'Chat' } }
var coreApiService = { 'core-api': { displayName: 'CoreAPI' } }
var vectorizationApiService = { 'vectorization-api': { displayName: 'VectorizationAPI' } }

var managementUiService = { 'management-ui': { displayName: 'ManagementUI' } }
var managementApiService = { 'management-api': { displayName: 'ManagementAPI' } }

@description('Workload Token used in naming resources.')
var workload = 'svc'

/** Outputs **/

/** Nested Modules **/
module agws 'modules/utility/agwData.bicep' = {
  name: 'agws-${timestamp}'
  scope: resourceGroup(agwResourceGroupName)
  params: {
    location: location
    environmentName: environmentName
    project: project
  }
}

@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

module aksBackend 'modules/aks.bicep' = {
  name: 'aksBackend-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    admnistratorObjectIds: [ administratorObjectId ]
    agw: first(filter(agws.outputs.applicationGateways, (agw) => agw.key == 'api'))
    agwResourceGroupName: agwResourceGroupName
    dnsResourceGroupName: dnsResourceGroupName
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains([ 'aks' ], zone.key))
    resourceSuffix: '${resourceSuffix}-backend'
    subnetId: '${vnetId}/subnets/FLLMBackend'
    subnetIdPrivateEndpoint: '${vnetId}/subnets/FLLMServices'
    tags: tags
  }
}

module aksFrontend 'modules/aks.bicep' = {
  name: 'aksFrontend-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    admnistratorObjectIds: [ administratorObjectId ]
    agw: first(filter(agws.outputs.applicationGateways, (agw) => agw.key == 'www'))
    agwResourceGroupName: agwResourceGroupName
    dnsResourceGroupName: dnsResourceGroupName
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains([ 'aks' ], zone.key))
    resourceSuffix: '${resourceSuffix}-frontend'
    subnetId: '${vnetId}/subnets/FLLMFrontend'
    subnetIdPrivateEndpoint: '${vnetId}/subnets/FLLMServices'
    tags: tags
  }
}

@batchSize(3)
module backendServiceResources 'modules/service.bicep' = [for service in items(backendServices): {
    name: 'beSvc-${service.key}'
    params: {
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      serviceName: service.key
      storageResourceGroupName: storageResourceGroupName
      tags: tags
    }
  }
]

module chatUiServiceResources 'modules/service.bicep' = [for service in items(chatUiService): {
    name: 'feSvc-${service.key}'
    params: {
      clientSecret: chatUiClientSecret
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksFrontend.outputs.oidcIssuerUrl
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      serviceName: service.key
      storageResourceGroupName: storageResourceGroupName
      tags: tags
      useOidc: true
    }
  }
]

module managementUiServiceResources 'modules/service.bicep' = [for service in items(managementUiService): {
    name: 'feSvc-${service.key}'
    params: {
      clientSecret: managementUiClientSecret
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksFrontend.outputs.oidcIssuerUrl
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      serviceName: service.key
      storageResourceGroupName: storageResourceGroupName
      tags: tags
      useOidc: true
    }
  }
]

module coreApiServiceResources 'modules/service.bicep' = [for service in items(coreApiService): {
    name: 'feSvc-${service.key}'
    params: {
      clientSecret: coreApiClientSecret
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      serviceName: service.key
      storageResourceGroupName: storageResourceGroupName
      tags: tags
      useOidc: true
    }
  }
]

module managementApiServiceResources 'modules/service.bicep' = [for service in items(managementApiService): {
    name: 'feSvc-${service.key}'
    params: {
      clientSecret: managementApiClientSecret
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
      opsResourceGroupName: opsResourceGroupName
      opsResourceSuffix: opsResourceSuffix
      resourceSuffix: resourceSuffix
      serviceName: service.key
      storageResourceGroupName: storageResourceGroupName
      tags: tags
      useOidc: true
    }
  }
]

module vectorizationApiServiceResources 'modules/service.bicep' = [for service in items(vectorizationApiService): {
  name: 'feSvc-${service.key}'
  params: {
    clientSecret: vectorizationApiClientSecret
    location: location
    namespace: k8sNamespace
    oidcIssuerUrl: aksBackend.outputs.oidcIssuerUrl
    opsResourceGroupName: opsResourceGroupName
    opsResourceSuffix: opsResourceSuffix
    resourceSuffix: resourceSuffix
    serviceName: service.key
    storageResourceGroupName: storageResourceGroupName
    tags: tags
    useOidc: true
  }
}
]
