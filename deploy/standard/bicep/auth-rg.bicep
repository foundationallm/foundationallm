/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('Administrator Object Id')
param administratorObjectId string

@description('APP Resource Group Name')
param appResourceGroupName string

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

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'


var services = {
  'authorization-api': { displayName: 'AuthorizationAPI'}
}

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Services'
}

@description('Workload Token used in naming resources.')
var workload = 'svc'

/** Outputs **/
@description('Read DNS Zones')
module dnsZones 'modules/utility/dnsZoneData.bicep' = {
  name: 'dnsZones-${timestamp}'
  scope: resourceGroup(dnsResourceGroupName)
  params: {
    location: location
  }
}

resource aksBackend 'Microsoft.ContainerService/managedClusters@2024-01-02-preview' existing = {
  name: 'aks-${resourceSuffix}-backend'
  scope: resourceGroup(appResourceGroupName)
}

module authStore 'modules/storageAccount.bicep' = {
  name: 'auth-store-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    containers: [
      'role-assignments'
    ]
    enableHns: true
    isDataLake: true
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => contains(['blob','dfs'], zone.key))
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMAuth'
    tags: tags
  }
}

module authKeyvault 'modules/keyVault.bicep' = {
  name: 'auth-kv-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    administratorObjectId: administratorObjectId
    administratorPrincipalType: 'Group'
    allowAzureServices: false
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(dnsZones.outputs.ids, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMAuth'
    tags: tags
  }
}

@batchSize(3)
module serviceResources 'modules/authService.bicep' = [for service in items(services): {
    name: 'beSvc-${service.key}-${timestamp}'
    params: {
      resourceSuffix: resourceSuffix
      serviceName: service.key
      location: location
      namespace: k8sNamespace
      oidcIssuerUrl: aksBackend.properties.oidcIssuerProfile.issuerURL
      tags: tags
    }
  }
]

