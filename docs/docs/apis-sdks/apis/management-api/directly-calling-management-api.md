# Directly Calling the Management API

This guide provides step-by-step instructions for configuring Postman and making direct calls to the FoundationaLLM Management API.

## Overview

The Management API enables programmatic management of FoundationaLLM resources:

- Agents, workflows, and tools
- Prompts
- Data sources and pipelines
- AI models and API endpoints
- Branding and configuration
- Role assignments

## Prerequisites

Before calling the Management API, you need:

1. **Management API URL** - From App Config: `FoundationaLLM:APIs:ManagementAPI:APIUrl`
2. **Instance ID** - From App Config: `FoundationaLLM:Instance:Id`
3. **Entra ID App Registration** - Management Portal client app

## Using Postman

### Install Postman

Download and install [Postman](https://www.getpostman.com/).

### Import the Collection

Click the button below to fork the official FoundationaLLM Management API collection:

[<img src="https://run.pstmn.io/button.svg" alt="Run In Postman" style="width: 128px; height: 32px;">](https://app.getpostman.com/run-collection/269456-839af3bb-2841-40d7-bb24-6409e9ef835e?action=collection%2Ffork&source=rip_markdown&collection-url=entityId%3D269456-839af3bb-2841-40d7-bb24-6409e9ef835e%26entityType%3Dcollection%26workspaceId%3D0d6298a2-c3cd-4530-900c-030ed0ae6dfa)

### Configure Variables

1. Select the **FoundationaLLM.Management.API** collection
2. Click the **Variables** tab
3. Update these **Current value** fields:

| Variable | Description | Where to Find |
|----------|-------------|---------------|
| `baseUrl` | Management API URL | App Config: `FoundationaLLM:APIs:ManagementAPI:APIUrl` |
| `instanceId` | FoundationaLLM instance GUID | App Config: `FoundationaLLM:Instance:Id` |
| `tenantId` | Azure AD tenant ID | Entra ID portal |
| `appClientId` | Management Portal client ID | Entra ID app registration |
| `appScope` | API scope | Entra ID app registration |

4. Click **Save**

### Configure Authentication

> **Important:** Add `https://oauth.pstmn.io/v1/callback` as a Redirect URI in your Management Portal Entra ID app registration.

1. Select the **Authorization** tab in the collection
2. Verify the OAuth 2.0 settings use the collection variables
3. Scroll down and click **Get New Access Token**
4. Complete the login flow
5. Click **Use Token**
6. Click **Save**

### Make Your First Request

1. Expand the collection and find **Get Agents**
2. Click **Send**
3. Verify a 200 OK response

## Using curl

### Get Authentication Token

```bash
# Login to Azure
az login

# Get token for Management API
TOKEN=$(az account get-access-token \
  --resource api://{management-api-client-id} \
  --query accessToken -o tsv)
```

### List Agents

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Specific Agent

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent" \
  -H "Authorization: Bearer $TOKEN"
```

### Create Agent

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/new-agent" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "agent",
    "name": "new-agent",
    "display_name": "New Agent",
    "description": "A new agent",
    "inline_context": false,
    "conversation_history_settings": {
      "enabled": true,
      "max_history": 5
    }
  }'
```

### Delete Agent

```bash
curl -X DELETE \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent" \
  -H "Authorization: Bearer $TOKEN"
```

### Purge Agent (Permanent Delete)

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent/purge" \
  -H "Authorization: Bearer $TOKEN"
```

## Common Operations

### Managing Prompts

**List Prompts:**

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts" \
  -H "Authorization: Bearer $TOKEN"
```

**Create Prompt:**

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/system-prompt" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "prompt",
    "name": "system-prompt",
    "display_name": "System Prompt",
    "category": "AgentWorkflow",
    "prefix": "You are a helpful assistant specialized in..."
  }'
```

### Managing Data Sources

**List Data Sources:**

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources" \
  -H "Authorization: Bearer $TOKEN"
```

**Create Azure Data Lake Source:**

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources/my-storage" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "data-source",
    "name": "my-storage",
    "display_name": "My Azure Storage",
    "data_source_type": "AzureDataLake",
    "configuration": {
      "authentication_type": "ManagedIdentity",
      "endpoint": "https://mystorageaccount.blob.core.windows.net"
    }
  }'
```

### Managing Branding

**Get Branding Settings:**

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations" \
  -H "Authorization: Bearer $TOKEN"
```

**Update Branding:**

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/Branding-CompanyName" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "app-configuration",
    "name": "Branding-CompanyName",
    "key": "FoundationaLLM:Branding:CompanyName",
    "value": "My Company"
  }'
```

### Managing Role Assignments

**List Role Assignments:**

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments" \
  -H "Authorization: Bearer $TOKEN"
```

**Create Role Assignment:**

```bash
curl -X POST \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/new-assignment" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "role-assignment",
    "name": "new-assignment",
    "principal_id": "user-guid",
    "principal_type": "User",
    "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
    "scope": "/instances/{instanceId}"
  }'
```

## Swagger UI

For interactive API exploration:

| Deployment | URL |
|------------|-----|
| ACA | `https://{management-api-url}/swagger/` |
| AKS | `https://{aks-url}/management/swagger/v1/swagger.json` |

## Error Handling

| Status | Meaning | Action |
|--------|---------|--------|
| 400 | Invalid request | Check JSON format and required fields |
| 401 | Unauthorized | Refresh authentication token |
| 403 | Forbidden | Check role permissions |
| 404 | Not found | Verify resource path and name |
| 409 | Conflict | Resource already exists |
| 500 | Server error | Check API logs |

## Tips

1. **Use purge carefully** - Purged resources cannot be recovered
2. **Check permissions** - Some operations require Owner or Contributor role
3. **Validate JSON** - API returns 400 for malformed requests
4. **Use instance ID** - All resource paths require the instance ID

## Related Topics

- [Management API Overview](index.md)
- [API Reference](api-reference.md)
- [Resource Providers Overview](resource-providers-overview.md)
- [.NET SDK](../../sdks/dotnet/index.md)
