locals {
  resource_prefix = upper(join("-", [var.project_id, var.environment]))

  tags = {
    "Environment" = var.environment
    "Project"     = var.project_id
  }
}

## Data Sources

## Resources
resource "azurerm_resource_group" "rgs" {
  for_each = {
    ops = {
      tags = {
        "Purpose" = "DevOps"
      }
    }
  }

  location = var.location
  name     = join("-", [local.resource_prefix, upper(each.key), "rg"])
  tags     = merge(each.value.tags, local.tags)
}

# Network
# Agent

## Modules