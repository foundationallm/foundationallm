/** Inputs **/
@description('Agent Pool Subnet Id')
param agentPoolSubnetId string

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('App Configuration logs to enable')
var logs = [ 'ContainerRegistryLoginEvents', 'ContainerRegistryRepositoryEvents' ]

@description('The Resource Name')
var name = replace('${serviceType}-${resourceSuffix}', '-', '')

@description('The Resource Service Type token')
var serviceType = 'cr'

/** Resources **/
resource main 'Microsoft.ContainerRegistry/registries@2023-08-01-preview' = {
  location: location
  name: name
  sku: { name: 'Premium' }
  tags: tags

  properties: {
    adminUserEnabled: true
    anonymousPullEnabled: true
    dataEndpointEnabled: false
    encryption: { status: 'disabled' }
    networkRuleBypassOptions: 'AzureServices'
    networkRuleSet: { defaultAction: 'Deny' }
    publicNetworkAccess: 'Disabled'
    zoneRedundancy: 'Disabled'

    policies: {
      azureADAuthenticationAsArmPolicy: { status: 'enabled' }
      exportPolicy: { status: 'enabled' }
      quarantinePolicy: { status: 'disabled' }

      retentionPolicy: {
        days: 30
        status: 'enabled'
      }

      softDeletePolicy: {
        retentionDays: 30
        status: 'enabled'
      }

      trustPolicy: {
        status: 'enabled'
        type: 'Notary'
      }
    }
  }
}

// resource agentPool 'Microsoft.ContainerRegistry/registries/agentPools@2019-06-01-preview' = {
//   parent: main
//   name: 'default'
//   location: location
//   properties: {
//     count: 2
//     os: 'Linux'
//     tier: 'S1'
//     virtualNetworkSubnetResourceId: agentPoolSubnetId
//   }
// }

@description('Diagnostic settings for App Configuration')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-${serviceType}'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs: {
      category: log
      enabled: true
    }]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
      }
    ]
  }
}

/** Nested Modules **/
@description('Private endpoint for the resource.')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'registry'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}
