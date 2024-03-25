/** Inputs **/
param environmentName string
param location string
param networkResourceGroupName string
param project string
param timestamp string = utcNow()
param vnetName string

@description('Workload Token used in naming resources.')
var workload = 'net'

var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

var resolverName = 'dns-${resourceSuffix}'

/** Locals **/
@description('Private DNS Zones to create.')
var privateDnsZone = {
  agentsvc: 'privatelink.agentsvc.azure-automation.net'
  aks: 'privatelink.${location}.azmk8s.io'
  blob: 'privatelink.blob.${environment().suffixes.storage}'
  cognitiveservices: 'privatelink.cognitiveservices.azure.com'
  configuration_stores: 'privatelink.azconfig.io'
  cosmosdb: 'privatelink.documents.azure.com'
  cr: 'privatelink.azurecr.io'
  cr_region: '${location}.privatelink.azurecr.io'
  dfs: 'privatelink.dfs.${environment().suffixes.storage}'
  eventgrid: 'privatelink.eventgrid.azure.net'
  file: 'privatelink.file.${environment().suffixes.storage}'
  gateway: 'privatelink.azure-api.net'
  gateway_developer: 'developer.azure-api.net'
  gateway_management: 'management.azure-api.net'
  gateway_portal: 'portal.azure-api.net'
  gateway_public: 'azure-api.net'
  gateway_scm: 'scm.azure-api.net'
  monitor: 'privatelink.monitor.azure.com'
  ods: 'privatelink.ods.opinsights.azure.com'
  oms: 'privatelink.oms.opinsights.azure.com'
  openai: 'privatelink.openai.azure.com'
  queue: 'privatelink.queue.${environment().suffixes.storage}'
  search: 'privatelink.search.windows.net'
  sites: 'privatelink.azurewebsites.net'
  sql_server: 'privatelink${environment().suffixes.sqlServerHostname}'
  table: 'privatelink.table.${environment().suffixes.storage}'
  vault: 'privatelink.vaultcore.azure.net'
}

/** Outputs **/

/** Resources **/
resource resolver 'Microsoft.Network/dnsResolvers@2022-07-01' = {
  name: resolverName
  location: location
  properties: {
    virtualNetwork: {
      id: resourceId(networkResourceGroupName, 'Microsoft.Network/virtualNetworks', vnetName)
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
          id: resourceId(networkResourceGroupName, 'Microsoft.Network/virtualNetworks/subnets', vnetName, 'FLLMNetSvc')
        }
      }
    ]
  }
}

/** Nested Modules **/
@description('Create the specified private DNS zones.')
module dns './modules/dns.bicep' = [for zone in items(privateDnsZone): {
  name: '${zone.value}-${timestamp}'
  params: {
    key: zone.key
    vnetId: resourceId(networkResourceGroupName, 'Microsoft.Network/virtualNetworks', vnetName)
    zone: zone.value

    tags: {
      Environment: environmentName
      IaC: 'Bicep'
      Project: project
      Purpose: 'Networking'
    }
  }
}]
