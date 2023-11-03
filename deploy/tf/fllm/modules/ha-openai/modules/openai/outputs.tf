output "endpoint" {
  description = "The endpoint for the OpenAI account."
  value       = azurerm_cognitive_account.main.endpoint
}

output "key_secret" {
  description = "The key vault secrets for the OpenAI account access keys."
  value = {
    primary = {
      id   = azurerm_key_vault_secret.openai_primary_key.id
      name = azurerm_key_vault_secret.openai_primary_key.name
    }
    secondary = {
      id   = azurerm_key_vault_secret.openai_secondary_key.id
      name = azurerm_key_vault_secret.openai_secondary_key.name
    }
  }
}