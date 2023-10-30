output "jbx_username" {
  value = module.jumpbox.administrator_username
  sensitive = true
}

output "jbx_password" {
  value = module.jumpbox.administrator_password
  sensitive = true
}