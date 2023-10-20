

data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = azurerm_resource_group.rgs["DNS"].name
}