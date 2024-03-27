param keyvaultName string
param name string
param location string = resourceGroup().location
param sku string = 'standard'
param tags object = {}

resource appconfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  properties: {
    encryption: {}
    disableLocalAuth: false
    softDeleteRetentionInDays: 7
    enablePurgeProtection: false
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource connectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'appconfig-connection-string'
  parent: keyvault
  tags: tags
  properties: {
    value: appconfig.listKeys().value[2].connectionString
  }
}

output endpoint string = appconfig.properties.endpoint
output connectionStringSecretName string = connectionStringSecret.name
output connectionStringSecretRef string = connectionStringSecret.properties.secretUri
output name string = appconfig.name
