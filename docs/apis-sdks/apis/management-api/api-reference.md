# Management API Reference

Complete API reference for the FoundationaLLM Management API.

## Overview

The Management API provides programmatic access to create, update, and manage FoundationaLLM resources.

## Base URL

```
https://{management-api-url}/
```

## Authentication

All requests require a valid bearer token:
```
Authorization: Bearer <token>
```

## Common Operations

### List Resources
```http
GET /instances/{instanceId}/providers/{providerName}/{resourceType}
```

### Get Resource
```http
GET /instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

### Create/Update Resource
```http
POST /instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
Content-Type: application/json

{
  // resource configuration
}
```

### Delete Resource
```http
DELETE /instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

### Purge Resource
```http
POST /instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}/purge
```

## Resource-Specific Endpoints

See the resource provider documentation for specific endpoints:
- [Agent Resources](../../../management-portal/reference/concepts/agents-workflows.md)
- [Prompt Resources](../../../management-portal/reference/concepts/prompts-resources.md)
- [Data Pipeline Resources](data-pipelines.md)

## Error Responses

```json
{
  "error": {
    "code": "ErrorCode",
    "message": "Error description"
  }
}
```

## Related Topics

- [Resource Providers Overview](resource-providers-overview.md)
- [Directly Calling Management API](directly-calling-management-api.md)
