/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('AKS Admnistrator Object Id to use for AKS.')
param aksAdmnistratorObjectId string

@description('Application Gateways')
param applicationGateways array

@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

@description('Log Analytics Workspace Resource Id to use for diagnostics')
param logAnalyticsWorkspaceResourceId string

@description('Networking Resource Group Name')
param networkingResourceGroupName string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Services'
}

@description('Workload Token used in naming resources.')
var workload = 'svc'

/** Nested Modules **/
module aksBackend 'modules/aks.bicep' = {
  name: 'aksBackend-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    admnistratorObjectIds: [ aksAdmnistratorObjectId ]
    agw: first(filter(applicationGateways, (agw) => agw.key == 'api'))
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    privateDnsZones: filter(privateDnsZones, (zone) => contains([ 'aks' ], zone.key))
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
    admnistratorObjectIds: [ aksAdmnistratorObjectId ]
    agw: first(filter(applicationGateways, (agw) => agw.key == 'www'))
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    logAnalyticWorkspaceResourceId: logAnalyticsWorkspaceResourceId
    networkingResourceGroupName: networkingResourceGroupName
    privateDnsZones: filter(privateDnsZones, (zone) => contains([ 'aks' ], zone.key))
    resourceSuffix: '${resourceSuffix}-frontend'
    subnetId: '${vnetId}/subnets/FLLMFrontend'
    subnetIdPrivateEndpoint: '${vnetId}/subnets/FLLMServices'
    tags: tags
  }
}
