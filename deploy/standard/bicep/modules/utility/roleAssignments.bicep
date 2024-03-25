// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
param principalId string
param roleDefinitionIds object
param principalType string = 'ServicePrincipal'

/** Locals **/
var roleAssignmentsToCreate = [
  for roleDefinitionId in items(roleDefinitionIds): {
    name: guid(principalId, resourceGroup().id, roleDefinitionId.value)
    roleDefinitionId: roleDefinitionId.value
  }
]

/** Resources **/
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [
  for roleAssignmentToCreate in roleAssignmentsToCreate: {
    name: roleAssignmentToCreate.name
    properties: {
      principalType: principalType
      principalId: principalId
      roleDefinitionId: subscriptionResourceId(
        'Microsoft.Authorization/roleDefinitions',
        roleAssignmentToCreate.roleDefinitionId
      )
    }
  }
]
