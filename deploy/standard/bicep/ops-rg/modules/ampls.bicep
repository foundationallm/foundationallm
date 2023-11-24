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

/*
  Resource representing the main Microsoft Insights Private Link Scope.
  This resource is used to configure access mode settings for ingestion and query.

  Parameters:
    - location: The location of the resource.
    - name: The name of the resource.
    - tags: The tags associated with the resource.

  Properties:
    - accessModeSettings: The access mode settings for the Private Link Scope.
      - ingestionAccessMode: The access mode for data ingestion (PrivateOnly or Public).
      - queryAccessMode: The access mode for data querying (Open or PrivateOnly).
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

/**
 * Creates a private DNS zone group for Azure Monitor.
 *
 * @param endpoint - The parent resource of the private DNS zone group.
 * @param privateDnsZones - The list of private DNS zones to associate with the private DNS zone group.
 * @returns The private DNS zone group resource.
 */
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


/**
 * Creates a private endpoint for Azure Monitor.
 *
 * @param endpoint - The parent resource of the private endpoint.
 * @param subnetId - The ID of the subnet to associate with the private endpoint.
 * @returns The private endpoint resource.
 */
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
