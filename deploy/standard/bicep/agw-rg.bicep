/** Inputs **/
@description('Action Group to use for alerts.')
param actionGroupId string

@description('The environment name token used in naming resources.')
param environmentName string

@description('Location used for all resources.')
param location string

@description('Log Analytics Workspace Id to use for diagnostics')
param logAnalyticsWorkspaceId string

param networkingResourceGroupName string

param opsResourceGroupName string

@description('Project Name, used in naming resources.')
param project string

@description('Timestamp used in naming nested deployments.')
param timestamp string = utcNow()

@description('Virtual Network ID, used to find the subnet IDs.')
param vnetId string

/** Locals **/
@description('The Application Gateway IDs')
var applicationGateways = [ 'www', 'api' ]

@description('Resource Suffix used in naming resources.')
var resourceSuffix = '${project}-${environmentName}-${location}-${workload}'

@description('Tags for all resources')
var tags = {
  Environment: environmentName
  IaC: 'Bicep'
  Project: project
  Purpose: 'Networking'
}

@description('Workload Token used in naming resources.')
var workload = 'agw'

/** Outputs **/
@description('Application Gateway Details to use in other modules.')
output applicationGateways array = [for (gateway, i) in applicationGateways: {
  id: agw[i].outputs.id
  key: gateway
}]

/** Resources **/
@description('User Assigned Identity for App Gateway')
resource uaiAgw 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: 'uai-${resourceSuffix}'
  tags: tags
}

/** Nested Modules **/
module agw 'modules/agw.bicep' = [for i in applicationGateways: {
  name: 'agw-${i}-${timestamp}'
  params: {
    actionGroupId: actionGroupId
    location: location
    logAnalyticsWorkspaceId: logAnalyticsWorkspaceId
    resourceSuffix: '${i}-${resourceSuffix}'
    subnetId: '${vnetId}/subnets/AppGateway'
    tags: tags
    uaiId: uaiAgw.id
  }
}]

module agwOpsRoleAssignments 'modules/utility/roleAssignments.bicep' = {
  name: 'opsrgagwra-${timestamp}'
  scope: resourceGroup(opsResourceGroupName)
  params: {
    principalId: uaiAgw.properties.principalId
    roleDefinitionIds: {
      'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
    }
  }
}

module agwNetRoleAssignments 'modules/utility/roleAssignments.bicep' = {
  name: 'netrgagwra-${timestamp}'
  scope: resourceGroup(networkingResourceGroupName)
  params: {
    principalId: uaiAgw.properties.principalId
    roleDefinitionIds: {
      Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
    }
  }
}
