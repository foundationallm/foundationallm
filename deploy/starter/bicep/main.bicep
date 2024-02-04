targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

param servicesExist object
@secure()
param serviceDefinition object
param services array

@description('Id of the user or app to assign application roles')
param principalId string

// Tags that should be applied to all resources.
// 
// Note that 'azd-service-name' tags should be applied separately to service host resources.
// Example usage:
//   tags: union(tags, { 'azd-service-name': <service name in azure.yaml> })
var tags = {
  'azd-env-name': environmentName
}

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module appConfig './shared/app-config.bicep' = {
  name: 'app-config'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.appConfigurationConfigurationStores}${resourceToken}'
    sku: 'standard'
    tags: tags
  }
  scope: rg
}

module contentSafety './shared/content-safety.bicep' = {
  name: 'content-safety'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.openAiAccounts}${resourceToken}'
    sku: 'S0'
    tags: tags
  }
  scope: rg
}

module cosmosDb './shared/cosmosdb.bicep' = {
  name: 'cosmos'
  params: {
    containers: [
      {
        name: 'UserSessions'
        partitionKeyPath: '/upn'
        maxThroughput: 1000
      }
      {
        name: 'UserProfiles'
        partitionKeyPath: '/upn'
        maxThroughput: 1000
      }
      {
        name: 'Sessions'
        partitionKeyPath: '/sessionId'
        maxThroughput: 1000
      }
      {
        name: 'leases'
        partitionKeyPath: '/id'
        maxThroughput: 1000
      }
    ]
    databaseName: 'database'
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.documentDBDatabaseAccounts}${resourceToken}'
    tags: tags
  }
  scope: rg
}

module cogSearch './shared/search.bicep' = {
  name: 'cogsearch'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.searchSearchServices}${resourceToken}'
    sku: 'basic'
    tags: tags
  }
  scope: rg
}

module dashboard './shared/dashboard-web.bicep' = {
  name: 'dashboard'
  params: {
    name: '${abbrs.portalDashboards}${resourceToken}'
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    location: location
    tags: tags
  }
  scope: rg
}

module keyVault './shared/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    tags: tags
    name: '${abbrs.keyVaultVaults}${resourceToken}'
    principalId: principalId
  }
  scope: rg
}

module monitoring './shared/monitoring.bicep' = {
  name: 'monitoring'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    tags: tags
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
  }
  scope: rg
}

module openAi './shared/openai.bicep' = {
  name: 'openai'
  params: {
    deployments: [
      {
        name: 'completions'
        sku: {
          name: 'Standard'
          capacity: 120
        }
        model: {
          name: 'gpt-35-turbo'
          version: '0613'
        }
      }
      {
        name: 'embeddings'
        sku: {
          name: 'Standard'
          capacity: 120
        }
        model: {
          name: 'text-embedding-ada-002'
          version: '2'
        }
      }
    ]
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.openAiAccounts}${resourceToken}'
    sku: 'S0'
    tags: tags
  }
  scope: rg
}

module registry './shared/registry.bicep' = {
  name: 'registry'
  params: {
    location: location
    tags: tags
    name: '${abbrs.containerRegistryRegistries}${resourceToken}'
  }
  scope: rg
}

module storage './shared/storage.bicep' = {
  name: 'storage'
  params: {
    containers: []
    files: []
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.storageStorageAccounts}${resourceToken}'
    tags: tags
  }
  scope: rg
}

module appsEnv './shared/apps-env.bicep' = {
  name: 'apps-env'
  params: {
    name: '${abbrs.appManagedEnvironments}${resourceToken}'
    location: location
    tags: tags
    applicationInsightsName: monitoring.outputs.applicationInsightsName
    logAnalyticsWorkspaceName: monitoring.outputs.logAnalyticsWorkspaceName
  }
  scope: rg
}

module coreApiService './app/acaService.bicep' = [ for service in services: {
    name: service.name
    params: {
      name: '${abbrs.appContainerApps}${service.name}-${resourceToken}'
      location: location
      tags: tags
      appConfigName: appConfig.outputs.name
      identityName: '${abbrs.managedIdentityUserAssignedIdentities}${service.name}-${resourceToken}'
      keyvaultName: keyVault.outputs.name
      applicationInsightsName: monitoring.outputs.applicationInsightsName
      containerAppsEnvironmentName: appsEnv.outputs.name
      containerRegistryName: registry.outputs.name
      exists: servicesExist['${service.name}']
      appDefinition: serviceDefinition
      hasIngress: service.hasIngress
      envSettings: service.useEndpoint ? [
        {
          name: service.appConfigEnvironmentVarName
          value: appConfig.outputs.endpoint
        }
      ] : []
      secretSettings: service.useEndpoint ? [] : [
        {
          name: service.appConfigEnvironmentVarName
          value: service.appConfigConnectionStringRef
          secretRef: 'appconfig-connection-string'
        }
      ]
    }
    scope: rg
    dependsOn: [ appConfig, cogSearch, contentSafety, cosmosDb, monitoring, openAi, storage ]
  }
]
