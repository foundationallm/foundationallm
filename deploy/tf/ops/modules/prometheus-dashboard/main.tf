resource "azurerm_dashboard_grafana" "main" {
  # public_network_access_enabled = false
  location            = var.resource_group.location
  name                = lower("${var.resource_prefix}gd")
  resource_group_name = var.resource_group.name
  tags                = var.tags

  identity {
    type = "SystemAssigned"
  }

  azure_monitor_workspace_integrations {
    resource_id = var.azure_monitor_workspace_id
  }
}

resource "azurerm_role_assignment" "datareaderrole" {
  scope              = var.azure_monitor_workspace_id
  role_definition_id = "/subscriptions/${split("/", var.azure_monitor_workspace_id)[2]}/providers/Microsoft.Authorization/roleDefinitions/b0d8363b-8ddd-447d-831f-62ca05bff136"
  principal_id       = azurerm_dashboard_grafana.main.identity.0.principal_id
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-grafana-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "grafana"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-grafana-connection"
    private_connection_resource_id = azurerm_dashboard_grafana.main.id
    subresource_names              = ["grafana"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    db = {
      id = azurerm_dashboard_grafana.main.id
    }
  }
}
