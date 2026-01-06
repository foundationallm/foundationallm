# API Key Limits

This document describes the limits and policies for API keys in FoundationaLLM.

## Overview

API keys provide programmatic access to FoundationaLLM APIs. Understanding key limits helps you design applications that work within platform constraints.

## Types of API Keys

| Key Type | Scope | Use Case |
|----------|-------|----------|
| **Instance API Key** | Full instance access | Backend service integration |
| **Agent Access Token** | Single agent | Public-facing applications |
| **User API Key** | User-scoped access | Personal automation |

<!-- [TODO: Confirm all API key types supported] -->

## Key Generation Limits

### Per-Instance Limits

| Limit | Value | Notes |
|-------|-------|-------|
| Max API keys per instance | <!-- [TODO] --> | Total keys |
| Max API keys per user | <!-- [TODO] --> | Per user |
| Max Agent Access Tokens per agent | <!-- [TODO] --> | Per agent |

### Key Creation Rate

| Limit | Value | Window |
|-------|-------|--------|
| Key creation rate | <!-- [TODO] --> | Per hour |

## Key Usage Limits

API keys are subject to the same quota limits as other authentication methods:

- [API Raw Request Rate](api-raw-request-rate.md)
- [Agent Request Rate](agent-request-rate.md)

### Additional Key-Specific Limits

| Limit | Description |
|-------|-------------|
| Max concurrent requests | Simultaneous requests per key |
| Burst limit | Short-term request spike allowance |

<!-- [TODO: Document specific limits when available] -->

## Key Lifecycle

### Creation

1. Generate via Management Portal or API
2. Key is immediately active
3. Store key securely (shown once)

### Rotation

Best practices for key rotation:

1. Generate new key before old key expires
2. Update applications to use new key
3. Verify new key works
4. Revoke old key

### Expiration

| Policy | Behavior |
|--------|----------|
| No expiration | Key valid until manually revoked |
| Time-based expiration | Auto-expire after duration |
| Activity-based | Expire after period of inactivity |

<!-- [TODO: Document expiration policies when available] -->

### Revocation

Revoking a key:

1. Navigate to key management
2. Select the key
3. Click **Revoke**
4. Key is immediately invalid

## Managing API Keys

### Via Management Portal

1. Navigate to **Settings** > **API Keys**
2. View, create, and manage keys

<!-- [TODO: Add screenshot of API key management] -->

### Via Management API

#### List Keys

```http
GET {{baseUrl}}/instances/{{instanceId}}/apiKeys
```

#### Create Key

```http
POST {{baseUrl}}/instances/{{instanceId}}/apiKeys
Content-Type: application/json

{
  "name": "my-api-key",
  "description": "Production backend service",
  "expirationDate": "2025-12-31T23:59:59Z"
}
```

#### Revoke Key

```http
DELETE {{baseUrl}}/instances/{{instanceId}}/apiKeys/{keyId}
```

## Security Best Practices

### Key Storage

| Do | Don't |
|----|-------|
| Store in secure secret manager | Hard-code in source code |
| Use environment variables | Commit to version control |
| Encrypt at rest | Store in plain text files |

### Key Scope

1. **Least privilege**: Use minimum required scope
2. **Separate keys**: Different keys for different environments
3. **Agent tokens**: Use agent tokens for single-agent access

### Monitoring

1. Enable API key usage logging
2. Set up alerts for unusual activity
3. Regularly audit active keys
4. Remove unused keys

## Quota Interaction

API keys inherit instance-level quota settings:

```
Instance Quota Definition
        |
        v
┌───────────────────┐
│    API Request    │
│   Rate Limiting   │
├───────────────────┤
│ Per-UPN limits    │
│ Per-Key limits    │
│ Global limits     │
└───────────────────┘
```

### Per-Key Quota Configuration

<!-- [TODO: Document per-key quota configuration if supported] -->

## Error Handling

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 401 Unauthorized | Invalid key | Verify key is correct and active |
| 403 Forbidden | Insufficient scope | Use key with appropriate permissions |
| 429 Too Many Requests | Rate limit exceeded | Implement backoff and retry |

### Rate Limit Response

When rate limited, the API returns:

```json
{
  "error": {
    "code": "RateLimitExceeded",
    "message": "Rate limit exceeded. Retry after 60 seconds.",
    "retryAfter": 60
  }
}
```

## Related Topics

- [Quota Definition](quota-definition.md) - Comprehensive quota documentation
- [API Raw Request Rate](api-raw-request-rate.md) - Request rate limits
- [Agent Access Token](../../setup-guides/agents/Agent_AccessToken.md) - Agent-specific tokens
- [Calling the Core API](../../development/calling-apis/directly-calling-core-api.md) - Using API keys
