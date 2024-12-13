param abbrs object
param environmentName string
param location string
param project string
param tags object

param secrets array = []
param principalId string
param principalType string = 'ServicePrincipal'

// Locals
var kvName = namer(abbrs.keyVaultVaults, environmentName, location, 'test', project)

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

resource adminRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(principalId, resourceGroup().id, 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefiniitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
    principalType: principalType
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: kvName
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: { family: 'A', name: 'standard' }
    enabledForTemplateDeployment: true
    enableRbacAuthorization: true
  }
}

resource kvSecrets 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [
  for secret in secrets: {
    name: secret.name
    parent: keyVault
    tags: tags
    properties: {
      value: secret.value
    }
  }
]

output endpoint string = keyVault.properties.vaultUri
output name string = keyVault.name
output secretNames array = [for (secret,i) in secrets: kvSecrets[i].name]
output secretRefs array = [for (secret,i) in secrets: kvSecrets[i].properties.secretUri]


