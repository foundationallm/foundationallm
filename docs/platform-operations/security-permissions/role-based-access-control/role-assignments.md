# Role Assignments

Role assignments grant access by associating a principal with a role at a specific scope.

## Role Assignment Components

| Component | Description |
|-----------|-------------|
| **Principal** | Who receives access (user, group, service principal) |
| **Role Definition** | What access is granted |
| **Scope** | Where access applies |
| **Name** | Unique identifier for the assignment |
| **Description** | Optional explanation |

## Assignment Structure

```json
{
  "RoleAssignmentName": "00000000-0000-0000-0000-000000000000",
  "RoleAssignmentId": "/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/00000000-0000-0000-0000-000000000000",
  "Scope": "/instances/{instanceId}",
  "RoleDefinitionName": "Contributor",
  "RoleDefinitionId": "e459c3a6-6b93-4062-85b3-fffc9fb253df",
  "ObjectId": "22222222-2222-2222-2222-222222222222",
  "ObjectType": "User",
  "DisplayName": "John Doe",
  "SignInName": "john.doe@contoso.com",
  "Description": "Platform administrator access"
}
```

## Properties

| Property | Description |
|----------|-------------|
| `RoleAssignmentName` | Unique GUID for the assignment |
| `RoleAssignmentId` | Full resource path including name |
| `Scope` | Resource identifier where assignment applies |
| `RoleDefinitionName` | Human-readable role name |
| `RoleDefinitionId` | Role definition GUID |
| `ObjectId` | Principal's unique identifier |
| `ObjectType` | `User`, `Group`, or `ServicePrincipal` |
| `DisplayName` | Principal's display name |
| `SignInName` | Principal's UPN |
| `Description` | Optional description |

## Principal Types

| Type | Description | Use Case |
|------|-------------|----------|
| **User** | Individual Entra ID user | Named user access |
| **Group** | Entra ID security group | Team/department access |
| **ServicePrincipal** | App registration or managed identity | Automation/integration |

## Assignment Examples

### Instance-Level Assignment

Grant Contributor access to entire instance:

```json
{
  "name": "unique-guid",
  "principal_id": "user-object-id",
  "principal_type": "User",
  "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/e459c3a6-6b93-4062-85b3-fffc9fb253df",
  "scope": "/instances/{instanceId}",
  "description": "Platform contributor"
}
```

### Resource-Level Assignment

Grant Reader access to specific agent:

```json
{
  "name": "unique-guid",
  "principal_id": "group-object-id",
  "principal_type": "Group",
  "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
  "scope": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-agent",
  "description": "Sales team read access"
}
```

## Creating Assignments

### Via Management Portal

1. Navigate to **Security** > **Instance Access Control** (or resource's Access Control)
2. Click **Add Role Assignment**
3. Click **Browse** to select principal
4. Select role from dropdown
5. Add description (optional)
6. Click **Save**

### Via Management API

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/{assignmentName}
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "55555555-4444-3333-2222-111111111111",
  "description": "Sales team agent access",
  "principal_id": "11111111-2222-3333-4444-555555555555",
  "principal_type": "Group",
  "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
  "scope": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-agent"
}
```

## Querying Assignments

### Filter by Scope

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter
Content-Type: application/json
Authorization: Bearer <token>

{
  "scope": "/instances/{instanceId}"
}
```

### Filter by Resource

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter
Content-Type: application/json

{
  "scope": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent"
}
```

## Deleting Assignments

### Via Management Portal

1. Navigate to role assignments list
2. Find the assignment
3. Click **Delete** icon

### Via Management API

```http
DELETE /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/{assignmentName}
Authorization: Bearer <token>
```

## Inheritance

| Assignment Level | Applies To |
|------------------|------------|
| Instance | All resources in instance |
| Resource | Only that specific resource |

When viewing resource-level access control:
- **This resource** = Direct assignment
- **Instance (inherited)** = Inherited from instance

## Best Practices

| Practice | Description |
|----------|-------------|
| **Use Groups** | Assign to groups, not individuals |
| **Least Privilege** | Grant minimum required access |
| **Document** | Add descriptions to assignments |
| **Review Regularly** | Audit assignments periodically |
| **Scope Appropriately** | Use resource scope when possible |

## Related Topics

- [Role Definitions](role-definitions.md)
- [Scope](scope.md)
- [Role Management](role-management.md)
