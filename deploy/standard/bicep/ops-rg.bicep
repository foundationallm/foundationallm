/** Inputs **/
@description('Administrator Object Id')
param administratorObjectId string

@description('Administrator principal type.')
param administratorPrincipalType string = 'Group'

@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Private DNS Zones for Private Link Endpoints')
param privateDnsZones array

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

@description('Workload Token used in naming resources.')
var workload = 'ops'

@description('Private DNS Zones for Azure Monitor Private Link Scope')
var zonesAmpls = filter(
  privateDnsZones,
  (zone) => contains([ 'monitor', 'blob', 'ods', 'oms', 'agentsvc' ], zone.key)
)

@description('Private DNS Zones for Container Registry')
var zonesRegistry = filter(
  privateDnsZones,
  (zone) => contains([ 'cr', 'cr_region' ], zone.key)
)

@description('Private DNS Zones for Storage Accounts')
var zonesStorage = filter(
  privateDnsZones,
  (zone) => contains([ 'blob', 'dfs', 'file', 'queue', 'table', 'web' ], zone.key)
)

/** Outputs **/
@description('Azure Monitor Action Group')
output actionGroupId string = actionGroup.outputs.id

@description('Log Analytics Workspace')
output logAnalyticsWorkspaceId string = logAnalytics.outputs.id

/** Resources **/
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
  dependsOn: [ uaiAppConfigRoleAssignments ]
  name: 'appConfig-${timestamp}'
  params: {
    administratorObjectId: administratorObjectId
    administratorPrincipalType: administratorPrincipalType
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
    resourceSuffix: resourceSuffix
    tags: tags
    workload: 'ops'
  }
}

// @description('Azure Container Registry')
// module containerRegistry 'modules/containerRegistry.bicep' = {
//   name: 'containerRegistry-${timestamp}'
//   params: {
//     agentPoolSubnetId: '${vnetId}/subnets/ops'
//     location: location
//     logAnalyticWorkspaceId: logAnalytics.outputs.id
//     privateDnsZones: zonesRegistry
//     resourceSuffix: resourceSuffix
//     subnetId: '${vnetId}/subnets/ops'
//     tags: tags
//   }
// }

// @description('Azure Managed Grafana')
// module grafana 'modules/grafana.bicep' = {
//   name: 'grafana-${timestamp}'
//   params: {
//     azureMonitorWorkspaceResourceId: monitorWorkspace.outputs.id
//     location: location
//     privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'grafana')
//     resourceSuffix: resourceSuffix
//     subnetId: '${vnetId}/subnets/ops'
//     tags: tags
//   }
// }

@description('Key Vault')
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    administratorObjectId: administratorObjectId
    administratorPrincipalType: administratorPrincipalType
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'vault')
    resourceSuffix: resourceSuffix
    subnetId: '${vnetId}/subnets/ops'
    tags: tags
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

// @description('Azure Monitor Workspace')
// module monitorWorkspace 'modules/monitorWorksapce.bicep' = {
//   name: 'monitorWorkspace-${timestamp}'
//   params: {
//     location: location
//     privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'prometheusMetrics')
//     resourceSuffix: resourceSuffix
//     subnetId: '${vnetId}/subnets/ops'
//     tags: tags
//   }
// }

// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
module uaiAppConfigRoleAssignments 'modules/utility/roleAssignments.bicep' = {
  name: 'uaiAppConfigRoleAssignments-${timestamp}'
  params: {
    principalId: uaiAppConfig.properties.principalId
    roleDefinitionIds: {
      'Key Vault Crypto Service Encryption User': 'e147488a-f6f5-4113-8e2d-b22465e65bf6'
    }
  }
}

@description('Storage Account')
module storage 'modules/storageAccount.bicep' = {
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
