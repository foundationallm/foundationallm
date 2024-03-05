param location string = resourceGroup().location
param name string
param sku string
param tags object = {}

resource search 'Microsoft.Search/searchServices@2022-09-01' = {
  name: name
  location: location
  sku: {
    name: sku
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
    networkRuleSet: {
      ipRules: []
    }
    encryptionWithCmk: {
      enforcement: 'Unspecified'
    }
    disableLocalAuth: true 
  }
  tags: tags
}

output endpoint string = 'https://${search.name}.search.windows.net'
output name string = search.name
