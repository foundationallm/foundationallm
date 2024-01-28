/** Inputs **/
@description('API Key for service')
@secure()
param apiKey string = newGuid()

@description('Entra authentication client secret')
@secure()
param clientSecret string = ''

@description('Display Name')
param displayName string

@description('Location for all resources')
param location string

@description('Target Kubernetes namespace for microservice deployment')
param namespace string

@description('OIDC Issuer URL')
param oidcIssuerUrl string

@description('OPS Resource Group name')
param opsResourceGroupName string

@description('OPS resource suffix')
param opsResourceSuffix string

@description('Resource suffix for all resources')
param resourceSuffix string

@description('Service name')
param serviceName string

@description('Storage Resource Group name')
param storageResourceGroupName string

@description('Tags for all resources')
param tags object

@description('Timestamp for nested deployments')
param timestamp string = utcNow()

@description('Flag enabling OIDC support.')
param useOidc bool = false

/** Locals **/
@description('The App Configuration Service name.')
var appConfigName = '${appConfigServiceType}-${opsResourceSuffix}'

@description('The App Configuration Service type token.')
var appConfigServiceType = 'appconfig'

@description('The Resource Name')
var formattedKvName = toLower('${kvServiceType}-${opsResourceSuffix}')

@description('The Resource Name')
var kvName = substring(formattedKvName,0,min([length(formattedKvName),24]))

@description('The Resource Service Type token')
var kvServiceType = 'kv'

/** Outputs **/
@description('Service Managed Identity Client Id.')
output serviceClientId string = managedIdentity.properties.clientId

@description('Service Api Key Secret KeyVault Uri.')
#disable-next-line outputs-should-not-contain-secrets
output serviceApiKeySecretUri string = useOidc ? '' : apiKeySecret.outputs.secretUri

@description('Service OIDC Client Secret KeyVault Uri')
#disable-next-line outputs-should-not-contain-secrets
output serviceApiClientSecretUri string = useOidc ? apiClientSecret.outputs.secretUri : ''

/** Resources **/
@description('Resource for configuring user managed identity for a microservice')
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'mi-${serviceName}-${resourceSuffix}'
  tags: tags
}

@description('OIDC Federated Identity Credential for managed identity for a microservice')
resource federatedIdentityCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  name: serviceName
  parent: managedIdentity
  properties: {
    audiences: ['api://AzureADTokenExchange']
    issuer: oidcIssuerUrl
    subject: 'system:serviceaccount:${namespace}:${serviceName}'
  }
}

@description('OPS Role assignments for microservice managed identity')
module opsRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'opsIAM-${serviceName}-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionIds: {
      'App Configuration Data Reader': '516239f1-63e1-4d78-a4de-a74fb236a071'
      'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
    }
  }
}

@description('Storage Role assignments for microservice managed identity')
module storageRoleAssignments 'utility/roleAssignments.bicep' = {
  name: 'storageIAM-${serviceName}-${timestamp}'
  scope: resourceGroup(storageResourceGroupName)
  params: {
    principalId: managedIdentity.properties.principalId
    roleDefinitionIds: {
      Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
    }
  }
}

@description('API Key for microservice (only created if not using Entra)')
module apiKeySecret 'kvSecret.bicep' = if(!useOidc) {
  name: 'apiKey-${serviceName}-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    kvName: kvName
    secretName: 'foundationallm-apis-${serviceName}-apikey'
    secretValue: useOidc ? '' : apiKey
    tags: tags
  }
}

// @description('App Configuration entry for API Key.')
// module apiKeyConfigEntry 'appConfigEntry.bicep' = if(!useOidc) {
//   name: 'acAPIKey-${serviceName}-${timestamp}'
//   scope: resourceGroup(opsResourceGroupName)
//   params: {
//     appConfigName: appConfigName
//     contentType: useOidc ? '' : 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8'
//     name: 'FoundationaLLM:APIs:${displayName}:APIKey'
//     tags: tags
//     value: useOidc ? '' : '{"uri":"${apiKeySecret.outputs.secretUri}"}'
//   }
// }

@description('Client secret for microservice (only created if using Entra)')
module apiClientSecret 'kvSecret.bicep' = if(useOidc) {
  name: 'apiClientSecret-${serviceName}-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    kvName: kvName
    secretName: 'foundationallm-apis-${serviceName}-entra-clientsecret'
    secretValue: useOidc ? clientSecret : ''
    tags: tags
  }
}

// @description('App Configuration entry for Client Secret.')
// module clientSecretConfigEntry 'appConfigEntry.bicep' = if(useOidc) {
//   name: 'acClientSecret-${serviceName}-${timestamp}'
//   scope: resourceGroup(opsResourceGroupName)
//   params: {
//     appConfigName: appConfigName
//     contentType: useOidc ? 'application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8' : ''
//     name: 'FoundationaLLM:${displayName}:Entra:ClientSecret'
//     tags: tags
//     value: useOidc ? '{"uri":"${apiClientSecret.outputs.secretUri}"}' : ''
//   }
// }
