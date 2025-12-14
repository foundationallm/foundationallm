# Configuration Reference

Reference documentation for FoundationaLLM configuration settings.

## Overview

FoundationaLLM uses Azure App Configuration as the central store for application settings. This document provides a reference for key configuration categories and their settings.

## Configuration Sources

| Source | Purpose |
|--------|---------|
| **Azure App Configuration** | Application settings, feature flags, branding |
| **Azure Key Vault** | Secrets, API keys, connection strings |
| **Environment Variables** | Runtime environment settings, container configuration |

## Configuration Key Structure

Keys follow a hierarchical naming convention:

```
FoundationaLLM:{Category}:{SubCategory}:{Setting}
```

Example: `FoundationaLLM:APIs:CoreAPI:APIUrl`

---

## API Configuration

### Core API

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:CoreAPI:APIUrl` | Core API base URL |
| `FoundationaLLM:APIs:CoreAPI:APIScope` | OAuth scope for Core API |
| `FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString` | Application Insights connection |

### Management API

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:ManagementAPI:APIUrl` | Management API base URL |
| `FoundationaLLM:APIs:ManagementAPI:APIScope` | OAuth scope for Management API |

### Gateway API

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:GatewayAPI:APIUrl` | Gateway API base URL |
| `FoundationaLLM:APIs:GatewayAPI:APIKey` | (Key Vault reference) API key |

### Authorization API

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:AuthorizationAPI:APIUrl` | Authorization API base URL |
| `FoundationaLLM:APIs:AuthorizationAPI:APIScope` | OAuth scope for Authorization API |

---

## Branding Configuration

See [Branding Reference](branding/index.md) for complete branding settings.

| Key | Description | Default |
|-----|-------------|---------|
| `FoundationaLLM:Branding:CompanyName` | Organization name | `FoundationaLLM` |
| `FoundationaLLM:Branding:PageTitle` | Browser tab title | `FoundationaLLM User Portal` |
| `FoundationaLLM:Branding:PrimaryColor` | Primary UI color | `#131833` |
| `FoundationaLLM:Branding:LogoUrl` | Logo image path | `foundationallm-logo-white.svg` |
| `FoundationaLLM:Branding:KioskMode` | Enable kiosk mode | `false` |

---

## Authentication Configuration

### Entra ID (Azure AD)

| Key | Description |
|-----|-------------|
| `FoundationaLLM:Instance:Id` | FoundationaLLM instance identifier |
| `FoundationaLLM:Entra:Instance` | Azure AD instance URL |
| `FoundationaLLM:Entra:TenantId` | Azure AD tenant ID |
| `FoundationaLLM:Entra:ClientId` | Application (client) ID |
| `FoundationaLLM:Entra:Scopes` | Default OAuth scopes |
| `FoundationaLLM:Entra:CallbackPath` | OAuth callback path |

### Portal-Specific Authentication

| Key | Description |
|-----|-------------|
| `FoundationaLLM:UserPortal:Entra:ClientId` | User Portal client ID |
| `FoundationaLLM:ManagementPortal:Entra:ClientId` | Management Portal client ID |

---

## Storage Configuration

### Azure Blob Storage

| Key | Description |
|-----|-------------|
| `FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection` | (Key Vault) Storage connection string |
| `FoundationaLLM:BlobStorageMemorySource:BlobStorageContainer` | Container name |

### CosmosDB

| Key | Description |
|-----|-------------|
| `FoundationaLLM:CosmosDB:Endpoint` | CosmosDB endpoint URL |
| `FoundationaLLM:CosmosDB:Key` | (Key Vault) CosmosDB key |
| `FoundationaLLM:CosmosDB:Database` | Database name |

---

## Orchestration Configuration

### Agent Settings

| Key | Description |
|-----|-------------|
| `FoundationaLLM:Agent:DefaultAgentName` | Default agent for new conversations |
| `FoundationaLLM:Agent:ConversationHistoryMaxMessages` | Max messages in conversation context |

### LLM Provider Settings

| Key | Description |
|-----|-------------|
| `FoundationaLLM:AzureOpenAI:Endpoint` | Azure OpenAI endpoint |
| `FoundationaLLM:AzureOpenAI:DeploymentName` | Default deployment name |
| `FoundationaLLM:AzureOpenAI:ApiKey` | (Key Vault) API key |

---

## Feature Flags

Feature flags control optional functionality:

| Key | Description | Default |
|-----|-------------|---------|
| `FoundationaLLM:Features:EnableRating` | Enable response rating | `true` |
| `FoundationaLLM:Features:EnableFileUpload` | Enable file uploads | `true` |
| `FoundationaLLM:Features:EnableAgentSelection` | Enable agent selection | `true` |

---

## Quota Configuration

Quotas are defined as JSON objects in App Configuration:

| Key | Description |
|-----|-------------|
| `FoundationaLLM:Quota:APIRawRequestRate` | API request rate limits |
| `FoundationaLLM:Quota:AgentRequestRate` | Agent completion request limits |

See [Quotas Reference](concepts/quotas.md) for quota structure details.

---

## Environment Variables

Common environment variables used by services:

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment (Development, Production) |
| `AZURE_CLIENT_ID` | Managed identity client ID |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights connection |

---

## Key Vault References

Sensitive values use Key Vault references in App Configuration:

```json
{
  "uri": "https://{keyvault-name}.vault.azure.net/secrets/{secret-name}"
}
```

The application automatically resolves these references at runtime using managed identity.

---

## Accessing Configuration

### Via Management Portal

1. Navigate to **FLLM Platform** > **Configuration**
2. View and edit available settings

### Via Azure Portal

1. Open your Azure App Configuration resource
2. Use **Configuration explorer** to browse keys
3. Use **Feature manager** for feature flags

### Via API

```bash
# List all configurations
GET /instances/{instanceId}/providers/FoundationaLLM.Configuration/appConfigurations
```

---

## Best Practices

1. **Use Key Vault** for all secrets and sensitive values
2. **Use labels** in App Configuration for environment-specific settings
3. **Document changes** when modifying configuration
4. **Test in non-production** before applying to production
5. **Monitor** configuration changes through App Configuration audit logs

## Related Topics

- [Branding Reference](branding/index.md)
- [Permissions & Roles Reference](permissions-roles.md)
- [Quotas Reference](concepts/quotas.md)
