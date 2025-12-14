# Branding via REST API

Learn how to configure branding programmatically using the REST API.

## Overview

The Management API provides endpoints for programmatic branding configuration.

## API Endpoints

### Get Branding Configuration
```http
GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/branding
```

### Update Branding Configuration
```http
PUT {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/branding
Content-Type: application/json

{
  "primary_color": "#0078D4",
  "logo_url": "https://example.com/logo.png",
  "portal_title": "My Enterprise AI"
}
```

## Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `primary_color` | string | Primary theme color (hex) |
| `secondary_color` | string | Secondary theme color (hex) |
| `logo_url` | string | URL to logo image |
| `portal_title` | string | Portal title text |
| `welcome_message` | string | Welcome screen message |

## Authentication

Requests require a valid bearer token:
```
Authorization: Bearer <token>
```

## Related Topics

- [Branding Overview](index.md)
- [Using App Configuration](using-app-configuration.md)
- [Using Management Portal](using-management-portal.md)
