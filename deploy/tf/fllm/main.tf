locals {
  resource_prefix         = { for k, _ in local.resource_group : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }
  resource_prefix_backend = { for k, _ in local.resource_group_backend : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }

  resource_group = {
    agw = {
      tags = {
        "Purpose" = "Networking"
      }
    }
  }

  resource_group_backend = {
    dns = null
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
data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = "GLB-FLLM-DEMO-DNS-rg"
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


# locals {
#   resource_prefix = join("-", [local.project_id, local.environment])



#   regional_resource_groups = {
#     "APP" = {
#       tags = {
#         "Purpose" = "Application"
#       }
#     }

#     "Data" = {
#       tags = {
#         "Purpose" = "Storage"
#       }
#     }
#     "FLLMStorage" = {
#       tags = {
#         Purpose = "Storage"
#       }
#     }
#     "JBX" = {
#       tags = {
#         "Purpose" = "DevOps"
#       }
#     }
#     "NET" = {
#       tags = {
#         "Purpose" = "Networking"
#       }
#     }
#     "OAI" = {
#       tags = {
#         "Purpose" = "OpenAI"
#       }
#     }
#     "OPS" = {
#       tags = {
#         "Purpose" = "DevOps"
#       }
#     }
#     "VEC" = {
#       tags = {
#         "Purpose" = "Vectorization"
#       }
#     }
#   }

#   regions = {
#     "East US" = {
#       location_short     = "EUS"
#       vnet_address_space = "10.0.0.0/16"
#     }
#   }

#   tags = merge(var.tags, {
#     "Project"     = local.project_id
#     "Environment" = local.environment
#   })
# }

# module "global" {
#   source = "./global"

#   location        = local.global_location
#   public_domain   = local.public_domain
#   resource_groups = local.global_resource_groups
#   resource_prefix = local.resource_prefix
#   tags            = local.tags
# }

# module "regions" {
#   source   = "./region"
#   for_each = local.regions

#   location           = each.key
#   location_short     = each.value.location_short
#   private_dns_zones  = module.global.private_dns_zones
#   public_dns_zone    = module.global.public_dns_zone
#   resource_groups    = local.regional_resource_groups
#   resource_prefix    = local.resource_prefix
#   tags               = local.tags
#   tfc_agent_token    = var.tfc_agent_token
#   vnet_address_space = each.value.vnet_address_space
# }