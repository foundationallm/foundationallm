param environmentName string
param location string
param privateDnsZones array
param project string
param resourceSuffix string
param subnetId string
param timestamp string = utcNow()

var name = 'ampls-${resourceSuffix}'

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

module privateEndpoint 'utility/privateEndpoint.bicep' = {
  name: 'pe-${main.name}-${timestamp}'
  params: {
    groupId: 'azuremonitor'
    location: location
    privateDnsZones: privateDnsZones
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}
