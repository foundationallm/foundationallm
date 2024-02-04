/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Containers to create')
param containers array = []

@description('Flag to enable HNS')
param enableHns bool = false

@description('Flag specifying if this is a datalake storage account')
param isDataLake bool = false

@description('KeyVault resource suffix for all resources')
param kvResourceSuffix string = resourceSuffix

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('OPS Resource Group name.')
param opsResourceGroupName string = resourceGroup().name

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Queues to create')
param queues array = []

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
    description: 'Alert on Storage Account Threshold - Account availability less than 99% for 5 minutes'
    evaluationFrequency: 'PT1M'
    metricName: 'Availability'
    name: 'storageUsage'
    operator: 'LessThan'
    severity: 1
    threshold: 99
    timeAggregation: 'Average'
    windowSize: 'PT5M'
  }
]

@description('Formatted untruncated resource name')
var kvFormattedName = toLower('${kvServiceType}-${substring(kvResourceSuffix, 0, length(kvResourceSuffix) - 4)}')

@description('The Resource Name')
var kvTruncatedName = substring(kvFormattedName,0,min([length(kvFormattedName),20]))
var kvName = '${kvTruncatedName}-${substring(kvResourceSuffix, length(kvResourceSuffix) - 3, 3)}'

@description('The Resource Service Type token')
var kvServiceType = 'kv'

@description('The Resource logs to enable')
var logs = {
  blobServices: [ 'StorageRead', 'StorageWrite', 'StorageDelete' ]
  fileServices: [ 'StorageRead', 'StorageWrite', 'StorageDelete' ]
}

@description('Formatted untruncated resource name')
var formattedName = toLower(replace('${serviceType}-${resourceSuffix}', '-', ''))

@description('The Resource Name')
var name = substring(formattedName,0,min([length(formattedName),24]))

@description('The Resource Service Type token')
var serviceType = isDataLake ? 'adls' : 'sa'

/** Outputs **/
@description('Storage Account Connection String KeyVault Secret Uri.')
output storageConnectionStringSecretUri string = storageConnectionString.outputs.secretUri

/** Resources **/
@description('The Storage Account')
resource main 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  kind: 'StorageV2'
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  sku: {
    name: 'Standard_LRS'
  }

  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowCrossTenantReplication: true
    allowSharedKeyAccess: true
    defaultToOAuthAuthentication: true
    isHnsEnabled: enableHns
    isNfsV3Enabled: false
    isSftpEnabled: false
    minimumTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Disabled'
    supportsHttpsTrafficOnly: true

    encryption: {
      keySource: 'Microsoft.Storage'
      requireInfrastructureEncryption: true

      services: {
        file: {
          enabled: true
          keyType: 'Account'
        }
        blob: {
          enabled: true
          keyType: 'Account'
        }
        queue: {
          enabled: true
        }
      }
    }

    networkAcls: {
      bypass: 'Logging, Metrics, AzureServices'
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []

      resourceAccessRules: [
        {
          tenantId: subscription().tenantId
          resourceId: subscriptionResourceId('Microsoft.Security/datascanners', 'storageDataScanner')
        }
      ]
    }

    sasPolicy: {
      sasExpirationPeriod: '00.04:00:00'
      expirationAction: 'Log'
    }
  }
}

@description('The blob service settings.')
resource blob 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: main
  name: 'default'

  properties: {
    changeFeed: { enabled: false }
    cors: { corsRules: [] }
    isVersioningEnabled: !isDataLake
    restorePolicy: { enabled: false }

    containerDeleteRetentionPolicy: isDataLake ? null : {
      days: 30
      enabled: true
    }

    deleteRetentionPolicy: isDataLake ? null : {
      allowPermanentDelete: false
      days: 30
      enabled: true
    }
  }
}

@description('Blob containers')
resource blobContainers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = [
  for container in containers: {
    name: container
    parent: blob
    properties: {
      publicAccess: 'None'
      metadata: null
    }
  }
]

@description('Diagnostic settings for the resource.')
resource blobServicesDiagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: blob
  name: 'diag-blobServices'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs.blobServices: {
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

@description('The file service settings.')
resource file 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
    protocolSettings: { smb: {} }

    shareDeleteRetentionPolicy: isDataLake ? null : {
      days: 30
      enabled: true
    }
  }
}

@description('Diagnostic settings for the resource.')
resource fileServicesDiagnostics 'Microsoft.Insights/diagnosticSettings@2017-05-01-preview' = {
  scope: file
  name: 'diag-fileServices'
  properties: {
    workspaceId: logAnalyticWorkspaceId
    logs: [for log in logs.fileServices: {
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

@description('The queue service settings.')
resource queue 'Microsoft.Storage/storageAccounts/queueServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
  }
}

@description('Queues')
resource storageQueues 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = [
  for q in queues: {
    name: q
    parent: queue
    properties: {
      metadata: null
    }
  }
]

@description('The table service settings.')
resource table 'Microsoft.Storage/storageAccounts/tableServices@2023-01-01' = {
  name: 'default'
  parent: main

  properties: {
    cors: { corsRules: [] }
  }
}

/** Nested Modules **/
@description('Metric alerts for the Resource')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.Storage/storageaccounts'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

@description('Private endpoint for the resource.')
module privateEndpoint 'utility/privateEndpoint.bicep' = [for zone in privateDnsZones: {
  name: 'pe-${main.name}-${zone.key}-${timestamp}'
  params: {
    groupId: zone.key
    location: location
    privateDnsZones: filter(privateDnsZones, item => item.key == zone.key)
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}]

@description('Storage Connection String KeyVault Secret.')
module storageConnectionString 'kvSecret.bicep' = {
  name: 'storageConn-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    kvName: kvName
    secretName: 'foundationallm-storage-connectionstring'
    secretValue: 'DefaultEndpointsProtocol=https;AccountName=${main.name};AccountKey=${main.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
    tags: tags
  }
}
