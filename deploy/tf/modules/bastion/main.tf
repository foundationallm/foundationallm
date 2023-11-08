resource "azurerm_bastion_host" "main" {
  copy_paste_enabled  = true
  file_copy_enabled   = true
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-bh"
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
  tags                = var.tags

  ip_configuration {
    name                 = "default"
    public_ip_address_id = azurerm_public_ip.pip.id
    subnet_id            = var.subnet_id
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

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    bh = {
      id = azurerm_bastion_host.main.id
    }
  }
}
