# Resource Providers Overview

Resource providers are the core platform components that manage specific types of resources in FoundationaLLM. This pattern mirrors Azure Resource Manager, providing a consistent API for all resource operations.

## Overview

Each resource provider:

- Manages a specific category of resources
- Exposes a consistent REST API pattern
- Enforces authorization through RBAC
- Handles resource lifecycle (create, read, update, delete, purge)

## API Pattern

All resource providers follow this URL structure:

```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

| Component | Description | Example |
|-----------|-------------|---------|
| `instanceId` | FoundationaLLM instance GUID | `abc123-def456-...` |
| `providerName` | Resource provider namespace | `FoundationaLLM.Agent` |
| `resourceType` | Type of resource | `agents` |
| `resourceName` | Resource identifier | `my-agent` |

## Available Resource Providers

### FoundationaLLM.Agent

Manages AI agents, their workflows, and tools.

| Resource Type | Description |
|---------------|-------------|
| `agents` | Agent configurations |
| `workflows` | Orchestration workflows |
| `tools` | Agent tools (code interpreter, knowledge search, etc.) |
| `accessTokens` | Agent access tokens for unauthenticated access |

**Example Endpoints:**

```http
GET  /instances/{id}/providers/FoundationaLLM.Agent/agents
GET  /instances/{id}/providers/FoundationaLLM.Agent/agents/{name}
POST /instances/{id}/providers/FoundationaLLM.Agent/agents/{name}
DELETE /instances/{id}/providers/FoundationaLLM.Agent/agents/{name}
```

---

### FoundationaLLM.AIModel

Manages AI model configurations and deployments.

| Resource Type | Description |
|---------------|-------------|
| `aiModels` | AI model definitions (GPT-4, Claude, etc.) |

**AI Model Types:**

| Type | Description |
|------|-------------|
| `completion` | Text generation models |
| `embedding` | Embedding/vectorization models |
| `image` | Image generation models (DALL-E) |

---

### FoundationaLLM.Attachment

Manages file attachments uploaded through the portal.

| Resource Type | Description |
|---------------|-------------|
| `attachments` | User-uploaded files |

---

### FoundationaLLM.Authorization

Manages role-based access control.

| Resource Type | Description |
|---------------|-------------|
| `roleAssignments` | Bindings between principals, roles, and scopes |
| `roleDefinitions` | Available role definitions (Owner, Contributor, Reader, etc.) |

**Built-in Roles:**

| Role | ID |
|------|-----|
| Owner | `1301f8d4-3bea-4880-945f-315dbd2ddb46` |
| Contributor | `a9f0020f-6e3a-49bf-8d1d-35fd53058edf` |
| Reader | `00a53e72-f66e-4c03-8f81-7e885fd2eb35` |
| User Access Administrator | `fb8e0fd0-f7e2-4957-89d6-19f44f7d6618` |

---

### FoundationaLLM.Configuration

Manages platform configuration and API endpoints.

| Resource Type | Description |
|---------------|-------------|
| `appConfigurations` | App Configuration key-value settings |
| `apiEndpointConfigurations` | External API endpoint definitions |

**Common Configuration Categories:**

- Branding settings (`FoundationaLLM:Branding:*`)
- API endpoints (`FoundationaLLM:APIs:*`)
- Feature flags (`FoundationaLLM:Features:*`)

---

### FoundationaLLM.Conversation

Manages user conversations and messages.

| Resource Type | Description |
|---------------|-------------|
| `conversations` | Chat sessions/conversations |

> **Note:** Conversations are typically managed through the Core API. The Management API provides administrative access.

---

### FoundationaLLM.DataPipeline

Manages data ingestion and processing pipelines.

| Resource Type | Description |
|---------------|-------------|
| `dataPipelines` | Pipeline definitions |
| `dataPipelineRuns` | Pipeline execution history |

See [Data Pipelines API](data-pipelines.md) for detailed documentation.

---

### FoundationaLLM.DataSource

Manages connections to external data repositories.

| Resource Type | Description |
|---------------|-------------|
| `dataSources` | Data source configurations |

**Supported Data Source Types:**

| Type | Description |
|------|-------------|
| `AzureDataLake` | Azure Blob Storage / Data Lake Gen2 |
| `OneLake` | Microsoft Fabric OneLake |
| `SharePointOnline` | SharePoint document libraries |
| `AzureSQLDatabase` | Azure SQL Database |
| `Web` | Web page sources |

---

### FoundationaLLM.Plugin

Manages the plugin system for extensibility.

| Resource Type | Description |
|---------------|-------------|
| `pluginPackages` | Plugin package definitions |
| `plugins` | Individual plugin configurations |

**Plugin Types:**

| Type | Description |
|------|-------------|
| `AgentWorkflow` | Custom workflow implementations |
| `AgentTool` | Custom agent tools |
| `DataSource` | Custom data source connectors |
| `DataPipelineStage` | Custom pipeline processing stages |

---

### FoundationaLLM.Prompt

Manages reusable prompt templates.

| Resource Type | Description |
|---------------|-------------|
| `prompts` | Prompt template definitions |

**Prompt Categories:**

| Category | Use Case |
|----------|----------|
| `AgentWorkflow` | Agent system prompts |
| `AgentTool` | Tool-specific instructions |
| `DataPipeline` | Pipeline processing instructions |

---

### FoundationaLLM.Vector

Manages vector database configurations.

| Resource Type | Description |
|---------------|-------------|
| `vectorDatabases` | Vector database connection definitions |

---

## Resource Object IDs

Every resource has a unique object ID following this format:

```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

Object IDs are used for:
- Cross-resource references
- Authorization scopes
- Resource identification

**Example:**

```json
{
  "object_id": "/instances/abc123/providers/FoundationaLLM.Agent/agents/my-agent",
  "workflow_object_id": "/instances/abc123/providers/FoundationaLLM.Agent/workflows/my-workflow"
}
```

## Standard Operations

All resource providers support these operations:

| Operation | HTTP Method | Description |
|-----------|-------------|-------------|
| List | `GET /{resourceType}` | List all resources |
| Get | `GET /{resourceType}/{name}` | Get specific resource |
| Create/Update | `POST /{resourceType}/{name}` | Create or update resource |
| Delete | `DELETE /{resourceType}/{name}` | Soft delete resource |
| Purge | `POST /{resourceType}/{name}/purge` | Permanent delete |

## Response Format

All GET operations return resources with metadata:

```json
{
  "resource": {
    "type": "agent",
    "name": "my-agent",
    "object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/my-agent",
    // Resource-specific properties
  },
  "roles": ["Owner", "Contributor"],
  "actions": ["read", "write", "delete"]
}
```

| Field | Description |
|-------|-------------|
| `resource` | The resource definition |
| `roles` | User's assigned roles for this resource |
| `actions` | Actions the user can perform |

## Related Topics

- [Management API Overview](index.md)
- [API Reference](api-reference.md)
- [Permissions & Roles Reference](../../../management-portal/reference/permissions-roles.md)
