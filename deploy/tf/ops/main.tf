locals {
  network_cidr = "10.0.0.0/16"

  # Reserve this range for OPS subnets
  # 10.0.252.0/22,10.0.252.0-10.0.255.255,1024 addresses
  ops_parent_cidr = cidrsubnet(local.network_cidr, 6, 63)

  resource_prefix = { for k, _ in local.resource_group : k => join("-", [local.short_location, var.project_id, var.environment, upper(k)]) }

  address_prefix = {
    ado = cidrsubnet(local.ops_parent_cidr, 5, 29)
    ops = cidrsubnet(local.ops_parent_cidr, 5, 27)
    tfc = cidrsubnet(local.ops_parent_cidr, 5, 31)
  }

  default_nsg_rules = {
    inbound = {
      "allow-ado-agents-inbound" = {
        access                     = "Allow"
        destination_address_prefix = "VirtualNetwork"
        destination_port_range     = "*"
        priority                   = 4094
        protocol                   = "Tcp"
        source_address_prefix      = local.address_prefix["ado"]
        source_port_range          = "*"
      }
      "allow-tfc-agents-inbound" = {
        access                     = "Allow"
        destination_address_prefix = "VirtualNetwork"
        destination_port_range     = "*"
        priority                   = 4095
        protocol                   = "*"
        source_address_prefix      = local.address_prefix["tfc"]
        source_port_range          = "*"
      }
      "deny-all-inbound" = {
        access                     = "Deny"
        destination_address_prefix = "*"
        destination_port_range     = "*"
        priority                   = 4096
        protocol                   = "*"
        source_address_prefix      = "*"
        source_port_range          = "*"
      }
    }
    outbound = {
      "deny-all-outbound" = {
        access                     = "Deny"
        destination_address_prefix = "*"
        destination_port_range     = "*"
        priority                   = 4096
        protocol                   = "*"
        source_address_prefix      = "*"
        source_port_range          = "*"
      }
    }
  }

  private_dns_zone = {
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

  resource_group = {
    dns = {
      tags = {
        "Purpose" = "Networking"
      }
    }
    net = {
      tags = {
        "Purpose" = "Networking"
      }
    }
    ops = {
      tags = {
        "Purpose" = "DevOps"
      }
    }
  }

  short_location = local.short_locations[var.location]
  short_locations = {
    eastus = "EUS"
  }

  subnet = {
    ado = {
      address_prefix = local.address_prefix["ado"]
      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-rdp-services" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3389"
            priority                   = 256
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-ado-services" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-ntp" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "123"
            priority                   = 257
            protocol                   = "Udp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 4068
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
    ops = {
      address_prefix = local.address_prefix["ops"]
      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    tfc = {
      address_prefix = local.address_prefix["tfc"]
      delegation = {
        "Microsoft.ContainerInstance/containerGroups" = [
          "Microsoft.Network/virtualNetworks/subnets/action"
        ]
      }
      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {})
        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-tfc-api" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.api
            destination_port_range       = "443"
            priority                     = 128
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-notifications" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.notifications
            destination_port_range       = "443"
            priority                     = 160
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-sentinel" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.sentinel
            destination_port_range       = "443"
            priority                     = 192
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-vcs" = {
            access                       = "Allow"
            destination_address_prefixes = data.tfe_ip_ranges.tfc.vcs
            destination_port_range       = "443"
            priority                     = 224
            protocol                     = "Tcp"
            source_address_prefix        = "*"
            source_port_range            = "*"
          }
          "allow-tfc-services" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "443"
            priority                   = 256
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-vnet" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 4068
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
  }

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
    "Workspace"   = terraform.workspace
  }
}

## Data Sources
data "tfe_ip_ranges" "tfc" {}

## Resources
resource "azurerm_monitor_action_group" "do_nothing" {
  name                = "${local.resource_prefix["ops"]}-ag"
  resource_group_name = azurerm_resource_group.rg["ops"].name
  short_name          = "do-nothing"
  tags                = azurerm_resource_group.rg["ops"].tags
}

resource "azurerm_private_dns_zone" "private_dns" {
  for_each = local.private_dns_zone

  name                = each.value
  resource_group_name = azurerm_resource_group.rg["dns"].name
  tags                = azurerm_resource_group.rg["dns"].tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "link" {
  for_each = azurerm_private_dns_zone.private_dns

  name                  = each.value.name
  private_dns_zone_name = each.value.name
  resource_group_name   = each.value.resource_group_name
  tags                  = each.value.tags
  virtual_network_id    = azurerm_virtual_network.network.id
}

resource "azurerm_resource_group" "rg" {
  for_each = local.resource_group

  location = var.location
  name     = "${local.resource_prefix[each.key]}-rg"
  tags     = merge(each.value.tags, local.tags)
}

resource "azurerm_subnet" "subnet" {
  for_each = local.subnet

  address_prefixes     = [each.value.address_prefix]
  name                 = each.key
  resource_group_name  = azurerm_resource_group.rg["net"].name
  service_endpoints    = lookup(each.value, "service_endpoints", [])
  virtual_network_name = azurerm_virtual_network.network.name

  dynamic "delegation" {
    for_each = lookup(each.value, "delegation", {})
    content {
      name = "${delegation.key}-delegation"

      service_delegation {
        actions = delegation.value
        name    = delegation.key
      }
    }
  }
}

resource "azurerm_virtual_network" "network" {
  address_space       = [local.network_cidr]
  location            = var.location
  name                = "${local.resource_prefix["net"]}-vnet"
  resource_group_name = azurerm_resource_group.rg["net"].name
  tags                = azurerm_resource_group.rg["net"].tags
}

## Modules
module "ado_agent" {
  source = "./modules/azure-devops-agent"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  data_collection_rule_id    = module.logs.data_collection_rule_id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = "${local.resource_prefix["ops"]}-ado"
  subnet_id                  = azurerm_subnet.subnet["ado"].id
  tags                       = azurerm_resource_group.rg["ops"].tags
}

module "ampls" {
  source = "./modules/monitor-private-link-scope"

  resource_group  = azurerm_resource_group.rg["ops"]
  resource_prefix = local.resource_prefix["ops"]
  tags            = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["blob"].id,
      azurerm_private_dns_zone.private_dns["monitor"].id,
    ]
  }
}

module "application_insights" {
  source = "./modules/application-insights"

  action_group_id                  = azurerm_monitor_action_group.do_nothing.id
  azure_monitor_private_link_scope = module.ampls
  log_analytics_workspace_id       = module.logs.id
  resource_group                   = azurerm_resource_group.rg["ops"]
  resource_prefix                  = local.resource_prefix["ops"]
  tags                             = azurerm_resource_group.rg["ops"].tags
}

module "keyvault" {
  source = "./modules/keyvault"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix["ops"]
  tags                       = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["vault"].id,
    ]
  }
}

module "logs" {
  source = "./modules/log-analytics-workspace"

  action_group_id                  = azurerm_monitor_action_group.do_nothing.id
  azure_monitor_private_link_scope = module.ampls
  resource_group                   = azurerm_resource_group.rg["ops"]
  resource_prefix                  = local.resource_prefix["ops"]
  tags                             = azurerm_resource_group.rg["ops"].tags
}

module "nsg" {
  for_each = azurerm_subnet.subnet
  source   = "./modules/nsg"

  resource_group  = azurerm_resource_group.rg["net"]
  resource_prefix = "${local.resource_prefix["net"]}-${each.key}"
  rules_inbound   = local.subnet[each.key].nsg_rules.inbound
  rules_outbound  = local.subnet[each.key].nsg_rules.outbound
  subnet_id       = each.value.id
  tags            = azurerm_resource_group.rg["net"].tags
}

module "tfc_agent" {
  source = "./modules/tfc-agent"

  action_group_id         = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace = module.logs
  resource_group          = azurerm_resource_group.rg["ops"]
  resource_prefix         = "${local.resource_prefix["ops"]}-tfca"
  subnet_id               = azurerm_subnet.subnet["tfc"].id
  tags                    = azurerm_resource_group.rg["ops"].tags
  tfc_agent_token         = var.tfc_agent_token
}
