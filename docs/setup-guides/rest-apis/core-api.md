# Core API

The Core API is the primary interface for developers to interact with FoundationaLLM agents programmatically. It provides endpoints for managing chat sessions, sending completions to agents, uploading files, and more.

## Overview

The Core API serves as the entry point for all user-facing interactions with FoundationaLLM. Key capabilities include:

- **Completions**: Send prompts to agents and receive responses
- **Sessions**: Manage chat sessions with conversation history
- **Attachments**: Upload files for agent analysis
- **Status**: Monitor API health and completion progress

### Architecture

```
Your Application --> Core API --> Orchestration --> LLM
                          |
                          v
                     Gatekeeper (optional)
                          |
                          v
                   Content Safety
                   Data Protection
```

For detailed architecture information, see [Directly Calling APIs](../../development/calling-apis/directly-calling-core-api.md).

## Base URL

The Core API URL depends on your deployment type:

| Deployment | URL Format |
|------------|------------|
| **Azure Container Apps (ACA)** | `https://[PREFIX]coreca.[REGION].azurecontainerapps.io` |
| **Azure Kubernetes Service (AKS)** | `https://[CLUSTER-FQDN]/core` |
| **Local Development** | `https://localhost:63279` |

**Finding Your Core API URL:**

1. Navigate to Azure App Configuration
2. Search for `FoundationaLLM:APIs:CoreAPI:APIUrl`
3. Copy the value

## API Versioning

All API requests should include the `api-version` query parameter:

```
?api-version=1.0
```

## Authentication

The Core API supports two authentication methods:

1. **Microsoft Entra ID (Azure AD) Bearer Tokens** - For user-authenticated and service-to-service scenarios
2. **Agent Access Tokens** - For public-facing applications and scenarios without user authentication

### Authentication Methods Comparison

| Feature | Entra ID Bearer Token | Agent Access Token |
|---------|----------------------|-------------------|
| **Use Case** | Enterprise apps, user-authenticated scenarios | Public apps, kiosks, embedded agents |
| **User Identity** | Full user identity from Entra ID | Virtual user identity per token |
| **Scope** | Access to all agents user has permission for | Access to single specific agent |
| **Token Lifetime** | Typically 1 hour (refreshable) | Configurable expiration date |
| **Setup Complexity** | Requires Entra ID app registration | Created per-agent in Management Portal |
| **User Login Required** | Yes (or service principal) | No |
| **Quota Tracking** | By user principal name | By token (shared across all users of token) |
| **Best For** | Internal enterprise applications | Customer-facing chatbots, public demos |

---

### Option 1: Microsoft Entra ID Bearer Token

Use Entra ID authentication for enterprise applications where users authenticate with their organizational credentials.

#### Obtaining an Access Token

##### Using Azure CLI

```bash
# Login to Azure
az login

# Get access token for the Core API
TOKEN=$(az account get-access-token \
  --resource "api://FoundationaLLM-Core" \
  --query accessToken \
  --output tsv)

echo $TOKEN
```

##### Using curl with Client Credentials

For service-to-service authentication:

```bash
# Set your tenant and app details
TENANT_ID="your-tenant-id"
CLIENT_ID="your-client-id"
CLIENT_SECRET="your-client-secret"
SCOPE="api://FoundationaLLM-Core/.default"

# Request token
TOKEN=$(curl -s -X POST \
  "https://login.microsoftonline.com/${TENANT_ID}/oauth2/v2.0/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=${CLIENT_ID}" \
  -d "client_secret=${CLIENT_SECRET}" \
  -d "scope=${SCOPE}" \
  -d "grant_type=client_credentials" | jq -r '.access_token')

echo $TOKEN
```

#### Using Entra ID Token

Include the token in the `Authorization` header:

```bash
curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json"
```

---

### Option 2: Agent Access Token

Use Agent Access Tokens for public-facing applications where users don't need to authenticate with Entra ID. This is ideal for:

- Public chatbots and demos
- Kiosk applications
- Embedded chat widgets
- Customer support interfaces

#### Creating an Agent Access Token

1. Navigate to **Management Portal** > **Agents** > [Your Agent]
2. Go to the **Security** section
3. Click **Create Access Token**
4. Enter a description and expiration date
5. Copy and securely store the generated token

> [!IMPORTANT]
> The token is only displayed once. Store it securely immediately after creation.

For detailed setup instructions, see [Agent Access Token](../agents/Agent_AccessToken.md).

#### Using Agent Access Token with curl

Agent Access Tokens use the `X-AGENT-ACCESS-TOKEN` header instead of the `Authorization` header:

##### Create a Session

```bash
AGENT_ACCESS_TOKEN="your-agent-access-token"

curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions?api-version=1.0" \
  -H "X-AGENT-ACCESS-TOKEN: ${AGENT_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Public Chat Session"
  }'
```

**Response:**

```json
{
  "id": "session-789",
  "name": "Public Chat Session",
  "type": "Session"
}
```

##### Send a Completion

```bash
SESSION_ID="session-789"

curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "X-AGENT-ACCESS-TOKEN: ${AGENT_ACCESS_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "'"${SESSION_ID}"'",
    "user_prompt": "Hello, what can you help me with?"
  }'
```

##### Poll for Completion Status

```bash
OPERATION_ID="op-abc123"

curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/async-completions/${OPERATION_ID}/status?api-version=1.0" \
  -H "X-AGENT-ACCESS-TOKEN: ${AGENT_ACCESS_TOKEN}"
```

##### Upload an Attachment

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/attachments?api-version=1.0" \
  -H "X-AGENT-ACCESS-TOKEN: ${AGENT_ACCESS_TOKEN}" \
  -F "file=@/path/to/document.pdf"
```

#### Agent Access Token Limitations

When using Agent Access Tokens, be aware of these limitations:

| Limitation | Description |
|------------|-------------|
| **Single Agent** | Token only provides access to the specific agent it was created for |
| **No Agent Override** | Cannot use `settings.agent_name` to switch agents |
| **Shared Identity** | All users of the token share the same virtual identity |
| **Quota Sharing** | Rate limits are shared across all token users |

#### Security Considerations for Agent Access Tokens

1. **Token Storage**: Store tokens in secure environment variables or secret managers
2. **Token Rotation**: Set appropriate expiration dates and rotate tokens regularly
3. **Access Control**: Assign only necessary RBAC permissions to the token's virtual security group
4. **Monitoring**: Monitor usage patterns to detect potential abuse
5. **Revocation**: Delete the token immediately if compromised

---

### Choosing the Right Authentication Method

| Scenario | Recommended Method |
|----------|-------------------|
| Internal enterprise application | Entra ID Bearer Token |
| User needs access to multiple agents | Entra ID Bearer Token |
| Per-user quota tracking required | Entra ID Bearer Token |
| Public website chatbot | Agent Access Token |
| Customer support widget | Agent Access Token |
| Kiosk or shared terminal | Agent Access Token |
| Demo or proof-of-concept | Agent Access Token |
| API integration with existing auth | Entra ID Bearer Token |

---

## API Endpoints

### Health Check

Check if the Core API is running and healthy.

#### GET /status

```bash
curl -X GET "${CORE_API_URL}/status?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}"
```

**Response:**

```
Core API - ready
```

---

### Sessions

Sessions provide conversation context for agents. Each session maintains message history.

#### List All Sessions

Retrieve all sessions for the authenticated user.

```bash
curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json"
```

**Response:**

```json
[
  {
    "id": "session-123",
    "name": "My Chat Session",
    "type": "Session",
    "upn": "user@contoso.com",
    "messages": []
  }
]
```

#### Create a New Session

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Chat Session"
  }'
```

**Response:**

```json
{
  "id": "session-456",
  "name": "New Chat Session",
  "type": "Session",
  "upn": "user@contoso.com",
  "messages": []
}
```

#### Get a Specific Session

```bash
SESSION_ID="session-123"

curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions/${SESSION_ID}?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json"
```

#### Rename a Session

```bash
SESSION_ID="session-123"

curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions/${SESSION_ID}/rename?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '"Updated Session Name"'
```

#### Delete a Session

```bash
SESSION_ID="session-123"

curl -X DELETE "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions/${SESSION_ID}?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}"
```

---

### Completions

Completions are the core interaction with agents. You can send prompts and receive AI-generated responses.

#### Session-Based Completion (Asynchronous)

Send a completion request within a session context. This is the recommended approach for maintaining conversation history.

```bash
SESSION_ID="session-123"

curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "'"${SESSION_ID}"'",
    "user_prompt": "What are the key features of FoundationaLLM?"
  }'
```

**Response:**

```json
{
  "operation_id": "op-789",
  "status": "Pending"
}
```

> [!NOTE]
> Completion requests are processed asynchronously. Use the operation ID to poll for the result.

#### Polling for Completion Status

```bash
OPERATION_ID="op-789"

curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/async-completions/${OPERATION_ID}/status?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}"
```

**Response (In Progress):**

```json
{
  "operation_id": "op-789",
  "status": "InProgress"
}
```

**Response (Completed):**

```json
{
  "operation_id": "op-789",
  "status": "Completed",
  "result": {
    "text": "FoundationaLLM provides several key features including...",
    "completion_tokens": 150,
    "prompt_tokens": 25,
    "total_tokens": 175,
    "user_prompt": "What are the key features of FoundationaLLM?"
  }
}
```

#### Sessionless Completion

For one-off queries without maintaining session context:

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "user_prompt": "What are your capabilities?",
    "settings": {
      "agent_name": "my-agent"
    }
  }'
```

#### Completion with Agent Override

Specify a different agent or override agent parameters:

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "'"${SESSION_ID}"'",
    "user_prompt": "Analyze this data and provide insights",
    "settings": {
      "agent_name": "analytics-agent",
      "model_parameters": {
        "temperature": 0.3,
        "max_new_tokens": 500
      }
    }
  }'
```

#### Completion with RAG Parameters

For knowledge-management agents, customize retrieval settings:

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "'"${SESSION_ID}"'",
    "user_prompt": "What does the documentation say about authentication?",
    "settings": {
      "agent_name": "docs-agent",
      "agent_parameters": {
        "index_filter_expression": "category eq '\''authentication'\''",
        "index_top_n": 10
      }
    }
  }'
```

---

### Attachments

Upload files for agents to analyze during completions.

#### Upload a File

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/attachments?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/document.pdf"
```

**Response:**

```json
{
  "object_id": "attachment-abc123",
  "original_file_name": "document.pdf",
  "content_type": "application/pdf"
}
```

#### Use Attachment in Completion

Reference the uploaded attachment in your completion request:

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "session_id": "'"${SESSION_ID}"'",
    "user_prompt": "Summarize the key points in this document",
    "attachments": ["attachment-abc123"]
  }'
```

#### Upload Multiple Files

You can upload up to 10 files in a single request:

```bash
curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/attachments?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: multipart/form-data" \
  -F "files=@/path/to/file1.pdf" \
  -F "files=@/path/to/file2.csv" \
  -F "files=@/path/to/file3.docx"
```

---

### Messages

Retrieve messages from a session.

#### Get Session Messages

```bash
SESSION_ID="session-123"

curl -X GET "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions/${SESSION_ID}/messages?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}"
```

**Response:**

```json
[
  {
    "id": "msg-1",
    "type": "Message",
    "session_id": "session-123",
    "sender": "User",
    "text": "What are the key features?",
    "timestamp": "2025-01-06T10:30:00Z"
  },
  {
    "id": "msg-2",
    "type": "Message",
    "session_id": "session-123",
    "sender": "Agent",
    "text": "FoundationaLLM provides several key features...",
    "timestamp": "2025-01-06T10:30:05Z",
    "completion_tokens": 150,
    "prompt_tokens": 25
  }
]
```

#### Rate a Message

```bash
MESSAGE_ID="msg-2"

curl -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions/${SESSION_ID}/messages/${MESSAGE_ID}/rate?api-version=1.0" \
  -H "Authorization: Bearer ${TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "rating": true
  }'
```

- `rating: true` = Thumbs up (helpful)
- `rating: false` = Thumbs down (not helpful)

---

## Request Parameters Reference

### Completion Request Body

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_prompt` | string | Yes | The user's message or question |
| `session_id` | string | No | Session ID for conversation context |
| `attachments` | array | No | List of attachment IDs |
| `settings` | object | No | Override settings (see below) |

### Settings Object

| Parameter | Type | Description |
|-----------|------|-------------|
| `agent_name` | string | Name of the agent to use |
| `model_parameters` | object | LLM parameter overrides |
| `agent_parameters` | object | Agent-specific parameter overrides |

### Model Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `temperature` | float | 0.0 | Controls randomness (0.0-1.0) |
| `max_new_tokens` | int | varies | Maximum tokens in response |
| `top_p` | float | 0.9 | Nucleus sampling parameter |
| `top_k` | int | null | Top-k sampling parameter |
| `deployment_name` | string | varies | Model deployment name |

### Agent Parameters (RAG)

| Parameter | Type | Description |
|-----------|------|-------------|
| `index_filter_expression` | string | Azure AI Search filter expression |
| `index_top_n` | int | Number of documents to retrieve |

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | Description |
|------|---------|-------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid request format or parameters |
| 401 | Unauthorized | Invalid or missing authentication token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |

### Error Response Format

```json
{
  "error": {
    "code": "InvalidRequest",
    "message": "The request body is invalid.",
    "details": [
      {
        "field": "user_prompt",
        "issue": "Field is required"
      }
    ]
  }
}
```

### Handling Rate Limits

When you receive a 429 response, implement exponential backoff:

```bash
#!/bin/bash

MAX_RETRIES=3
RETRY_DELAY=1

for i in $(seq 1 $MAX_RETRIES); do
  RESPONSE=$(curl -s -w "\n%{http_code}" -X POST \
    "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{"user_prompt": "Hello", "session_id": "'"${SESSION_ID}"'"}')
  
  HTTP_CODE=$(echo "$RESPONSE" | tail -1)
  BODY=$(echo "$RESPONSE" | head -n -1)
  
  if [ "$HTTP_CODE" -eq 429 ]; then
    echo "Rate limited, retrying in ${RETRY_DELAY}s..."
    sleep $RETRY_DELAY
    RETRY_DELAY=$((RETRY_DELAY * 2))
  else
    echo "$BODY"
    break
  fi
done
```

---

## Rate Limiting and Quotas

The Core API enforces rate limits to ensure fair usage:

| Limit Type | Default | Scope |
|------------|---------|-------|
| API Request Rate | 120/minute | Per user |
| Agent Completion Rate | Configurable | Per agent |
| File Upload Size | 10 MB | Per file |
| Files per Message | 10 | Per request |

For detailed quota configuration, see [Quota Definitions](../../concepts/quota/quota-definition.md).

---

## Complete Example: Chat Application

Here's a complete example of building a simple chat interaction:

```bash
#!/bin/bash

# Configuration
CORE_API_URL="https://your-core-api-url"
INSTANCE_ID="your-instance-id"
TOKEN="your-bearer-token"
AGENT_NAME="your-agent-name"

# Function to create a session
create_session() {
  curl -s -X POST "${CORE_API_URL}/instances/${INSTANCE_ID}/sessions?api-version=1.0" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{"name": "CLI Chat Session"}' | jq -r '.id'
}

# Function to send a message and get response
send_message() {
  local session_id=$1
  local message=$2
  
  # Send completion request
  local response=$(curl -s -X POST \
    "${CORE_API_URL}/instances/${INSTANCE_ID}/completions?api-version=1.0" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{
      "session_id": "'"${session_id}"'",
      "user_prompt": "'"${message}"'",
      "settings": {
        "agent_name": "'"${AGENT_NAME}"'"
      }
    }')
  
  local operation_id=$(echo "$response" | jq -r '.operation_id')
  
  # Poll for completion
  while true; do
    local status_response=$(curl -s -X GET \
      "${CORE_API_URL}/instances/${INSTANCE_ID}/async-completions/${operation_id}/status?api-version=1.0" \
      -H "Authorization: Bearer ${TOKEN}")
    
    local status=$(echo "$status_response" | jq -r '.status')
    
    if [ "$status" = "Completed" ]; then
      echo "$status_response" | jq -r '.result.text'
      break
    elif [ "$status" = "Failed" ]; then
      echo "Error: Completion failed"
      break
    fi
    
    sleep 0.5
  done
}

# Main chat loop
echo "Creating chat session..."
SESSION_ID=$(create_session)
echo "Session created: ${SESSION_ID}"
echo "Type 'quit' to exit"
echo ""

while true; do
  read -p "You: " user_input
  
  if [ "$user_input" = "quit" ]; then
    echo "Goodbye!"
    break
  fi
  
  echo -n "Agent: "
  send_message "$SESSION_ID" "$user_input"
  echo ""
done
```

---

## Code Examples

### Python

```python
import requests
import time

class FoundationaLLMClient:
    def __init__(self, base_url, instance_id, token):
        self.base_url = base_url
        self.instance_id = instance_id
        self.headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }
    
    def create_session(self, name="New Session"):
        response = requests.post(
            f"{self.base_url}/instances/{self.instance_id}/sessions",
            params={"api-version": "1.0"},
            headers=self.headers,
            json={"name": name}
        )
        response.raise_for_status()
        return response.json()["id"]
    
    def complete(self, session_id, prompt, agent_name=None):
        payload = {
            "session_id": session_id,
            "user_prompt": prompt
        }
        if agent_name:
            payload["settings"] = {"agent_name": agent_name}
        
        # Send request
        response = requests.post(
            f"{self.base_url}/instances/{self.instance_id}/completions",
            params={"api-version": "1.0"},
            headers=self.headers,
            json=payload
        )
        response.raise_for_status()
        operation_id = response.json()["operation_id"]
        
        # Poll for result
        while True:
            status_response = requests.get(
                f"{self.base_url}/instances/{self.instance_id}/async-completions/{operation_id}/status",
                params={"api-version": "1.0"},
                headers=self.headers
            )
            status_response.raise_for_status()
            result = status_response.json()
            
            if result["status"] == "Completed":
                return result["result"]["text"]
            elif result["status"] == "Failed":
                raise Exception("Completion failed")
            
            time.sleep(0.5)

# Usage
client = FoundationaLLMClient(
    base_url="https://your-core-api-url",
    instance_id="your-instance-id",
    token="your-token"
)

session_id = client.create_session("My Chat")
response = client.complete(session_id, "Hello, what can you do?")
print(response)
```

### JavaScript/Node.js

```javascript
const axios = require('axios');

class FoundationaLLMClient {
  constructor(baseUrl, instanceId, token) {
    this.baseUrl = baseUrl;
    this.instanceId = instanceId;
    this.client = axios.create({
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
  }

  async createSession(name = 'New Session') {
    const response = await this.client.post(
      `${this.baseUrl}/instances/${this.instanceId}/sessions`,
      { name },
      { params: { 'api-version': '1.0' } }
    );
    return response.data.id;
  }

  async complete(sessionId, prompt, agentName = null) {
    const payload = {
      session_id: sessionId,
      user_prompt: prompt
    };
    if (agentName) {
      payload.settings = { agent_name: agentName };
    }

    // Send request
    const response = await this.client.post(
      `${this.baseUrl}/instances/${this.instanceId}/completions`,
      payload,
      { params: { 'api-version': '1.0' } }
    );
    const operationId = response.data.operation_id;

    // Poll for result
    while (true) {
      const statusResponse = await this.client.get(
        `${this.baseUrl}/instances/${this.instanceId}/async-completions/${operationId}/status`,
        { params: { 'api-version': '1.0' } }
      );

      if (statusResponse.data.status === 'Completed') {
        return statusResponse.data.result.text;
      } else if (statusResponse.data.status === 'Failed') {
        throw new Error('Completion failed');
      }

      await new Promise(resolve => setTimeout(resolve, 500));
    }
  }
}

// Usage
const client = new FoundationaLLMClient(
  'https://your-core-api-url',
  'your-instance-id',
  'your-token'
);

(async () => {
  const sessionId = await client.createSession('My Chat');
  const response = await client.complete(sessionId, 'Hello, what can you do?');
  console.log(response);
})();
```

---

## Best Practices

### 1. Use Sessions for Conversations

Always use sessions when building conversational applications to maintain context:

```bash
# Good: Use sessions
curl -X POST ".../completions" -d '{"session_id": "...", "user_prompt": "..."}'

# Avoid: Sessionless for conversations (loses context)
curl -X POST ".../completions" -d '{"user_prompt": "..."}'
```

### 2. Poll Efficiently

Use appropriate polling intervals and implement timeout handling:

- Start with 100-500ms intervals
- Implement maximum timeout (e.g., 120 seconds)
- Handle failed states gracefully

### 3. Handle Errors Gracefully

```bash
# Check response status codes
if [ "$HTTP_CODE" -eq 401 ]; then
  # Refresh token and retry
elif [ "$HTTP_CODE" -eq 429 ]; then
  # Back off and retry
elif [ "$HTTP_CODE" -ge 500 ]; then
  # Log error, retry with backoff
fi
```

### 4. Secure Your Tokens

- Never hardcode tokens in source code
- Use environment variables or secure vaults
- Implement token refresh logic
- Set appropriate token expiration

### 5. Optimize File Uploads

- Compress files when appropriate
- Use supported file formats
- Stay within size limits
- Upload files once and reference by ID

---

## Related Topics

- [Directly Calling Core API](../../development/calling-apis/directly-calling-core-api.md) - Detailed setup guide
- [API Validation and Errors](../../development/calling-apis/api-validation-errors.md) - Error handling reference
- [Quota Definitions](../../concepts/quota/quota-definition.md) - Rate limiting details
- [Agents and Workflows](../agents/agents_workflows.md) - Agent configuration
