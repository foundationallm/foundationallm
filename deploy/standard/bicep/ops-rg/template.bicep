param blobPrivateDnsZoneId string
param environmentName string
param location string
param monitorPrivateDnsZoneId string
param project string
param timestamp string = utcNow()
param vnetId string

var privateDnsZones = [
  {
    id: blobPrivateDnsZoneId
    name: 'blob'
  }
  {
    id: monitorPrivateDnsZoneId
    name: 'monitor'
  }
]

module actionGroup 'modules/actionGroup.bicep' = {
  name: 'actionGroup-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    project: project
    workload: 'ops'
  }
}

module ampls 'modules/ampls.bicep' = {
  name: 'ampls-${timestamp}'
  params: {
    environmentName: environmentName
    location: location
    privateDnsZones: privateDnsZones
    project: project
    subnetId: '${vnetId}/subnets/ops'
    workload: 'ops'
  }
}
