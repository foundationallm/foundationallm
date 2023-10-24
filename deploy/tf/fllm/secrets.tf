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

#resource "azurerm_key_vault_secret" "content_safety_apikey" {
#    name         = "foundationallm-azurecontentsafety-apikey"
#    key_vault_id = data.azurerm_key_vault.keyvault_ops.id
#    value        = module.content_safety.api_key
#}

resource "azurerm_key_vault_secret" "openai_key" {
  name         = "foundationallm-openai-apikey"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = ""
}

resource "azurerm_key_vault_secret" "memory_source_connection_string" {
  name         = "foundationallm-blobstoragememorysource-blobstorageconnection"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.storage_data.primary_connection_string
}



