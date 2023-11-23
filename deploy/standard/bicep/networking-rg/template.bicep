param environment string
param location string
param project string

var name = 'vnet-${environment}-${location}-net-${project}'

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
    Environment: environment
    IaC: 'Bicep'
    Project: project
    Purpose: 'Networking'
  }
}
