/** Inputs **/
@description('Action Group Id for alerts')
param actionGroupId string

@description('Model deployment capacity')
param capacity object = {
  completions: 1
  embeddings: 1
}

@description('Key Vault Name for secrets')
param keyVaultName string

@description('Location for all resources')
param location string

@description('Log Analytic Workspace Id to use for diagnostics')
param logAnalyticWorkspaceId string

@description('KeyVault resource suffix for all resources')
param opsKvResourceSuffix string = resourceSuffix

@description('OPS Resource Group name.')
param opsResourceGroupName string = resourceGroup().name

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
    metricName: 'SuccessRate'
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
    metricName: 'Latency'
    name: 'latency'
    operator: 'GreaterThan'
    severity: 0
    threshold: 1000
    timeAggregation: 'Average'
    windowSize: 'PT1H'
  }
]

@description('The Model Deployment Config')
var deploymentConfig = [
  {
    name: 'completions'
    capacity: capacity.completions
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0613'  
    }
  }
  {
    name: 'embeddings'
    capacity: capacity.embeddings
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
  }
]

@description('The Account Keys to place in Key Vault')
var keyNames = map([ 'key1', 'key2' ], item => {
    name: item
    secretName: '${item}-${resourceSuffix}'
  })

@description('The Resource Service Type token')
var kvServiceType = 'kv'

@description('The Resource logs to enable')
var logs = [ 'Trace', 'RequestResponse', 'Audit' ]

@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('Formatted untruncated resource name')
var opsFormattedKvName = toLower('${kvServiceType}-${substring(opsKvResourceSuffix, 0, length(opsKvResourceSuffix) - 4)}')

@description('The Resource Name')
var kvTruncatedName = substring(opsFormattedKvName,0,min([length(opsFormattedKvName),20]))
var opsKvName = '${kvTruncatedName}-${substring(opsKvResourceSuffix, length(opsKvResourceSuffix) - 3, 3)}'

// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
@description('Role Definition Ids')
var roleDefinitionIds = {
  'Key Vault Crypto User': '12338af0-0e69-4776-bea7-57ae8d297424'
}

@description('Role Assignments to create')
var roleAssignmentsToCreate = [for roleDefinitionId in items(roleDefinitionIds): {
  name: guid(main.id, resourceGroup().id, roleDefinitionId.value)
  roleDefinitionId: roleDefinitionId.value
}]

@description('The Resource Service Type token')
var serviceType = 'oai'

/** Outputs **/
@description('The Account Name')
output name string = main.name

@description('The Account Endpoint')
output endpoint string = main.properties.endpoint

@description('The Account Access Keys')
output keys array = [for (k, i) in keyNames: {
  name: k.secretName
  secretIdentifier: secret[i].properties.secretUri
}]

@description('The OpenAI API Key Secret URI (Currently just a placeholder).')
output openAiKeySecretUri string = apiKeySecret.outputs.secretUri

/** Resources **/
resource main 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  kind: 'OpenAI'
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    allowedFqdnList: []
    customSubDomainName: name
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
  }

  sku: {
    name: 'S0'
  }

}

@batchSize(1)
resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = [for config in deploymentConfig: {
  name: config.name
  parent: main

  properties: {
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    raiPolicyName: config.raiPolicyName
    model: config.model
  }

  sku: {
    capacity: config.capacity
    name: 'Standard'
  }
}]

@description('Resource for configuring the diagnostics.')
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

@description('Place the account keys in Key Vault.')
resource secret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = [for k in keyNames: {
  name: '${keyVaultName}/${k.secretName}'
  tags: tags
  properties: {
    value: filter(items(main.listKeys()), item => item.key == k.name)[0].value
  }
}]

@description('Role Assignments')
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for roleAssignmentToCreate in roleAssignmentsToCreate: {
  name: roleAssignmentToCreate.name
  scope: resourceGroup()
  properties: {
    principalId: main.identity.principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleAssignmentToCreate.roleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}]

/** Nested Modules **/
@description('Resource for configuring the Key Vault metric alerts.')
module metricAlerts 'utility/metricAlerts.bicep' = {
  name: 'alert-${main.name}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    alerts: alerts
    metricNamespace: 'Microsoft.CognitiveServices/accounts'
    nameSuffix: name
    serviceId: main.id
    tags: tags
  }
}

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

@description('OpenAI API Key OPS KeyVault Secret (Currently unused but added as a placeholder).')
module apiKeySecret 'kvSecret.bicep' = {
  name: 'oaiApiKey-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    kvName: opsKvName
    secretName: 'foundationallm-azureopenai-api-key'
    secretValue: 'HAOpenAIKey'
    tags: tags
  }
}
