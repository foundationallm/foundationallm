# Using REST API for Branding

Configure branding settings programmatically through the Management API.

## Overview

The Management API provides endpoints for branding configuration, enabling:
- Custom tooling integration
- Automation scripts
- CI/CD pipelines
- External system integration

## API Endpoints

### Base URL

```
{Management API URL}/instances/{instanceId}/providers/FoundationaLLM.Configuration
```

### Authentication

All requests require authentication:
- Bearer token (Azure AD)
- Appropriate permissions for configuration management

## Retrieve All Branding Settings

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations
```

### Response

```json
[
  {
    "resource": {
      "type": "app-configuration",
      "name": "Branding-PrimaryColor",
      "key": "FoundationaLLM:Branding:PrimaryColor",
      "value": "#131833",
      "description": "The primary color for the portal UI."
    },
    "actions": ["read", "write"]
  },
  ...
]
```

## Update a Branding Setting

```http
PUT /instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/{settingName}
Content-Type: application/json
```

### Request Body

```json
{
  "type": "app-configuration",
  "name": "Branding-PrimaryColor",
  "display_name": "Primary Color",
  "description": "The primary color for the portal UI.",
  "key": "FoundationaLLM:Branding:PrimaryColor",
  "value": "#1a2b3c",
  "content_type": "text/plain"
}
```

### Response

```json
{
  "success": true,
  "resource": {
    "type": "app-configuration",
    "name": "Branding-PrimaryColor",
    "key": "FoundationaLLM:Branding:PrimaryColor",
    "value": "#1a2b3c"
  }
}
```

## Code Examples

### Python

```python
import requests

base_url = "https://your-management-api.azurewebsites.net"
instance_id = "your-instance-id"
access_token = "your-bearer-token"

headers = {
    "Authorization": f"Bearer {access_token}",
    "Content-Type": "application/json"
}

# Get branding settings
response = requests.get(
    f"{base_url}/instances/{instance_id}/providers/FoundationaLLM.Configuration/appConfigurations",
    headers=headers
)
branding = response.json()

# Update a setting
update_data = {
    "type": "app-configuration",
    "name": "Branding-CompanyName",
    "key": "FoundationaLLM:Branding:CompanyName",
    "value": "Contoso",
    "content_type": "text/plain"
}

response = requests.put(
    f"{base_url}/instances/{instance_id}/providers/FoundationaLLM.Configuration/appConfigurations/Branding-CompanyName",
    headers=headers,
    json=update_data
)
```

### PowerShell

```powershell
$baseUrl = "https://your-management-api.azurewebsites.net"
$instanceId = "your-instance-id"
$accessToken = "your-bearer-token"

$headers = @{
    "Authorization" = "Bearer $accessToken"
    "Content-Type" = "application/json"
}

# Get branding settings
$response = Invoke-RestMethod -Uri "$baseUrl/instances/$instanceId/providers/FoundationaLLM.Configuration/appConfigurations" `
    -Headers $headers -Method Get

# Update a setting
$body = @{
    type = "app-configuration"
    name = "Branding-CompanyName"
    key = "FoundationaLLM:Branding:CompanyName"
    value = "Contoso"
    content_type = "text/plain"
} | ConvertTo-Json

Invoke-RestMethod -Uri "$baseUrl/instances/$instanceId/providers/FoundationaLLM.Configuration/appConfigurations/Branding-CompanyName" `
    -Headers $headers -Method Put -Body $body
```

### JavaScript/Node.js

```javascript
const baseUrl = "https://your-management-api.azurewebsites.net";
const instanceId = "your-instance-id";
const accessToken = "your-bearer-token";

// Get branding settings
const response = await fetch(
  `${baseUrl}/instances/${instanceId}/providers/FoundationaLLM.Configuration/appConfigurations`,
  {
    headers: {
      "Authorization": `Bearer ${accessToken}`,
      "Content-Type": "application/json"
    }
  }
);
const branding = await response.json();

// Update a setting
const updateResponse = await fetch(
  `${baseUrl}/instances/${instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/Branding-CompanyName`,
  {
    method: "PUT",
    headers: {
      "Authorization": `Bearer ${accessToken}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      type: "app-configuration",
      name: "Branding-CompanyName",
      key: "FoundationaLLM:Branding:CompanyName",
      value: "Contoso",
      content_type: "text/plain"
    })
  }
);
```

## Setting Name Mapping

When using the API, setting names follow this pattern:

| Key | Setting Name |
|-----|--------------|
| `FoundationaLLM:Branding:CompanyName` | `Branding-CompanyName` |
| `FoundationaLLM:Branding:PrimaryColor` | `Branding-PrimaryColor` |
| `FoundationaLLM:Branding:LogoUrl` | `Branding-LogoUrl` |

General pattern: `Branding-{SettingName}` (colons replaced with hyphens, only the last segment)

## Error Handling

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 400 | Invalid request body |
| 401 | Unauthorized |
| 403 | Forbidden (insufficient permissions) |
| 404 | Setting not found |
| 500 | Internal server error |

## Related Topics

- [Branding Reference](index.md)
- [Using Management Portal for Branding](using-management-portal.md)
- [Using App Configuration for Branding](using-app-configuration.md)
