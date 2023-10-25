locals {
  api_keys = {
    "agentfactoryapi" = {}
    "agenthubapi" = {}
    "datasourcehubapi" = {}
    "gatekeeperapi" = {}
    "langchainapi" = {}
    "prompthubapi" = {}
    "semantickernelapi" = {}
  }
}

resource "azurerm_key_vault_secret" "ai_connection_string" {
  name         = "foundationallm-app-insights-connection-string"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = data.azurerm_application_insights.ai.connection_string
}

resource "random_string" "api_key" {
  for_each = local.api_keys

    length  = 32
    special = false
    upper   = true
}

resource "azurerm_key_vault_secret" "api_key" {
  for_each = local.api_keys

  name = "foundationallm-apis-${each.key}-apikey"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value = random_string.api_key[each.key].result
}

resource "azurerm_key_vault_secret" "agenthub_storage_connection_string" {
  name         = "foundationallm-agenthub-storagemanager-blobstorage-connectionstring"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.storage.primary_connection_string
}

resource "azurerm_key_vault_secret" "content_safety_apikey" {
    name         = "foundationallm-azurecontentsafety-apikey"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = data.azurerm_cognitive_account.content_safety.primary_access_key
}

resource "azurerm_key_vault_secret" "openai_key" {
  name         = "foundationallm-azureopenai-api-key"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = "" # For HA OpenAI, there is currently no key.
}

resource "azurerm_key_vault_secret" "memory_source_blob_connection_string" {
  name         = "foundationallm-blobstoragememorysource-blobstorageconnection"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.storage.primary_connection_string
}

data "azuread_application" "chat_entra" {
  display_name = var.chat_entra_application
}

resource "time_rotating" "chat_entra" {
  rotation_days = 30
}

resource "azuread_application_password" "chat_entra" {
  application_object_id = data.azuread_application.chat_entra.object_id
  rotate_when_changed = {
    rotation = time_rotating.chat_entra.id
  }
}

resource "azurerm_key_vault_secret" "chat_entra_clientsecret" {
  name         = "foundationallm-chat-entra-clientsecret"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = azuread_application_password.chat_entra.value
}

resource "azurerm_key_vault_secret" "cogsearch_blob_connection_string" {
  name = "foundationallm-cognitivesearch-configblobstorageconnection"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = module.storage.primary_connection_string
}

resource "azurerm_key_vault_secret" "cogsearch_service_key" {
  name         = "foundationallm-cognitivesearch-key"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.search.key
}

resource "azurerm_key_vault_secret" "cogsearch_memory_blob_connection_string" {
    name         = "foundationallm-cognitivesearchmemorysource-blobstorageconnection"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = module.storage.primary_connection_string
}

resource "azurerm_key_vault_secret" "cogsearch_memory_key" {
  name         = "foundationallm-cognitivesearchmemorysource-key"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.search.key
}

data "azuread_application" "client_entra" {
  display_name = var.client_entra_application
}

resource "time_rotating" "client_entra" {
  rotation_days = 30
}

resource "azuread_application_password" "client_entra" {
  application_object_id = data.azuread_application.client_entra.object_id
  rotate_when_changed = {
    rotation = time_rotating.client_entra.id
  }
}

resource "azurerm_key_vault_secret" "client_entra_clientsecret" {
  name         = "foundationallm-coreapi-entra-clientsecret"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = azuread_application_password.client_entra.value
}

resource "azurerm_key_vault_secret" "cosmosdb_key" {
  name         = "foundationallm-cosmosdb-key"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.cosmosdb.key
}

resource "azurerm_key_vault_secret" "datasourcehub_blob_connection_string" {
    name         = "foundationallm-datasourcehub-storagemanager-blobstorage-connectionstring"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = module.storage.primary_connection_string
}

resource "azurerm_key_vault_secret" "durablesystemprompt_storage_connection_string" {
    name         = "foundationallm-durablesystemprompt-blobstorageconnection"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = module.storage.primary_connection_string
}

resource "azurerm_key_vault_secret" "langchain_csvfile_url" {
    name         = "foundationallm-langchain-csvfile-url"
    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
    value        = ""
}

resource "azurerm_key_vault_secret" "langchain_sqldatabase_testdb_pw" {
  name         = "foundationallm-langchain-sqldatabase-testdb-password"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = ""
}

resource "azurerm_key_vault_secret" "prompthub_blob_connection_string" {
  name         = "foundationallm-prompthub-storagemanager-blobstorage-connectionstring"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.storage.primary_connection_string
}

