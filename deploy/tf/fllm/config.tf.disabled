locals {
  config_keys = {
    "FoundationaLLM:APIs:AgentFactoryAPI:APIUrl" = {
      value       = "http://foundationallm-agent-factory-api/agentfactory"
      contentType = null
    }
    "FoundationaLLM:APIs:AgentHubAPI:APIUrl" = {
      value       = "http://foundationallm-agent-hub-api/agenthub"
      contentType = null
    }
    "FoundationaLLM:APIs:CoreAPI:APIUrl" = {
      value       = "https://${data.azurerm_dns_zone.public_dns.name}/core"
      contentType = null
    }
    "FoundationaLLM:APIs:DataSourceHubAPI:APIUrl" = {
      value       = "http://foundationallm-data-source-hub-api/datasourcehub"
      contentType = null
    }
    "FoundationaLLM:APIs:GatekeeperAPI:APIUrl" = {
      value       = "http://foundationallm-gatekeeper-api/gatekeeper"
      contentType = null
    }
    "FoundationaLLM:APIs:LangChainAPI:APIUrl" = {
      value       = "http://foundationallm-langchain-api/langchain"
      contentType = null
    }
    "FoundationaLLM:APIs:PromptHubAPI:APIUrl" = {
      value       = "http://foundationallm-prompt-hub-api/prompthub"
      contentType = null
    }
    "FoundationaLLM:APIs:SemanticKernelAPI:APIUrl" = {
      value       = "http://foundationallm-semantic-kernel-api/semantickernel"
      contentType = null
    }
    "FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "agentfactoryapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "agenthubapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:DataSourceHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "datasourcehubapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "gatekeeperapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:LangChainAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "langchainapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:PromptHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "prompthubapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:SemanticKernelAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "semantickernelapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AgentHub:AgentMetadata:StorageContainer" = {
      value       = "agents"
      contentType = null
    }
    "FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureContentSafety:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.content_safety_apikey.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureContentSafety:APIUrl" = {
      value       = module.content_safety.endpoint
      contentType = null
    }
    "FoundationaLLM:AzureContentSafety:HateSeverity" = {
      value       = "2"
      contentType = null
    }
    "FoundationaLLM:AzureContentSafety:SelfHarmSeverity" = {
      value       = "2"
      contentType = null
    }
    "FoundationaLLM:AzureContentSafety:SexualSeverity" = {
      value       = "2"
      contentType = null
    }
    "FoundationaLLM:AzureContentSafety:ViolenceSeverity" = {
      value       = "2"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName" = {
      value       = "completions"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:MaxTokens" = {
      value       = "8096"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:ModelName" = {
      value       = "gpt-35-turbo"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion" = {
      value       = "0301"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Completions:Temperature" = {
      value       = "0"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:DeploymentName" = {
      value       = "embeddings"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:MaxTokens" = {
      value       = "8191"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:ModelName" = {
      value       = "text-embedding-ada-002"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Embeddings:Temperature" = {
      value       = "0"
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Endpoint" = {
      value       = module.openai_ha.endpoint
      contentType = null
    }
    "FoundationaLLM:AzureOpenAI:API:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.openai_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureOpenAI:API:Version" = {
      value       = "2023-05-15"
      contentType = null
    }
    "FoundationaLLM:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:BlobStorageMemorySource:ConfigBlobStorageContainer" = {
      value       = "memory-source"
      contentType = null
    }
    "FoundationaLLM:BlobStorageMemorySource:ConfigFilePath" = {
      value       = "BlobMemorySourceConfig.json"
      contentType = null
    }
    "FoundationaLLM:Branding:AccentColor" = {
      value       = "#FFF"
      contentType = null
    }
    "FoundationaLLM:Branding:CompanyName" = {
      value       = "FoundationaLLM"
      contentType = null
    }
    "FoundationaLLM:Branding:FavIconUrl" = {
      value       = "favicon.ico"
      contentType = null
    }
    "FoundationaLLM:Branding:KioskMode" = {
      value       = "false"
      contentType = null
    }
    "FoundationaLLM:Branding:LogoText" = {
      value       = ""
      contentType = null
    }
    "FoundationaLLM:Branding:LogoUrl" = {
      value       = "foundationallm-logo-white.svg"
      contentType = null
    }
    "FoundationaLLM:Branding:PageTitle" = {
      value       = "FoundationaLLM Chat Copilot"
      contentType = null
    }
    "FoundationaLLM:Branding:PrimaryColor" = {
      value       = "#131833"
      contentType = null
    }
    "FoundationaLLM:Branding:PrimaryTextColor" = {
      value       = "#FFF"
      contentType = null
    }
    "FoundationaLLM:Branding:SecondaryColor" = {
      value       = "#334581"
      contentType = null
    }
    "FoundationaLLM:Branding:SecondaryTextColor" = {
      value       = "#FFF"
      contentType = null
    }
    "FoundationaLLM:Chat:Entra:CallbackPath" = {
      value       = "/signin-oidc"
      contentType = null
    }
    "FoundationaLLM:Chat:Entra:ClientId" = {
      value       = data.azuread_application.client_entra.client_id
      contentType = null
    }
    "FoundationaLLM:Chat:Entra:ClientSecret" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.client_entra_clientsecret.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:Chat:Entra:Instance" = {
      value       = "https://login.microsoftonline.com/"
      contentType = null
    }
    "FoundationaLLM:Chat:Entra:Scopes" = {
      value       = "api://FoundationaLLM-Auth/Data.Read"
      contentType = null
    }
    "FoundationaLLM:Chat:Entra:TenantId" = {
      value       = data.azurerm_client_config.current.tenant_id
      contentType = null
    }
    "FoundationaLLM:CognitiveSearch:ConfigBlobStorageConnection" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CognitiveSearch:EndPoint" = {
      value       = module.search.endpoint
      contentType = null
    }
    "FoundationaLLM:CognitiveSearch:IndexName" = {
      value       = "vector-index"
      contentType = null
    }
    "FoundationaLLM:CognitiveSearch:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.search_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CognitiveSearch:MaxVectorSearchResults" = {
      value       = "10"
      contentType = null
    }
    "FoundationaLLM:CognitiveSearchMemorySource:BlobStorageConnection" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CognitiveSearchMemorySource:ConfigBlobStorageContainer" = {
      value       = "memory-source"
      contentType = null
    }
    "FoundationaLLM:CognitiveSearchMemorySource:ConfigFilePath" = {
      value       = "BlobMemorySourceConfig.json"
      contentType = null
    }
    "FoundationaLLM:CognitiveSearchMemorySource:EndPoint" = {
      value       = module.search.endpoint
      contentType = null
    }
    "FoundationaLLM:CognitiveSearchMemorySource:IndexName" = {
      value       = "vector-index"
      contentType = null
    }
    "FoundationaLLM:CognitiveSearchMemorySource:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.search_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CoreAPI:Entra:CallbackPath" = {
      value       = "/signin-oidc"
      contentType = null
    }
    "FoundationaLLM:CoreAPI:Entra:ClientId" = {
      value       = data.azuread_application.core_entra.client_id
      contentType = null
    }
    "FoundationaLLM:CoreAPI:Entra:ClientSecret" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.core_entra_clientsecret.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CoreAPI:Entra:Instance" = {
      value       = "https://login.microsoftonline.com/"
      contentType = null
    }
    "FoundationaLLM:CoreAPI:Entra:Scopes" = {
      value       = "Data.Read"
      contentType = null
    }
    "FoundationaLLM:CoreAPI:Entra:TenantId" = {
      value       = data.azurerm_client_config.current.tenant_id
      contentType = null
    }
    "FoundationaLLM:CosmosDB:ChangeFeedLeaseContainer" = {
      value       = "leases"
      contentType = null
    }
    "FoundationaLLM:CosmosDB:Containers" = {
      value       = "completions, customer, product"
      contentType = null
    }
    "FoundationaLLM:CosmosDB:Database" = {
      value       = "database"
      contentType = null
    }
    "FoundationaLLM:CosmosDB:Endpoint" = {
      value       = module.cosmosdb.endpoint
      contentType = null
    }
    "FoundationaLLM:CosmosDB:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.cosmosdb_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:CosmosDB:MonitoredContainers" = {
      value       = "customer, product"
      contentType = null
    }
    "FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer" = {
      value       = "data-sources"
      contentType = null
    }
    "FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:DurableSystemPrompt:BlobStorageConnection" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:DurableSystemPrompt:BlobStorageContainer" = {
      value       = "system-prompt"
      contentType = null
    }
    "FoundationaLLM:LangChain:CSVFile:URL" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.langchain_csvfile_url.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:LangChain:SQLDatabase:TestDB:Password" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.langchain_sqldatabase_testdb_pw.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:LangChain:Summary:MaxTokens" = {
      value       = "4097"
      contentType = null
    }
    "FoundationaLLM:LangChain:Summary:ModelName" = {
      value       = "gpt-35-turbo"
      contentType = null
    }
    "FoundationaLLM:LangChainAPI:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "langchainapi"
        ].versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:OpenAI:API:Endpoint" = {
      value       = module.openai_ha.endpoint
      contentType = null
    }
    "FoundationaLLM:OpenAI:API:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.openai_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:OpenAI:API:Temperature" = {
      value       = "0"
      contentType = null
    }
    "FoundationaLLM:PromptHub:PromptMetadata:StorageContainer" = {
      value       = "system-prompt"
      contentType = null
    }
    "FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:Refinement" = {
      value       = ""
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.ChatCompletionPromptName" = {
      value       = "RetailAssistant.Default"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.CompletionsDeploymentName" = {
      value       = "completions"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentName" = {
      value       = "embeddings"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.EmbeddingsDeploymentMaxTokens" = {
      value       = "8191"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.Endpoint" = {
      value       = module.openai_ha.endpoint
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMaxTokens" = {
      value       = "300"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.CompletionsMinTokens" = {
      value       = "50"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMaxTokens" = {
      value       = "3000"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MemoryMinTokens" = {
      value       = "1500"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMaxTokens" = {
      value       = "3000"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.MessagesMinTokens" = {
      value       = "100"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.PromptOptimization.SystemMaxTokens" = {
      value       = "1500"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI.ShortSummaryPromptName" = {
      value       = "Summarizer.TwoWords"
      contentType = null
    }
    "FoundationaLLM:SemanticKernelAPI:OpenAI:Key" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.openai_key.versionless_id
      })
      contentType = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
  }
}

resource "azurerm_app_configuration_key" "config_key" {
  for_each               = local.config_keys
  configuration_store_id = data.azurerm_app_configuration.appconfig.id
  key                    = each.key
  value                  = each.value.value
  content_type           = each.value.contentType
}

