// See: https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
param principalId string
param roleDefinitionIds object = {}
param principalType string = 'ServicePrincipal'
param roleDefinitionNames array = []
param targetOpenAiName string

/** Locals **/
var roleDefinitionsToCreate = union(selectedRoleDefinitions, items(roleDefinitionIds))
var selectedRoleDefinitions = filter(items(roleDefinitions), (item) => contains(roleDefinitionNames, item.key))
var roleDefinitions = {
  'Cognitive Services OpenAI Contributor': 'a001fd3d-188f-4b5d-821b-7da978bf7442'
  'Cognitive Services OpenAI User':        '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
  'Cognitive Services User':               'a97b65f3-24c7-4388-baec-2e87135dc908'
  'Contributor':                           'b24988ac-6180-42a0-ab88-20f7382dd24c'
  'Reader':                                'acdd72a7-3385-48ef-bd42-f606fba81ae7'
}

var roleAssignmentsToCreate = [
  for roleDefinitionId in roleDefinitionsToCreate: {
    name: guid(principalId, resourceGroup().id, roleDefinitionId.value)
    roleDefinitionId: roleDefinitionId.value
  }
]

resource targetOpenAi 'Microsoft.CognitiveServices/accounts@2024-04-01-preview' existing = {
  name: targetOpenAiName
}

/** Resources **/
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [
  for roleAssignmentToCreate in roleAssignmentsToCreate: {
    scope: targetOpenAi
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
