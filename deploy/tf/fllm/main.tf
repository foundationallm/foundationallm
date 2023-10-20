locals {
  resource_prefix = { for k, _ in local.resource_group : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }

  resource_group = {
    dns = null
  }

  short_location = local.short_locations[var.location]
  short_locations = {
    eastus = "EUS"
  }
}

# Data Sources
data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = "GLB-FLLM-DEMO-DNS-rg"
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
  resource_group_name = data.azurerm_resource_group.rg["dns"].name
}

data "azurerm_resource_group" "rg" {
  for_each = toset(["dns"])

  name = "${local.resource_prefix[each.key]}-rg"
}

# Resources
# Modules

# locals {
#   environment     = var.environment
#   global_location = var.global_location
#   project_id      = var.project_id
#   public_domain   = var.public_domain
#   resource_prefix = join("-", [local.project_id, local.environment])

#   global_resource_groups = {
#     "DNS" = {
#       tags = {
#         "Purpose" = "Networking"
#       }
#     }
#   }

#   regional_resource_groups = {
#     "APP" = {
#       tags = {
#         "Purpose" = "Application"
#       }
#     }
#     "AppGateway" = {
#       tags = {
#         "Purpose" = "Networking"
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