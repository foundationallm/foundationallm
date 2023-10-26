resource "azurerm_bastion_host" "main" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-bh"
  resource_group_name = var.resource_group.name
  tags                = var.tags
  sku = "Standard"

  ip_configuration {
    name                 = "default"
    subnet_id            = var.subnet_id
    public_ip_address_id = azurerm_public_ip.pip.id
  }
}

resource "azurerm_public_ip" "pip" {
  allocation_method   = "Static"
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-pip"
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
  tags                = var.tags
}