/** Inputs **/
param keyvaultName string
param location string = resourceGroup().location
param name string
param services array
param sku string = 'standard'
param tags object = {}

/** Locals **/
var readWriteServices = ['management-api']

/** Data Sources **/
resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

/** Resources **/
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

resource connectionStringReadOnlySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'appconfig-connection-string-readonly'
  parent: keyvault
  tags: tags
  properties: {
    value: appconfig.listKeys().value[2].connectionString
  }
}

resource connectionStringReadWriteSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: 'appconfig-connection-string'
  parent: keyvault
  tags: tags
  properties: {
    value: appconfig.listKeys().value[0].connectionString
  }
}

/** Outputs **/
output endpoint string = appconfig.properties.endpoint
output name string = appconfig.name

output connectionStringSecret array = [
  for service in services: {
    name: service.name
    uri: contains(readWriteServices, service.name)
      ? connectionStringReadWriteSecret.properties.secretUri
      : connectionStringReadOnlySecret.properties.secretUri
    secretName: contains(readWriteServices, service.name)
      ? connectionStringReadWriteSecret.name
      : connectionStringReadOnlySecret.name
  }
]
