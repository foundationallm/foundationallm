# Management API

The Management API provides programmatic access to create, update, and manage FoundationaLLM resources. While the Core API handles user-facing chat interactions, the Management API handles administrative operations.

## Overview

The Management API enables:

- **Resource Management**: Create, read, update, delete agents, prompts, data sources, pipelines
- **Configuration**: Manage branding, app settings, API endpoints
- **Authorization**: Manage role assignments and access control
- **Monitoring**: Check API health and service status

## Comparison: Core API vs Management API

| Aspect | Core API | Management API |
|--------|----------|----------------|
| **Purpose** | User interactions | Administrative operations |
| **Primary Users** | End users, applications | Administrators, CI/CD pipelines |
| **Operations** | Chat completions, sessions | Resource CRUD, configuration |
| **Data Access** | Read-only resources | Full CRUD on resources |

## Base URL

| Deployment Type | URL Pattern |
|-----------------|-------------|
| Azure Container Apps (ACA) | `https://{prefix}managementca.{region}.azurecontainerapps.io` |
| Azure Kubernetes Service (AKS) | `https://{cluster-fqdn}/management` |

Find your URL in App Configuration: `FoundationaLLM:APIs:ManagementAPI:APIUrl`

## Authentication

All Management API endpoints require Entra ID authentication:

```http
Authorization: Bearer <entra-id-token>
```

## API Architecture

The Management API uses a resource provider pattern similar to Azure Resource Manager:

```
/instances/{instanceId}/providers/{resourceProvider}/{resourceType}/{resourceName}
```

### Resource Providers

| Provider | Description | Resource Types |
|----------|-------------|----------------|
| `FoundationaLLM.Agent` | Agent management | agents, workflows, tools |
| `FoundationaLLM.Prompt` | Prompt templates | prompts |
| `FoundationaLLM.DataSource` | Data connections | dataSources |
| `FoundationaLLM.DataPipeline` | Data pipelines | dataPipelines, dataPipelineRuns |
| `FoundationaLLM.AIModel` | AI model configs | aiModels |
| `FoundationaLLM.Configuration` | Platform settings | appConfigurations, apiEndpoints |
| `FoundationaLLM.Authorization` | Access control | roleAssignments, roleDefinitions |
| `FoundationaLLM.Plugin` | Plugin system | plugins, pluginPackages |

See [Resource Providers Overview](resource-providers-overview.md) for complete details.

## Common Operations

### List Resources

```http
GET /instances/{instanceId}/providers/{provider}/{resourceType}
```

Example - List all agents:

```bash
curl -X GET \
  "https://{management-api}/instances/{instanceId}/providers/FoundationaLLM.Agent/agents" \
  -H "Authorization: Bearer $TOKEN"
```

### Get Resource

```http
GET /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
```

### Create/Update Resource

```http
POST /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
Content-Type: application/json

{
  // resource definition
}
```

### Delete Resource

```http
DELETE /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
```

### Purge Resource (Permanent Delete)

```http
POST /instances/{instanceId}/providers/{provider}/{resourceType}/{name}/purge
```

## Access Methods

### 1. Direct REST Calls

Use tools like Postman, curl, or HTTP clients. See [Directly Calling Management API](directly-calling-management-api.md).

### 2. Management Portal

The Management Portal UI consumes the Management API internally. All portal operations translate to API calls.

### 3. .NET SDK

Use the `FoundationaLLM.Client.Management` NuGet package:

```csharp
var managementClient = new ManagementClient(apiUrl, credential, instanceId);
var agents = await managementClient.Agents.GetAgentsAsync();
```

See [.NET SDK Documentation](../../sdks/dotnet/index.md).

### 4. CLI (Coming Soon)

A command-line interface for Management API operations is planned.

## Key Features

### Resource Lifecycle

The Management API supports full resource lifecycle:

1. **Create**: Define new resources
2. **Read**: Retrieve resource configurations
3. **Update**: Modify existing resources
4. **Delete**: Soft delete (recoverable)
5. **Purge**: Permanent deletion

### Validation

Resources are validated on create/update:
- Required fields
- Name uniqueness
- Reference integrity
- Schema compliance

### Auditing

All operations are logged with:
- User identity
- Timestamp
- Operation type
- Resource affected

## Swagger Documentation

Access interactive API documentation:

| Deployment | URL |
|------------|-----|
| ACA | `https://{management-api-url}/swagger/` |
| AKS | `https://{aks-url}/management/swagger/v1/swagger.json` |

## Error Responses

| Status | Description |
|--------|-------------|
| 400 | Bad request (validation error) |
| 401 | Unauthorized |
| 403 | Forbidden (insufficient permissions) |
| 404 | Resource not found |
| 409 | Conflict (resource already exists) |
| 500 | Internal server error |

Error response format:

```json
{
  "error": {
    "code": "ResourceNotFound",
    "message": "The agent 'my-agent' was not found."
  }
}
```

## Related Topics

- [API Reference](api-reference.md)
- [Directly Calling Management API](directly-calling-management-api.md)
- [Resource Providers Overview](resource-providers-overview.md)
- [Data Pipelines API](data-pipelines.md)
- [.NET SDK](../../sdks/dotnet/index.md)
