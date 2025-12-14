# Core API

The Core API is the primary entry point for user-facing interactions with FoundationaLLM. It handles chat completions, session management, file uploads, and agent queries.

## Overview

The Core API provides:

- **Completions**: Synchronous and asynchronous chat/completion endpoints
- **Sessions**: Conversation management (create, list, delete)
- **Agents**: List available agents for the current user
- **Attachments**: File upload capabilities
- **Branding**: Portal branding configuration
- **Status**: Health and status endpoints

## Base URL

| Deployment Type | URL Pattern |
|-----------------|-------------|
| Azure Container Apps (ACA) | `https://{prefix}coreca.{region}.azurecontainerapps.io` |
| Azure Kubernetes Service (AKS) | `https://{cluster-fqdn}/core` |

See [Finding Your Core API URL](finding-core-api-url.md) for detailed instructions.

## Authentication

All Core API endpoints require authentication:

| Method | Header | Description |
|--------|--------|-------------|
| **Entra ID (Azure AD)** | `Authorization: Bearer <token>` | Standard user authentication |
| **Agent Access Token** | `X-AGENT-ACCESS-TOKEN: <token>` | Token-based agent access |

## API Endpoints

### Completions

#### Synchronous Completion

```http
POST /instances/{instanceId}/completions
Content-Type: application/json
```

Request body:

```json
{
  "user_prompt": "What are your capabilities?",
  "session_id": "optional-session-id",
  "agent_name": "agent-name",
  "settings": {
    "model_parameters": {
      "temperature": 0.4,
      "max_new_tokens": 1000
    },
    "agent_parameters": {
      "index_top_n": 5
    }
  }
}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_prompt` | string | Yes | The user's question or prompt |
| `session_id` | string | No | Session ID for conversation context |
| `agent_name` | string | No | Specific agent to use |
| `settings` | object | No | Override model and agent parameters |

Response:

```json
{
  "operation_id": "guid",
  "user_prompt": "What are your capabilities?",
  "completion": "I can help you with...",
  "citations": [],
  "user_prompt_embedding": [],
  "prompt_tokens": 150,
  "completion_tokens": 200
}
```

#### Asynchronous Completion

For long-running completions, use the async endpoint:

```http
POST /instances/{instanceId}/async-completions
Content-Type: application/json
```

Returns a `LongRunningOperation` object with operation status:

```json
{
  "operation_id": "guid",
  "status": "InProgress",
  "status_message": "Processing request"
}
```

### Sessions (Conversations)

#### List Sessions

```http
GET /instances/{instanceId}/sessions
```

#### Create Session

```http
POST /instances/{instanceId}/sessions
Content-Type: application/json

{
  "name": "New Conversation"
}
```

#### Get Session Messages

```http
GET /instances/{instanceId}/sessions/{sessionId}/messages
```

#### Delete Session

```http
DELETE /instances/{instanceId}/sessions/{sessionId}
```

#### Rate Message

```http
POST /instances/{instanceId}/sessions/{sessionId}/message/{messageId}/rate
Content-Type: application/json

{
  "rating": true,
  "comments": "Helpful response"
}
```

### Agents

#### List Available Agents

```http
GET /instances/{instanceId}/completions/agents
```

Returns agents the current user has access to:

```json
[
  {
    "resource": {
      "name": "my-agent",
      "display_name": "My Agent",
      "description": "Agent description"
    },
    "roles": ["Owner"],
    "actions": ["read", "write", "delete"]
  }
]
```

### Status

#### Service Status

```http
GET /instances/{instanceId}/status
```

## Model Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `temperature` | float | Randomness (0.0-1.0). Lower = more deterministic |
| `top_p` | float | Nucleus sampling probability |
| `top_k` | int | Top-k filtering |
| `max_new_tokens` | int | Maximum tokens in response |
| `deployment_name` | string | Specific model deployment |
| `do_sample` | bool | Enable sampling vs greedy decoding |

## Agent Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `index_filter_expression` | string | Search filter for index retriever |
| `index_top_n` | int | Number of search results for context |

## Error Responses

| Status | Description |
|--------|-------------|
| 400 | Bad request (invalid parameters) |
| 401 | Unauthorized (invalid/missing token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Resource not found |
| 429 | Rate limit exceeded (quota) |
| 500 | Internal server error |

Error response body:

```json
{
  "error": {
    "code": "QuotaExceeded",
    "message": "Rate limit exceeded. Try again in 60 seconds."
  }
}
```

## Swagger Documentation

Access the interactive API documentation:

- **ACA Deployment**: `https://{core-api-url}/swagger/`
- **AKS Deployment**: `https://{aks-url}/core/swagger/v1/swagger.json`

## Related Topics

- [API Reference](api-reference.md)
- [Directly Calling Core API](directly-calling-core-api.md)
- [Finding Your Core API URL](finding-core-api-url.md)
- [Standard Deployment Local API Access](standard-deployment-local-api-access.md)
- [.NET SDK](../../sdks/dotnet/index.md)
