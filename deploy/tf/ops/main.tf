locals {
  network_cidr    = "10.0.0.0/16"
  resource_prefix = { for k, _ in local.resource_group : k => join("-", [var.project_id, var.environment, upper(k)]) }

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

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
  }
}

## Data Sources

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

resource "azurerm_virtual_network" "network" {
  address_space       = [local.network_cidr]
  location            = var.location
  name                = join("-", [local.resource_prefix["net"], "vnet"])
  resource_group_name = azurerm_resource_group.rg["net"].name
  tags                = azurerm_resource_group.rg["net"].tags
}

# Agent

## Modules