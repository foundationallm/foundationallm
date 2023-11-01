locals {
  alert = {}
}

data "azurerm_cognitive_account" "main" {
  name                = jsondecode(azapi_resource.resource.output).name
  resource_group_name = var.resource_group.name
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-content-safety-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [data.azurerm_cognitive_account.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.CognitiveServices/accounts"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}


resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-content-safety-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "contentSafety"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-content-safety-connection"
    private_connection_resource_id = data.azurerm_cognitive_account.main.id
    subresource_names              = ["account"]
  }
}

moved{
  from = azapi_resource.main
  to = azapi_resource.resource
}
resource "azapi_resource" "resource" {
  location                  = var.resource_group.location
  name                      = "${var.resource_prefix}-content-safety"
  parent_id                 = var.resource_group.id
  type                      = "Microsoft.CognitiveServices/accounts@2023-05-01"
  tags                      = var.tags
  response_export_values    = ["*"]
  schema_validation_enabled = false

  body = jsonencode({
    kind = "ContentModerator"

    identity = {
      type = "SystemAssigned"
    }

    properties = {
      allowedFqdnList               = []
      customSubDomainName           = lower("${var.resource_prefix}-content-safety")
      disableLocalAuth              = false
      dynamicThrottlingEnabled      = false
      publicNetworkAccess           = "Disabled"
      restrictOutboundNetworkAccess = false
    }
    sku = {
      name = "S0"
    }
  })
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    contentSafety = {
      id = data.azurerm_cognitive_account.main.id
    }
  }
}
