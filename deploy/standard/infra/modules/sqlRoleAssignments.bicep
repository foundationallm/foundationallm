@description('Principal ID to assign roles to.')
param principalId string

@description('Roles to assign.')
param roleDefinitionIds object

@description('CosmosDB Resource Id')
param accountName string

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' existing = {
  name: accountName
}

/** Locals **/
@description('Role Assignments to create')
var roleAssignmentsToCreate = [for roleDefinitionId in items(roleDefinitionIds): {
  name: guid(principalId, resourceGroup().id, roleDefinitionId.value)
  roleDefinitionId: roleDefinitionId.value
}]

/** Resources **/
resource roleAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-11-15' = [
  for roleAssignmentToCreate in roleAssignmentsToCreate: {
    name: guid(roleAssignmentToCreate.roleDefinitionId, principalId, cosmosDb.id)
    parent: cosmosDb
    properties: {
      principalId: principalId
      roleDefinitionId: resourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions', cosmosDb.name, roleAssignmentToCreate.roleDefinitionId)
      scope: cosmosDb.id
    }
  }
]
