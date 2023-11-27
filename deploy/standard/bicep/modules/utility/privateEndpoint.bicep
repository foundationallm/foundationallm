param location string
param nameSuffix string
param privateDnsZones array
param service object
param subnetId string
param tags object
param groupIds array

/**
 * Creates a private DNS zone group for Azure Monitor.
 */
resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = [for zone in privateDnsZones: {
  name: zone.name
  parent: main

  properties: {
    privateDnsZoneConfigs: [
      {
        name: zone.name
        properties: {
          privateDnsZoneId: zone.id
        }
      }
    ]
  }
}]

/**
 * Creates a private endpoint for Azure Monitor.
 */
resource main 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: 'pe-${service.name}'
  location: location
  tags: tags

  properties: {
    privateLinkServiceConnections: [for groupId in groupIds: {
      name: 'connection-${groupId}-${nameSuffix}'

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
