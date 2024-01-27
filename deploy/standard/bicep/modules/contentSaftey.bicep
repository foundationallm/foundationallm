/** Inputs **/
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
@description('The Resource logs to enable')
var logs = [ 'Trace', 'RequestResponse', 'Audit' ]

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'content-safety'

/** Outputs **/

/** Resources **/
resource main 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  name: name
  location: location
  tags: tags
  kind: 'ContentSafety'

  identity: {
    type: 'SystemAssigned'
  }

  sku: {
    name: 'S0'
  }

  properties: {
    allowedFqdnList: []
    customSubDomainName: name
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
  }
}

@description('Resource for configuring the Key Vault diagnostics.')
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
@description('Resource for configuring the private endpoint.')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'account'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      name: main.name
      id: main.id
    }
  }
}
