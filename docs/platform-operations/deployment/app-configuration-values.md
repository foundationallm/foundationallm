# App Configuration Values

FoundationaLLM uses Azure App Configuration as the central store for configuration values, Key Vault secret references, and feature flags.

## Overview

Benefits of centralized configuration:
- **Single source of truth** for all services
- **No redeployment required** for configuration changes
- **Key Vault integration** for secrets
- **Shared settings** across multiple services

## Configuration Categories

### Instance Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:Instance:Id` | Generated GUID | Unique instance identifier |
| `FoundationaLLM:Configuration:KeyVaultURI` | (Required) | Key Vault URL for secrets |

### API Endpoints

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:CoreAPI:APIUrl` | Core API base URL |
| `FoundationaLLM:APIs:ManagementAPI:APIUrl` | Management API base URL |
| `FoundationaLLM:APIs:OrchestrationAPI:APIUrl` | Orchestration API URL |
| `FoundationaLLM:APIs:GatekeeperAPI:APIUrl` | Gatekeeper API URL |
| `FoundationaLLM:APIs:LangChainAPI:APIUrl` | LangChain API URL |
| `FoundationaLLM:APIs:SemanticKernelAPI:APIUrl` | Semantic Kernel API URL |

### API Keys (Key Vault References)

| Key | Key Vault Secret |
|-----|------------------|
| `FoundationaLLM:APIs:OrchestrationAPI:APIKey` | `foundationallm-apis-orchestrationapi-apikey` |
| `FoundationaLLM:APIs:GatekeeperAPI:APIKey` | `foundationallm-apis-gatekeeperapi-apikey` |
| `FoundationaLLM:APIs:LangChainAPI:APIKey` | `foundationallm-apis-langchainapi-apikey` |
| `FoundationaLLM:APIs:SemanticKernelAPI:APIKey` | `foundationallm-apis-semantickernelapi-apikey` |

### Gatekeeper Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:APIs:CoreAPI:BypassGatekeeper` | `true` | Skip content filtering for performance |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableAzureContentSafety` | `true` | Enable Azure Content Safety |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableLakeraGuard` | `true` | Enable Lakera Guard |
| `FoundationaLLM:APIs:GatekeeperAPI:Configuration:EnableMicrosoftPresidio` | `true` | Enable PII detection |

### Azure Content Safety

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:APIUrl` | (Required) | Content Safety endpoint |
| `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:HateSeverity` | `2` | Threshold (0-6) |
| `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SelfHarmSeverity` | `2` | Threshold (0-6) |
| `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:SexualSeverity` | `2` | Threshold (0-6) |
| `FoundationaLLM:APIs:Gatekeeper:AzureContentSafety:ViolenceSeverity` | `2` | Threshold (0-6) |

### Azure OpenAI Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:AzureOpenAI:API:Endpoint` | (Required) | Azure OpenAI endpoint |
| `FoundationaLLM:AzureOpenAI:API:Version` | `2023-05-15` | API version |
| `FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName` | `completions` | Completion model deployment |
| `FoundationaLLM:AzureOpenAI:API:Completions:ModelName` | `gpt-35-turbo` | Model name |
| `FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens` | `8096` | Max tokens |
| `FoundationaLLM:AzureOpenAI:API:Completions:Temperature` | `0` | Temperature |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName` | `embeddings` | Embedding deployment |
| `FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName` | `text-embedding-ada-002` | Embedding model |

### Branding Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:Branding:CompanyName` | `FoundationaLLM` | Organization name |
| `FoundationaLLM:Branding:PageTitle` | `FoundationaLLM Chat Copilot` | Browser title |
| `FoundationaLLM:Branding:LogoUrl` | `foundationallm-logo-white.svg` | Logo path |
| `FoundationaLLM:Branding:FavIconUrl` | `favicon.ico` | Favicon path |
| `FoundationaLLM:Branding:PrimaryColor` | `#131833` | Primary color |
| `FoundationaLLM:Branding:PrimaryTextColor` | `#fff` | Primary text color |
| `FoundationaLLM:Branding:SecondaryColor` | `#334581` | Secondary color |
| `FoundationaLLM:Branding:BackgroundColor` | `#fff` | Background color |
| `FoundationaLLM:Branding:KioskMode` | `false` | Enable kiosk mode |

### Entra ID Authentication

#### Core API / Chat Portal

| Key | Description |
|-----|-------------|
| `FoundationaLLM:CoreAPI:Entra:Instance` | Entra login URL |
| `FoundationaLLM:CoreAPI:Entra:TenantId` | Tenant ID |
| `FoundationaLLM:CoreAPI:Entra:ClientId` | Client ID |
| `FoundationaLLM:CoreAPI:Entra:Scopes` | API scopes |
| `FoundationaLLM:CoreAPI:Entra:CallbackPath` | `/signin-oidc` |

#### Management API / Portal

| Key | Description |
|-----|-------------|
| `FoundationaLLM:ManagementAPI:Entra:Instance` | Entra login URL |
| `FoundationaLLM:ManagementAPI:Entra:TenantId` | Tenant ID |
| `FoundationaLLM:ManagementAPI:Entra:ClientId` | Client ID |
| `FoundationaLLM:ManagementAPI:Entra:Scopes` | API scopes |

### CosmosDB Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `FoundationaLLM:CosmosDB:Endpoint` | (Required) | CosmosDB endpoint |
| `FoundationaLLM:CosmosDB:Database` | `database` | Database name |
| `FoundationaLLM:CosmosDB:Containers` | `Sessions, UserSessions` | Container list |
| `FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer` | `leases` | Change feed container |

### Storage Configuration

| Key | Description |
|-----|-------------|
| `FoundationaLLM:AgentHub:AgentMetadata:StorageContainer` | Agent metadata container |
| `FoundationaLLM:PromptHub:PromptMetadata:StorageContainer` | Prompt metadata container |
| `FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer` | Data source container |
| `FoundationaLLM:BlobStorageMemorySource:BlobStorageContainer` | Memory source container |

### Vectorization Configuration

| Key | Description |
|-----|-------------|
| `FoundationaLLM:APIs:VectorizationAPI:APIUrl` | Vectorization API URL |
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint` | AI Search endpoint |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint` | Embedding endpoint |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName` | `embeddings` |

### Event Grid Configuration

| Key | Description |
|-----|-------------|
| `FoundationaLLM:Events:AzureEventGridEventService:Endpoint` | Event Grid endpoint |
| `FoundationaLLM:Events:AzureEventGridEventService:NamespaceId` | Event Grid namespace |
| `FoundationaLLM:Events:AzureEventGridEventService:AuthenticationType` | `APIKey` |

## Key Vault Secret References

Many App Configuration values reference Key Vault secrets. The format is:

```json
{
  "uri": "https://{keyvault-name}.vault.azure.net/secrets/{secret-name}"
}
```

Common secret references:

| App Config Key | Key Vault Secret |
|----------------|------------------|
| `FoundationaLLM:AzureOpenAI:API:Key` | `foundationallm-azureopenai-api-key` |
| `FoundationaLLM:CoreAPI:Entra:ClientSecret` | `foundationallm-coreapi-entra-clientsecret` |
| `FoundationaLLM:ManagementAPI:Entra:ClientSecret` | `foundationallm-managementapi-entra-clientsecret` |

## Modifying Configuration

### Via Azure Portal

1. Navigate to your App Configuration resource
2. Select **Configuration explorer**
3. Find and edit the key
4. Save changes

### Via Azure CLI

```bash
az appconfig kv set \
  --name <app-config-name> \
  --key "FoundationaLLM:Branding:CompanyName" \
  --value "My Company"
```

### Via Management Portal

Some settings (like branding) can be modified through the Management Portal UI.

## Related Topics

- [Deployment Configuration](deployment-configuration.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [Branding Configuration](../../management-portal/reference/branding/index.md)
