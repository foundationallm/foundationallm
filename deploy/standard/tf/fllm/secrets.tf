locals {
  api_keys = {
    "orchestrationapi"   = {}
    "agenthubapi"       = {}
    "datasourcehubapi"  = {}
    "gatekeeperapi"     = {}
    "langchainapi"      = {}
    "prompthubapi"      = {}
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

  name         = "foundationallm-apis-${each.key}-apikey"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = random_string.api_key[each.key].result
}

resource "azurerm_key_vault_secret" "content_safety_apikey" {
  name         = "foundationallm-azurecontentsafety-apikey"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = module.content_safety.key
}

resource "azurerm_key_vault_secret" "openai_key" {
  name         = "foundationallm-azureopenai-api-key"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = "HAOpenAIKey" # For HA OpenAI, there is currently no key.
}

#data "azuread_application" "core_entra" {
#  display_name = var.core_entra_application
#}
#
#resource "time_rotating" "core_entra" {
#  rotation_days = 30
#}

#resource "azuread_application_password" "core_entra" {
#  application_id = "/applications/${data.azuread_application.core_entra.object_id}"
#  rotate_when_changed = {
#    rotation = time_rotating.core_entra.id
#  }
#}

resource "azurerm_key_vault_secret" "core_entra_clientsecret" {
  name         = "foundationallm-chat-entra-clientsecret"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  #  value        = azuread_application_password.core_entra.value
  value = ""

  lifecycle {
    ignore_changes = [value] // TODO: gross
  }
}



#data "azuread_application" "client_entra" {
#  display_name = var.client_entra_application
#}
#
#resource "time_rotating" "client_entra" {
#  rotation_days = 30
#}

#resource "azuread_application_password" "client_entra" {
#  application_id = "/applications/${data.azuread_application.client_entra.object_id}"
#  rotate_when_changed = {
#    rotation = time_rotating.client_entra.id
#  }
#}

resource "azurerm_key_vault_secret" "client_entra_clientsecret" {
  name         = "foundationallm-coreapi-entra-clientsecret"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  #  value        = azuread_application_password.client_entra.value
  value = ""

  lifecycle {
    ignore_changes = [value] // TODO: gross
  }
}

resource "azurerm_key_vault_secret" "langchain_csvfile_url" {
  name         = "foundationallm-langchain-csvfile-url"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = ""
}

resource "azurerm_key_vault_secret" "langchain_sqldatabase_testdb_pw" {
  name         = "foundationallm-langchain-sqldatabase-testdb-password"
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  value        = var.test_db_password
}
