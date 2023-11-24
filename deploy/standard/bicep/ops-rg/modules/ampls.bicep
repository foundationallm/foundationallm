param environmentName string
param location string
param privateDnsZones array
param project string
param subnetId string
param workload string

var name = 'ampls-${environmentName}-${location}-${workload}-${project}'

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

resource main 'microsoft.insights/privatelinkscopes@2021-07-01-preview' = {
  location: 'global'
  name: name
  tags: tags

  properties: {
    accessModeSettings: {
      ingestionAccessMode: 'PrivateOnly'
      queryAccessMode: 'Open'
    }
  }
}

resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
  name: 'azuremonitor'
  parent: endpoint

  properties: {
    privateDnsZoneConfigs: [for zone in privateDnsZones: {
      name: zone.name
      properties: {
        privateDnsZoneId: zone.id
      }
    }]
  }
}

resource endpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: 'pe-${main.name}'
  location: location
  tags: tags

  properties: {
    privateLinkServiceConnections: [
      {
        name: 'connection-azuremonitor-${name}'

        properties: {
          groupIds: [ 'azuremonitor' ]
          privateLinkServiceId: main.id
        }
      }
    ]

    subnet: {
      id: subnetId
    }
  }
}
