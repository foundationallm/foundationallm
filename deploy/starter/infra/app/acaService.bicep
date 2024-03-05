param name string
param location string = resourceGroup().location
param tags object = {}

param appConfigName string
param eventgridName string
param cogsearchName string
param identityName string
param keyvaultName string
param containerRegistryName string
param containerAppsEnvironmentName string
param applicationInsightsName string
param exists bool
@secure()
param appDefinition object
param hasIngress bool = false
param envSettings array = []
param secretSettings array = []
param apiKeySecretName string
param serviceName string
param imageName string

var secretNames = [
  '${serviceName}-apikey'
  apiKeySecretName
]

var formattedAppName = replace(name, '-', '')
var truncatedAppName = substring(formattedAppName, 0, min(length(formattedAppName), 32))

var appSettingsArray = filter(array(appDefinition.settings), i => i.name != '')
var secrets = union(map(filter(appSettingsArray, i => i.?secret != null), i => {
  name: i.name
  value: i.value
  secretRef: i.?secretRef ?? take(replace(replace(toLower(i.name), '_', '-'), '.', '-'), 32)
}), secretSettings)

var env = union(map(filter(appSettingsArray, i => i.?secret == null), i => {
  name: i.name
  value: i.value
}), envSettings)

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' existing = {
  name: containerRegistryName
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-04-01-preview' existing = {
  name: containerAppsEnvironmentName
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(subscription().id, resourceGroup().id, identity.id, 'acrPullRole')
  properties: {
    roleDefinitionId:  subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
  }
}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigName
}

resource appConfigReaderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: appConfig
  name: guid(subscription().id, resourceGroup().id, identity.id, 'appConfigReaderRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '516239f1-63e1-4d78-a4de-a74fb236a071')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyvaultName
}

resource secretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyvault
  name: guid(subscription().id, resourceGroup().id, identity.id, 'secretsUserRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

resource secretsAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyvault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        objectId: identity.properties.principalId
        permissions: { secrets: [ 'get', 'list' ] }
        tenantId: subscription().tenantId
      }
    ]
  }
}

resource eventgrid 'Microsoft.EventGrid/namespaces@2023-12-15-preview' existing = {
  name: eventgridName
}

resource eventGridContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: eventgrid
  name: guid(subscription().id, resourceGroup().id, identity.id, 'eventGridContributorRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '1e241071-0855-49ea-94dc-649edcd759de')
      principalType: 'ServicePrincipal'
      principalId: identity.properties.principalId
  }
}

module fetchLatestImage '../modules/fetch-container-image.bicep' = {
  name: '${name}-fetch-image'
  params: {
    exists: exists
    name: imageName
  }
}

resource app 'Microsoft.App/containerApps@2023-04-01-preview' = {
  name: truncatedAppName
  location: location
  tags: union(tags, {'azd-service-name':  serviceName })
  dependsOn: [ acrPullRole, secretsAccessPolicy, appConfigReaderRole ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: { '${identity.id}': {} }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      ingress: hasIngress ? {
        external: true
        targetPort: 80
        transport: 'auto'
      } : null
      registries: [
        {
          server: '${containerRegistryName}.azurecr.io'
          identity: identity.id
        }
      ]
      secrets: union([
      ],
      map(secrets, secret => {
        identity: identity.id
        keyVaultUrl: secret.value
        name: secret.secretRef
      }))
    }
    template: {
      containers: [
        {
          image: fetchLatestImage.outputs.?containers[?0].?image ?? imageName
          name: 'main'
          env: union([
            {
              name: 'AZURE_CLIENT_ID'
              value: identity.properties.clientId
            }
            {
              name: 'AZURE_TENANT_ID'
              value: identity.properties.tenantId
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
            {
              name: 'PORT'
              value: '80'
            }
          ],
          env,
          map(secrets, secret => {
            name: secret.name
            secretRef: secret.secretRef
          }))
          resources: {
            cpu: json('1.0')
            memory: '2.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
    workloadProfileName: 'Warm'
  }
}

resource apiKey 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = [
  for secretName in secretNames: {
    name: secretName
    parent: keyvault
    tags: tags
    properties: {
      value: uniqueString(subscription().id, resourceGroup().id, app.id, serviceName)
    }
  }
]

resource search 'Microsoft.Search/searchServices@2022-09-01' existing = {
  name: cogsearchName
}

resource searchIndexDataReaderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: search
  name: guid(subscription().id, resourceGroup().id, identity.id, 'searchIndexDataReaderRole')
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions', '1407120a-92aa-4202-b7e9-c0e197c71c8f')
    principalType: 'ServicePrincipal'
    principalId: identity.properties.principalId
  }
}


output defaultDomain string = containerAppsEnvironment.properties.defaultDomain
output name string = app.name
output uri string = hasIngress ? 'https://${app.properties.configuration.ingress.fqdn}' : ''
output id string = app.id
