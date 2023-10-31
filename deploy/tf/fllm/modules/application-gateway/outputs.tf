output "id" {
  description = "The Application Gateway Resource ID."
  value       = azurerm_application_gateway.main.id
}

output "identity_id" {
  description = "The Application Gateway identity."
  value       = tolist(azurerm_application_gateway.main.identity[0].identity_ids)[0]
}

output "agw_subnet_id" {
  description = "The Application Gateway subnet ID."
  value       = var.subnet_id
}