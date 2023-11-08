output "administrator_username" {
  description = "The administrator username for the jumpbox instances."
  value       = random_id.user.hex
}

output "administrator_password" {
  description = "The administrator password for the jumpbox instances."
  sensitive   = true
  value       = random_password.password.result
}

output "id" {
  description = "The ID of the jumpbox scale set."
  value       = azurerm_windows_virtual_machine_scale_set.main.id
}