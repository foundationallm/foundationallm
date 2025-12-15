# Role Definitions

A role definition is a collection of permissions that defines what actions can be performed.

## Role Definition Structure

| Property | Description |
|----------|-------------|
| `Name` | Display name of the role |
| `Id` | Unique identifier (GUID) |
| `Description` | Purpose of the role |
| `Actions` | Control plane actions allowed |
| `NotActions` | Actions excluded from Actions |
| `DataActions` | Data plane actions allowed |
| `NotDataActions` | Actions excluded from DataActions |
| `AssignableScopes` | Where the role can be assigned |

## Built-in Roles

### Owner

Full control over all resources, including role assignment management.

```json
{
  "Name": "Owner",
  "Id": "1301f8d4-3bea-4880-945f-315dbd2ddb46",
  "Description": "Full access to manage all resources, including the ability to assign roles.",
  "Actions": ["*"],
  "NotActions": [],
  "AssignableScopes": ["/"]
}
```

### Contributor

Manage all resources except role assignments.

```json
{
  "Name": "Contributor",
  "Id": "e459c3a6-6b93-4062-85b3-fffc9fb253df",
  "Description": "Manage everything except access to resources.",
  "Actions": ["*"],
  "NotActions": [
    "FoundationaLLM.Authorization/*/delete",
    "FoundationaLLM.Authorization/*/write"
  ],
  "AssignableScopes": ["/"]
}
```

### Reader

Read-only access to all resources.

```json
{
  "Name": "Reader",
  "Id": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
  "Description": "Read-only access to all resources.",
  "Actions": ["*/read"],
  "NotActions": [],
  "AssignableScopes": ["/"]
}
```

### User Access Administrator

Manage role assignments only.

```json
{
  "Name": "User Access Administrator",
  "Id": "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
  "Description": "Manage user access to resources.",
  "Actions": [
    "FoundationaLLM.Authorization/roleAssignments/read",
    "FoundationaLLM.Authorization/roleAssignments/write",
    "FoundationaLLM.Authorization/roleAssignments/delete"
  ],
  "NotActions": [],
  "AssignableScopes": ["/"]
}
```

## Action Format

Actions follow this pattern:

```
FoundationaLLM.{ProviderName}/{resourceType}/{action}
```

### Examples

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Agent/agents/read` | Read agents |
| `FoundationaLLM.Agent/agents/write` | Create/update agents |
| `FoundationaLLM.Agent/agents/delete` | Delete agents |
| `FoundationaLLM.Prompt/prompts/*` | All prompt actions |
| `*/read` | Read all resources |
| `*` | All actions |

### Wildcards

| Wildcard | Meaning |
|----------|---------|
| `*` | All actions on all resources |
| `*/read` | Read all resource types |
| `FoundationaLLM.Agent/*` | All Agent provider actions |
| `FoundationaLLM.Agent/agents/*` | All actions on agents |

## Control vs Data Plane

### Control Plane Actions

Specified in `Actions` and `NotActions`. Examples:

- Create/update/delete resources
- Manage configurations
- Manage role assignments

### Data Plane Actions

Specified in `DataActions` and `NotDataActions`. Examples:

- Read resource content
- Execute operations
- Access data

> **Important:** Control plane access is NOT inherited to data plane. Having `FoundationaLLM.Agent/agents/write` does not grant `FoundationaLLM.Agent/agents/read`.

## Listing Role Definitions

### Via Management API

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions
Authorization: Bearer <token>
```

### Via Azure CLI

```bash
token=$(az account get-access-token \
  --scope api://FoundationaLLM-Management/Data.Manage \
  --query accessToken -o tsv)

curl -H "Authorization: Bearer $token" \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions"
```

## Role Selection Guide

| Use Case | Recommended Role |
|----------|------------------|
| Full administration | Owner |
| Resource management only | Contributor |
| View resources only | Reader |
| Manage access only | User Access Administrator |
| Agent users | Reader (on specific agents) |

## Related Topics

- [Role Assignments](role-assignments.md)
- [Scope](scope.md)
- [Role Management](role-management.md)
