@description('Resource suffix')
param resourceSuffix string

@description('Resource name.')
var name = 'ai-${resourceSuffix}'

resource main 'microsoft.insights/components@2020-02-02' existing = {
  name: name
}

output appInsightsConnectionString string = main.properties.ConnectionString
