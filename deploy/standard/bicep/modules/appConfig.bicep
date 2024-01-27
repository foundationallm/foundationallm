/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

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

@description('User Assigned Identity Id')
param uaiId string

@description('Vault Name to create keys in')
param vaultName string

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
    description: 'Service maximum storage usage greater than 75% for 1 hour'
    evaluationFrequency: 'PT1M'
    metricName: 'DailyStorageUsage'
    name: 'storageUsage'
    operator: 'GreaterThan'
    severity: 0
    threshold: 75
    timeAggregation: 'Maximum'
    windowSize: 'PT1H'
  }
  {
    description: 'Throttling occured within the last 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'ThrottledHttpRequestCount'
    name: 'latency'
    operator: 'GreaterThan'
    severity: 0
    threshold: 3
    timeAggregation: 'Maximum'
    windowSize: 'PT5M'
  }
]

/** Outputs **/
@description('App Configuration resource Id')
output appConfigId string = main.id

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

@description('CMK Key for App Configuration (TODO)')
resource key 'Microsoft.KeyVault/vaults/keys@2021-11-01-preview' = {
  name: '${vaultName}/key-${name}'
  tags: tags
  properties: {
    keySize: 2048
    kty: 'RSA'
    keyOps: [
      'decrypt'
      'encrypt'
      'sign'
      'unwrapKey'
      'verify'
      'wrapKey'
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
    groupId: 'configurationStores'
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

@description('App Configuration Data Owner Role for Administrator')
module adminRole 'utility/roleAssignments.bicep' = {
  name: 'appConfigAdminIAM-${timestamp}'
  params: {
    principalId: administratorObjectId
    principalType: administratorPrincipalType
    roleDefinitionIds: {
      'App Configuration Data Owner': '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b'
    }
  }
}
