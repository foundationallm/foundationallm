targetScope = 'subscription'

param administratorObjectId string
param authAppRegistrationClientId string
param authAppRegistrationInstance string
param authAppRegistrationScopes string
param authAppRegistrationTenantId string
param createDate string = utcNow('u')
param createVpnGateway bool = false
param environmentName string
param externalDnsResourceGroupName string = ''
param externalNetworkingResourceGroupName string = ''
param instanceId string
param k8sNamespace string
param location string
param networkName string = ''
param project string
param timestamp string = utcNow()

@secure()
param authClientSecret string

// Locals
var abbrs = loadJsonContent('./abbreviations.json')
var useExternalDns = !empty(externalDnsResourceGroupName)
var useExternalNetworking = !empty(externalNetworkingResourceGroupName)

var tags = {
  'compute-type': 'aks'
  'create-date': createDate
  'env-name': environmentName
  'iac-type': 'bicep'
  'project-name': project
}

// TODO: BYO Resource Groups
var resourceGroups = union(defaultResourceGroups, externalResourceGroups)
var externalResourceGroups = union(externalDnsResourceGroup, externalNetworkingResourceGroup)
var externalNetworkingResourceGroup = useExternalNetworking ? { net: externalNetworkingResourceGroupName } : {}
var externalDnsResourceGroup = useExternalDns ? { dns: externalDnsResourceGroupName } : {}

var defaultResourceGroups = reduce(
  map(
    workloads,
    workload => { '${workload}': namer(abbrs.resourcesResourceGroups, environmentName, location, workload, project) }
  ),
  {},
  (cur, next) => union(cur, next)
)

var workloads = [
  'app'
  'auth'
  'data'
  'dns'
  'jbx'
  'net'
  'oai'
  'ops'
  'storage'
  'vec'
]

// Functions
func namer(resourceAbbr string, env string, region string, workloadName string, projectId string) string =>
  '${resourceAbbr}${env}-${region}-${workloadName}-${projectId}'

// Resources
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = [
  for rgName in items(resourceGroups): {
    name: rgName.value
    location: location
    tags: tags
  }
]

// Nested Deployments
module app 'app-rg.bicep' = {
  dependsOn: [rg]
  name: 'app-${timestamp}'
  scope: resourceGroup(resourceGroups.app)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    chatUiClientSecret: 'PLACEHOLDER'
    coreApiClientSecret: 'PLACEHOLDER'
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    k8sNamespace: k8sNamespace
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    logAnalyticsWorkspaceResourceId: ops.outputs.logAnalyticsWorkspaceId
    managementApiClientSecret: 'PLACEHOLDER'
    managementUiClientSecret: 'PLACEHOLDER'
    networkingResourceGroupName: resourceGroups.net
    openAiResourceGroupName: resourceGroups.oai
    opsResourceGroupName: resourceGroups.ops
    project: project
    storageResourceGroupName: resourceGroups.storage
    vectorizationApiClientSecret: 'PLACEHOLDER'
    vectorizationResourceGroupName: resourceGroups.vec
    vnetName: networking.outputs.vnetName
  }
}

module auth 'auth-rg.bicep' = {
  dependsOn: [rg]
  name: 'auth-${timestamp}'
  scope: resourceGroup(resourceGroups.auth)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    appResourceGroupName: resourceGroups.app
    authAppRegistrationClientId: authAppRegistrationClientId
    authAppRegistrationInstance: authAppRegistrationInstance
    authAppRegistrationScopes: authAppRegistrationScopes
    authAppRegistrationTenantId: authAppRegistrationTenantId
    authClientSecret: authClientSecret
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    instanceId: instanceId
    k8sNamespace: k8sNamespace
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    opsResourceGroupName: resourceGroups.ops
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module dns 'dns-rg.bicep' = if (!useExternalDns) {
  dependsOn: [rg]
  name: 'dns-${timestamp}'
  scope: resourceGroup(resourceGroups.dns)
  params: {
    environmentName: environmentName
    location: location
    networkResourceGroupName: resourceGroups.net
    project: project
    vnetName: networking.outputs.vnetName
  }
}

module networking 'networking-rg.bicep' = {
  dependsOn: [rg]
  name: 'networking-${timestamp}'
  scope: resourceGroup(resourceGroups.net)
  params: {
    createVpnGateway: createVpnGateway
    environmentName: environmentName
    location: location
    networkName: networkName
    project: project
  }
}

module openai 'openai-rg.bicep' = {
  dependsOn: [rg]
  name: 'openai-${timestamp}'
  scope: resourceGroup(resourceGroups.oai)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    administratorObjectId: administratorObjectId
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    opsResourceGroupName: resourceGroups.ops
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module ops 'ops-rg.bicep' = {
  dependsOn: [rg]
  name: 'ops-${timestamp}'
  scope: resourceGroup(resourceGroups.ops)
  params: {
    administratorObjectId: administratorObjectId
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    location: location
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module storage 'storage-rg.bicep' = {
  dependsOn: [rg]
  name: 'storage-${timestamp}'
  scope: resourceGroup(resourceGroups.storage)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    project: project
    vnetId: networking.outputs.vnetId
  }
}

module vec 'vec-rg.bicep' = {
  dependsOn: [rg]
  name: 'vec-${timestamp}'
  scope: resourceGroup(resourceGroups.vec)
  params: {
    actionGroupId: ops.outputs.actionGroupId
    dnsResourceGroupName: resourceGroups.dns
    environmentName: environmentName
    location: location
    logAnalyticsWorkspaceId: ops.outputs.logAnalyticsWorkspaceId
    project: project
    vnetId: networking.outputs.vnetId
  }
}

output managedResourceGroupNames object = reduce(
  filter(items(resourceGroups), (item) => !contains(objectKeys(externalResourceGroups), item.key)),
  {},
  (curr, next) => union(curr, { '${next.key}': next.value })
)
