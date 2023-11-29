/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

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

@description('User Assigned Identity Id')
param uaiId string

/** Locals **/
@description('App Configuration logs to enable')
var logs = [ 'HttpRequest', 'Audit' ]

@description('App Configuration name')
var name = 'appconfig-${resourceSuffix}'

@description('App Configuration Service Type token')
var serviceType = 'appconfig'

@description('Metric alerts for App Configuration')
var alerts = [
  {
    name: 'storageUsage'
    metricName: 'DailyStorageUsage'
    operator: 'GreaterThan'
    threshold: 75
    timeAggregation: 'Maximum'
    windowSize: 'PT1H'
    evaluationFrequency: 'PT1M'
    description: 'Service maximum storage usage greater than 75% for 1 hour'
    severity: 0
  }
  {
    name: 'latency'
    metricName: 'ThrottledHttpRequestCount'
    operator: 'GreaterThan'
    threshold: 3
    timeAggregation: 'Maximum'
    windowSize: 'PT5M'
    evaluationFrequency: 'PT1M'
    description: 'Throttling occured within the last 5 minutes'
    severity: 0
  }
]

/** Resources **/
@description('App Configuration')
resource main 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  tags: tags

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uaiId}': {}
    }
  }

  properties: {
    disableLocalAuth: false
    enablePurgeProtection: true
    encryption: {}
    publicNetworkAccess: 'Disabled'
    softDeleteRetentionInDays: 1
  }

  sku: {
    name: 'standard'
  }
}

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
@description('Metric alerts for App Configuration')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.AppConfiguration/configurationStores'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Private endpoint for App Configuration')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupIds: [ 'configurationStores' ]
    location: location
    nameSuffix: name
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}
