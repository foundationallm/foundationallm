locals {
  alert = {
    availability = {
      aggregation = "Average"
      description = "Service availability less than 99% for 1 hour"
      frequency   = "PT1M"
      metric_name = "Availability"
      operator    = "LessThan"
      threshold   = 99
      window_size = "PT1H"
      severity    = 0
    }
    saturation = {
      aggregation = "Average"
      description = "Service saturation more than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "SaturationShoebox"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
    latency = {
      aggregation = "Average"
      description = "Service latency more than 1000ms for 1 hour"
      frequency   = "PT1M"
      metric_name = "ServiceApiLatency"
      operator    = "GreaterThan"
      threshold   = 1000
      window_size = "PT1H"
      severity    = 0
    }
  }
}

resource "azurerm_key_vault" "main" {
  enable_rbac_authorization     = true
  location                      = var.resource_group.location
  name                          = "${var.resource_prefix}-kv"
  public_network_access_enabled = false
  purge_protection_enabled      = true
  resource_group_name           = var.resource_group.name
  sku_name                      = "standard"
  soft_delete_retention_days    = 7
  tenant_id                     = var.tenant_id
  tags                          = var.tags
}


resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-kv-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_key_vault.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.KeyVault/vaults"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-kv-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "vaultcore"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-kv-connection"
    private_connection_resource_id = azurerm_key_vault.main.id
    subresource_names              = ["vault"]
  }
}

# Modules
module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    kv = {
      id = azurerm_key_vault.main.id
    }
  }
}
