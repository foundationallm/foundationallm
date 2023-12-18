param key string
param tags object
param vnetId string
param zone string

output id string = main.id
output key string = key
output name string = main.name

/*
  Creates a private DNS zone in Azure. It can be used to manage DNS records 
  within the zone.
*/
resource main 'Microsoft.Network/privateDnsZones@2018-09-01' = {
  location: 'global'
  name: zone
  tags: tags
}

/*
  Creates a virtual network link to a private DNS zone. It allows the virtual 
  network to resolve DNS names within the private DNS zone.
*/
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
