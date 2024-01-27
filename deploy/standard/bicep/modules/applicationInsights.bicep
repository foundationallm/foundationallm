param amplsName string
param environmentName string
param location string
param logAnalyticWorkspaceId string
param project string
param workload string

var name = 'ai-${environmentName}-${location}-${workload}-${project}'

/**
 * Resource representing an Azure Application Insights component.
 */
resource main 'microsoft.insights/components@2020-02-02' = {
  kind: 'web'
  location: location
  name: name

  properties: {
    Application_Type: 'web'
    DisableIpMasking: false
    DisableLocalAuth: false
    ForceCustomerStorageForProfiler: false
    IngestionMode: 'LogAnalytics'
    RetentionInDays: 30
    SamplingPercentage: 100
    WorkspaceResourceId: logAnalyticWorkspaceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'DevOps'
  }
}

/**
 * Creates a scoped service for private link integration with Azure Log Analytics.
 */
resource scopedService 'microsoft.insights/privatelinkscopes/scopedresources@2021-07-01-preview' = {
  name: '${amplsName}/amplss-${name}'
  properties: {
    linkedResourceId: main.id
  }
}
