output "id" {
  description = "The AMPLS ID"
  value       = azurerm_monitor_private_link_scope.main.id
}

output "name" {
  description = "The AMPLS name"
  value       = azurerm_monitor_private_link_scope.main.name
}

output "resource_group_name" {
  description = "The AMPLS resource group name"
  value       = azurerm_monitor_private_link_scope.main.resource_group_name
}