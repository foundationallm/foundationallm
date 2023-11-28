param actionGroupId string
param environmentName string
param location string
param logAnalyticWorkspaceId string
param privateDnsZones array
param project string
param subnetId string
param timestamp string = utcNow()
param workload string

var logs = ['HttpRequest', 'Audit']
var name = '${serviceType}-${environmentName}-${location}-${workload}-${project}'
var serviceType = 'appconfig'

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

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

resource main 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  tags: tags

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uai.id}': {}
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

/**
 * Resource for configuring diagnostic settings.
 */
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

resource uai 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-${name}'
  tags: tags
}

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
