locals {
  alert                = {}
  customer_managed_key = var.customer_managed_key_id == null ? {} : { cmk = { key_vault_key_id = var.customer_managed_key_id } }
  id                   = "${var.resource_group.id}/providers/Microsoft.CognitiveServices/accounts/${local.name}"
  name                 = "${var.resource_prefix}-content-safety"
  principal_id         = jsondecode(azapi_resource.resource.output).identity.principalId
}

# Data Sources
data "azurerm_cognitive_account" "main" {
  name                = jsondecode(azapi_resource.resource.output).name
  resource_group_name = var.resource_group.name
}

# Resources
resource "azapi_resource" "resource" {
  location                  = var.resource_group.location
  name                      = "${var.resource_prefix}-content-safety"
  parent_id                 = var.resource_group.id
  type                      = "Microsoft.CognitiveServices/accounts@2023-05-01"
  tags                      = var.tags
  response_export_values    = ["*"]
  schema_validation_enabled = false

  body = jsonencode({
    kind = "ContentSafety"

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

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_cognitive_account_customer_managed_key" "cmk" {
  for_each   = local.customer_managed_key
  depends_on = [azapi_resource.resource, azurerm_role_assignment.key_vault_crypto_user]

  cognitive_account_id = local.id
  key_vault_key_id     = each.value.key_vault_key_id
}

resource "azurerm_monitor_metric_alert" "alert" {
  depends_on = [azapi_resource.resource]

  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-content-safety-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [local.id]
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
  depends_on = [azapi_resource.resource]

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
    private_connection_resource_id = local.id
    subresource_names              = ["account"]
  }
}

resource "azurerm_role_assignment" "key_vault_crypto_user" {
  for_each = local.customer_managed_key

  principal_id         = jsondecode(azapi_resource.resource.output).identity.principalId
  role_definition_name = "Key Vault Crypto User"
  scope                = var.resource_group.id
}

# Modules
module "diagnostics" {
  depends_on = [azapi_resource.resource]
  source     = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    contentSafety = {
      id = local.id
    }
  }
}
