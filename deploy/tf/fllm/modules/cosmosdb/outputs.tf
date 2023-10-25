output "endpoint" {
  description = "The Cosmos Account Endpoint."
  value       = azurerm_cosmosdb_account.main.endpoint
}

output "id" {
  description = "The Cosmos Account Resource ID."
  value       = azurerm_cosmosdb_account.main.id
}

output "key" {
  description = "The Cosmos Account Primary Key."
  value       = azurerm_cosmosdb_account.main.primary_key
}