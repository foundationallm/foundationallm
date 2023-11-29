param location string
param privateDnsZones array
param service object
param subnetId string
param tags object
param groupId string

@description('DNS registration for the private endpoint.')
resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: 'default'
  parent: main

  properties: {
    privateDnsZoneConfigs: [for zone in privateDnsZones: {
      name: zone.key
      properties: {
        privateDnsZoneId: zone.id
      }
    }]
  }
}

@description('The private endpoint resource.')
resource main 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: 'pe-${service.name}-${groupId}'
  location: location
  tags: tags

  properties: {
    privateLinkServiceConnections: [
      {
        name: 'connection-${service.name}-${groupId}'

        properties: {
          groupIds: [ groupId ]
          privateLinkServiceId: service.id
        }
      }
    ]

    subnet: {
      id: subnetId
    }
  }
}
