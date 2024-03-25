@description('Principal ID to assign roles to.')
param principalId string

@description('Roles to assign.')
param roleDefinitionIds object

@description('Principal type.')
param principalType string = 'ServicePrincipal'

/** Locals **/
@description('Role Assignments to create')
var roleAssignmentsToCreate = [for roleDefinitionId in items(roleDefinitionIds): {
  name: guid(principalId, resourceGroup().id, roleDefinitionId.value)
  roleDefinitionId: roleDefinitionId.value
}]

/** Resources **/
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for roleAssignmentToCreate in roleAssignmentsToCreate: {
  name: roleAssignmentToCreate.name
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleAssignmentToCreate.roleDefinitionId)
    principalType: principalType
  }
}]
