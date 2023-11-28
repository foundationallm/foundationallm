param actionGroupId string
param environmentName string
param location string
param logAnalyticWorkspaceId string
param project string
param subnetId string
param workload string
param privateDnsZones array 
param timestamp string = utcNow()

var logs = [ 'AuditEvent', 'AzurePolicyEvaluationDetails' ]
var name = 'kv-${environmentName}-${location}-${workload}-${project}'

var alerts = [
  {
    description: 'Service availability less than 99% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'Availability'
    name: 'availability'
    operator: 'LessThan'
    severity: 0
    threshold: 99
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
  {
    description: 'Service latency more than 1000ms for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'ServiceApiLatency'
    name: 'latency'
    operator: 'GreaterThan'
    severity: 0
    threshold: 1000
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
  {
    description: 'Service saturation more than 75% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'SaturationShoebox'
    name: 'saturation'
    operator: 'GreaterThan'
    severity: 0
    threshold: 75
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

/*
* Creates a key vault.  
*/
resource main 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags

  properties: {
    enablePurgeProtection: true
    enableRbacAuthorization: true
    enableSoftDelete: true
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    publicNetworkAccess: 'Disabled'
    softDeleteRetentionInDays: 7
    tenantId: subscription().tenantId

    sku: {
      family: 'A'
      name: 'standard'
    }
  }
}

/**
 * Resource for configuring diagnostic settings.
 */
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: main
  name: 'diag-kv'
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

module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.KeyVault/vaults'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupIds: [ 'vault' ]
    location: location
    nameSuffix: name
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      name: main.name
      id: main.id
    }
  }
}
