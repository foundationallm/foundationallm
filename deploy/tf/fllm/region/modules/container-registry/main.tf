resource "azurerm_container_registry" "main" {
  location            = var.resource_group.location
  name                = replace("${var.resource_prefix}-cr", "-", "")
  resource_group_name = var.resource_group.name
  sku                 = "Basic"
  tags                = var.tags
  #   public_network_access_enabled = false
  #network_rule_set
}