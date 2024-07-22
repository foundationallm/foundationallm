param keyvaultName string
param openAiInstance object
param tags object = {}
@description('Timestamp for nested deployments')
param timestamp string = utcNow()

var secretNames = [
  'openai-apikey'
  'foundationallm-azureopenai-api-key'
  'foundationallm-openai-api-key'
  'foundationallm-semantickernelapi-openai-key'
]

resource apiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [for secretName in secretNames: {
  name: '${keyvaultName}/${secretName}'
  tags: tags

  properties: {
    value: openAi.listKeys().key1
  }
}]

resource openAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  scope: subscription(openAiInstance.subscriptionId)
  name: openAiInstance.resourceGroup
}

resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' existing = {
  name: openAiInstance.name
  scope: openAiResourceGroup
}

output keySecretName string = apiKeySecret[0].name
output keySecretRef string = apiKeySecret[0].properties.secretUri
