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
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:AppInsightsConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.ai_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentFactoryAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "agentfactoryapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:AgentHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "agenthubapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:DataSourceHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "datasourcehubapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:GatekeeperAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "gatekeeperapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:LangChainAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "langchainapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:PromptHubAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "prompthubapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:APIs:SemanticKernelAPI:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.api_key[
          "semantickernelapi"
        ].versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AgentHub:AgentMetadata:StorageContainer" = "agents"
    "FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureContentSafety:APIKey" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.content_safety_apikey.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureContentSafety:APIUrl" = {
      value = data.azurerm_cognitive_account.content_safety.endpoint
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
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.openai_key.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:AzureOpenAI:API:Version" = {
      value = "2023-05-15"
    }
    "FoundationaLLM:BlobStorage:ConnectionString" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.storage_connection_string.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
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
      value = data.azuread_application.chat_entra.client_id
    }
    "FoundationaLLM:Chat:Entra:ClientSecret" = {
      value = jsonencode({
        "uri" = azurerm_key_vault_secret.chat_entra_clientsecret.versionless_id
      })
      "contentType" = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8"
    }
    "FoundationaLLM:Chat:Entra:Instance" = {
      value = "https://login.microsoftonline.com/"
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

