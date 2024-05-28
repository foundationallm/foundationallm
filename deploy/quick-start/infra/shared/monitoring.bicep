param applicationInsightsName string
param keyvaultName string
param location string = resourceGroup().location
param logAnalyticsName string
param tags object = {}

var secretNames = [
  'foundationallm-app-insights-connection-string'
]

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsName
  location: location
  tags: tags
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-02-01' existing = {
  name: keyvaultName
}

resource connectionSecretRef 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: applicationInsights.properties.ConnectionString
    }
  }
]

output applicationInsightsName string = applicationInsights.name
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output applicationInsightsConnectionSecretName string = connectionSecretRef[0].name
output applicationInsightsConnectionSecretRef string = connectionSecretRef[0].properties.secretUri
output logAnalyticsWorkspaceId string = logAnalytics.id
output logAnalyticsWorkspaceName string = logAnalytics.name
