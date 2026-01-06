> **This article is still being authored.** This feature is currently under development. Content will be updated as the feature is finalized.

# OpenAI Model Endpoint Facades

Learn about FoundationaLLM's OpenAI-compatible endpoint facades for simplified model integration.

## Overview

> **TODO**: This feature is under active development. Documentation will be completed when the feature is released.

OpenAI Model Endpoint Facades provide an OpenAI-compatible API layer in front of FoundationaLLM's agent capabilities. This allows applications built for the OpenAI API to integrate with FoundationaLLM with minimal code changes.

## Purpose and Use Cases

### Why Use Endpoint Facades?

| Benefit | Description |
|---------|-------------|
| **Compatibility** | Use existing OpenAI SDK code |
| **Migration Path** | Gradually move from OpenAI to FoundationaLLM |
| **Familiar Interface** | Standard chat completion format |
| **Agent Benefits** | Access FoundationaLLM's agent capabilities |

### Ideal Scenarios

- Migrating existing OpenAI-integrated applications
- Using libraries that expect OpenAI-compatible endpoints
- Building applications that may switch between providers
- Teams familiar with OpenAI API patterns

## Feature Status

| Component | Status |
|-----------|--------|
| Chat Completions endpoint | Under Development |
| Embeddings endpoint | Under Development |
| Streaming responses | Under Development |
| Function calling | Under Development |

## Configuration

> **TODO**: Document configuration steps once the feature is finalized, including:
> - Enabling endpoint facades in deployment
> - Configuring facade endpoints
> - Mapping facades to agents
> - Authentication configuration

### Prerequisites

- FoundationaLLM deployment with endpoint facades enabled
- Appropriate API credentials configured
- Agent(s) to expose via the facade

### Setup Steps

> **TODO**: Provide step-by-step setup instructions

## API Reference

### Chat Completions Endpoint

> **TODO**: Document the endpoint specification

**Expected Endpoint:**
```
POST /v1/chat/completions
```

**Expected Request Format:**
```json
{
  "model": "agent-name",
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Hello!"}
  ],
  "temperature": 0.7,
  "max_tokens": 1000
}
```

**Expected Response Format:**
```json
{
  "id": "chatcmpl-xxx",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "agent-name",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "Hello! How can I help you today?"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 10,
    "completion_tokens": 15,
    "total_tokens": 25
  }
}
```

### Embeddings Endpoint

> **TODO**: Document embeddings endpoint if included

## Usage Examples

### Python with OpenAI SDK

> **TODO**: Verify and complete example

```python
from openai import OpenAI

# Configure client to use FoundationaLLM facade
client = OpenAI(
    base_url="https://{fllm-api}/v1",
    api_key="your-api-key"  # TODO: Document key format
)

# Use standard OpenAI SDK methods
response = client.chat.completions.create(
    model="your-agent-name",  # Maps to FLLM agent
    messages=[
        {"role": "user", "content": "What can you help me with?"}
    ]
)

print(response.choices[0].message.content)
```

### curl Example

> **TODO**: Verify and complete example

```bash
curl -X POST "https://{fllm-api}/v1/chat/completions" \
  -H "Authorization: Bearer your-api-key" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "your-agent-name",
    "messages": [
      {"role": "user", "content": "Hello!"}
    ]
  }'
```

## Mapping to FoundationaLLM Concepts

| OpenAI Concept | FoundationaLLM Equivalent |
|----------------|---------------------------|
| `model` parameter | Agent name |
| `messages` array | Conversation context |
| `temperature` | Model parameter override |
| `max_tokens` | `max_new_tokens` parameter |
| API Key | Agent Access Token or Bearer token |

## Limitations

> **TODO**: Document known limitations once feature is finalized

**Expected Limitations:**

- Not all OpenAI parameters may be supported
- Some agent features may not map to OpenAI concepts
- Streaming behavior may differ
- Rate limits follow FoundationaLLM quotas

## Migration Guide

### From OpenAI to FoundationaLLM Facades

> **TODO**: Provide migration steps

1. Configure FoundationaLLM endpoint facade
2. Update base URL in your application
3. Map model names to agent names
4. Test and verify functionality

### Considerations

- Review agent configurations match expected behavior
- Test streaming if used
- Verify token counting differences
- Update error handling for FLLM-specific errors

## Troubleshooting

### Common Issues

> **TODO**: Document common issues and solutions

| Issue | Possible Cause | Solution |
|-------|----------------|----------|
| 404 Not Found | Facade not enabled | Enable in configuration |
| Authentication Error | Invalid credentials | Check API key configuration |
| Model Not Found | Agent doesn't exist | Verify agent name |

## Related Topics

- [Core API Reference](api-reference.md)
- [Agent Access Tokens](../../../management-portal/reference/concepts/agent-access-tokens.md)
- [AI Models Configuration](../../../management-portal/how-to-guides/models-endpoints/ai-models.md)
