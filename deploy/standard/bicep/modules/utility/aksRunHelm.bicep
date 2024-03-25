/** Inputs **/
param aksName string
param forceUpdateTag string = utcNow()
param helmApp string = 'azure-marketplace/wordpress'
param helmAppName string = 'my-wordpress'
param helmAppParams string = ''
param helmAppSettings object = {}
param helmRepo string = 'azure-marketplace'
param helmRepoURL string = 'https://marketplace.azurecr.io/helm/v1/repo'
param location string = resourceGroup().location
param uaiId string

@allowed([
  'OnSuccess'
  'OnExpiration'
  'Always'
])
@description('When the script resource is cleaned up')
param cleanupPreference string = 'OnSuccess'

@description('A delay before the script import operation starts. Primarily to allow Azure AAD Role Assignments to propagate')
param initialScriptDelay string = '120s'

/** Locals **/
var command  = 'helm repo add ${helmRepo} ${helmRepoURL} && helm repo update && helm upgrade --install ${helmAppName} ${helmApp} ${helmAppParams} ${appSettings}'
var settings = map(items(helmAppSettings), item => '--set ${item.key}=${item.value}')
var appSettings = join(settings, ' ')

/** Outputs **/
@description('Array of command output from each Deployment Script AKS run command')
output commandOutput object = {
  CommandOutput: runAksCommand.properties.outputs
  Name: runAksCommand.name
}

/** Data Sources **/
resource aks 'Microsoft.ContainerService/managedClusters@2022-11-01' existing = {
  name: aksName
}

/** Resources **/
resource runAksCommand 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  kind: 'AzureCLI'
  location: location
  name: 'ds-${aks.name}-${helmAppName}'

  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uaiId}': {}
    }
  }

  properties: {
    azCliVersion: '2.35.0'
    cleanupPreference: cleanupPreference
    forceUpdateTag: forceUpdateTag
    retentionInterval: 'P1D'
    scriptContent: loadTextContent('aks-run-command.sh')
    timeout: 'PT10M'
    environmentVariables: [
      { name: 'RG', value: resourceGroup().name }
      { name: 'aksName', value: aksName }
      { name: 'command', value: command }
      { name: 'initialDelay', value: initialScriptDelay }
    ]
  }
}
