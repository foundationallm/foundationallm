param environmentName string
param location string
param privateDnsZones array
param project string
param subnetId string
param timestamp string = utcNow()
param workload string

var name = 'ampls-${environmentName}-${location}-${workload}-${project}'

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

output id string = main.id
output name string = main.name

/*
  Resource representing the main Microsoft Insights Private Link Scope.
  This resource is used to configure access mode settings for ingestion and query.
*/
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

// /**
//  * Creates a private DNS zone group for Azure Monitor.
//  */
// resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
//   name: 'azuremonitor'
//   parent: endpoint

//   properties: {
//     privateDnsZoneConfigs: [for zone in privateDnsZones: {
//       name: zone.name
//       properties: {
//         privateDnsZoneId: zone.id
//       }
//     }]
//   }
// }

// /**
//  * Creates a private endpoint for Azure Monitor.
//  */
// resource endpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
//   name: 'pe-${main.name}'
//   location: location
//   tags: tags

//   properties: {
//     privateLinkServiceConnections: [
//       {
//         name: 'connection-azuremonitor-${name}'

//         properties: {
//           groupIds: [ 'azuremonitor' ]
//           privateLinkServiceId: main.id
//         }
//       }
//     ]

//     subnet: {
//       id: subnetId
//     }
//   }
// }

// /**
//  * Creates a private DNS zone group for Azure Monitor.
//  */
// resource dns 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-05-01' = {
//   name: 'default'
//   parent: endpoint

//   properties: {
//     privateDnsZoneConfigs: [for zone in privateDnsZones: {
//       name: zone.name
//       properties: {
//         privateDnsZoneId: zone.id
//       }
//     }]
//   }
// }

// /**
//  * Creates a private endpoint for Azure Monitor.
//  */
// resource endpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
//   name: 'pe-${main.name}'
//   location: location
//   tags: tags

//   properties: {
//     privateLinkServiceConnections: [for groupId in [ 'azuremonitor' ]: {
//       name: 'connection-${groupId}-${name}'

//       properties: {
//         groupIds: [ groupId ]
//         privateLinkServiceId: main.id
//       }
//     }
//     ]

//     subnet: {
//       id: subnetId
//     }
//   }
// }

module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupIds: [ 'azuremonitor' ]
    location: location
    nameSuffix: name
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags
    
    service:{
      id: main.id
      name: main.name
    }
  }
}
