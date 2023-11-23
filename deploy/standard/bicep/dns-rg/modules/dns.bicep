param tags object
param zone string
param vnetId string

resource main 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  location: 'global'
  name: zone
  tags: tags
}

resource link 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2018-09-01' = {
  parent: main
  name: zone
  location: 'global'
  tags: tags

  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: vnetId
    }
  }
}
