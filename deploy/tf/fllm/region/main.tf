locals {
  hostname           = "www.internal.foundationallm.ai"
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = merge(var.tags, { workspace = terraform.workspace })
  vnet_address_space = var.vnet_address_space
}

# Data Sources
data "azurerm_client_config" "current" {}


# Resources

# TODO: need principal ID for the following
# resource "azurerm_role_assignment" "storgage_blob_data_contributor_diagnostic_services" {
#   principal_id         = "562db366-1b96-45d2-aa4a-f2148cef2240"
#   role_definition_name = "Storage Blob Data Contributor"
#   scope                = azurerm_resource_group.rgs["OPS"].id
# }

# Modules








module "backend_aks" {
  source = "./modules/aks"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  agw_id                     = module.application_gateway.id
  aks_admin_object_id        = var.sql_admin_ad_group.object_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["APP"]
  resource_prefix            = "${local.resource_prefix}-BACKEND"
  tags                       = local.tags

  private_endpoint = {
    subnet = azurerm_subnet.subnets["FLLMServices"]
    private_dns_zone_ids = {
      aks = [
        var.private_dns_zones["privatelink.eastus.azmk8s.io"].id,
      ]
    }
  }
}

module "data_storage" {
  source = "./modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = "${local.resource_prefix}-DATA"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = {
      blob  = [var.private_dns_zones["privatelink.blob.core.windows.net"].id]
      dfs   = [var.private_dns_zones["privatelink.dfs.core.windows.net"].id]
      file  = [var.private_dns_zones["privatelink.file.core.windows.net"].id]
      queue = [var.private_dns_zones["privatelink.queue.core.windows.net"].id]
      table = [var.private_dns_zones["privatelink.table.core.windows.net"].id]
      web   = [var.private_dns_zones["privatelink.azurewebsites.net"].id]
    }
  }
}

module "content_safety" {
  source = "./modules/content-safety"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["OPS"]
  resource_prefix            = "${local.resource_prefix}-FLLM"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.cognitiveservices.azure.com"].id,
    ]
  }
}

module "cosmosdb" {
  source = "./modules/cosmosdb"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["FLLMStorage"]
  resource_prefix            = "${local.resource_prefix}-FLLM"
  tags                       = local.tags

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
    subnet_id = azurerm_subnet.subnets["FLLMStorage"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.documents.azure.com"].id,
    ]
  }
}

module "container_registry" {
  source = "./modules/container-registry"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["OPS"]
  resource_prefix            = local.resource_prefix
  tags                       = local.tags

  #     private_endpoint = {
  #     subnet_id = azurerm_subnet.subnets["Services"].id
  #     private_dns_zone_ids = [
  #       var.private_dns_zones["privatelink.azurecr.io.
  # {regionName}.privatelink.azurecr.io"].id,
  #     ]
  #   }
}

module "data_cosmosdb" {
  source = "./modules/cosmosdb"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = "${local.resource_prefix}-DATA"
  tags                       = local.tags

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
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.documents.azure.com"].id,
    ]
  }
}

module "frontend_aks" {
  source = "./modules/aks"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  agw_id                     = module.application_gateway.id
  aks_admin_object_id        = var.sql_admin_ad_group.object_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["APP"]
  resource_prefix            = "${local.resource_prefix}-FRONTEND"
  tags                       = local.tags

  private_endpoint = {
    subnet = azurerm_subnet.subnets["FLLMFrontEnd"]
    private_dns_zone_ids = {
      aks = [
        var.private_dns_zones["privatelink.eastus.azmk8s.io"].id,
      ]
    }
  }
}

module "ha_openai" {
  source = "./modules/ha-openai"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  instance_count             = 4
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["OAI"]
  resource_prefix            = "${local.resource_prefix}-OAI"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMOpenAI"].id
    apim_private_dns_zones = [
      local.private_dns_zones["azure-api.net"],
      local.private_dns_zones["developer.azure-api.net"],
      local.private_dns_zones["management.azure-api.net"],
      local.private_dns_zones["portal.azure-api.net"],
      local.private_dns_zones["scm.azure-api.net"]
    ]
    kv_private_dns_zone_ids = [
      local.private_dns_zones["privatelink.vaultcore.azure.net"].id
    ]
    openai_private_dns_zone_ids = [
      local.private_dns_zones["privatelink.openai.azure.com"].id
    ]
  }
}





module "search" {
  source = "./modules/search"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["VEC"]
  resource_prefix            = local.resource_prefix
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Vectorization"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.search.windows.net"].id,
    ]
  }
}

module "sql" {
  source = "./modules/mssql-server"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["Data"]
  resource_prefix            = local.resource_prefix
  sql_admin_object_id        = var.sql_admin_ad_group.object_id
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Datasources"].id
    private_dns_zone_ids = [
      var.private_dns_zones["privatelink.database.windows.net"].id,
    ]
  }
}

module "storage" {
  source = "./modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["FLLMStorage"]
  resource_prefix            = "${local.resource_prefix}-FLLM-prompt"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["FLLMStorage"].id
    private_dns_zone_ids = {
      blob  = [var.private_dns_zones["privatelink.blob.core.windows.net"].id]
      dfs   = [var.private_dns_zones["privatelink.dfs.core.windows.net"].id]
      file  = [var.private_dns_zones["privatelink.file.core.windows.net"].id]
      queue = [var.private_dns_zones["privatelink.queue.core.windows.net"].id]
      table = [var.private_dns_zones["privatelink.table.core.windows.net"].id]
      web   = [var.private_dns_zones["privatelink.azurewebsites.net"].id]
    }
  }
}

module "storage_ops" {
  source = "./modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rgs["OPS"]
  resource_prefix            = "${local.resource_prefix}-ops"
  tags                       = local.tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnets["Services"].id
    private_dns_zone_ids = {
      blob  = [var.private_dns_zones["privatelink.blob.core.windows.net"].id]
      dfs   = [var.private_dns_zones["privatelink.dfs.core.windows.net"].id]
      file  = [var.private_dns_zones["privatelink.file.core.windows.net"].id]
      queue = [var.private_dns_zones["privatelink.queue.core.windows.net"].id]
      table = [var.private_dns_zones["privatelink.table.core.windows.net"].id]
      web   = [var.private_dns_zones["privatelink.azurewebsites.net"].id]
    }
  }
}
