locals {
  network_cidr = "10.0.0.0/16"

  # Reserve this range for OPS subnets
  # 10.0.252.0/22,10.0.252.0-10.0.255.255,1024 addresses
  ops_parent_cidr = cidrsubnet(local.network_cidr, 6, 63)

  resource_prefix         = { for k, _ in local.resource_group : k => join("-", [local.location_short, var.project_id, var.environment, upper(k)]) }
  resource_prefix_compact = { for k, _ in local.resource_group : k => join("", [local.location_compact, var.project_id, local.environment_compact, upper(k)]) }

  address_prefix = {
    ado           = cidrsubnet(local.ops_parent_cidr, 5, 29)
    agw           = cidrsubnet(local.network_cidr, 8, 0)
    fllm_backend  = cidrsubnet(local.network_cidr, 6, 4) //10.0.16.0/22
    fllm_frontend = cidrsubnet(local.network_cidr, 6, 3) //10.0.12.0/22
    fllm_openai   = cidrsubnet(local.network_cidr, 8, 5)
    jumpbox       = cidrsubnet(local.ops_parent_cidr, 5, 25)
    ops           = cidrsubnet(local.ops_parent_cidr, 5, 27)
    tfc           = cidrsubnet(local.ops_parent_cidr, 5, 31)
  }

  default_nsg_rules = {
    inbound = {
      "allow-jumpbox-inbound" = {
        access                     = "Allow"
        destination_address_prefix = "VirtualNetwork"
        destination_port_range     = "*"
        priority                   = 4093
        protocol                   = "Tcp"
        source_address_prefix      = local.address_prefix["jumpbox"]
        source_port_range          = "*"
      }
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

  environment_compact = local.environment_compacts[var.environment]
  environment_compacts = {
    DEMO = "d"
  }

  # TODO: Need to figure out how to restore the deny-all rules to the NSGs using these modified rules.
  no_deny_nsg_rules = {
    inbound  = { for k, v in local.default_nsg_rules.inbound : k => v if k != "deny-all-inbound" }
    outbound = { for k, v in local.default_nsg_rules.outbound : k => v if k != "deny-all-outbound" }
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
    grafana              = "privatelink.grafana.azure.com"
    monitor              = "privatelink.monitor.azure.com"
    openai               = "privatelink.openai.azure.com"
    prometheus           = "privatelink.${var.location}.prometheus.monitor.azure.com"
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
    jbx = {
      tags = {
        "Purpose" = "DevOps"
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

  location_compact = local.location_compacts[var.location]
  location_compacts = {
    eastus = "E"
  }

  location_short = local.location_shorts[var.location]
  location_shorts = {
    eastus = "EUS"
  }

  subnet = {
    "AppGateway" = {
      address_prefix = local.address_prefix["agw"]
      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-internet-http-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "80"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "Internet"
            source_port_range          = "*"
          }
          "allow-internet-https-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "443"
            priority                   = 132
            protocol                   = "Tcp"
            source_address_prefix      = "Internet"
            source_port_range          = "*"
          }
          "allow-gatewaymanager-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "*"
            destination_port_range     = "65200-65535"
            priority                   = 148
            protocol                   = "Tcp"
            source_address_prefix      = "GatewayManager"
            source_port_range          = "*"
          }
          "allow-loadbalancer-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "*"
            destination_port_range     = "*"
            priority                   = 164
            protocol                   = "*"
            source_address_prefix      = "AzureLoadBalancer"
            source_port_range          = "*"
          }
        })
        outbound = merge({})
      }
    }
    "AzureBastionSubnet" = {
      address_prefix = cidrsubnet(local.ops_parent_cidr, 4, 10)
      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-https-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "*"
            destination_port_range     = "443"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "Internet"
            source_port_range          = "*"
          }
          "allow-gateway-manager-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "*"
            destination_port_range     = "443"
            priority                   = 136
            protocol                   = "Tcp"
            source_address_prefix      = "GatewayManager"
            source_port_range          = "*"
          }
          "allow-load-balancer-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "*"
            destination_port_range     = "443"
            priority                   = 144
            protocol                   = "Tcp"
            source_address_prefix      = "AzureLoadBalancer"
            source_port_range          = "*"
          }
          "allow-bastion-host-communicaiton-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_ranges    = ["5701", "8080"]
            priority                   = 152
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {
          "allow-ssh-rdp-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_ranges    = ["22", "3389"]
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-azure-cloud-communication-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "AzureCloud"
            destination_port_range     = "443"
            priority                   = 136
            protocol                   = "Tcp"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
          "allow-bastion-host-communication-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_ranges    = ["5701", "8080"]
            priority                   = 144
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-get-session-information-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_ranges    = ["80", "443"]
            priority                   = 152
            protocol                   = "*"
            source_address_prefix      = "*"
            source_port_range          = "*"
          }
        })
      }
    }
    "Datasources" = {
      address_prefix = cidrsubnet(local.network_cidr, 8, 2)
      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-aks-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "*"
            source_address_prefixes    = [local.address_prefix["fllm_backend"]]
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "FLLMBackend" = {
      address_prefix = local.address_prefix["fllm_backend"]
      delegations = {
        "Microsoft.ContainerService/managedClusters" = [
          "Microsoft.Network/virtualNetworks/subnets/action"
        ]
      }

      nsg_rules = {
        inbound  = merge(local.no_deny_nsg_rules.inbound, {})
        outbound = merge({}, {})
      }
    }
    "FLLMServices" = {
      address_prefix = cidrsubnet(local.network_cidr, 8, 3)
      nsg_rules = {
        inbound  = merge(local.default_nsg_rules.inbound, {})
        outbound = merge({}, {})
      }
    }
    "FLLMFrontEnd" = {
      address_prefix = local.address_prefix["fllm_frontend"]
      delegations = {
        "Microsoft.ContainerService/managedClusters" = [
          "Microsoft.Network/virtualNetworks/subnets/action"
        ]
      }

      nsg_rules = {
        inbound  = merge(local.no_deny_nsg_rules.inbound, {})
        outbound = merge({}, {})
      }
    }
    "FLLMOpenAI" = {
      address_prefix = local.address_prefix["fllm_openai"]
      service_endpoints = [
        "Microsoft.CognitiveServices"
      ]

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-apim" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3443"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "ApiManagement"
            source_port_range          = "*"
          }
          "allow-lb" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "6390"
            priority                   = 192
            protocol                   = "Tcp"
            source_address_prefix      = "AzureLoadBalancer"
            source_port_range          = "*"
          }
          "allow-aks-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "*"
            source_address_prefixes    = [local.address_prefix["fllm_backend"]]
            source_port_range          = "*"
          }
          "allow-apim-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "443"
            priority                   = 320
            protocol                   = "Tcp"
            source_address_prefixes    = [local.address_prefix["fllm_openai"]]
            source_port_range          = "*"
          }
        })
        outbound = merge({}, {
          "allow-storage" = {
            access                     = "Allow"
            destination_address_prefix = "Storage"
            destination_port_range     = "443"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-sql" = {
            access                     = "Allow"
            destination_address_prefix = "SQL"
            destination_port_range     = "1443"
            priority                   = 192
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
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
    "FLLMStorage" = {
      address_prefix = cidrsubnet(local.network_cidr, 8, 4)

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-aks-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "*"
            source_address_prefixes    = [local.address_prefix["fllm_backend"]]
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    "Vectorization" = {
      address_prefix = cidrsubnet(local.network_cidr, 8, 6)

      nsg_rules = {
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-aks-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "*"
            source_address_prefixes    = [local.address_prefix["fllm_backend"]]
            source_port_range          = "*"
          }
        })
        outbound = merge(local.default_nsg_rules.outbound, {})
      }
    }
    # OPS subnets
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
        inbound = merge(local.default_nsg_rules.inbound, {
          "allow-rdp-services" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "Tcp"
            source_address_prefixes    = [local.address_prefix["agw"]]
            source_port_range          = "*"
          }
          "allow-aks-inbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 264
            protocol                   = "*"
            source_address_prefixes    = [local.address_prefix["fllm_backend"], local.address_prefix["fllm_frontend"]]
            source_port_range          = "*"
          }
        })
        outbound = merge({}, {})
      }
    }
    jumpbox = {
      address_prefix = local.address_prefix["jumpbox"]

      nsg_rules = {
        inbound = merge(local.no_deny_nsg_rules.inbound, {
          "allow-rdp" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "3389"
            priority                   = 128
            protocol                   = "Tcp"
            source_address_prefix      = "VirtualNetwork"
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

        outbound = merge(local.no_deny_nsg_rules.outbound, {
          "allow-vnet-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "VirtualNetwork"
            destination_port_range     = "*"
            priority                   = 128
            protocol                   = "*"
            source_address_prefix      = "VirtualNetwork"
            source_port_range          = "*"
          }
          "allow-internet-outbound" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "*"
            priority                   = 256
            protocol                   = "*"
            source_address_prefix      = "*"
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
          "allow-dns" = {
            access                     = "Allow"
            destination_address_prefix = "Internet"
            destination_port_range     = "53"
            priority                   = 288
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

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
    "Workspace"   = terraform.workspace
  }
}

## Data Sources
data "azurerm_client_config" "current" {}

data "azurerm_dns_zone" "public_dns" {
  name                = var.public_domain
  resource_group_name = "GLB-FLLM-DEMO-DNS-rg"
}

data "azurerm_subscription" "current" {}

data "tfe_ip_ranges" "tfc" {}

## Resources
resource "azurerm_key_vault_secret" "secret" {
  for_each = {
    "jumpbox-administrator-username" = module.jumpbox.administrator_username
    "jumpbox-administrator-password" = module.jumpbox.administrator_password
  }

  key_vault_id = module.keyvault.id
  name         = each.key
  value        = each.value
}

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

resource "azurerm_role_assignment" "owner" {
  for_each = toset(["App Configuration Data Owner", "Key Vault Administrator"])

  principal_id         = data.azurerm_client_config.current.object_id
  role_definition_name = each.value
  scope                = data.azurerm_subscription.current.id
}

# TODO: need principal ID for the following
# resource "azurerm_role_assignment" "storgage_blob_data_contributor_diagnostic_services" {
#   principal_id         = "562db366-1b96-45d2-aa4a-f2148cef2240"
#   role_definition_name = "Storage Blob Data Contributor"
#   scope                = azurerm_resource_group.rgs["OPS"].id
# }

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
  source = "../modules/azure-devops-agent"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  data_collection_rule_id    = module.logs.data_collection_rule_id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = "${local.resource_prefix["ops"]}-ado"
  subnet_id                  = azurerm_subnet.subnet["ado"].id
  tags                       = azurerm_resource_group.rg["ops"].tags
}

module "ampls" {
  source = "../modules/monitor-private-link-scope"

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

module "appconfig" {
  source = "../modules/app-config"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  encryption_keyvault_id     = module.keyvault.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix["ops"]
  tags                       = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["configuration_stores"].id,
    ]
  }
}

module "application_gateway_certificate" {
  source   = "../modules/keyvault-acme-certificate"
  for_each = toset(["api", "www", ])

  administrator_email = "tbd@solliance.net"
  domain              = "${each.key}.${var.public_domain}"
  key_vault_id        = module.keyvault.id
  public_dns_zone     = data.azurerm_dns_zone.public_dns
}

module "application_insights" {
  source = "../modules/application-insights"

  action_group_id                  = azurerm_monitor_action_group.do_nothing.id
  azure_monitor_private_link_scope = module.ampls
  log_analytics_workspace_id       = module.logs.id
  resource_group                   = azurerm_resource_group.rg["ops"]
  resource_prefix                  = local.resource_prefix["ops"]
  tags                             = azurerm_resource_group.rg["ops"].tags
}

module "bastion" {
  source = "../modules/bastion"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["net"]
  resource_prefix            = local.resource_prefix["net"]
  subnet_id                  = azurerm_subnet.subnet["AzureBastionSubnet"].id
  tags                       = azurerm_resource_group.rg["net"].tags
}

module "container_registry" {
  source = "../modules/container-registry"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix["ops"]
  tags                       = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["cr"].id,
      azurerm_private_dns_zone.private_dns["cr_region"].id,
    ]
  }
}

module "jumpbox" {
  source = "../modules/jumpbox"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  data_collection_rule_id    = module.logs.data_collection_rule_id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["jbx"]
  resource_prefix            = local.resource_prefix["jbx"]
  subnet_id                  = azurerm_subnet.subnet["jumpbox"].id
  tags                       = azurerm_resource_group.rg["jbx"].tags
}

module "keyvault" {
  source = "../modules/keyvault"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix["ops"]
  tags                       = azurerm_resource_group.rg["ops"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["vault"].id,
    ]
  }
}

module "logs" {
  source = "../modules/log-analytics-workspace"

  action_group_id                  = azurerm_monitor_action_group.do_nothing.id
  azure_monitor_private_link_scope = module.ampls
  resource_group                   = azurerm_resource_group.rg["ops"]
  resource_prefix                  = local.resource_prefix["ops"]
  tags                             = azurerm_resource_group.rg["ops"].tags
}

module "monitor_workspace" {
  source = "../modules/monitor-workspace"

  action_group_id = azurerm_monitor_action_group.do_nothing.id
  resource_group  = azurerm_resource_group.rg["ops"]
  resource_prefix = local.resource_prefix["ops"]
  tags            = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["prometheus"].id,
    ]
  }
}

module "nsg" {
  for_each = azurerm_subnet.subnet
  source   = "../modules/nsg"

  resource_group  = azurerm_resource_group.rg["net"]
  resource_prefix = "${local.resource_prefix["net"]}-${each.key}"
  rules_inbound   = local.subnet[each.key].nsg_rules.inbound
  rules_outbound  = local.subnet[each.key].nsg_rules.outbound
  subnet_id       = each.value.id
  tags            = azurerm_resource_group.rg["net"].tags
}

module "prometheus_dashboard" {
  source = "../modules/prometheus-dashboard"

  azure_monitor_workspace_id = module.monitor_workspace.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix_compact["ops"]
  tags                       = azurerm_resource_group.rg["ops"].tags

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.private_dns["grafana"].id,
    ]
  }
}

module "storage_ops" {
  source = "../modules/storage-account"

  action_group_id            = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace_id = module.logs.id
  resource_group             = azurerm_resource_group.rg["ops"]
  resource_prefix            = local.resource_prefix["ops"]
  subscription_id            = data.azurerm_client_config.current.subscription_id
  tags                       = azurerm_resource_group.rg["ops"].tags
  tenant_id                  = data.azurerm_client_config.current.tenant_id

  private_endpoint = {
    subnet_id = azurerm_subnet.subnet["ops"].id
    private_dns_zone_ids = {
      blob  = [azurerm_private_dns_zone.private_dns["blob"].id]
      dfs   = [azurerm_private_dns_zone.private_dns["dfs"].id]
      file  = [azurerm_private_dns_zone.private_dns["file"].id]
      queue = [azurerm_private_dns_zone.private_dns["queue"].id]
      table = [azurerm_private_dns_zone.private_dns["table"].id]
      web   = [azurerm_private_dns_zone.private_dns["sites"].id]
    }
  }
}

module "tfc_agent" {
  source = "../modules/tfc-agent"

  action_group_id         = azurerm_monitor_action_group.do_nothing.id
  log_analytics_workspace = module.logs
  resource_group          = azurerm_resource_group.rg["ops"]
  resource_prefix         = "${local.resource_prefix["ops"]}-tfca"
  subnet_id               = azurerm_subnet.subnet["tfc"].id
  tags                    = azurerm_resource_group.rg["ops"].tags
  tfc_agent_token         = var.tfc_agent_token
}
