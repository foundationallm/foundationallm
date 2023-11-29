param environmentName string
param location string
param privateDnsZones array
param project string
param timestamp string = utcNow()
param vnetId string

var resourceSuffix = '${environmentName}-${location}-${workload}-${project}'
var workload = 'ops'

var amplsZones = filter(
  privateDnsZones,
  (zone) => contains([ 'monitor', 'blob', 'ods', 'oms', 'agentsvc' ], zone.key)
)

var roleAssignmentsToCreate = [for roleDefinitionId in items(roleDefinitionIds): {
  name: guid(uaiAppConfig.id, resourceGroup().id, roleDefinitionId.value)
  roleDefinitionId: roleDefinitionId.value
}]

// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var roleDefinitionIds = {
  'Key Vault Crypto Service Encryption User': 'e147488a-f6f5-4113-8e2d-b22465e65bf6'
}

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

// Resources
resource uaiAppConfig 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-appconfig-${resourceSuffix}'
  tags: tags
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for roleAssignmentToCreate in roleAssignmentsToCreate: {
  name: roleAssignmentToCreate.name
  scope: resourceGroup()
  properties: {
    principalId: uaiAppConfig.properties.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleAssignmentToCreate.roleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}]

// Modules
module actionGroup 'modules/actionGroup.bicep' = {
  name: 'actionGroup-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    project: project
    workload: 'ops'
  }
}

module ampls 'modules/ampls.bicep' = {
  name: 'ampls-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    privateDnsZones: amplsZones
    project: project
    subnetId: '${vnetId}/subnets/ops'
    workload: 'ops'
  }
}

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
