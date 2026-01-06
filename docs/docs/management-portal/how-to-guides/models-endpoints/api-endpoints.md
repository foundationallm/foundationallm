# API Endpoints

Learn how to configure and manage API endpoint configurations in the Management Portal.

## Overview

API Endpoints define the connection configurations for external services used by FoundationaLLM, including AI model providers, authentication services, and other backend systems.

## Accessing API Endpoints

1. In the Management Portal sidebar, click **API Endpoints** under the **Models and Endpoints** section
2. The endpoints list loads, showing all configured endpoints

## Endpoint List

The table displays:

| Column | Description |
|--------|-------------|
| **Name** | Endpoint identifier |
| **Category** | Type of endpoint |
| **Subcategory** | More specific classification |
| **Edit** | Settings icon to modify configuration |
| **Delete** | Trash icon to remove the endpoint |

### Searching and Managing

- Use the search box to filter by name or description
- Click the refresh button to reload the list
- Click column headers to sort

## Endpoint Categories

| Category | Description |
|----------|-------------|
| **LLM** | Large Language Model endpoints |
| **Embedding** | Embedding model endpoints |
| **Authorization** | Authentication and authorization services |
| **Storage** | Storage service endpoints |
| **Search** | Search service endpoints |

## Creating an API Endpoint

1. Click **Create API Endpoint** at the top right of the page
2. Configure the endpoint settings

### Endpoint Configuration

> **TODO**: Document specific API endpoint configuration fields when available in the UI, including:

| Field | Description |
|-------|-------------|
| **Endpoint Name** | Unique identifier |
| **Category** | Endpoint type |
| **Subcategory** | Specific classification |
| **URL** | Service endpoint URL |
| **Authentication** | Auth configuration |

### Azure OpenAI Endpoints

For Azure OpenAI services:

| Field | Description |
|-------|-------------|
| **Endpoint URL** | `https://<resource>.openai.azure.com/` |
| **API Version** | API version (e.g., `2024-02-01`) |
| **Authentication** | API Key or Managed Identity |

### Other Service Endpoints

For other services, configure:

- Service URL
- Authentication method
- Required headers or parameters

## Authentication Configuration

### API Key Authentication

1. Select API Key authentication
2. Enter the key value (stored securely)
3. Configure header name if non-standard

### Managed Identity

1. Select Managed Identity authentication
2. Ensure the identity has required permissions
3. No credentials required in configuration

### OAuth/Token Authentication

1. Configure token endpoint
2. Set client credentials
3. Configure scopes as needed

## Editing Endpoints

1. Locate the endpoint in the list
2. Click the **Settings** icon (⚙️)
3. Modify settings as needed
4. Click **Save Changes**

## Deleting Endpoints

1. Click the **Trash** icon (🗑️) for the endpoint
2. Confirm deletion in the dialog

> **Warning:** Deleting an endpoint affects any models or services referencing it. Verify dependencies before deleting.

## Endpoint References

API Endpoints are referenced by:

- **AI Models**: Models use endpoints for API connectivity
- **Data Sources**: Some sources reference endpoints for authentication
- **Internal Services**: Platform services use configured endpoints

## Access Control

Configure who can access and manage endpoints:

| Permission | Description |
|------------|-------------|
| `FoundationaLLM.Configuration/apiEndpointConfigurations/read` | View endpoints |
| `FoundationaLLM.Configuration/apiEndpointConfigurations/write` | Edit endpoints |
| `FoundationaLLM.Configuration/apiEndpointConfigurations/delete` | Delete endpoints |

## Best Practices

### Naming Conventions

- Use descriptive names indicating service and purpose
- Include environment when relevant (dev, prod)
- Example: `azure-openai-eastus-prod`, `search-service-main`

### Security

- Use Managed Identity when possible
- Rotate API keys regularly
- Avoid hardcoding credentials

### Organization

- Group related endpoints logically
- Document endpoint purposes
- Maintain consistent naming

## Troubleshooting

### Connection Failures

- Verify endpoint URL is correct
- Check authentication credentials
- Review network/firewall configuration

### Authentication Errors

- Verify API key or credentials
- Check managed identity permissions
- Review token expiration

### Endpoint Not Available

- Verify the endpoint exists
- Check your permissions
- Ensure the service is running

## Related Topics

- [AI Models](ai-models.md)
- [Configuration Reference](../../reference/configuration-reference.md)
- [Data Sources](../data/data-sources.md)
