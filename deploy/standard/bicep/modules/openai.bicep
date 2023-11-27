param environmentName string
param instanceId int
param location string
param project string

param capacity object = {
  completions: 1
  embeddings: 1
}

var name = 'oai-${environmentName}-${location}-oai-${project}-${instanceId}'

var deploymentConfig = [
  {
    name: 'completions'
    capacity: capacity.completions
    raiPolicyName: ''
    model: {
      format: 'OpenAI'
      name: 'gpt-35-turbo'
      version: '0301'
    }
  }
  {
    name: 'embeddings'
    capacity: capacity.embeddings
    raiPolicyName: 'Microsoft.Default'
    model: {
      format: 'OpenAI'
      name: 'text-embedding-ada-002'
      version: '2'
    }
  }
]

resource main 'Microsoft.CognitiveServices/accounts@2023-10-01-preview' = {
  kind: 'OpenAI'
  location: location
  name: name

  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    allowedFqdnList: []
    customSubDomainName: name
    disableLocalAuth: false
    dynamicThrottlingEnabled: false
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: false
  }

  sku: {
    name: 'S0'
  }

  tags: {
    Environment: environmentName
    IaC: 'Bicep'
    Project: project
    Purpose: 'OpenAI'
  }
}

@batchSize(1)
resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2023-10-01-preview' = [for config in deploymentConfig: {
  name: config.name
  parent: main

  properties: {
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
    raiPolicyName: config.raiPolicyName
    model: config.model
  }

  sku: {
    capacity: config.capacity
    name: 'Standard'
  }
}]
