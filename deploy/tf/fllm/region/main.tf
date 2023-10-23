locals {
  hostname           = "www.internal.foundationallm.ai"
  location           = var.location
  location_short     = var.location_short
  private_dns_zones  = var.private_dns_zones
  resource_prefix    = upper(join("-", [local.location_short, var.resource_prefix]))
  tags               = merge(var.tags, { workspace = terraform.workspace })
  vnet_address_space = var.vnet_address_space

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



  }
}



