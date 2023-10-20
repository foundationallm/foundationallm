locals {
  network_cidr = "10.0.0.0/16"

  # Reserve this range for OPS subnets
  # 10.0.252.0/22,10.0.252.0-10.0.255.255,1024 addresses
  ops_parent_cidr = cidrsubnet(local.network_cidr, 6, 63)

  resource_prefix = { for k, _ in local.resource_group : k => join("-", [var.project_id, var.environment, upper(k)]) }

  address_prefix = {
    ado = cidrsubnet(local.ops_parent_cidr, 5, 29)
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

  resource_group = {
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
  }
}

## Data Sources
data "tfe_ip_ranges" "tfc" {}

## Resources
resource "azurerm_resource_group" "rg" {
  for_each = {
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

  location = var.location
  name     = join("-", [local.resource_prefix[each.key], "rg"])
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

# Agent

## Modules
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