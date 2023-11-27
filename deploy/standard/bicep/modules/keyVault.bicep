param actionGroupId string
param environmentName string
param location string
param logAnalyticWorkspaceId string
param project string
param subnetId string
param workload string
param zoneId string

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

/*
  This resource block creates a metric alert for a Key Vault resource.
  It iterates over the 'alerts' array and creates a metric alert for each alert object.
  The alert properties are defined based on the values provided in the alert object.
*/
resource alert 'Microsoft.Insights/metricAlerts@2018-03-01' = [for alert in alerts: {
  name: 'alert-${alert.name}-${name}'
  location: 'global'
  tags: tags
  properties: {
    autoMitigate: true
    description: alert.description
    enabled: true
    evaluationFrequency: alert.evaluationFrequency
    scopes: [ main.id ]
    severity: alert.severity
    windowSize: alert.windowSize

    actions: [
      {
        actionGroupId: actionGroupId
        webHookProperties: {}
      }
    ]

    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.MultipleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          metricName: alert.metricName
          metricNamespace: 'Microsoft.KeyVault/vaults'
          name: alert.name
          operator: alert.operator
          skipMetricValidation: false
          threshold: alert.threshold
          timeAggregation: alert.timeAggregation
        }
      ]
    }
  }
}]

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

/**
 * Creates a private DNS zone group for Azure Monitor.
 */
resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: 'vault'
  parent: endpoint

  properties: {
    privateDnsZoneConfigs: [ {
        name: 'vault'
        properties: {
          privateDnsZoneId: zoneId
        }
      } ]
  }
}

/**
 * Creates a private endpoint for Azure Monitor.
 */
resource endpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: 'pe-${main.name}'
  location: location
  tags: tags

  properties: {
    privateLinkServiceConnections: [
      {
        name: 'connection-vault-${name}'

        properties: {
          groupIds: [ 'vault' ]
          privateLinkServiceId: main.id
        }
      }
    ]

    subnet: {
      id: subnetId
    }
  }
}
