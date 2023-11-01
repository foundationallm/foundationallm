locals {
  config_keys = {
    "FoundationaLLM:APIs:AgentFactoryAPI:APIUrl" = {
      value = "http://foundationallm-agent-factory-api/agentfactory"
    }
    "FoundationaLLM:APIs:AgentHubAPI:APIUrl" = {
      value = "http://foundationallm-agent-hub-api/agenthub"
    }
    "FoundationaLLM:APIs:CoreAPI:APIUrl" = {
      value = "https://${data.azurerm_dns_zone.public_dns.name}/core"
    }
    "FoundationaLLM:APIs:DataSourceHubAPI:APIUrl" = {
      value = "http://foundationallm-data-source-hub-api/datasourcehub"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:APIUrl" = {
      value = "http://foundationallm-gatekeeper-api/gatekeeper"
    }
    "FoundationaLLM:APIs:LangChainAPI:APIUrl" = {
      value = "http://foundationallm-langchain-api/langchain"
    }
    "FoundationaLLM:APIs:PromptHubAPI:APIUrl" = {
      value = "http://foundationallm-prompt-hub-api/prompthub"
    }
    "FoundationaLLM:APIs:SemanticKernelAPI:APIUrl" = {
      value = "http://foundationallm-semantic-kernel-api/semantickernel"
    }
    "FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.ai_connection_string.versionless_id
    }
    "FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.ai_connection_string.versionless_id
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.ai_connection_string.versionless_id
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["agentfactoryapi"].versionless_id
    }
    "FoundationaLLM:APIs:AgentHubAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["agenthubapi"].versionless_id
    }
    "FoundationaLLM:APIs:DataSourceHubAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["datasourcehubapi"].versionless_id
    }
    "FoundationaLLM:APIs:GatekeeperAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["gatekeeperapi"].versionless_id
    }
    "FoundationaLLM:APIs:LangChainAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["langchainapi"].versionless_id
    }
    "FoundationaLLM:APIs:PromptHubAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["prompthubapi"].versionless_id
    }
    "FoundationaLLM:APIs:SemanticKernelAPI:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["semantickernelapi"].versionless_id
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:ForceHttpsRedirection" = {
      value = "false"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:ForceHttpsRedirection" = {
      value = "false"
    }
    "FoundationaLLM:AgentHub:AgentMetadata:StorageContainer" = {
      value = "agents"
    }
    "FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:AzureContentSafety:APIKey" = {
      vault_key_reference = azurerm_key_vault_secret.content_safety_apikey.versionless_id
    }
    "FoundationaLLM:AzureContentSafety:APIUrl" = {
      value = module.content_safety.endpoint
    }
    "FoundationaLLM:AzureContentSafety:HateSeverity" = {
      value = "2"
    }
    "FoundationaLLM:AzureContentSafety:SelfHarmSeverity" = {
      value = "2"
    }
    "FoundationaLLM:AzureContentSafety:SexualSeverity" = {
      value = "2"
    }
    "FoundationaLLM:AzureContentSafety:ViolenceSeverity" = {
      value = "2"
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName" = {
      value = "completions"
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens" = {
      value = "8096"
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:ModelName" = {
      value = "gpt-35-turbo"
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion" = {
      value = "0301"
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:Temperature" = {
      value = "0"
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName" = {
      value = "embeddings"
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:MaxTokens" = {
      value = "8191"
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName" = {
      value = "text-embedding-ada-002"
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:Temperature" = {
      value = "0"
    }
    "FoundationaLLM:AzureOpenAI:API:Endpoint" = {
      value = module.openai_ha.endpoint
    }
    "FoundationaLLM:AzureOpenAI:API:Key" = {
      vault_key_reference = azurerm_key_vault_secret.openai_key.versionless_id
    }
    "FoundationaLLM:AzureOpenAI:API:Version" = {
      value = "2023-05-15"
    }
    "FoundationaLLM:BlobStorage:ConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:BlobStorageMemorySource:ConfigBlobStorageContainer" = {
      value = "memory-source"
    }
    "FoundationaLLM:BlobStorageMemorySource:ConfigFilePath" = {
      value = "BlobMemorySourceConfig.json"
    }
    "FoundationaLLM:Branding:AccentColor" = {
      value = "#FFF"
    }
    "FoundationaLLM:Branding:CompanyName" = {
      value = "FoundationaLLM"
    }
    "FoundationaLLM:Branding:FavIconUrl" = {
      value = "favicon.ico"
    }
    "FoundationaLLM:Branding:KioskMode" = {
      value = "false"
    }
    "FoundationaLLM:Branding:LogoText" = {
      value = ""
    }
    "FoundationaLLM:Branding:LogoUrl" = {
      value = "foundationallm-logo-white.svg"
    }
    "FoundationaLLM:Branding:PageTitle" = {
      value = "FoundationaLLM Chat Copilot"
    }
    "FoundationaLLM:Branding:PrimaryColor" = {
      value = "#131833"
    }
    "FoundationaLLM:Branding:PrimaryTextColor" = {
      value = "#FFF"
    }
    "FoundationaLLM:Branding:SecondaryColor" = {
      value = "#334581"
    }
    "FoundationaLLM:Branding:SecondaryTextColor" = {
      value = "#FFF"
    }
    "FoundationaLLM:Chat:Entra:CallbackPath" = {
      value = "/signin-oidc"
    }
    "FoundationaLLM:Chat:Entra:ClientId" = {
      #      value       = data.azuread_application.client_entra.client_id
      value = ""
    }
    "FoundationaLLM:Chat:Entra:ClientSecret" = {
      vault_key_reference = azurerm_key_vault_secret.client_entra_clientsecret.versionless_id
    }
    "FoundationaLLM:Chat:Entra:Instance" = {
      value = "https://login.microsoftonline.com/"
    }
    "FoundationaLLM:Chat:Entra:Scopes" = {
      value = "api://FoundationaLLM-Auth/Data.Read"
    }
    "FoundationaLLM:Chat:Entra:TenantId" = {
      value = data.azurerm_client_config.current.tenant_id
    }
    "FoundationaLLM:CognitiveSearch:ConfigBlobStorageConnection" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:CognitiveSearch:EndPoint" = {
      value = module.search.endpoint
    }
    "FoundationaLLM:CognitiveSearch:IndexName" = {
      value = "vector-index"
    }
    "FoundationaLLM:CognitiveSearch:Key" = {
      vault_key_reference = azurerm_key_vault_secret.search_key.versionless_id
    }
    "FoundationaLLM:CognitiveSearch:MaxVectorSearchResults" = {
      value = "10"
    }
    "FoundationaLLM:CognitiveSearchMemorySource:BlobStorageConnection" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:CognitiveSearchMemorySource:ConfigBlobStorageContainer" = {
      value = "memory-source"
    }
    "FoundationaLLM:CognitiveSearchMemorySource:ConfigFilePath" = {
      value = "BlobMemorySourceConfig.json"
    }
    "FoundationaLLM:CognitiveSearchMemorySource:EndPoint" = {
      value = module.search.endpoint
    }
    "FoundationaLLM:CognitiveSearchMemorySource:IndexName" = {
      value = "vector-index"
    }
    "FoundationaLLM:CognitiveSearchMemorySource:Key" = {
      vault_key_reference = azurerm_key_vault_secret.search_key.versionless_id
    }
    "FoundationaLLM:CoreAPI:Entra:CallbackPath" = {
      value = "/signin-oidc"
    }
    "FoundationaLLM:CoreAPI:Entra:ClientId" = {
      #      value       = data.azuread_application.core_entra.client_id
      value = ""
    }
    "FoundationaLLM:CoreAPI:Entra:ClientSecret" = {
      vault_key_reference = azurerm_key_vault_secret.core_entra_clientsecret.versionless_id
    }
    "FoundationaLLM:CoreAPI:Entra:Instance" = {
      value = "https://login.microsoftonline.com/"
    }
    "FoundationaLLM:CoreAPI:Entra:Scopes" = {
      value = "Data.Read"
    }
    "FoundationaLLM:CoreAPI:Entra:TenantId" = {
      value = data.azurerm_client_config.current.tenant_id
    }
    "FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer" = {
      value = "leases"
    }
    "FoundationaLLM:CosmosDB:Containers" = {
      value = "completions, customer, product"
    }
    "FoundationaLLM:CosmosDB:Database" = {
      value = "database"
    }
    "FoundationaLLM:CosmosDB:Endpoint" = {
      value = module.cosmosdb.endpoint
    }
    "FoundationaLLM:CosmosDB:Key" = {
      vault_key_reference = azurerm_key_vault_secret.cosmosdb_key.versionless_id
    }
    "FoundationaLLM:CosmosDB:MonitoredContainers" = {
      value = "customer, product"
    }
    "FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer" = {
      value = "data-sources"
    }
    "FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:DurableSystemPrompt:BlobStorageConnection" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:DurableSystemPrompt:BlobStorageContainer" = {
      value = "system-prompt"
    }
    "FoundationaLLM:LangChain:CSVFile:URL" = {
      vault_key_reference = azurerm_key_vault_secret.langchain_csvfile_url.versionless_id
    }
    "FoundationaLLM:LangChain:SQLDatabase:TestDB:Password" = {
      vault_key_reference = azurerm_key_vault_secret.langchain_sqldatabase_testdb_pw.versionless_id
    }
    "FoundationaLLM:LangChain:Summary:MaxTokens" = {
      value = "4097"
    }
    "FoundationaLLM:LangChain:Summary:ModelName" = {
      value = "gpt-35-turbo"
    }
    "FoundationaLLM:LangChainAPI:Key" = {
      vault_key_reference = azurerm_key_vault_secret.api_key["langchainapi"].versionless_id
    }
    "FoundationaLLM:OpenAI:API:Endpoint" = {
      value = module.openai_ha.endpoint
    }
    "FoundationaLLM:OpenAI:API:Key" = {
      vault_key_reference = azurerm_key_vault_secret.openai_key.versionless_id
    }
    "FoundationaLLM:OpenAI:API:Temperature" = {
      value = "0"
    }
    "FoundationaLLM:PromptHub:PromptMetadata:StorageContainer" = {
      value = "system-prompt"
    }
    "FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString" = {
      vault_key_reference = azurerm_key_vault_secret.storage_connection_string.versionless_id
    }
    "FoundationaLLM:Refinement" = {
      value = ""
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.ChatCompletionPromptName" = {
      value = "RetailAssistant.Default"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeploymentName" = {
      value = "completions"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentName" = {
      value = "embeddings"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentMaxTokens" = {
      value = "8191"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.Endpoint" = {
      value = module.openai_ha.endpoint
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMaxTokens" = {
      value = "300"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMinTokens" = {
      value = "50"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMaxTokens" = {
      value = "3000"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMinTokens" = {
      value = "1500"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMaxTokens" = {
      value = "3000"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMinTokens" = {
      value = "100"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.SystemMaxTokens" = {
      value = "1500"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.ShortSummaryPromptName" = {
      value = "Summarizer.TwoWords"
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI:Key" = {
      vault_key_reference = azurerm_key_vault_secret.openai_key.versionless_id
    }
  }
}

resource "azurerm_app_configuration_key" "config_key" {
  for_each = { for k, v in local.config_keys : k => v if lookup(v, "value", null) != null }

  configuration_store_id = data.azurerm_app_configuration.appconfig.id
  key                    = each.key
  type                   = "kv"
  value                  = each.value.value
  # tags = local.tags
}

resource "azurerm_app_configuration_key" "config_key_vault" {
  for_each = { for k, v in local.config_keys : k => v if lookup(v, "vault_key_reference", null) != null }

  configuration_store_id = data.azurerm_app_configuration.appconfig.id
  key                    = each.key
  type                   = "vault"
  vault_key_reference    = each.value.value
  tags                   = local.tags
}