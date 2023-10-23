locals {
  hostname           = "www.internal.foundationallm.ai"
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = merge(var.tags, { workspace = terraform.workspace })
  vnet_address_space = var.vnet_address_space
}
locals {
  tfc_address_prefix   = cidrsubnet(local.vnet_address_space, 11, 2046)
  agent_address_prefix = cidrsubnet(local.vnet_address_space, 11, 2047)

  default_nsg_rules = {
    inbound = {
      "allow-ado-agents-inbound" = {
        access                     = "Allow"
        destination_address_prefix = "VirtualNetwork"
        destination_port_range     = "*"
        priority                   = 4094
        protocol                   = "Tcp"
        source_address_prefix      = local.agent_address_prefix
        source_port_range          = "*"
      }
      "allow-tfc-agents-inbound" = {
        access                     = "Allow"
        destination_address_prefix = "VirtualNetwork"
        destination_port_range     = "*"
        priority                   = 4095
        protocol                   = "*"
        source_address_prefix      = local.tfc_address_prefix
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

  subnets = {

    "Services" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 1)
      service_endpoints = []

      nsg_rules = {
        inbound = merge({}, {
          "allow-vnet-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 192
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
        outbound = merge({}, {
          "allow-kv" = {
            access                     = "Allow"
            destination_address_prefix = "AzureKeyVault"
            destination_port_range     = "443"
            priority                   = 224
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
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






    "Jumpbox" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 7)
      service_endpoints = []

      nsg_rules = {
        inbound = merge({}, {
          "allow-rdp" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3389"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "Internet"
            source_port_range          = "*"
          }
          "allow-vnet-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 192
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })

        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-vnet-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 128
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
      }
    }
    "Gateway" = {
      address_prefix    = cidrsubnet(local.vnet_address_space, 8, 8)
      service_endpoints = []

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {})

        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    # Small networks at the end
    "tfc" = {
      address_prefix    = local.tfc_address_prefix
      service_endpoints = []
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
    "Agents" = {
      address_prefix    = local.agent_address_prefix
      service_endpoints = []
      delegation        = {}

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
  }
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