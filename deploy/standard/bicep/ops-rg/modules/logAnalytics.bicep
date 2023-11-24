param amplsName string
param environmentName string
param location string
param project string
param workload string

var name = 'la-${environmentName}-${location}-${workload}-${project}'

var solutions = [
  {
    name: 'ContainerInsights'
    product: 'OMSGallery/ContainerInsights'
    publisher: 'Microsoft'
  }
  {
    name: 'Security'
    product: 'OMSGallery/Security'
    publisher: 'Microsoft'
  }
  {
    name: 'SecurityCenterFree'
    product: 'OMSGallery/SecurityCenterFree'
    publisher: 'Microsoft'
  }
  {
    name: 'SQLAdvancedThreatProtection'
    product: 'OMSGallery/SQLAdvancedThreatProtection'
    publisher: 'Microsoft'
  }
  {
    name: 'SQLVulnerabilityAssessment'
    product: 'OMSGallery/SQLVulnerabilityAssessment'
    publisher: 'Microsoft'
  }
]

var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'DevOps'
}

/**
 * Resource representing a Log Analytics workspace.
 */
resource main 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: name
  location: location
  tags: tags

  properties: {
    forceCmkForQuery: false
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30

    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
      disableLocalAuth: false
    }

    sku: {
      name: 'PerGB2018'
    }

    workspaceCapping: {
      dailyQuotaGb: -1
    }
  }
}

/*
  Resource representing a data collection rule for VM Insights.
*/
resource dcr 'Microsoft.Insights/dataCollectionRules@2022-06-01' = {
  name: 'MSVMI-${name}'
  location: location
  tags: tags

  properties: {
    description: 'Data collection rule for VM Insights.'

    dataSources: {
      extensions: [ {
          extensionName: 'DependencyAgent'
          inputDataSources: []
          name: 'DependencyAgentDataSource'
          streams: [ 'Microsoft-ServiceMap' ]
        } ]

      performanceCounters: [ {
          counterSpecifiers: [ '\\VmInsights\\DetailedMetrics' ]
          name: 'VMInsightsPerfCounters'
          samplingFrequencyInSeconds: 60
          streams: [ 'Microsoft-InsightsMetrics' ]
        } ]
    }

    dataFlows: [
      {
        destinations: [ 'VMInsightsPerf-Logs-Dest' ]
        streams: [ 'Microsoft-InsightsMetrics' ]
      }
      {
        destinations: [ 'VMInsightsPerf-Logs-Dest' ]
        streams: [ 'Microsoft-ServiceMap' ]
      }
    ]

    destinations: {
      logAnalytics: [ {
          workspaceResourceId: main.id
          name: 'VMInsightsPerf-Logs-Dest'
        } ]
    }
  }
}

resource scopedService 'microsoft.insights/privatelinkscopes/scopedresources@2021-07-01-preview' = {
  name: '${amplsName}/amplss-${name}'
  properties: {
    linkedResourceId: main.id
  }
}

/*
  This resource block deploys a Log Analytics solution.
  It creates a Microsoft.OperationsManagement/solutions resource and associates it
  with the specified workspace.
  The solution is defined based on the input solutions array.
*/
resource solution 'Microsoft.OperationsManagement/solutions@2015-11-01-preview' = [for solution in solutions: {
  location: location
  name: solution.name

  plan: {
    name: '${solution.name}(${main.name})'
    product: solution.product
    publisher: solution.publisher
  }

  properties: {
    workspaceResourceId: main.id
  }
}]


