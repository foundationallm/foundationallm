// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
param principalId string
param roleDefinitionIds object = {}
param principalType string = 'ServicePrincipal'
param roleDefinitionNames array = []

/** Locals **/
var roleDefinitionsToCreate = union(selectedRoleDefinitions, items(roleDefinitionIds))
var selectedRoleDefinitions = filter(items(roleDefinitions), (item) => contains(roleDefinitionNames, item.key))
var roleDefinitions = {
  'Key Vault Secrets User': '4633458b-17de-408a-b874-0445c86b69e6'
  'Storage Blob Data Contributor': 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  'Storage Queue Data Contributor': '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
  Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
}

var roleAssignmentsToCreate = [
  for roleDefinitionId in roleDefinitionsToCreate: {
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
