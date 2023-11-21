param environment string
param instanceCount int = 2
param location string
param project string

var name = 'oai-${environment}-${location}-oai-${project}'

resource main 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = [for x in range(0, instanceCount): {
  kind: 'OpenAI'
  location: location
  name: '${name}-${x}'

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    allowedFqdnList: []
    customSubDomainName: '${name}-${x}'
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
  }

  sku: {
    name: 'S0'
  }

  tags: {
    Environment: environment
    Project: project
    Purpose: 'OpenAI'
  }
}]
