/** Inputs **/
param environmentName string
param location string
param project string
param timestamp string = utcNow()
param vnetId string

@description('Workload Token used in naming resources.')
var workload = 'net'

var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

var resolverName = 'dns-${resourceSuffix}'

/** Locals **/
@description('Private DNS Zones to create.')
var privateDnsZone = {
  // grafana: 'privatelink.grafana.azure.com'
  // prometheusMetrics: 'privatelink.${location}.prometheus.monitor.azure.com'
  agentsvc :'privatelink.agentsvc.azure-automation.net'
  aks: 'privatelink.${location}.azmk8s.io'
  blob: 'privatelink.blob.${environment().suffixes.storage}'
  cognitiveservices: 'privatelink.cognitiveservices.azure.com'
  configuration_stores: 'privatelink.azconfig.io'
  cosmosdb: 'privatelink.documents.azure.com'
  cr: 'privatelink.azurecr.io'
  cr_region: '${location}.privatelink.azurecr.io'
  dfs: 'privatelink.dfs.${environment().suffixes.storage}'
  file: 'privatelink.file.${environment().suffixes.storage}'
  gateway: 'privatelink.azure-api.net'
  gateway_developer: 'developer.azure-api.net'
  gateway_management: 'management.azure-api.net'
  gateway_portal: 'portal.azure-api.net'
  gateway_public: 'azure-api.net'
  gateway_scm: 'scm.azure-api.net'
  monitor: 'privatelink.monitor.azure.com'
  ods :'privatelink.ods.opinsights.azure.com'
  oms :'privatelink.oms.opinsights.azure.com'
  openai: 'privatelink.openai.azure.com'
  queue: 'privatelink.queue.${environment().suffixes.storage}'
  search: 'privatelink.search.windows.net'
  sites: 'privatelink.azurewebsites.net'
  sql_server: 'privatelink${environment().suffixes.sqlServerHostname}'
  table: 'privatelink.table.${environment().suffixes.storage}'
  vault: 'privatelink.vaultcore.azure.net'
}

/** Outputs **/
@description('Private DNS Zones to use in other modules.')
output ids array = [for (zone, i) in items(privateDnsZone): {
  id: dns[i].outputs.id
  key: dns[i].outputs.key
  name: dns[i].outputs.name
}]

/** Nested Modules **/
@description('Create the specified private DNS zones.')
module dns './modules/dns.bicep' = [for zone in items(privateDnsZone): {
  name: '${zone.value}-${timestamp}'
  params: {
    key: zone.key
    vnetId: vnetId
    zone: zone.value

    tags: {
      Environment: environmentName
      IaC: 'Bicep'
      Project: project
      Purpose: 'Networking'
    }
  }
}]


resource resolver 'Microsoft.Network/dnsResolvers@2022-07-01' = {
  name: resolverName
  location: location
  properties: {
    virtualNetwork: {
      id: vnetId
    }
  }
}

resource inboundEndpoint 'Microsoft.Network/dnsResolvers/inboundEndpoints@2022-07-01' = {
  parent: resolver
  name: resolverName
  location: location
  properties: {
    ipConfigurations: [
      {
        privateIpAllocationMethod: 'Dynamic'
        subnet: {
          id: '${vnetId}/subnets/FLLMNetSvc'
        }
      }
    ]
  }
}
