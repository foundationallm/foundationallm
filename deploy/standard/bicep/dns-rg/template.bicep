param environmentName string
param location string
param project string
param timestamp string = utcNow()
param vnetId string

var private_dns_zone = {
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
  grafana: 'privatelink.grafana.azure.com'
  monitor: 'privatelink.monitor.azure.com'
  openai: 'privatelink.openai.azure.com'
  prometheus: 'privatelink.${location}.prometheus.monitor.azure.com'
  queue: 'privatelink.queue.${environment().suffixes.storage}'
  search: 'privatelink.search.windows.net'
  sites: 'privatelink.azurewebsites.net'
  sql_server: 'privatelink${environment().suffixes.sqlServerHostname}'
  table: 'privatelink.table.${environment().suffixes.storage}'
  vault: 'privatelink.vaultcore.azure.net'
}

module dns './modules/dns.bicep' = [for zone in items(private_dns_zone): {
  name: '${zone.value}-${timestamp}'
  params: {
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
