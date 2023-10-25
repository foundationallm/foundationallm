output "endpoint" {
  description = "The Search Service Endpoint."
  value       = "https://${azurerm_search_service.main.name}.search.windows.net"
}

output "id" {
  description = "The Search Service Resource ID."
  value       = azurerm_search_service.main.id
}

output "key" {
  description = "The Search Service Key."
  value       = azurerm_search_service.main.primary_key
}