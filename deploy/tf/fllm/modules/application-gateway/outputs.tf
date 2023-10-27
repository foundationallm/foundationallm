output "id" {
  description = "The Application Gateway Resource ID."
  value       = azurerm_application_gateway.main.id
}

output "identity_id" {
  description = "The Application Gateway identity."
  value       = azurerm_application_gateway.main.identity[0]
}