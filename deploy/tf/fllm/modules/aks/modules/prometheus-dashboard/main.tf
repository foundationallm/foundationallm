resource "azurerm_dashboard_grafana" "grafana" {
  name                = "${var.resource_prefix}-gd" # TODO: make this shorter
  resource_group_name = var.resource_group.name
  location            = var.resource_group.location

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
  principal_id       = azurerm_dashboard_grafana.grafana.identity.0.principal_id
}