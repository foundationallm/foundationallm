# API and Token Limits

Reference documentation for API rate limits, token usage limits, and quota enforcement in FoundationaLLM.

## Overview

FoundationaLLM enforces various limits to ensure fair usage, protect system stability, and manage costs. Understanding these limits helps you build applications that work reliably within the platform's constraints.

## Types of Limits

| Limit Type | Purpose | Scope |
|------------|---------|-------|
| **API Rate Limits** | Prevent excessive API calls | Per user or global |
| **Token Usage Limits** | Control model consumption | Per request/agent/user |
| **Quota Limits** | Manage resource allocation | Per agent or platform |

---

## API Rate Limits

### Overview

API rate limits restrict how many requests can be made within a time window. When limits are exceeded, the API returns a 429 (Too Many Requests) response.

### Rate Limit Structure

Rate limits are defined by:

- **Metric Limit**: Maximum number of requests
- **Time Window**: Period over which requests are counted (typically 60 seconds)
- **Lockout Duration**: How long you're blocked after exceeding limits
- **Partition**: Whether limits apply globally, per user, or per agent

### Common Rate Limits

| Controller | Typical Limit | Description |
|------------|---------------|-------------|
| **Completions** | 100/minute/user | Chat completion requests |
| **CompletionsStatus** | 500/minute/user | Polling for async completion status |
| **Sessions** | 60/minute/user | Session management operations |
| **Files** | 30/minute/user | File upload operations |

### Rate Limit Response

When you exceed a rate limit:

```json
{
  "quota_exceeded": true,
  "quota_name": "CoreAPICompletionsRateLimit",
  "retry_after_seconds": 60,
  "message": "Rate limit exceeded. Try again later."
}
```

### Best Practices

1. **Implement retry logic** with exponential backoff
2. **Monitor 429 responses** in your application
3. **Use async endpoints** for long operations to reduce polling
4. **Cache responses** where appropriate

---

## Token Usage Limits

### What Are Tokens?

Tokens are the units AI models use to process text:

- ~1 token ≈ 4 characters in English
- ~1 token ≈ ¾ of a word
- 100 tokens ≈ 75 words

### Token Counting

Each completion request consumes tokens in three categories:

| Category | Description |
|----------|-------------|
| **Prompt Tokens** | Tokens in your input (system prompt + user message + context) |
| **Completion Tokens** | Tokens in the model's response |
| **Total Tokens** | Sum of prompt and completion tokens |

### Per-Request Token Limits

The `max_new_tokens` parameter limits response length:

```json
{
  "user_prompt": "Explain quantum computing",
  "settings": {
    "model_parameters": {
      "max_new_tokens": 2000
    }
  }
}
```

| Parameter | Typical Range | Default |
|-----------|---------------|---------|
| `max_new_tokens` | 1-4000+ | Model-dependent |

### Model Context Limits

Different models have different context window sizes:

| Model | Context Window | Notes |
|-------|----------------|-------|
| GPT-4 | 8K-128K tokens | Varies by deployment |
| GPT-4o | 128K tokens | Large context support |
| GPT-3.5-turbo | 4K-16K tokens | Varies by version |
| Claude 3 | 200K tokens | Extended context |

If your request exceeds the context window, the API returns an error.

### Token Limit Errors

When token limits are exceeded:

```json
{
  "error": {
    "code": "TokenLimitExceeded",
    "message": "Request exceeds maximum token limit for this model."
  }
}
```

---

## API Key Limits

### Agent Access Token Limits

Agent Access Tokens have configurable constraints:

| Limit | Description | Configuration |
|-------|-------------|---------------|
| **Expiration** | Token validity period | Set at creation time |
| **Agent Scope** | Which agent(s) can be accessed | Bound to single agent |
| **Permission Scope** | What actions are allowed | Defined by role assignments |

### Creating Tokens with Appropriate Limits

When creating Agent Access Tokens:

1. Set appropriate expiration dates (shorter = more secure)
2. Limit scope to necessary agents only
3. Review and rotate tokens periodically

---

## Quota Configuration

### Quota Definition Structure

Quotas are defined with these properties:

```json
{
  "name": "CoreAPICompletionsRateLimit",
  "description": "100 requests per minute per user",
  "context": "CoreAPI:Completions",
  "type": "RawRequestRateLimit",
  "metric_partition": "UserPrincipalName",
  "metric_limit": 100,
  "metric_window_seconds": 60,
  "lockout_duration_seconds": 60,
  "distributed_enforcement": false
}
```

### Quota Types

| Type | Description |
|------|-------------|
| `RawRequestRateLimit` | Limits total API requests |
| `AgentRequestRateLimit` | Limits requests to specific agents |

### Partition Strategies

| Partition | Description | Use Case |
|-----------|-------------|----------|
| `None` | Global limit for all users | System protection |
| `UserPrincipalName` | Per-user limits | Standard user access |
| `UserIdentifier` | Per-identity limits | Service accounts |

---

## Agent-Specific Limits

### Per-Agent Rate Limits

Specific agents can have individual rate limits:

```json
{
  "name": "KnowledgeAgentRateLimit",
  "context": "CoreAPI:Completions:knowledge-agent",
  "type": "AgentRequestRateLimit",
  "metric_partition": "UserPrincipalName",
  "metric_limit": 50,
  "metric_window_seconds": 60
}
```

### Agent Token Configuration

Agents can be configured with:

- Maximum response tokens
- Temperature constraints
- Context window usage limits

---

## Monitoring and Managing Limits

### Viewing Current Usage

Users can monitor token consumption in the User Portal:

1. Enable **Show Message Tokens** in agent configuration
2. Token counts appear on each message
3. Review consumption patterns over time

### Administrator Monitoring

Administrators can:

- Review quota definitions in storage
- Monitor API logs for 429 responses
- Adjust quotas based on usage patterns

### Adjusting Limits

To modify quota limits:

1. Access the quota configuration in the main storage account
2. Edit the `quota-store.json` file in the `quota` container
3. Update metric limits, windows, or partitions
4. Changes take effect on next API service restart

---

## Limit Recommendations

### For Developers

| Scenario | Recommendation |
|----------|----------------|
| Interactive chat | Use default limits, implement retry |
| Batch processing | Implement rate limiting in your code |
| High-volume API | Request quota increase from admin |
| Long operations | Use async endpoints |

### For Administrators

| Goal | Configuration |
|------|---------------|
| Prevent abuse | Set per-user limits with lockouts |
| Protect critical agents | Add agent-specific rate limits |
| Allow high-volume integrations | Consider higher limits for service accounts |
| Cost management | Monitor and set token-based quotas |

---

## Error Handling Examples

### Handling Rate Limits (Python)

```python
import time
import requests

def call_api_with_retry(url, headers, data, max_retries=3):
    for attempt in range(max_retries):
        response = requests.post(url, headers=headers, json=data)
        
        if response.status_code == 429:
            retry_after = response.json().get('retry_after_seconds', 60)
            print(f"Rate limited. Waiting {retry_after} seconds...")
            time.sleep(retry_after)
            continue
            
        return response
    
    raise Exception("Max retries exceeded")
```

### Handling Token Limits

```python
def truncate_to_token_limit(text, max_tokens=4000):
    # Approximate: 4 chars per token
    max_chars = max_tokens * 4
    if len(text) > max_chars:
        return text[:max_chars] + "..."
    return text
```

---

## Related Topics

- [Quotas Reference](quotas.md) — Detailed quota configuration
- [Core API Reference](../../../apis-sdks/apis/core-api/api-reference.md) — API endpoint details
- [Monitoring Token Consumption](../../../chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md) — User token monitoring
