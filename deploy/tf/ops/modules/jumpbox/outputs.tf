output "administrator_username" {
  description = "The administrator username for the jumpbox"
  value       = random_id.user.hex
}

output "administrator_password" {
  description = "The administrator password for the jumpbox"
  sensitive   = true
  value       = random_password.password.result
}

output "id" {
  description = "The ID of the jumpbox"
  value       = azurerm_windows_virtual_machine.main.id
}