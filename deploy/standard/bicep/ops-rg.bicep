/** Inputs **/
@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Private DNS Zones for Private Link Endpoints')
param privateDnsZones array

@description('Project Name, used in naming resources.')
param project string

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'

@description('Role Assignments to create')
var roleAssignmentsToCreate = [for roleDefinitionId in items(roleDefinitionIds): {
  name: guid(uaiAppConfig.id, resourceGroup().id, roleDefinitionId.value)
  roleDefinitionId: roleDefinitionId.value
}]

// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
@description('Role Definition Ids')
var roleDefinitionIds = {
  'Key Vault Crypto Service Encryption User': 'e147488a-f6f5-4113-8e2d-b22465e65bf6'
}

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

@description('Workload Token used in naming resources.')
var workload = 'ops'

@description('Private DNS Zones for Azure Monitor Private Link Scope')
var zonesAmpls = filter(
  privateDnsZones,
  (zone) => contains([ 'monitor', 'blob', 'ods', 'oms', 'agentsvc' ], zone.key)
)

@description('Private DNS Zones for Storage Accounts')
var zonesStorage = filter(
  privateDnsZones,
  (zone) => contains([ 'blob','dfs','file','queue','table','web' ], zone.key)
)

/** Resources **/
@description('Role Assignments')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for roleAssignmentToCreate in roleAssignmentsToCreate: {
  name: roleAssignmentToCreate.name
  scope: resourceGroup()
  properties: {
    principalId: uaiAppConfig.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleAssignmentToCreate.roleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}]


@description('User Assigned Identity for App Configuration')
resource uaiAppConfig 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-appconfig-${resourceSuffix}'
  tags: tags
}

/** Modules **/
@description('Azure Monitor Action Group')
module actionGroup 'modules/actionGroup.bicep' = {
  name: 'actionGroup-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    project: project
    workload: 'ops'
  }
}

@description('Azure Monitor Private Link Scope')
module ampls 'modules/ampls.bicep' = {
  name: 'ampls-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    privateDnsZones: zonesAmpls
    project: project
    subnetId: '${vnetId}/subnets/ops'
    workload: 'ops'
  }
}

@description('App Configuration')
module appConfig 'modules/appConfig.bicep' = {
  dependsOn: [ roleAssignment ]
  name: 'appConfig-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'configuration_stores')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
    uaiId: uaiAppConfig.id
    vaultName: keyVault.outputs.name
  }
}

@description('Application Insights')
module applicationInights 'modules/applicationInsights.bicep' = {
  name: 'appInsights-${timestamp}'
  params: {
    amplsName: ampls.outputs.name
    environmentName: environmentName
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    project: project
    workload: 'ops'
  }
}

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    environmentName: environmentName
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'vault')
    project: project
    subnetId: '${vnetId}/subnets/ops'
    workload: 'ops'
  }
}

@description('Log Analytics')
module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    environmentName: environmentName
    location: location
    project: project
    workload: 'ops'

    ampls: {
      id: ampls.outputs.id
      name: ampls.outputs.name
    }
  }
}

@description('Azure Monitor Workspace')
module monitorWorkspace 'modules/monitorWorksapce.bicep' = {
  name: 'monitorWorkspace-${timestamp}'
  params: {
    location: location
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'prometheusMetrics')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
  }
}

@description('Storage Account')
module storage 'modules/storageAccount.bicep'={
  name: 'storage-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: zonesStorage
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
  }
}
