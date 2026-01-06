# Core API Reference

Complete endpoint reference for the FoundationaLLM Core API, designed for developers integrating FoundationaLLM into applications.

## Developer Overview

The Core API is the primary interface for building applications that interact with FoundationaLLM agents. Use this API to:

- **Send prompts to agents** and receive AI-generated responses
- **Manage conversations** (sessions) and message history
- **Upload files** for agent analysis
- **List available agents** the user has access to
- **Check service status** and configuration

### Common Integration Patterns

| Pattern | Use Case | Key Endpoints |
|---------|----------|---------------|
| **Chatbot Integration** | Embed AI chat in your app | `POST /completions`, `POST /sessions` |
| **Batch Processing** | Process many requests | `POST /async-completions` |
| **Document Analysis** | Analyze uploaded files | `POST /files/upload`, `POST /completions` |
| **Custom UI** | Build custom chat interface | All session and completion endpoints |

## Base URL

```
https://{core-api-url}/instances/{instanceId}
```

| Variable | Description | Where to Find |
|----------|-------------|---------------|
| `{core-api-url}` | Core API endpoint | App Config: `FoundationaLLM:APIs:CoreAPI:APIUrl` |
| `{instanceId}` | FoundationaLLM instance ID | App Config: `FoundationaLLM:Instance:Id` |

---

## Authentication

All Core API endpoints require authentication. FoundationaLLM supports two authentication methods:

### Method 1: Entra ID Bearer Token (Recommended)

Use Microsoft Entra ID (Azure AD) JWT tokens for authenticated user access. This is the standard approach for applications where users sign in.

**Header:**
```
Authorization: Bearer <jwt-token>
```

**Getting a token with Azure CLI:**

```bash
# Login to Azure
az login

# Get token for Core API (replace with your client ID)
TOKEN=$(az account get-access-token \
  --resource api://{core-api-client-id} \
  --query accessToken -o tsv)
```

**Example curl request with Bearer token:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/completions/agents" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json"
```

### Method 2: Agent Access Token

Use Agent Access Tokens for scenarios where Entra ID authentication isn't practical, such as:

- Public-facing applications
- Embedded widgets
- Automated systems without user context

**Header:**
```
X-AGENT-ACCESS-TOKEN: <agent-token>
```

**Example curl request with Agent Access Token:**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/completions" \
  -H "X-AGENT-ACCESS-TOKEN: your-agent-access-token-here" \
  -H "Content-Type: application/json" \
  -d '{"user_prompt": "What can you help me with?"}'
```

### Authentication Comparison

| Feature | Entra ID Bearer Token | Agent Access Token |
|---------|----------------------|-------------------|
| **User Identity** | Tracks actual user | Uses agent's virtual identity |
| **Setup** | Requires Entra ID app registration | Created in Management Portal |
| **Token Lifetime** | Short-lived (typically 1 hour) | Configurable expiration |
| **Multi-agent** | Access all permitted agents | Bound to single agent |
| **Audit Trail** | Full user attribution | Agent-level attribution |
| **Best For** | User-authenticated apps | Public integrations |

### Creating Agent Access Tokens

1. Open the Management Portal
2. Navigate to the agent's edit page
3. Scroll to the **Security** section
4. Click **Create Access Token**
5. Set description and expiration date
6. Copy the generated token immediately

See [Agent Access Tokens](../../../management-portal/reference/concepts/agent-access-tokens.md) for detailed setup instructions.

---

## Completions Endpoints

### POST /completions

Request a synchronous completion from an agent. The API waits for the full response before returning.

**curl Example (Bearer Token):**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/completions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "user_prompt": "What is FoundationaLLM?",
    "agent_name": "knowledge-agent"
  }'
```

**curl Example (Agent Access Token):**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/completions" \
  -H "X-AGENT-ACCESS-TOKEN: your-token-here" \
  -H "Content-Type: application/json" \
  -d '{
    "user_prompt": "What is FoundationaLLM?"
  }'
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
| `user_prompt` | string | **Yes** | User's question or instruction |
| `session_id` | string | No | Conversation session for context continuity |
| `agent_name` | string | No | Specific agent to use (required for Bearer auth if multiple agents) |
| `attachments` | array | No | File attachment references from uploads |
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

| Status | Description | Example |
|--------|-------------|---------|
| 400 | Invalid request body | Missing required field `user_prompt` |
| 401 | Authentication required | Invalid or expired token |
| 403 | Access denied | No permission for specified agent |
| 429 | Quota exceeded | Rate limit reached |

---

### POST /async-completions

Start an asynchronous completion operation for long-running requests. Returns immediately with an operation ID.

**curl Example:**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/async-completions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "user_prompt": "Analyze this complex document and provide a detailed summary...",
    "agent_name": "document-agent"
  }'
```

**Response (202 Accepted):**

```json
{
  "operation_id": "abc123-def456",
  "status": "InProgress",
  "status_message": "Operation started",
  "result": null
}
```

Use the `operation_id` to poll for completion status.

---

### GET /completions/agents

List agents available to the current user.

**curl Example:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/completions/agents" \
  -H "Authorization: Bearer $TOKEN"
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

Sessions maintain conversation context across multiple messages.

### GET /sessions

List all conversations for the current user.

**curl Example:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/sessions" \
  -H "Authorization: Bearer $TOKEN"
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

**curl Example (Bearer Token):**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/sessions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "New Conversation"}'
```

**curl Example (Agent Access Token):**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/sessions" \
  -H "X-AGENT-ACCESS-TOKEN: your-token-here" \
  -H "Content-Type: application/json" \
  -d '{"name": "New Conversation"}'
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

**curl Example:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/sessions/{sessionId}/messages" \
  -H "Authorization: Bearer $TOKEN"
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

### DELETE /sessions/{sessionId}

Delete a conversation and all its messages.

**curl Example:**

```bash
curl -X DELETE "https://{core-api-url}/instances/{instanceId}/sessions/{sessionId}" \
  -H "Authorization: Bearer $TOKEN"
```

**Response (200 OK):** Empty body

---

### POST /sessions/{sessionId}/message/{messageId}/rate

Rate an assistant response (thumbs up/down).

**curl Example:**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/sessions/{sessionId}/message/{messageId}/rate" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": true,
    "comments": "Very helpful response"
  }'
```

---

## Files Endpoints

### POST /files/upload

Upload a file attachment for agent analysis.

**curl Example:**

```bash
curl -X POST "https://{core-api-url}/instances/{instanceId}/files/upload" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@/path/to/document.pdf" \
  -F "agent_name=knowledge-agent" \
  -F "session_id=session-guid"
```

**Response (200 OK):**

```json
{
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Attachment/attachments/file-guid",
  "display_name": "document.pdf",
  "content_type": "application/pdf"
}
```

---

## Status Endpoints

### GET /status

Get Core API service status.

**curl Example:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/status" \
  -H "Authorization: Bearer $TOKEN"
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

**curl Example:**

```bash
curl -X GET "https://{core-api-url}/instances/{instanceId}/branding" \
  -H "Authorization: Bearer $TOKEN"
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

## Rate Limiting and Quotas

The Core API enforces quota limits to prevent abuse and ensure fair usage.

### Quota Response

When quota is exceeded:

**Response (429 Too Many Requests):**

```json
{
  "quota_exceeded": true,
  "quota_name": "AgentCompletionRequestRate",
  "retry_after_seconds": 60,
  "message": "Rate limit exceeded. Try again later."
}
```

### Handling Rate Limits in Code

```bash
# Example retry logic with curl
response=$(curl -s -w "\n%{http_code}" -X POST \
  "https://{core-api-url}/instances/{instanceId}/completions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"user_prompt": "Hello"}')

http_code=$(echo "$response" | tail -1)
if [ "$http_code" = "429" ]; then
  echo "Rate limited. Waiting 60 seconds..."
  sleep 60
  # Retry request
fi
```

---

## Validation Rules

### Request Validation

| Field | Validation | Error Message |
|-------|------------|---------------|
| `user_prompt` | Required, non-empty | "user_prompt is required" |
| `session_id` | Valid GUID format | "Invalid session_id format" |
| `agent_name` | Must exist and be accessible | "Agent not found or access denied" |
| `temperature` | 0.0 to 1.0 | "temperature must be between 0 and 1" |
| `max_new_tokens` | Positive integer | "max_new_tokens must be positive" |

### Common Validation Errors

```json
{
  "error": {
    "code": "ValidationError",
    "message": "One or more validation errors occurred",
    "details": [
      {
        "field": "user_prompt",
        "message": "The user_prompt field is required."
      }
    ]
  }
}
```

---

## Complete Workflow Example

Here's a complete example of creating a session, sending a message, and retrieving the response:

```bash
# Set variables
CORE_API="https://your-core-api.azurecontainerapps.io"
INSTANCE_ID="your-instance-guid"
TOKEN=$(az account get-access-token --resource api://your-client-id --query accessToken -o tsv)

# 1. Create a new session
SESSION=$(curl -s -X POST "$CORE_API/instances/$INSTANCE_ID/sessions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "API Test Conversation"}')

SESSION_ID=$(echo $SESSION | jq -r '.id')
echo "Created session: $SESSION_ID"

# 2. Send a message
RESPONSE=$(curl -s -X POST "$CORE_API/instances/$INSTANCE_ID/completions" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"user_prompt\": \"What is FoundationaLLM?\",
    \"session_id\": \"$SESSION_ID\",
    \"agent_name\": \"knowledge-agent\"
  }")

echo "Response: $(echo $RESPONSE | jq -r '.completion')"

# 3. Retrieve conversation history
MESSAGES=$(curl -s -X GET "$CORE_API/instances/$INSTANCE_ID/sessions/$SESSION_ID/messages" \
  -H "Authorization: Bearer $TOKEN")

echo "Messages in session: $(echo $MESSAGES | jq length)"
```

---

## Related Topics

- [Core API Overview](index.md)
- [Directly Calling Core API](directly-calling-core-api.md)
- [Agent Access Tokens](../../../management-portal/reference/concepts/agent-access-tokens.md)
- [Quotas Reference](../../../management-portal/reference/concepts/quotas.md)
- [.NET SDK](../../sdks/dotnet/index.md)
- [Python SDK](../../sdks/python/index.md)
