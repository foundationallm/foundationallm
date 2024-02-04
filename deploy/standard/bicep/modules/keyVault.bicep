/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Allows Azure services to bypass network rules')
param allowAzureServices bool = true

@description('Administrator Object Id')
param administratorObjectId string

@description('Administrator principal type.')
param administratorPrincipalType string = 'Group'

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
@description('Metric alerts for the resource.')
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

@description('The Resource logs to enable')
var logs = [ 'AuditEvent', 'AzurePolicyEvaluationDetails' ]

@description('Formatted untruncated resource name')
var formattedName = toLower('${serviceType}-${substring(resourceSuffix, 0, length(resourceSuffix) - 4)}')

@description('The Resource Name')
var truncatedName = substring(formattedName,0,min([length(formattedName),20]))
var name = '${truncatedName}-${substring(resourceSuffix, length(resourceSuffix) - 3, 3)}'

@description('The Resource Service Type token')
var serviceType = 'kv'

/** Outputs **/
@description('KeyVault resource Id')
output keyVaultId string = main.id

output name string = main.name

/** Resources **/
@description('Resource for configuring the Key Vault.')
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

    networkAcls: {
      bypass: allowAzureServices ? 'AzureServices' : null
      defaultAction: 'Deny'
    }

    sku: {
      family: 'A'
      name: 'standard'
    }
  }
}

@description('Resource for configuring the Key Vault diagnostics.')
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

/** Nested Modules **/
@description('Resource for configuring the Key Vault metric alerts.')
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

@description('Resource for configuring the Key Vault private endpoint.')
module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'vault'
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

@description('Key Vault Secrets Officer Role for Administrator')
module adminRole 'utility/roleAssignments.bicep' = {
  name: 'kvAdminIAM-${timestamp}'
  params: {
    principalId: administratorObjectId
    principalType: administratorPrincipalType
    roleDefinitionIds: {
      'Key Vault Secrets Officer': 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
    }
  }
}
