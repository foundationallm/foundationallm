# Understand FoundationaLLM role assignments

Role assignments enable you to grant a principal (such as a user, a group, a managed identity, or a service principal) access to a specific FoundationaLLM resource.

## Role assignment

Acess to FoundationaLLM resources is granted through role assignments, and is reoked by removing a role assignment. A role assignment has several components:

- The _principal_, or _who_ is being given access.
- The _role definition_ (_role_), or _what_ access is being granted.
- The _scope_ at which the role is assigned, or _where_ the access applies.
- The _name_ of the role assignment.
- The _description_ of the role assignment that helps explain why it exists.

The following is an example of a FoundationaLLM role assignment:

```json
{
  "RoleAssignmentName": "00000000-0000-0000-0000-000000000000",
  "RoleAssignmentId": "/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Authorization/roleAssignments/00000000-0000-0000-0000-000000000000",
  "Scope": "/instances/11111111-1111-1111-1111-111111111111",
  "RoleDefinitionName": "Contributor",
  "RoleDefinitionId": "e459c3a6-6b93-4062-85b3-fffc9fb253df",
  "ObjectId": "22222222-2222-2222-2222-222222222222",
  "ObjectType": "User",
  "DisplayName": "Jack The Cat",
  "SignInName": "jackthecat@foundationallm.ai",
  "Description": "Jack The Cat has contributor access to the FoundationaLLM instance."
}
```

The following table describes the properties of a role assignment.

Property | Description
--- | ---
RoleAssignmentName | The name of the role assignment (is always a GUID).
RoleAssignmentId | The unique identifier of the role assignment which includes the name.
Scope | The FoundationaLLM resource identifier that the role assignment applies to.
RoleDefinitionName | The name of the role definition.
RoleDefinitionId | The unique identifier of the role definition.
DisplayName | The display name of the principal.
ObjectId | The unique identifier of the principal (can be a user, a group, a managed identity, or a service principal).
ObjectType | The type of the principal. Valid values include `User`, `Group`, and `ServicePrincipal`.
DisplayName | The display name of the principal.
SignInName | The unique principal name (UPN) of the principal.
Description | The description of the role assignment.
