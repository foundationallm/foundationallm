/** Input **/
@description('KeyVault name')
param kvName string

@description('Secret name')
param secretName string

@description('Secret value')
@secure()
param secretValue string

@description('Tags')
param tags object

/** Locals **/

/** Outputs **/
output secretUri string = main.properties.secretUri

@description('KeyVault resource reference')
resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: kvName
}

@description('KeyVault secret')
resource main 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: secretName
  parent: keyvault
  tags: tags
  properties: {
    value: secretValue
  }
}
