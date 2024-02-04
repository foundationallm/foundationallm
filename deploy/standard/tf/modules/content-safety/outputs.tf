output "endpoint" {
  description = "The endpoint for the cognitive services account."
  value       = data.azurerm_cognitive_account.main.endpoint
}

output "key" {
  description = "The primary access key for the cognitive services account."
  value       = data.azurerm_cognitive_account.main.primary_access_key
}
