locals {
  resource_prefix         = { for k, _ in local.resource_group : k => join("-", [local.location_short, var.project_id, var.environment, upper(k)]) }
  resource_prefix_backend = { for k in local.resource_group_backend : k => join("-", [local.location_short, var.project_id, var.environment, upper(k)]) }
  resource_group_backend  = ["dns", "net", "ops"]

  location_short = local.location_shorts[var.location]
  location_shorts = {
    eastus = "EUS"
  }

  resource_group = {
    data = { tags = { "Purpose" = "Storage" } }
  }

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
    "Workspace"   = terraform.workspace
  }
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg"
  to = azurerm_resource_group.rg["data"]
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb"
  to = module.cosmosdb_data.azurerm_cosmosdb_account.main
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_database.db
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database"
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_container.container["completions"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database/containers/completions"
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_container.container["customer"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database/containers/customer"
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_container.container["embedding"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database/containers/embedding"
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_container.container["leases"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database/containers/leases"
}

import {
  to = module.cosmosdb_data.azurerm_cosmosdb_sql_container.container["product"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb/sqlDatabases/database/containers/product"
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/elasticPools/EUS-FLLM-DEMO-DATA-mssql-ep"
  to = module.sql.azurerm_mssql_elasticpool.pool
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql"
  to = module.sql.azurerm_mssql_server.main
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Storage/storageAccounts/eusfllmdemodatasa"
  to = module.storage_data.azurerm_storage_account.main
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Insights/metricAlerts/EUS-FLLM-DEMO-DATA-cdb-availability-alert"
  to = module.cosmosdb_data.azurerm_monitor_metric_alert.alert["availability"]
}

import {
  to = module.cosmosdb_data.azurerm_private_endpoint.ple
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-sql-pe"
}

import {
  to = module.cosmosdb_data.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["cdb"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.DocumentDB/databaseAccounts/eus-fllm-demo-data-cdb|diag-cdb"
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/sqlVulnerabilityAssessments/default?api-version=2023-05-01-preview"
  to = module.sql.azapi_resource.vulnerability_assessment
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Insights/metricAlerts/EUS-FLLM-DEMO-DATA-mssql-cpu-alert"
  to = module.sql.azurerm_monitor_metric_alert.alert["cpu"]
}

import {
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/databases/master/extendedAuditingSettings/default"
  to = module.sql.azurerm_mssql_database_extended_auditing_policy.master
}

import {
  to = module.sql.azurerm_mssql_server_extended_auditing_policy.server
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/extendedAuditingSettings/default"
}

import {
  to = module.sql.azurerm_private_endpoint.ple
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-sqlServer-pe"
}

import {
  to = module.sql.azurerm_mssql_firewall_rule.allow_azure_services
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/firewallRules/AllowAzureServices"
}

import {
  to = module.sql.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["audits"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/databases/master|diag-audits"
}

import {
  to = module.sql.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["mssql"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql|diag-mssql"
}

import {
  to = module.sql.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["pool"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Sql/servers/eusfllmdemodatamssql/elasticPools/EUS-FLLM-DEMO-DATA-mssql-ep|diag-pool"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["blob"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-blob-pe"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["dfs"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-dfs-pe"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["file"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-file-pe"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["queue"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-queue-pe"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["table"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-table-pe"
}

import {
  to = module.storage_data.azurerm_private_endpoint.ple["web"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Network/privateEndpoints/EUS-FLLM-DEMO-DATA-web-pe"
}

import {
  to = module.storage_data.azurerm_monitor_metric_alert.alert["availability"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Insights/metricAlerts/EUS-FLLM-DEMO-DATA-sa-availability-alert"
}

import {
  to = module.storage_data.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["blobs"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Storage/storageAccounts/eusfllmdemodatasa/blobServices/default/|diag-blobs"
}

import {
  to = module.storage_data.module.diagnostics.azurerm_monitor_diagnostic_setting.setting["files"]
  id = "/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/EUS-FLLM-DEMO-DATA-rg/providers/Microsoft.Storage/storageAccounts/eusfllmdemodatasa/fileServices/default/|diag-files"
}


# Data Sources
data "azurerm_client_config" "current" {}

data "azurerm_log_analytics_workspace" "logs" {
  name                = "${local.resource_prefix_backend["ops"]}-la"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_monitor_action_group" "do_nothing" {
  name                = "${local.resource_prefix_backend["ops"]}-ag"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_private_dns_zone" "private_dns" {
  for_each = {
    blob       = "privatelink.blob.core.windows.net"
    cosmosdb   = "privatelink.documents.azure.com"
    dfs        = "privatelink.dfs.core.windows.net"
    file       = "privatelink.file.core.windows.net"
    queue      = "privatelink.queue.core.windows.net"
    sites      = "privatelink.azurewebsites.net"
    sql_server = "privatelink.database.windows.net"
    table      = "privatelink.table.core.windows.net"
  }

  name                = each.value
  resource_group_name = data.azurerm_resource_group.backend["dns"].name
}

data "azurerm_resource_group" "backend" {
  for_each = toset(local.resource_group_backend)

  name = "${local.resource_prefix_backend[each.key]}-rg"
}

data "azurerm_subnet" "subnet" {
  for_each = toset(["Datasources", ])

  name                 = each.key
  resource_group_name  = data.azurerm_virtual_network.network.resource_group_name
  virtual_network_name = data.azurerm_virtual_network.network.name
}

data "azurerm_virtual_network" "network" {
  name                = "${local.resource_prefix_backend["net"]}-vnet"
  resource_group_name = data.azurerm_resource_group.backend["net"].name
}

# Resources
resource "azurerm_resource_group" "rg" {
  for_each = local.resource_group

  location = var.location
  name     = "${local.resource_prefix[each.key]}-rg"
  tags     = merge(each.value.tags, local.tags)
}

# Modules
module "cosmosdb_data" {
  source = "../modules/cosmosdb"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["data"]
  resource_prefix            = local.resource_prefix["data"]
  tags                       = azurerm_resource_group.rg["data"].tags

  containers = {
    embedding = {
      partition_key_path = "/id"
      max_throughput     = 1000
    }
    completions = {
      partition_key_path = "/sessionId"
      max_throughput     = 1000
    }
    product = {
      partition_key_path = "/categoryId"
      max_throughput     = 1000
    }
    customer = {
      partition_key_path = "/customerId"
      max_throughput     = 1000
    }
    leases = {
      partition_key_path = "/id"
      max_throughput     = 1000
    }
  }

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["Datasources"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["cosmosdb"].id,
    ]
  }
}

module "sql" {
  source = "../modules/mssql-server"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["data"]
  resource_prefix            = local.resource_prefix["data"]
  tags                       = azurerm_resource_group.rg["data"].tags

  azuread_administrator = {
    id   = var.sql_azuread_administrator.object_id
    name = var.sql_azuread_administrator.name
  }

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["Datasources"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["sql_server"].id,
    ]
  }
}

module "storage_data" {
  source = "../modules/storage-account"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["data"]
  resource_prefix            = local.resource_prefix["data"]
  subscription_id            = data.azurerm_client_config.current.subscription_id
  tags                       = azurerm_resource_group.rg["data"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["Datasources"].id
    private_dns_zone_ids = {
      blob  = [data.azurerm_private_dns_zone.private_dns["blob"].id]
      dfs   = [data.azurerm_private_dns_zone.private_dns["dfs"].id]
      file  = [data.azurerm_private_dns_zone.private_dns["file"].id]
      queue = [data.azurerm_private_dns_zone.private_dns["queue"].id]
      table = [data.azurerm_private_dns_zone.private_dns["table"].id]
      web   = [data.azurerm_private_dns_zone.private_dns["sites"].id]
    }
  }
}
