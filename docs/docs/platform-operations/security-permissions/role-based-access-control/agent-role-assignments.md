# Automating Agent Role Assignments

This guide covers programmatic management of role assignments for agents using the Management API.

## Overview

Automate agent access control for:
- Bulk user provisioning
- Integration with identity management systems
- Scheduled access reviews
- Temporary access grants

## Prerequisites

### Authentication

```bash
az login
TOKEN=$(az account get-access-token \
  --scope api://FoundationaLLM-Management/Data.Manage \
  --query accessToken -o tsv)
```

### Required Role

The caller must have at the agent scope:
- **Owner** (`1301f8d4-3bea-4880-945f-315dbd2ddb46`), OR
- **User Access Administrator** (`fb8e0fd0-f7e2-4957-89d6-19f44f7d6618`)

### Get Management API URL

Find the URL in Management Portal under **Deployment Information**, or from your deployment configuration.

## Listing Agents

Get all agents in your instance:

```http
GET {managementApiUrl}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents
Authorization: Bearer {token}
```

### Response

```json
[
  {
    "resource": {
      "name": "sales-agent",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-agent",
      "display_name": "Sales Assistant",
      "description": "Helps sales team with customer inquiries"
    }
  },
  {
    "resource": {
      "name": "support-agent",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/support-agent",
      "display_name": "Support Bot",
      "description": "Customer support automation"
    }
  }
]
```

Note the `object_id` for use in role assignments.

## Managing External Role Assignments

Use the external role assignments endpoint for bulk operations:

```http
POST {managementApiUrl}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}/externalRoleAssignments
Authorization: Bearer {token}
Content-Type: application/json
```

### Request Body

```json
{
  "roleAssignmentsToAdd": [
    {
      "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
      "identities": [
        "user1@contoso.com",
        "user2@contoso.com"
      ],
      "expirationDate": "2024-12-31T23:59:59Z"
    }
  ],
  "roleAssignmentsToRemove": [
    {
      "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
      "identities": [
        "former.employee@contoso.com"
      ]
    }
  ]
}
```

### Parameters

| Parameter | Description |
|-----------|-------------|
| `roleDefinitionId` | Role to assign/remove |
| `identities` | Array of user principal names (UPNs) |
| `expirationDate` | Optional expiration for added assignments |

### Common Role Definition IDs

| Role | ID |
|------|-------|
| Owner | `1301f8d4-3bea-4880-945f-315dbd2ddb46` |
| Contributor | `e459c3a6-6b93-4062-85b3-fffc9fb253df` |
| Reader | `00a53e72-f66e-4c03-8f81-7e885fd2eb35` |

## Examples

### Grant Reader Access to Multiple Users

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-agent/externalRoleAssignments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleAssignmentsToAdd": [
      {
        "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
        "identities": [
          "alice@contoso.com",
          "bob@contoso.com",
          "charlie@contoso.com"
        ]
      }
    ],
    "roleAssignmentsToRemove": []
  }'
```

### Grant Temporary Access

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/demo-agent/externalRoleAssignments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleAssignmentsToAdd": [
      {
        "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
        "identities": ["demo.user@contoso.com"],
        "expirationDate": "2024-03-15T17:00:00Z"
      }
    ],
    "roleAssignmentsToRemove": []
  }'
```

### Remove Access

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-agent/externalRoleAssignments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleAssignmentsToAdd": [],
    "roleAssignmentsToRemove": [
      {
        "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
        "identities": [
          "departed.employee@contoso.com"
        ]
      }
    ]
  }'
```

### Add and Remove in Single Call

```bash
curl -X POST \
  "https://<management-api>/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/support-agent/externalRoleAssignments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleAssignmentsToAdd": [
      {
        "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
        "identities": ["new.hire@contoso.com"]
      }
    ],
    "roleAssignmentsToRemove": [
      {
        "roleDefinitionId": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
        "identities": ["contractor@external.com"]
      }
    ]
  }'
```

## PowerShell Script Example

```powershell
$managementApiUrl = "https://<management-api>"
$instanceId = "your-instance-id"
$agentName = "sales-agent"

# Get token
$token = az account get-access-token `
  --scope api://FoundationaLLM-Management/Data.Manage `
  --query accessToken -o tsv

# Define assignments
$body = @{
    roleAssignmentsToAdd = @(
        @{
            roleDefinitionId = "00a53e72-f66e-4c03-8f81-7e885fd2eb35"
            identities = @("user1@contoso.com", "user2@contoso.com")
        }
    )
    roleAssignmentsToRemove = @()
} | ConvertTo-Json -Depth 3

# Make request
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$uri = "$managementApiUrl/instances/$instanceId/providers/FoundationaLLM.Agent/agents/$agentName/externalRoleAssignments"

Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $body
```

## Integration Scenarios

### HR System Integration

Sync agent access with HR onboarding/offboarding:

1. HR system triggers webhook on employee status change
2. Azure Function processes webhook
3. Function calls Management API to add/remove assignments
4. Access is immediately updated

### Access Review Automation

Schedule periodic access reviews:

1. Azure Logic App runs on schedule
2. Queries current assignments
3. Compares against approved access list
4. Removes unauthorized assignments
5. Sends report to administrators

## Related Topics

- [Role Definitions](role-definitions.md)
- [Role Assignments](role-assignments.md)
- [Role Management](role-management.md)
- [Management API Reference](../../../../apis-sdks/apis/management-api/api-reference.md)
