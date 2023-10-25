locals {
  resource_prefix         = { for k, _ in local.resource_group : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }
  resource_prefix_compact = { for k, _ in local.resource_group : k => join("-", [local.location_compact, var.project_id, local.environment_compact, upper(k)]) }
  resource_prefix_backend = { for k, _ in local.resource_group_backend : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }

  environment_compact = local.environment_compacts[var.environment]
  environment_compacts = {
    DEMO = "d"
  }

  location_compact = local.location_compacts[var.location]
  location_compacts = {
    eastus = "e"
  }

  resource_group = {
    agw = {
      tags = {
        "Purpose" = "Networking"
      }
    }
    app = {
      tags = {
        "Purpose" = "Application"
      }
    }
    data = {
      tags = {
        "Purpose" = "Storage"
      }
    }
    storage = {
      tags = {
        Purpose = "Storage"
      }
    }
    oai = {
      tags = {
        "Purpose" = "OpenAI"
      }
    }
    vec = {
      tags = {
        "Purpose" = "Vectorization"
      }
    }
  }

  resource_group_backend = {
    dns = null
    net = null
    ops = null
  }

  short_location = local.short_locations[var.location]
  short_locations = {
    eastus = "EUS"
  }

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
    "Workspace"   = terraform.workspace
  }
}

# Data Sources
data "azurerm_application_insights" "ai" {
  name                = "${local.resource_prefix_backend["ops"]}-ai"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_app_configuration" "appconfig" {
  name                = "${local.resource_prefix_backend["ops"]}-appconfig"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_cognitive_account" "content_safety" {
  name                = "${local.resource_prefix_backend["ops"]}-content-safety"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = "GLB-FLLM-DEMO-DNS-rg"
}

data "azurerm_key_vault" "keyvault_ops" {
  name                = "${local.resource_prefix_backend["ops"]}-kv"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_key_vault_certificate" "agw" {
  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  name         = replace("www.${var.public_domain}", ".", "-")
}

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
    aks                  = "privatelink.${var.location}.azmk8s.io"
    blob                 = "privatelink.blob.core.windows.net"
    cognitiveservices    = "privatelink.cognitiveservices.azure.com"
    configuration_stores = "privatelink.azconfig.io"
    cosmosdb             = "privatelink.documents.azure.com"
    cr                   = "privatelink.azurecr.io"
    cr_region            = "${var.location}.privatelink.azurecr.io"
    dfs                  = "privatelink.dfs.core.windows.net"
    file                 = "privatelink.file.core.windows.net"
    gateway              = "privatelink.azure-api.net"
    gateway_developer    = "developer.azure-api.net"
    gateway_management   = "management.azure-api.net"
    gateway_portal       = "portal.azure-api.net"
    gateway_public       = "azure-api.net"
    gateway_scm          = "scm.azure-api.net"
    monitor              = "privatelink.monitor.azure.com"
    openai               = "privatelink.openai.azure.com"
    queue                = "privatelink.queue.core.windows.net"
    search               = "privatelink.search.windows.net"
    sites                = "privatelink.azurewebsites.net"
    sql_server           = "privatelink.database.windows.net"
    table                = "privatelink.table.core.windows.net"
    vault                = "privatelink.vaultcore.azure.net"
  }

  name                = each.value
  resource_group_name = data.azurerm_resource_group.backend["dns"].name
}

data "azurerm_resource_group" "backend" {
  for_each = local.resource_group_backend

  name = "${local.resource_prefix_backend[each.key]}-rg"
}

data "azurerm_subnet" "subnet" {
  for_each = toset([
    "AppGateway",
    "Datasources",
    "FLLMFrontEnd",
    "FLLMOpenAI",
    "FLLMServices",
    "FLLMStorage",
    "Vectorization",
    "ado",
    "jumpbox",
    "ops",
    "tfc",
  ])

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

resource "azurerm_role_assignment" "keyvault_secrets_user_agw" {
  principal_id         = azurerm_user_assigned_identity.agw.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = data.azurerm_resource_group.backend["ops"].id
}

resource "azurerm_user_assigned_identity" "agw" {
  location            = azurerm_resource_group.rg["agw"].location
  name                = "${local.resource_prefix["agw"]}-agw-uai"
  resource_group_name = azurerm_resource_group.rg["agw"].name
  tags                = azurerm_resource_group.rg["agw"].tags
}

# Modules
module "aks_backend" {
  source = "./modules/aks"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  agw_id                     = module.application_gateway.id
  aks_admin_object_id        = var.sql_admin_ad_group.object_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["app"]
  resource_prefix            = "${local.resource_prefix["app"]}-BACKEND"
  tags                       = azurerm_resource_group.rg["app"].tags

  private_endpoint = {
    subnet = data.azurerm_subnet.subnet["FLLMServices"]
    private_dns_zone_ids = {
      aks = [
        data.azurerm_private_dns_zone.private_dns["aks"].id,
      ]
    }
  }
}

module "aks_frontend" {
  source = "./modules/aks"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  agw_id                     = module.application_gateway.id
  aks_admin_object_id        = var.sql_admin_ad_group.object_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["app"]
  resource_prefix            = "${local.resource_prefix["app"]}-FRONTEND"
  tags                       = azurerm_resource_group.rg["app"].tags

  private_endpoint = {
    subnet = data.azurerm_subnet.subnet["FLLMFrontEnd"]
    private_dns_zone_ids = {
      aks = [
        data.azurerm_private_dns_zone.private_dns["aks"].id,
      ]
    }
  }
}

module "application_gateway" {
  source     = "./modules/application-gateway"
  depends_on = [azurerm_role_assignment.keyvault_secrets_user_agw]

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  backend_pool_ip_addresses  = ["10.255.255.255"] # TODO: Replace with AKS frontend IP address
  hostname                   = "www.${var.public_domain}"
  identity_id                = azurerm_user_assigned_identity.agw.id
  key_vault_secret_id        = data.azurerm_key_vault_certificate.agw.versionless_secret_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  public_dns_zone            = data.azurerm_dns_zone.public_dns
  resource_group             = azurerm_resource_group.rg["agw"]
  resource_prefix            = local.resource_prefix["agw"]
  subnet_id                  = data.azurerm_subnet.subnet["AppGateway"].id
  tags                       = azurerm_resource_group.rg["agw"].tags
}

module "cosmosdb" {
  source = "./modules/cosmosdb"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["storage"]
  resource_prefix            = local.resource_prefix["storage"]
  tags                       = azurerm_resource_group.rg["storage"].tags

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
    subnet_id = data.azurerm_subnet.subnet["FLLMStorage"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["cosmosdb"].id,
    ]
  }
}

module "cosmosdb_data" {
  source = "./modules/cosmosdb"

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

module "openai_ha" {
  source = "./modules/ha-openai"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  instance_count             = 2
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["oai"]
  resource_prefix            = local.resource_prefix["oai"]
  tags                       = azurerm_resource_group.rg["oai"].tags

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["FLLMOpenAI"].id
    apim_private_dns_zones = [
      data.azurerm_private_dns_zone.private_dns["gateway_public"],
      data.azurerm_private_dns_zone.private_dns["gateway_developer"],
      data.azurerm_private_dns_zone.private_dns["gateway_management"],
      data.azurerm_private_dns_zone.private_dns["gateway_portal"],
      data.azurerm_private_dns_zone.private_dns["gateway_scm"]
    ]
    kv_private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["vault"].id
    ]
    openai_private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["openai"].id
    ]
  }
}

module "search" {
  source = "./modules/search"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["vec"]
  resource_prefix            = local.resource_prefix["vec"]
  tags                       = azurerm_resource_group.rg["vec"].tags

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["Vectorization"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["search"].id,
    ]
  }
}

module "sql" {
  source = "./modules/mssql-server"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["data"]
  resource_prefix            = local.resource_prefix["data"]
  sql_admin_object_id        = var.sql_admin_ad_group.object_id
  tags                       = azurerm_resource_group.rg["data"].tags

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["Datasources"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["sql_server"].id,
    ]
  }
}

module "storage" {
  source = "./modules/storage-account"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["storage"]
  resource_prefix            = "${local.resource_prefix_compact["storage"]}-prompt"
  tags                       = azurerm_resource_group.rg["storage"].tags

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["FLLMStorage"].id
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

module "storage_data" {
  source = "./modules/storage-account"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["data"]
  resource_prefix            = local.resource_prefix["data"]
  tags                       = azurerm_resource_group.rg["data"].tags

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
