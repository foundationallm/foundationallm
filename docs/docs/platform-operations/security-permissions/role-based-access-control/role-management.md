# Managing Role Assignments

This guide covers managing role assignments via the Management Portal and API.

## Prerequisites

To manage role assignments, you must have:
- **User Access Administrator** role at the appropriate scope, OR
- **Owner** role

Role ID: `/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618`

## Management Portal

### Accessing Role Management

**Instance-Level:**
1. Navigate to Management Portal
2. Select **Security** > **Instance Access Control**

**Resource-Level:**
1. Navigate to the resource (e.g., Agent)
2. Click **Access Control** button

### Viewing Assignments

The role assignment list shows:

| Column | Description |
|--------|-------------|
| **Name** | Role assignment name (expandable for details) |
| **Type** | User or Group |
| **Scope** | Instance or resource-level |
| **Delete** | Delete action |

Click the expand arrow to view full details.

### Creating Assignments

1. Click **Add Role Assignment**
2. Click **Browse** to search for identities
3. Select user, group, or service principal
4. The form auto-populates:
   - Principal Type
   - Principal Name
   - Principal Email
   - Principal ID
5. Select **Role** from dropdown
6. Add **Description** (optional)
7. Click **Save**

### Deleting Assignments

1. Find the assignment in the list
2. Click the **Delete** icon
3. Confirm deletion

> **Note:** You cannot edit assignments. Delete and recreate to make changes.

## Management API

### Authentication

All API calls require:
- HTTPS
- Bearer token with `Data.Manage` scope
- User Access Administrator or Owner role

Get token:
```bash
az login
az account get-access-token --scope api://FoundationaLLM-Management/Data.Manage
```

### Role Definition Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions` | List all role definitions |

### Role Assignment Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/.../roleAssignments/filter` | Query assignments by scope |
| POST | `/.../roleAssignments/{name}` | Create assignment |
| DELETE | `/.../roleAssignments/{name}` | Delete assignment |

### Identity Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/instances/{instanceId}/identity/users/retrieve` | Search users |
| POST | `/instances/{instanceId}/identity/groups/retrieve` | Search groups |
| POST | `/instances/{instanceId}/identity/objects/retrievebyids` | Get objects by ID |

### API Examples

#### List Role Definitions

```bash
curl -X GET \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions" \
  -H "Authorization: Bearer $TOKEN"
```

#### Query Assignments

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"scope":"/instances/{instanceId}"}'
```

#### Create Assignment

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/{assignmentName}" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "unique-assignment-guid",
    "description": "Platform contributor access",
    "principal_id": "user-or-group-object-id",
    "principal_type": "User",
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/e459c3a6-6b93-4062-85b3-fffc9fb253df",
    "scope": "/instances/{instanceId}"
  }'
```

#### Delete Assignment

```bash
curl -X DELETE \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/{assignmentName}" \
  -H "Authorization: Bearer $TOKEN"
```

#### Search Users

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/identity/users/retrieve" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "john",
    "ids": [],
    "page_number": 1,
    "page_size": 20
  }'
```

#### Search Groups

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/identity/groups/retrieve" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "sales",
    "ids": [],
    "page_number": 1,
    "page_size": 20
  }'
```

## Data Store

Role assignments are stored in a dedicated data store:
- Isolated from main platform data
- Uses Azure Cosmos DB or Data Lake Storage Gen2
- Enables independent scaling
- Supports compliance auditing

## Auditing

All role assignment changes are audited:
- Creation timestamp
- Modification history
- User who made changes
- Previous values

View audit history in Management Portal or query via API.

## Related Topics

- [Role Definitions](role-definitions.md)
- [Role Assignments](role-assignments.md)
- [Scope](scope.md)
- [Agent Role Assignments](agent-role-assignments.md)
