locals {
  alert = {

  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-cr-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azapi_resource.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ContainerRegistry/registries"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-registry-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "registry"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-registry-connection"
    private_connection_resource_id = azapi_resource.main.id
    subresource_names              = ["registry"]
  }
}

# TODO: Switch to Azure RM when 4.0 releases.
resource "azapi_resource" "main" {
  location               = var.resource_group.location
  name                   = replace("${var.resource_prefix}-cr", "-", "")
  parent_id              = var.resource_group.id
  response_export_values = ["id"]
  tags                   = var.tags
  type                   = "Microsoft.ContainerRegistry/registries@2023-01-01-preview"

  body = jsonencode({
    properties = {
      adminUserEnabled         = true
      anonymousPullEnabled     = true
      dataEndpointEnabled      = false
      networkRuleBypassOptions = "AzureServices"
      publicNetworkAccess      = "Disabled"
      zoneRedundancy           = "Disabled"

      encryption = {
        status = "disabled"
      }

      policies = {
        azureADAuthenticationAsArmPolicy = {
          status = "enabled"
        }
        exportPolicy = {
          status = "enabled"
        }
        quarantinePolicy = {
          status = "disabled"
        }
        retentionPolicy = {
          days   = 30
          status = "enabled"
        }
        softDeletePolicy = {
          retentionDays = 30
          status        = "enabled"
        }
        trustPolicy = {
          type   = "Notary"
          status = "enabled"
        }
      }
    }

    sku = {
      name = "Premium"
    }
  })
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    cr = {
      id = azapi_resource.main.id
    }
  }
}
