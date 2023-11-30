/** Inputs **/
@description('Location for all resources')
param location string

@description('Private DNS Zones for private endpoint')
param privateDnsZones array

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Subnet Id for private endpoint')
param subnetId string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

/** Locals **/
@description('The Resource Name')
var name = '${serviceType}-${resourceSuffix}'

@description('The Resource Service Type token')
var serviceType = 'amw'

/** Outputs **/
@description('The Resource Id')
output id string = main.id

/** Resources **/
resource main 'microsoft.monitor/accounts@2023-04-03' = {
  location: location
  name: name
  properties: {}
  tags: tags
}

/** Nested Modules **/
@description('Private endpoint for the resource.')
module privateEndpoint 'utility/privateEndpoint.bicep' = [for zone in privateDnsZones: {
  name: 'pe-${main.name}-${zone.key}-${timestamp}'
  params: {
    groupId: zone.key
    location: location
    privateDnsZones: filter(privateDnsZones, item => item.key == zone.key)
    subnetId: subnetId
    tags: tags

    service: {
      id: main.id
      name: main.name
    }
  }
}]
