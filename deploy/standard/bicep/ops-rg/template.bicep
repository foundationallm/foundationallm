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

module applicationInights 'modules/applicationInsights.bicep' = {
  name: 'appInsights-${timestamp}'
  params: {
    amplsName: ampls.outputs.name
    environmentName: environmentName
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    project: project
    workload: 'ops'
  }
}

module logAnalytics 'modules/logAnalytics.bicep' = {
  name: 'logAnalytics-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    environmentName: environmentName
    location: location
    project: project
    workload: 'ops'

    ampls: {
      id: ampls.outputs.id
      name: ampls.outputs.name
    }
  }
}
