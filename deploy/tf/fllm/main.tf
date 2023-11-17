locals {
  azure_monitor_workspace_id = jsondecode(data.azapi_resource.amw.output).id
  resource_prefix            = { for k, _ in local.resource_group : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }
  resource_prefix_compact    = { for k, _ in local.resource_group : k => join("-", [local.location_compact, var.project_id, local.environment_compact, upper(k)]) }
  resource_prefix_backend    = { for k, _ in local.resource_group_backend : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }

  environment_compact = local.environment_compacts[var.environment]
  environment_compacts = {
    DEMO = "d"
  }

  location_compact = local.location_compacts[var.location]
  location_compacts = {
    eastus = "e"
  }

  resource_group = {
    agw     = { tags = { "Purpose" = "Networking" } }
    app     = { tags = { "Purpose" = "Application" } }
    oai     = { tags = { "Purpose" = "OpenAI" } }
    storage = { tags = { "Purpose" = "Storage" } }
    vec     = { tags = { "Purpose" = "Vectorization" } }
  }

  resource_group_backend = {
    dns = null
    net = null
    ops = null
  }

  role_agw_mi = {
    vault = {
      role  = "Key Vault Secrets User"
      scope = data.azurerm_resource_group.backend["ops"].id
    }
    network = {
      role  = "Contributor"
      scope = data.azurerm_virtual_network.network.id
    }
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
data "azapi_resource" "amw" {
  name                   = "${local.resource_prefix_backend["ops"]}-amw"
  parent_id              = data.azurerm_resource_group.backend["ops"].id
  response_export_values = ["*"]
  type                   = "Microsoft.Monitor/accounts@2023-04-03"
}

data "azurerm_application_insights" "ai" {
  name                = "${local.resource_prefix_backend["ops"]}-ai"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_app_configuration" "appconfig" {
  name                = "${local.resource_prefix_backend["ops"]}-appconfig"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_client_config" "current" {}

data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = "GLB-FLLM-DEMO-DNS-rg"
}

data "azurerm_key_vault" "keyvault_ops" {
  name                = "${local.resource_prefix_backend["ops"]}-kv"
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_key_vault_certificate" "agw" {
  for_each = toset(["api", "www"])

  key_vault_id = data.azurerm_key_vault.keyvault_ops.id
  name         = replace("${each.key}.${var.public_domain}", ".", "-")
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

data "azurerm_storage_account" "storage_ops" {
  name                = lower(replace("${local.resource_prefix_backend["ops"]}-sa", "-", ""))
  resource_group_name = data.azurerm_resource_group.backend["ops"].name
}

data "azurerm_subnet" "subnet" {
  for_each = toset([
    "AppGateway",
    "Datasources",
    "FLLMBackend",
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

resource "azurerm_role_assignment" "role_agw_mi" {
  for_each = local.role_agw_mi

  principal_id         = azurerm_user_assigned_identity.agw.principal_id
  role_definition_name = each.value.role
  scope                = each.value.scope
}

resource "azurerm_user_assigned_identity" "agw" {
  location            = azurerm_resource_group.rg["agw"].location
  name                = "${local.resource_prefix["agw"]}-agw-uai"
  resource_group_name = azurerm_resource_group.rg["agw"].name
  tags                = azurerm_resource_group.rg["agw"].tags
}

# Modules
module "aks_backend" {
  source = "../modules/aks"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  application_gateway        = module.application_gateway["api"]
  azure_monitor_workspace_id = local.azure_monitor_workspace_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["app"]
  resource_prefix            = "${local.resource_prefix["app"]}-BACKEND"
  subnet_id                  = data.azurerm_subnet.subnet["FLLMBackend"].id
  tags                       = azurerm_resource_group.rg["app"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  administrator_object_ids = [
    data.azurerm_client_config.current.object_id,
    var.sql_admin_ad_group.object_id
  ]

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["FLLMServices"].id
    private_dns_zone_ids = {
      aks = [
        data.azurerm_private_dns_zone.private_dns["aks"].id,
      ]
    }
  }
}

module "aks_frontend" {
  source = "../modules/aks"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  application_gateway        = module.application_gateway["www"]
  azure_monitor_workspace_id = local.azure_monitor_workspace_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["app"]
  resource_prefix            = "${local.resource_prefix["app"]}-FRONTEND"
  subnet_id                  = data.azurerm_subnet.subnet["FLLMFrontEnd"].id
  tags                       = azurerm_resource_group.rg["app"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  administrator_object_ids = [
    data.azurerm_client_config.current.object_id,
    var.sql_admin_ad_group.object_id
  ]

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["FLLMServices"].id
    private_dns_zone_ids = {
      aks = [
        data.azurerm_private_dns_zone.private_dns["aks"].id,
      ]
    }
  }
}

module "application_gateway" {
  source     = "../modules/application-gateway-ingress-controller"
  depends_on = [azurerm_role_assignment.role_agw_mi]
  for_each   = toset(["api", "www", ])

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  hostname                   = each.key
  identity                   = azurerm_user_assigned_identity.agw
  key_vault_secret_id        = data.azurerm_key_vault_certificate.agw[each.key].versionless_secret_id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  public_dns_zone            = data.azurerm_dns_zone.public_dns
  resource_group             = azurerm_resource_group.rg["agw"]
  resource_prefix            = "${local.resource_prefix["agw"]}-${each.key}"
  subnet_id                  = data.azurerm_subnet.subnet["AppGateway"].id
  tags                       = azurerm_resource_group.rg["agw"].tags
}

module "content_safety" {
  source = "../modules/content-safety"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["oai"]
  resource_prefix            = local.resource_prefix["oai"]
  tags                       = azurerm_resource_group.rg["oai"].tags

  private_endpoint = {
    subnet_id = data.azurerm_subnet.subnet["FLLMOpenAI"].id
    private_dns_zone_ids = [
      data.azurerm_private_dns_zone.private_dns["cognitiveservices"].id,
    ]
  }
}

module "cosmosdb" {
  source = "../modules/cosmosdb"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["storage"]
  resource_prefix            = local.resource_prefix["storage"]
  tags                       = azurerm_resource_group.rg["storage"].tags

  containers = {
    UserSessions = {
      partition_key_path = "/upn"
      max_throughput     = 1000
    }
    Sessions = {
      partition_key_path = "/sessionId"
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

module "openai_ha" {
  source = "../modules/ha-openai"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  instance_count             = 2
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["oai"]
  resource_prefix            = local.resource_prefix["oai"]
  subnet_id                  = data.azurerm_subnet.subnet["FLLMOpenAI"].id
  tags                       = azurerm_resource_group.rg["oai"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

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
  source = "../modules/search"

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

module "storage" {
  source = "../modules/storage-account"

  action_group_id            = data.azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = data.azurerm_log_analytics_workspace.logs.id
  resource_group             = azurerm_resource_group.rg["storage"]
  resource_prefix            = local.resource_prefix_compact["storage"]
  subscription_id            = data.azurerm_client_config.current.subscription_id
  tags                       = azurerm_resource_group.rg["storage"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

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
