param location string = resourceGroup().location
param name string
param sku string = 'standard'
param tags object = {}

resource main 'Microsoft.Search/searchServices@2023-11-01' = {
  location: location
  name: name
  tags: tags

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    disableLocalAuth: true
    hostingMode: 'default'
    partitionCount: 1
    publicNetworkAccess: 'enabled'
    replicaCount: 1
    semanticSearch: 'disabled'

    encryptionWithCmk: {
      enforcement: 'Disabled'
    }

    networkRuleSet: {
      ipRules: []
    }
  }

  sku: {
    name: sku
  }
}

output endpoint string = 'https://${main.name}.search.windows.net'
output name string = main.name
