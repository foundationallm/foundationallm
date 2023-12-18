/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('The environment name token used in naming resources.')
param environmentName string

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

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Storage'
}

@description('Workload Token used in naming resources.')
var workload = 'storage'

@description('Private DNS Zones for Storage Accounts')
var zonesStorage = filter(
  privateDnsZones,
  (zone) => contains([ 'blob', 'dfs', 'file', 'queue', 'table', 'web' ], zone.key)
)

/** Nested Modules **/
@description('Cosmos DB Account')
module cosmosdb 'modules/cosmosdb.bicep' = {
  name: 'cosmosdb-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'cosmosdb')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMStorage'
    tags: tags
  }
}

@description('Storage Account')
module storage 'modules/storageAccount.bicep' = {
  name: 'storage-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticWorkspaceId: logAnalyticsWorkspaceId
    privateDnsZones: zonesStorage
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/FLLMStorage'
    tags: tags
  }
}
