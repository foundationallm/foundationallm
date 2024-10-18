targetScope = 'subscription'

param cidrVnet string = '192.168.100.0/24'
param createDate string = utcNow('u')
param environmentName string
param location string
param project string
param timestamp string = utcNow()
param vpnClientAddressPool string

// Locals
var abbrs = loadJsonContent('./abbreviations.json')
var resourceGroup = namer(abbrs.resourcesResourceGroups, environmentName, location, 'net', project)

param privateDnsLocations array = [
  'australiaeast'
  'canadaeast'
  'eastus'
  'eastus2'
  'francecentral'
  'japaneast'
  'northcentralus'
  'norwayeast'
  'southcentralus'
  'swedencentral'
  'switzerlandnorth'
  'southindia'
  'uksouth'
  'westeurope'
  'westus'
  'westus3'
]

var regionalZones = [for zoneLocation in privateDnsLocations: { 
    'aks_${zoneLocation}': 'privatelink.${zoneLocation}.azmk8s.io'
    'cr_${zoneLocation}': '${zoneLocation}.privatelink.azurecr.io'
  }
]

var regionalPrivateDnsZones = reduce(regionalZones,
  {},
  (curr, acc) => union(curr, acc)
)

var privateDnsZones = union({
  agentsvc: 'privatelink.agentsvc.azure-automation.net'
  blob: 'privatelink.blob.${environment().suffixes.storage}'
  cognitiveservices: 'privatelink.cognitiveservices.azure.com'
  configuration_stores: 'privatelink.azconfig.io'
  cosmosdb: 'privatelink.documents.azure.com'
  cr: 'privatelink.azurecr.io'
  dfs: 'privatelink.dfs.${environment().suffixes.storage}'
  eventgrid: 'privatelink.eventgrid.azure.net'
  file: 'privatelink.file.${environment().suffixes.storage}'
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
}, regionalPrivateDnsZones)

var tags = {
  'azd-env-name': environmentName 
  'create-date': createDate
  'iac-type': 'bicep'
  'project-name': project
}

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
    name: resourceGroup
    location: location
    tags: tags
}

/** Nested Modules **/
module vnet './core/networking/vnet.bicep' = {
  name: 'vnet-${timestamp}'
  params: {
    abbrs: abbrs
    cidrVnet: cidrVnet
    environmentName:environmentName
    location: location
    project: project
    tags: tags
  }
  scope: rg
}

module dns './core/networking/dns.bicep' = [for zone in items(privateDnsZones): {
  dependsOn: [vnet]
  name: '${zone.value}-${timestamp}'
  params: {
    key: zone.key
    vnetId: vnet.outputs.vnetId
    zone: zone.value
    tags: tags
  }
  scope: rg
}]

module dnsResolver './core/networking/dnsResolver.bicep' = {
  dependsOn: [vnet]
  name: 'dnsResolver-${timestamp}'
  params: {
    abbrs: abbrs
    environmentName: environmentName
    location: location
    project: project
    vnetName: vnet.outputs.vnetName
  }
  scope: rg
}

module vpn './core/networking/vpnGateway.bicep' = {
  dependsOn: [vnet]
  name: 'vpnGw-${timestamp}'
  params: {
    abbrs: abbrs
    environmentName: environmentName
    location: location
    project: project
    subnetId: '${vnet.outputs.vnetId}/subnets/GatewaySubnet'
    tags: tags
    vpnClientAddressPool: vpnClientAddressPool
  }
  scope: rg
}

output DNS_RESOLVER_ENDPOINT_IP string = dnsResolver.outputs.dnsResolverEndpointIp
output FLLM_PROJECT string = project
output RESOURCE_GROUP_NAME string = rg.name
output VPN_GATEWAY_NAME string = vpn.outputs.vpnGatewayName

