# API Validation Rules and Errors

This document describes validation rules, error responses, and best practices for handling errors when calling FoundationaLLM APIs.

## Overview

FoundationaLLM APIs return structured error responses when validation fails or errors occur. Understanding these responses helps you build robust integrations.

## Error Response Format

### Standard Error Response

All API errors follow this format:

```json
{
  "error": {
    "code": "ErrorCode",
    "message": "Human-readable error description",
    "details": [
      {
        "field": "fieldName",
        "issue": "Specific validation issue"
      }
    ]
  }
}
```

### HTTP Status Codes

| Code | Meaning | Typical Cause |
|------|---------|---------------|
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Invalid/missing authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource already exists |
| 422 | Unprocessable Entity | Semantic validation failure |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |
| 503 | Service Unavailable | Service temporarily unavailable |

## Common Validation Rules

### Agent Resource Validation

| Field | Rule | Error Code |
|-------|------|------------|
| `name` | Required, alphanumeric with hyphens/underscores | `InvalidName` |
| `name` | Max 64 characters | `NameTooLong` |
| `name` | Unique within instance | `DuplicateName` |
| `display_name` | Max 128 characters | `DisplayNameTooLong` |
| `description` | Max 1024 characters | `DescriptionTooLong` |
| `prompt_object_id` | Must exist | `PromptNotFound` |
| `temperature` | 0.0 to 1.0 | `InvalidTemperature` |

### Prompt Resource Validation

| Field | Rule | Error Code |
|-------|------|------------|
| `name` | Required, alphanumeric | `InvalidName` |
| `content` | Required, non-empty | `ContentRequired` |
| `content` | Max token limit | `ContentTooLong` |

### Data Source Validation

| Field | Rule | Error Code |
|-------|------|------------|
| `type` | Valid type enum | `InvalidDataSourceType` |
| `folders` | Valid paths | `InvalidFolderPath` |
| `site_url` | Valid URL format | `InvalidUrl` |

### Data Pipeline Validation

| Field | Rule | Error Code |
|-------|------|------------|
| `name` | Unique within instance | `DuplicateName` |
| `stages` | At least one starting stage | `NoStartingStage` |
| `stage.name` | Unique within pipeline | `DuplicateStageName` |
| `trigger.trigger_cron_schedule` | Valid cron format | `InvalidCronSchedule` |

## Error Code Reference

### Authentication Errors

| Code | Description | Resolution |
|------|-------------|------------|
| `InvalidToken` | Bearer token invalid or expired | Refresh authentication |
| `MissingToken` | No authentication provided | Add Authorization header |
| `TokenExpired` | Token has expired | Re-authenticate |

### Authorization Errors

| Code | Description | Resolution |
|------|-------------|------------|
| `InsufficientPermissions` | User lacks required role | Request appropriate role |
| `ResourceAccessDenied` | No access to specific resource | Request resource access |
| `OperationNotAllowed` | Operation not permitted for role | Use different endpoint/method |

### Resource Errors

| Code | Description | Resolution |
|------|-------------|------------|
| `ResourceNotFound` | Resource doesn't exist | Verify resource ID |
| `ResourceDeleted` | Resource was soft-deleted | Restore or use different resource |
| `ResourceExists` | Duplicate resource name | Use unique name |
| `DependencyNotFound` | Referenced resource missing | Create dependency first |

### Validation Errors

| Code | Description | Resolution |
|------|-------------|------------|
| `RequiredFieldMissing` | Mandatory field not provided | Include required field |
| `InvalidFieldValue` | Value doesn't match constraints | Correct value format |
| `InvalidFieldFormat` | Format constraint violated | Use correct format |
| `FieldTooLong` | String exceeds length limit | Shorten value |
| `InvalidReference` | Referenced ID invalid | Verify reference |

### Rate Limit Errors

| Code | Description | Resolution |
|------|-------------|------------|
| `RateLimitExceeded` | Too many requests | Implement backoff |
| `QuotaExceeded` | Usage quota exhausted | Wait for quota reset |
| `TokenLimitExceeded` | Token usage exceeded | Reduce token consumption |

## Validation Error Examples

### Missing Required Field

**Request:**
```http
POST /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/myagent
Content-Type: application/json

{
  "type": "knowledge-management",
  "description": "My agent"
}
```

**Response:**
```json
{
  "error": {
    "code": "RequiredFieldMissing",
    "message": "One or more required fields are missing",
    "details": [
      {
        "field": "name",
        "issue": "The 'name' field is required"
      }
    ]
  }
}
```

### Invalid Reference

**Request:**
```http
POST /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/myagent
Content-Type: application/json

{
  "type": "knowledge-management",
  "name": "myagent",
  "prompt_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/nonexistent"
}
```

**Response:**
```json
{
  "error": {
    "code": "DependencyNotFound",
    "message": "Referenced resource does not exist",
    "details": [
      {
        "field": "prompt_object_id",
        "issue": "Prompt 'nonexistent' not found"
      }
    ]
  }
}
```

### Rate Limit Exceeded

**Response:**
```json
{
  "error": {
    "code": "RateLimitExceeded",
    "message": "Rate limit exceeded. Retry after specified duration.",
    "retryAfter": 60,
    "details": [
      {
        "limit": "120 requests per minute",
        "current": "125 requests",
        "window": "60 seconds"
      }
    ]
  }
}
```

## Error Handling Best Practices

### Implement Retry Logic

```python
import time
import requests

def call_api_with_retry(url, headers, body, max_retries=3):
    for attempt in range(max_retries):
        response = requests.post(url, headers=headers, json=body)
        
        if response.status_code == 429:
            retry_after = response.json().get('error', {}).get('retryAfter', 60)
            time.sleep(retry_after)
            continue
            
        if response.status_code >= 500:
            time.sleep(2 ** attempt)  # Exponential backoff
            continue
            
        return response
    
    raise Exception("Max retries exceeded")
```

### Validate Before Submitting

```python
def validate_agent(agent_config):
    errors = []
    
    if not agent_config.get('name'):
        errors.append({'field': 'name', 'issue': 'Name is required'})
    elif len(agent_config['name']) > 64:
        errors.append({'field': 'name', 'issue': 'Name exceeds 64 characters'})
    
    if 'temperature' in agent_config:
        temp = agent_config['temperature']
        if not (0.0 <= temp <= 1.0):
            errors.append({'field': 'temperature', 'issue': 'Must be 0.0-1.0'})
    
    return errors
```

### Handle Specific Errors

```python
def handle_api_error(response):
    error = response.json().get('error', {})
    code = error.get('code')
    
    if code == 'ResourceNotFound':
        # Resource doesn't exist, maybe create it
        pass
    elif code == 'DuplicateName':
        # Name conflict, suggest alternative
        pass
    elif code == 'InsufficientPermissions':
        # User needs more permissions
        raise PermissionError(error.get('message'))
    else:
        # Generic error handling
        raise APIError(error.get('message'))
```

## Accessibility Considerations

When displaying errors to users:

1. **Clear language**: Use plain language, not error codes
2. **Actionable messages**: Tell users how to fix the error
3. **Screen reader support**: Ensure errors are announced
4. **Focus management**: Move focus to error messages
5. **Visual indicators**: Use icons and colors (not just color)

### Example User-Facing Error

```
❌ Unable to create agent

The agent name "my agent" contains invalid characters. 
Agent names can only include letters, numbers, hyphens (-), 
and underscores (_).

Please update the name and try again.
```

## Related Topics

- [Directly Calling Core API](directly-calling-core-api.md)
- [Directly Calling Management API](directly-calling-management-api.md)
- [Quota Definition](../../concepts/quota/quota-definition.md)
- [Rate Limits](../../concepts/quota/api-raw-request-rate.md)
