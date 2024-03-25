@description('Action Group Id for alerts')
param actionGroupId string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Storage resource suffix')
param storageResourceSuffix string = resourceSuffix

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('Metric alerts for the resource.')
var alerts = []

@description('The Resource logs to enable')
var logs = []

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'eg'

@description('Formatted untruncated resource name')
var stFormattedName = toLower(replace('${stServiceType}-${storageResourceSuffix}', '-', ''))

@description('The Resource Name')
var storageAccountName = substring(stFormattedName,0,min([length(stFormattedName),24]))

@description('The Resource Service Type token')
var stServiceType = 'adls'

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource main 'Microsoft.EventGrid/systemTopics@2023-12-15-preview' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    source: storage.id
    topicType: 'Microsoft.Storage.StorageAccounts'
  }
}

@description('Diagnostic settings for the resource')
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-storage-${serviceType}'
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
@description('Metric alerts for the resource')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.EventGrid/namespaces'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

output name string = main.name
