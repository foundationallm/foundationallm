# Deployment Configuration Reference

This document provides a reference for environment variables and configuration settings used during FoundationaLLM deployment.

## Environment Variable Reference

> **Note:** These settings are primarily used during initial deployment. For runtime configuration, see [App Configuration Values](app-configuration-values.md).

### Core API Configuration

| Variable | Type | Description |
|----------|------|-------------|
| `foundationallm-core-api-url` | URL | Core API endpoint |
| `foundationallm-core-api-keyvault-name` | String | Key Vault name |
| `foundationallm-core-api-entra-instance` | URL | Entra login URL (default: `https://login.microsoftonline.com/`) |
| `foundationallm-core-api-entra-tenant-id` | GUID | Azure AD tenant ID |
| `foundationallm-core-api-entra-client-id` | GUID | App registration client ID |
| `foundationallm-core-api-entra-callback-path` | Path | OAuth callback (default: `/signin-oidc`) |
| `foundationallm-core-api-entra-scopes` | String | Required scopes |
| `foundationallm-core-api-gatekeeper-api-url` | URL | Gatekeeper API endpoint |

### Gatekeeper API Configuration

| Variable | Type | Description |
|----------|------|-------------|
| `foundationallm-gatekeeper-api-keyvault-name` | String | Key Vault name |
| `foundationallm-gatekeeper-api-key` | Secret | API key (Key Vault) |
| `foundationallm-gatekeeper-api-orchestration-api-url` | URL | Orchestration API endpoint |

### Orchestration API Configuration

| Variable | Type | Description |
|----------|------|-------------|
| `foundationallm-orchestration-api-keyvault-name` | String | Key Vault name |
| `foundationallm-orchestration-api-key` | Secret | API key (Key Vault) |
| `foundationallm-orchestration-api-agenthub-api-url` | URL | Agent Hub API endpoint |
| `foundationallm-orchestration-api-prompthub-api-url` | URL | Prompt Hub API endpoint |
| `foundationallm-orchestration-api-datasourcehub-api-url` | URL | Data Source Hub API endpoint |
| `foundationallm-orchestration-api-langchain-api-url` | URL | LangChain API endpoint |
| `foundationallm-orchestration-api-semantickernel-api-url` | URL | Semantic Kernel API endpoint |

### Hub API Keys

| Variable | Type | Description |
|----------|------|-------------|
| `foundationallm-agenthub-api-key` | Secret | Agent Hub API key |
| `foundationallm-prompthub-api-key` | Secret | Prompt Hub API key |
| `foundationallm-datasourcehub-api-key` | Secret | Data Source Hub API key |
| `foundationallm-langchain-api-key` | Secret | LangChain API key |

### Azure OpenAI Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `foundationallm-azure-openai-api-url` | (Required) | Azure OpenAI endpoint |
| `foundationallm-azure-openai-api-key` | (Secret) | API key |
| `foundationallm-azure-openai-api-completions-deployment` | (Required) | Completion deployment name |
| `foundationallm-azure-openai-api-completions-model-version` | (Required) | Model version |
| `foundationallm-azure-openai-api-version` | (Required) | API version |

### LangChain Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `foundationallm-langchain-summary-model-name` | `gpt-35-turbo` | Summary model |
| `foundationallm-langchain-summary-max-tokens` | `4097` | Max input tokens |

### SQL Database Configuration (Optional)

| Variable | Description |
|----------|-------------|
| `foundationallm-langchain-sqldb-testdb-server-name` | SQL Server name |
| `foundationallm-langchain-sqldb-testdb-database-name` | Database name |
| `foundationallm-langchain-sqldb-testdb-username` | Username |
| `foundationallm-langchain-sqldb-testdb-database-password` | Password (Key Vault) |

### Storage Configuration

| Variable | Description |
|----------|-------------|
| `foundationallm-keyvault-name` | Key Vault name for SDK |
| `foundationallm-prompt-metadata-storage-container` | Prompt storage container |
| `foundationallm-datasource-metadata-storage-container` | Data source container |
| `foundationallm-agent-metadata-storage-container` | Agent metadata container |

### Python SDK Configuration

| Variable | Description |
|----------|-------------|
| `foundationallm-configuration-allow-environment-variables` | When `True`, checks environment before Key Vault |

## Temporary Configuration

> **Note:** The following settings are temporary and will be removed in future versions.

| Variable | Type | Description |
|----------|------|-------------|
| `foundationallm-langchain-csv-file-url` | URL | CSV file URL with SAS token |

## Configuration Sources

FoundationaLLM uses multiple configuration sources in this priority order:

1. **Environment Variables** (highest priority)
2. **Azure App Configuration**
3. **Key Vault Secrets**
4. **Default Values** (lowest priority)

## Best Practices

### Security

- Store sensitive values in Key Vault
- Use managed identities where possible
- Rotate API keys regularly
- Avoid storing secrets in environment variables

### Organization

- Use consistent naming conventions
- Document custom configurations
- Use separate App Configuration instances for dev/test/prod
- Enable versioning for audit trails

### Troubleshooting

If configuration values aren't being read:

1. Verify App Configuration connection string
2. Check managed identity permissions
3. Verify Key Vault access policies
4. Review service logs for configuration errors

## Related Topics

- [App Configuration Values](app-configuration-values.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
