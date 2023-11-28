param environmentName string
param location string
param privateDnsZones array
param project string
param timestamp string = utcNow()
param vnetId string

var amplsZones = filter(
  privateDnsZones, 
  (zone) => contains(['monitor', 'blob', 'ods', 'oms', 'agentsvc'], zone.key)
)

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
    privateDnsZones: amplsZones
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

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-${timestamp}'
  params: {
    actionGroupId: actionGroup.outputs.id
    environmentName: environmentName
    location: location
    logAnalyticWorkspaceId: logAnalytics.outputs.id
    privateDnsZones: filter(privateDnsZones, (zone) => zone.key == 'vault')
    project: project
    subnetId: '${vnetId}/subnets/ops'
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
