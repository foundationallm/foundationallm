locals {
  database_id_master = "${azurerm_mssql_server.main.id}/databases/master"

  alert = {
    cpu = {
      aggregation = "Average"
      description = "Service CPU utilization greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "cpu_percent"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-mssql-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_mssql_elasticpool.pool.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Sql/servers/elasticpools"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_mssql_database_extended_auditing_policy" "master" {
  depends_on = [azurerm_mssql_server_extended_auditing_policy.server]

  database_id            = local.database_id_master
  log_monitoring_enabled = true
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_server" "main" {
  location                      = var.resource_group.location
  minimum_tls_version           = "1.2"
  name                          = lower(join("", split("-", "${var.resource_prefix}-mssql"))) # TODO: we don't need to remove the dashes
  public_network_access_enabled = true
  resource_group_name           = var.resource_group.name
  tags                          = var.tags
  version                       = "12.0"

  azuread_administrator {
    login_username              = var.azuread_administrator.name
    object_id                   = var.azuread_administrator.id
    azuread_authentication_only = false # TODO: set to false when we have apps that no longer use SQL auth
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_mssql_server_extended_auditing_policy" "server" {
  log_monitoring_enabled = true
  server_id              = azurerm_mssql_server.main.id
}

resource "azapi_resource" "vulnerability_assessment" {
  type      = "Microsoft.Sql/servers/sqlVulnerabilityAssessments@2023-05-01-preview"
  name      = "default"
  parent_id = azurerm_mssql_server.main.id
  body = jsonencode({
    properties = {
      state = "Enabled"
    }
  })
}

resource "azurerm_mssql_elasticpool" "pool" {
  license_type        = "LicenseIncluded"
  location            = var.resource_group.location
  max_size_gb         = 128
  name                = "${var.resource_prefix}-mssql-ep"
  resource_group_name = var.resource_group.name
  server_name         = azurerm_mssql_server.main.name
  tags                = var.tags

  sku {
    capacity = 4
    family   = "Gen5"
    name     = "GP_Gen5"
    tier     = "GeneralPurpose"
  }

  per_database_settings {
    max_capacity = 4
    min_capacity = 0.25
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-sqlServer-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "sqlServer"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-sql-connection"
    private_connection_resource_id = azurerm_mssql_server.main.id
    subresource_names              = ["SqlServer"]
  }
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    audits = { id = local.database_id_master }
    mssql  = { id = azurerm_mssql_server.main.id }
    pool   = { id = azurerm_mssql_elasticpool.pool.id }

  }
}


# TODO: automatic tuning