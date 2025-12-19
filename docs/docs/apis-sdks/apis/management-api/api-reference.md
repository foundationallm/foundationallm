# Management API Reference

Complete endpoint reference for the FoundationaLLM Management API.

## Base URL

```
https://{management-api-url}/instances/{instanceId}
```

## Authentication

All endpoints require Entra ID bearer token:

```http
Authorization: Bearer <jwt-token>
```

---

## Resource Operations

All resource providers follow a consistent pattern:

```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

### GET - List Resources

```http
GET /instances/{instanceId}/providers/{provider}/{resourceType}
```

**Response (200 OK):**

```json
[
  {
    "resource": {
      "type": "agent",
      "name": "my-agent",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent",
      "display_name": "My Agent",
      "description": "Agent description"
    },
    "roles": ["Owner"],
    "actions": ["read", "write", "delete"]
  }
]
```

### GET - Get Resource

```http
GET /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
```

**Response (200 OK):**

```json
{
  "resource": {
    "type": "agent",
    "name": "my-agent",
    "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent",
    // Full resource definition
  },
  "roles": ["Owner"],
  "actions": ["read", "write", "delete"]
}
```

### POST - Create/Update Resource

```http
POST /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
Content-Type: application/json

{
  // Resource definition
}
```

**Response (200 OK):**

```json
{
  "object_id": "/instances/{instanceId}/providers/{provider}/{resourceType}/{name}"
}
```

### DELETE - Delete Resource

```http
DELETE /instances/{instanceId}/providers/{provider}/{resourceType}/{name}
```

**Response (200 OK):**

```json
{
  "object_id": "/instances/{instanceId}/providers/{provider}/{resourceType}/{name}",
  "deleted": true
}
```

### POST - Purge Resource

Permanently removes a soft-deleted resource:

```http
POST /instances/{instanceId}/providers/{provider}/{resourceType}/{name}/purge
```

---

## Agent Resource Provider

**Provider:** `FoundationaLLM.Agent`

### Agents

#### List Agents

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Agent/agents
```

#### Get Agent

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

#### Create/Update Agent

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
Content-Type: application/json

{
  "type": "agent",
  "name": "my-agent",
  "display_name": "My Agent",
  "description": "Agent description",
  "inline_context": false,
  "conversation_history_settings": {
    "enabled": true,
    "max_history": 5
  },
  "gatekeeper_settings": {
    "use_system_setting": false,
    "options": ["ContentSafety"]
  },
  "workflow_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/workflows/my-workflow",
  "tool_object_ids": []
}
```

### Workflows

#### List Workflows

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Agent/workflows
```

#### Create/Update Workflow

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Agent/workflows/{workflowName}
Content-Type: application/json

{
  "type": "workflow",
  "name": "my-workflow",
  "workflow_type": "OpenAIAssistants",
  "workflow_host": "LangChain",
  "main_ai_model_object_id": "/instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/gpt-4o",
  "main_prompt_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/system-prompt"
}
```

### Tools

#### List Tools

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Agent/tools
```

#### Create/Update Tool

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Agent/tools/{toolName}
Content-Type: application/json

{
  "type": "tool",
  "name": "knowledge-search",
  "tool_type": "knowledge-search",
  "description": "Search knowledge base"
}
```

---

## Prompt Resource Provider

**Provider:** `FoundationaLLM.Prompt`

### List Prompts

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts
```

### Get Prompt

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/{promptName}
```

### Create/Update Prompt

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/{promptName}
Content-Type: application/json

{
  "type": "prompt",
  "name": "system-prompt",
  "display_name": "System Prompt",
  "description": "Main system prompt",
  "category": "AgentWorkflow",
  "prefix": "You are a helpful assistant...",
  "suffix": ""
}
```

---

## Data Source Resource Provider

**Provider:** `FoundationaLLM.DataSource`

### List Data Sources

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources
```

### Create/Update Data Source

```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources/{dataSourceName}
Content-Type: application/json

{
  "type": "data-source",
  "name": "azure-storage",
  "display_name": "Azure Storage",
  "data_source_type": "AzureDataLake",
  "configuration": {
    "connection_string_secret_name": "storage-connection",
    "containers": ["documents"]
  }
}
```

---

## Data Pipeline Resource Provider

**Provider:** `FoundationaLLM.DataPipeline`

See [Data Pipelines API](data-pipelines.md) for complete documentation.

### List Pipelines

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines
```

### Execute Pipeline

```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}/process
```

### List Pipeline Runs

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns
```

---

## AI Model Resource Provider

**Provider:** `FoundationaLLM.AIModel`

### List AI Models

```http
GET /instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels
```

### Create/Update AI Model

```http
POST /instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/{modelName}
Content-Type: application/json

{
  "type": "ai-model",
  "name": "gpt-4o",
  "display_name": "GPT-4o",
  "model_type": "completion",
  "deployment_name": "gpt-4o-deployment",
  "api_endpoint_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/azure-openai"
}
```

---

## Configuration Resource Provider

**Provider:** `FoundationaLLM.Configuration`

### App Configurations

#### List Configurations

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations
```

#### Update Configuration

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/{configName}
Content-Type: application/json

{
  "type": "app-configuration",
  "name": "Branding-CompanyName",
  "key": "FoundationaLLM:Branding:CompanyName",
  "value": "Contoso"
}
```

### API Endpoints

#### List API Endpoints

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations
```

---

## Authorization Resource Provider

**Provider:** `FoundationaLLM.Authorization`

### Role Assignments

#### List Role Assignments

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments
```

#### Create Role Assignment

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/{assignmentName}
Content-Type: application/json

{
  "type": "role-assignment",
  "name": "assignment-guid",
  "principal_id": "user-or-group-guid",
  "principal_type": "User",
  "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
  "scope": "/instances/{instanceId}"
}
```

### Role Definitions

#### List Role Definitions

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions
```

---

## Status Endpoints

### Service Status

```http
GET /status
```

**Response:**

```json
{
  "status": "ready",
  "name": "ManagementAPI",
  "version": "1.0.0",
  "instance_id": "instance-guid"
}
```

---

## Identity Endpoints

### Get Current User

```http
GET /instances/{instanceId}/identity
```

### Search Security Principals

```http
GET /instances/{instanceId}/identity/securityPrincipals?search={searchTerm}
```

---

## Error Responses

| Status | Code | Description |
|--------|------|-------------|
| 400 | BadRequest | Invalid request body |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Insufficient permissions |
| 404 | NotFound | Resource not found |
| 409 | Conflict | Resource already exists |
| 500 | InternalError | Server error |

**Error Response Format:**

```json
{
  "error": {
    "code": "ResourceNotFound",
    "message": "The resource 'my-agent' was not found in provider 'FoundationaLLM.Agent'."
  }
}
```

---

## Related Topics

- [Management API Overview](index.md)
- [Resource Providers Overview](resource-providers-overview.md)
- [Data Pipelines API](data-pipelines.md)
