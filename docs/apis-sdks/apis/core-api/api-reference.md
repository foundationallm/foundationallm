# Core API Reference

Complete endpoint reference for the FoundationaLLM Core API.

## Base URL

```
https://{core-api-url}/instances/{instanceId}
```

## Authentication

All endpoints require authentication via one of:

| Scheme | Header | Description |
|--------|--------|-------------|
| Bearer | `Authorization: Bearer <jwt>` | Entra ID JWT token |
| Agent Token | `X-AGENT-ACCESS-TOKEN: <token>` | Agent access token |

---

## Completions Endpoints

### POST /completions

Request a synchronous completion from an agent.

**Request:**

```http
POST /instances/{instanceId}/completions
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**

```json
{
  "operation_id": "optional-guid",
  "user_prompt": "What can you help me with?",
  "session_id": "conversation-session-id",
  "agent_name": "my-agent",
  "attachments": [],
  "settings": {
    "model_parameters": {
      "temperature": 0.7,
      "max_new_tokens": 2000,
      "top_p": 0.95
    },
    "agent_parameters": {
      "index_top_n": 10,
      "index_filter_expression": "category eq 'documentation'"
    }
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `operation_id` | string | No | Custom operation ID (auto-generated if not provided) |
| `user_prompt` | string | Yes | User's question or instruction |
| `session_id` | string | No | Conversation session for context |
| `agent_name` | string | No | Specific agent to use |
| `attachments` | array | No | File attachment references |
| `settings` | object | No | Runtime parameter overrides |

**Response (200 OK):**

```json
{
  "operation_id": "abc123-def456",
  "user_prompt": "What can you help me with?",
  "completion": "I can assist you with...",
  "content": [],
  "citations": [],
  "analysis_results": [],
  "user_prompt_embedding": [],
  "prompt_tokens": 125,
  "completion_tokens": 350,
  "total_tokens": 475,
  "total_cost": 0.0024
}
```

**Error Responses:**

| Status | Description |
|--------|-------------|
| 400 | Invalid request body |
| 401 | Authentication required |
| 429 | Quota exceeded |

---

### POST /async-completions

Start an asynchronous completion operation.

**Request:**

```http
POST /instances/{instanceId}/async-completions
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:** Same as `/completions`

**Response (202 Accepted):**

```json
{
  "operation_id": "abc123-def456",
  "status": "InProgress",
  "status_message": "Operation started",
  "result": null
}
```

---

### GET /completions/agents

List agents available to the current user.

**Request:**

```http
GET /instances/{instanceId}/completions/agents
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
[
  {
    "resource": {
      "type": "agent",
      "name": "knowledge-agent",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/knowledge-agent",
      "display_name": "Knowledge Agent",
      "description": "Answers questions from documentation"
    },
    "roles": ["Reader"],
    "actions": ["read"]
  }
]
```

---

## Sessions Endpoints

### GET /sessions

List all conversations for the current user.

**Request:**

```http
GET /instances/{instanceId}/sessions
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
[
  {
    "id": "session-guid",
    "name": "My Conversation",
    "type": "Session",
    "upn": "user@example.com",
    "messages": [],
    "created_on": "2024-01-15T10:30:00Z",
    "updated_on": "2024-01-15T11:45:00Z"
  }
]
```

---

### POST /sessions

Create a new conversation session.

**Request:**

```http
POST /instances/{instanceId}/sessions
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**

```json
{
  "name": "New Conversation"
}
```

**Response (200 OK):**

```json
{
  "id": "new-session-guid",
  "name": "New Conversation",
  "type": "Session",
  "upn": "user@example.com",
  "messages": [],
  "created_on": "2024-01-15T12:00:00Z"
}
```

---

### GET /sessions/{sessionId}/messages

Retrieve messages for a conversation.

**Request:**

```http
GET /instances/{instanceId}/sessions/{sessionId}/messages
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
[
  {
    "id": "message-guid",
    "type": "Message",
    "session_id": "session-guid",
    "sender": "User",
    "sender_display_name": "John Doe",
    "text": "What is FoundationaLLM?",
    "tokens": 10,
    "timestamp": "2024-01-15T10:30:00Z"
  },
  {
    "id": "message-guid-2",
    "type": "Message",
    "session_id": "session-guid",
    "sender": "Assistant",
    "sender_display_name": "Knowledge Agent",
    "text": "FoundationaLLM is a platform for...",
    "tokens": 150,
    "timestamp": "2024-01-15T10:30:05Z"
  }
]
```

---

### GET /sessions/{sessionId}/messagescount

Get the count of messages in a conversation.

**Request:**

```http
GET /instances/{instanceId}/sessions/{sessionId}/messagescount
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
42
```

---

### POST /sessions/{sessionId}/update

Update conversation properties.

**Request:**

```http
POST /instances/{instanceId}/sessions/{sessionId}/update
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**

```json
{
  "name": "Renamed Conversation"
}
```

---

### DELETE /sessions/{sessionId}

Delete a conversation and all its messages.

**Request:**

```http
DELETE /instances/{instanceId}/sessions/{sessionId}
Authorization: Bearer <token>
```

**Response (200 OK):** Empty body

---

### POST /sessions/{sessionId}/message/{messageId}/rate

Rate an assistant response.

**Request:**

```http
POST /instances/{instanceId}/sessions/{sessionId}/message/{messageId}/rate
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**

```json
{
  "rating": true,
  "comments": "Very helpful response"
}
```

---

### GET /sessions/{sessionId}/completionprompts/{completionPromptId}

Retrieve the compiled prompt for a specific completion.

**Request:**

```http
GET /instances/{instanceId}/sessions/{sessionId}/completionprompts/{completionPromptId}
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
{
  "id": "prompt-guid",
  "session_id": "session-guid",
  "prefix": "You are a helpful assistant...",
  "suffix": "",
  "prompt": "Full compiled prompt..."
}
```

---

## Status Endpoints

### GET /status

Get Core API service status.

**Request:**

```http
GET /instances/{instanceId}/status
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
{
  "status": "ready",
  "name": "CoreAPI",
  "version": "1.0.0",
  "instance_id": "instance-guid"
}
```

---

## Branding Endpoints

### GET /branding

Get portal branding configuration.

**Request:**

```http
GET /instances/{instanceId}/branding
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
{
  "company_name": "FoundationaLLM",
  "page_title": "FoundationaLLM User Portal",
  "logo_url": "foundationallm-logo-white.svg",
  "primary_color": "#131833",
  "primary_text_color": "#fff"
}
```

---

## Files Endpoints

### POST /files/upload

Upload a file attachment.

**Request:**

```http
POST /instances/{instanceId}/files/upload
Content-Type: multipart/form-data
Authorization: Bearer <token>
```

**Form Fields:**

| Field | Type | Description |
|-------|------|-------------|
| `file` | file | The file to upload |
| `agent_name` | string | Agent for the attachment |
| `session_id` | string | Conversation session |

**Response (200 OK):**

```json
{
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Attachment/attachments/file-guid",
  "display_name": "document.pdf",
  "content_type": "application/pdf"
}
```

---

## Rate Limiting

The Core API enforces quota limits. When exceeded:

**Response (429 Too Many Requests):**

```json
{
  "quota_exceeded": true,
  "quota_name": "AgentCompletionRequestRate",
  "retry_after_seconds": 60,
  "message": "Rate limit exceeded. Try again later."
}
```

---

## Related Topics

- [Core API Overview](index.md)
- [Directly Calling Core API](directly-calling-core-api.md)
- [.NET SDK - Core Client](../../sdks/dotnet/index.md)
