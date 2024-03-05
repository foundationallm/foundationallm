param topics array = []
param location string = resourceGroup().location
param name string
param tags object = {}
param keyvaultName string

var secretNames = [
  'event-grid-key'
  'foundationallm-events-azureeventgrid-apikey'
]

resource namespace 'Microsoft.EventGrid/namespaces@2023-12-15-preview' = {
  name: name
  location: location
  sku: {
    name: 'Standard'
    capacity: 1
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publicNetworkAccess: 'Enabled'
    inboundIpRules: []
  }
  tags: tags
}

resource egTopics 'Microsoft.EventGrid/namespaces/topics@2023-12-15-preview' = [
  for topic in topics: {
    parent: namespace
    name: topic.name
    properties: {
      publisherType: 'Custom'
      inputSchema: 'CloudEventSchemaV1_0'
      eventRetentionInDays: 1
    }
  }
]

var topicNames = [for topic in topics: topic.name]

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource eventGridKey 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: namespace.listKeys().key1
    }
  }
]

output endpoint string = 'https://${namespace.properties.topicsConfiguration.hostname}'
output id string = namespace.id
output keySecretName string = eventGridKey[0].name
output keySecretRef string = eventGridKey[0].properties.secretUri
output name string = namespace.name
output topicNames array = topicNames
output topicIds array = [for (topicName,i) in topicNames: egTopics[i].id]
