locals {
  location        = "eastus"
  resource_prefix = local.test_namespace
  test_namespace  = random_pet.instance_id.id

  tags = {
    contact     = "nobody@example.com"
    environment = "sbx"
    location    = local.location
    repository  = "foundationallm/deploy/tf/modules/openai"
    workload    = "default"
  }
}

# Data Sources
data "azurerm_client_config" "current" {}

# Pre-requisite Resources
resource "azurerm_key_vault" "example" {
  enable_rbac_authorization   = true
  enabled_for_disk_encryption = true
  location                    = azurerm_resource_group.example.location
  name                        = "${local.test_namespace}-kv"
  purge_protection_enabled    = true # must be true for encryption
  resource_group_name         = azurerm_resource_group.example.name
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  tags                        = local.tags
  tenant_id                   = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_log_analytics_workspace" "example" {
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-la"
  resource_group_name = azurerm_resource_group.example.name
  retention_in_days   = 30
  sku                 = "PerGB2018"
  tags                = local.tags
}

resource "azurerm_monitor_action_group" "example" {
  name                = "Example Action Group"
  resource_group_name = azurerm_resource_group.example.name
  short_name          = "example"
  tags                = local.tags
}

resource "azurerm_private_dns_zone" "example" {
  for_each = {
    openai = "privatelink.openai.azure.com"
  }

  name                = each.value
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "azurerm_resource_group" "example" {
  location = local.location
  name     = "${local.test_namespace}-rg"
  tags     = local.tags
}

resource "azurerm_subnet" "example" {
  address_prefixes     = [cidrsubnet(azurerm_virtual_network.example.address_space.0, 1, 0)]
  name                 = "internal"
  resource_group_name  = azurerm_resource_group.example.name
  virtual_network_name = azurerm_virtual_network.example.name
}

resource "azurerm_virtual_network" "example" {
  address_space       = ["192.168.0.0/24"]
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-vnet"
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "random_pet" "instance_id" {}

# How to call the OpenAI module
module "example" {
  source = "../.."

  action_group_id            = azurerm_monitor_action_group.example.id
  instance_id                = 10
  key_vault_id               = azurerm_key_vault.example.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.example.id
  resource_group             = azurerm_resource_group.example
  resource_prefix            = local.resource_prefix
  tags                       = azurerm_resource_group.example.tags

  capacity = {
    completions = 1
    embeddings  = 1
  }

  private_endpoint = {
    subnet_id = azurerm_subnet.example.id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.example["openai"].id,
    ]
  }
}