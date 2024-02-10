targetScope = 'subscription'

param appRegistrations array

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

param instanceId string = guid(subscription().id, location, environmentName)

var clientSecrets = [
  {
    name: 'foundationallm-apis-chat-ui-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-core-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-management-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-management-ui-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-apis-vectorization-api-entra-clientsecret'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-langchain-csvfile-url'
    value: 'PLACEHOLDER'
  }
  {
    name: 'foundationallm-langchain-sqldatabase-testdb-password'
    value: 'PLACEHOLDER'
  }
]

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
  dependsOn: [ keyVault ]
}

module contentSafety './shared/content-safety.bicep' = {
  name: 'content-safety'
  params: {
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.cognitiveServicesAccounts}${resourceToken}'
    sku: 'S0'
    tags: tags
  }
  scope: rg
  dependsOn: [ keyVault, openAi ]
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
  dependsOn: [ keyVault ]
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
  dependsOn: [ keyVault ]
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
    secrets: clientSecrets
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
  dependsOn: [ keyVault ]
}

module openAi './shared/openai.bicep' = {
  name: 'openai'
  params: {
    deployments: [
      {
        name: 'completions'
        sku: {
          name: 'Standard'
          capacity: 10
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
          capacity: 10
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
  dependsOn: [ keyVault ]
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
    containers: [
      {
        name: 'agents'
      }
      {
        name: 'data-sources'
      }
      {
        name: 'foundationallm-source'
      }
      {
        name: 'prompts'
      }
      {
        name: 'resource-provider'
      }
      {
        name: 'vectorization-input'
      }
      {
        name: 'vectorization-state'
      }
    ]
    files: []
    queues: [
      {
        name: 'extract'
      }
      {
        name: 'embed'
      }
      {
        name: 'partition'
      }
      {
        name: 'index'
      }
    ]
    keyvaultName: keyVault.outputs.name
    location: location
    name: '${abbrs.storageStorageAccounts}${resourceToken}'
    tags: tags
  }
  scope: rg
  dependsOn: [ keyVault ]
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

@batchSize(1)
module acaServices './app/acaService.bicep' = [ for service in services: {
    name: service.name
    params: {
      name: '${abbrs.appContainerApps}${service.name}${resourceToken}'
      location: location
      tags: tags
      appConfigName: appConfig.outputs.name
      identityName: '${abbrs.managedIdentityUserAssignedIdentities}${service.name}-${resourceToken}'
      keyvaultName: keyVault.outputs.name
      applicationInsightsName: monitoring.outputs.applicationInsightsName
      containerAppsEnvironmentName: appsEnv.outputs.name
      containerRegistryName: registry.outputs.name
      exists: servicesExist['${service.name}'] == 'true'
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
          value: appConfig.outputs.connectionStringSecretRef
          secretRef: 'appconfig-connection-string'
        }
      ]
      serviceName: service.name
    }
    scope: rg
    dependsOn: [ appConfig, cogSearch, contentSafety, cosmosDb, keyVault, monitoring, openAi, storage ]
  }
]

output AZURE_APP_CONFIG_NAME string = appConfig.outputs.name
output AZURE_COGNITIVE_SEARCH_ENDPOINT string = cogSearch.outputs.endpoint
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.outputs.loginServer
output AZURE_CONTENT_SAFETY_ENDPOINT string = contentSafety.outputs.endpoint
output AZURE_COSMOS_DB_ENDPOINT string = cosmosDb.outputs.endpoint
output AZURE_KEY_VAULT_NAME string = keyVault.outputs.name
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.outputs.endpoint
output AZURE_OPENAI_ENDPOINT string = openAi.outputs.endpoint
output AZURE_STORAGE_ACCOUNT_NAME string = storage.outputs.name

var appRegNames = [for appRegistration in appRegistrations: appRegistration.name]

output ENTRA_CHAT_UI_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'chat-ui')].clientId
output ENTRA_CHAT_UI_SCOPES string = appRegistrations[indexOf(appRegNames, 'chat-ui')].scopes
output ENTRA_CHAT_UI_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'chat-ui')].tenantId

output ENTRA_CORE_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'core-api')].clientId
output ENTRA_CORE_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'core-api')].scopes
output ENTRA_CORE_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'core-api')].tenantId

output ENTRA_MANAGEMENT_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'management-api')].clientId
output ENTRA_MANAGEMENT_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'management-api')].scopes
output ENTRA_MANAGEMENT_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'management-api')].tenantId

output ENTRA_MANAGEMENT_UI_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'management-ui')].clientId
output ENTRA_MANAGEMENT_UI_SCOPES string = appRegistrations[indexOf(appRegNames, 'management-ui')].scopes
output ENTRA_MANAGEMENT_UI_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'management-ui')].tenantId

output ENTRA_VECTORIZATION_API_CLIENT_ID string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].clientId
output ENTRA_VECTORIZATION_API_SCOPES string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].scopes
output ENTRA_VECTORIZATION_API_TENANT_ID string = appRegistrations[indexOf(appRegNames, 'vectorization-api')].tenantId

output FOUNDATIONALLM_INSTANCE_ID string = instanceId

var serviceNames = [for service in services: service.name]

output SERVICE_AGENT_FACTORY_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'agent-factory-api')].outputs.uri
output SERVICE_AGENT_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'agent-hub-api')].outputs.uri
output SERVICE_CHAT_UI_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'chat-ui')].outputs.uri
output SERVICE_CORE_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'core-api')].outputs.uri
output SERVICE_CORE_JOB_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'core-job')].outputs.uri
output SERVICE_DATA_SOURCE_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'data-source-hub-api')].outputs.uri
output SERVICE_GATEKEEPER_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'gatekeeper-api')].outputs.uri
output SERVICE_GATEKEEPER_INTEGRATION_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'gatekeeper-integration-api')].outputs.uri
output SERVICE_LANGCHAIN_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'langchain-api')].outputs.uri
output SERVICE_MANAGEMENT_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'management-api')].outputs.uri
output SERVICE_MANAGEMENT_UI_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'management-ui')].outputs.uri
output SERVICE_PROMPT_HUB_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'prompt-hub-api')].outputs.uri
output SERVICE_SEMANTIC_KERNEL_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'semantic-kernel-api')].outputs.uri
output SERVICE_VECTORIZATION_API_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'vectorization-api')].outputs.uri
output SERVICE_VECTORIZATION_JOB_ENDPOINT_URL string = acaServices[indexOf(serviceNames, 'vectorization-job')].outputs.uri
