output "id" {
  description = "The Search Service Resource ID."
  value       = azurerm_search_service.main.id
}

output "key" {
  description = "The Search Service Key."
  value       = azurerm_search_service.main.primary_key
}