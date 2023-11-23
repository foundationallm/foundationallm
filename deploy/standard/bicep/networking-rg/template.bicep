param environmentName string
param location string
param project string

var name = 'vnet-${environmentName}-${location}-net-${project}'

resource main 'Microsoft.Network/virtualNetworks@2023-05-01' = {
  name: name
  location: location

  properties: {
    enableDdosProtection: false
    addressSpace: {
      addressPrefixes: [ '10.0.0.0/16' ]
    }
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'Networking'
  }
}

output vnetId string = main.id
