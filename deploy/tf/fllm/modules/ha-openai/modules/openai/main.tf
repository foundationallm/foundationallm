locals {
  alert = {
    availability = {
      aggregation = "Average"
      description = "Service availability less than 99% for 1 hour"
      frequency   = "PT1M"
      metric_name = "SuccessRate"
      operator    = "LessThan"
      threshold   = 99
      window_size = "PT1H"
      severity    = 0
    }
    latency = {
      aggregation = "Average"
      description = "Service latency greater than 1000ms for 1 hour"
      frequency   = "PT1M"
      metric_name = "Latency"
      operator    = "GreaterThan"
      threshold   = 1000
      window_size = "PT1H"
      severity    = 0
    }
  }
}

# Data Sources

# Resources
resource "azurerm_cognitive_account" "main" {
  custom_subdomain_name         = "${lower(var.resource_prefix)}-${var.instance_id}"
  kind                          = "OpenAI"
  location                      = var.resource_group.location
  name                          = "${var.resource_prefix}-${var.instance_id}-openai"
  public_network_access_enabled = false
  resource_group_name           = var.resource_group.name
  sku_name                      = "S0"
  tags                          = var.tags
}

resource "azurerm_cognitive_deployment" "completions" {
  cognitive_account_id = azurerm_cognitive_account.main.id
  name                 = "completions"

  model {
    format  = "OpenAI"
    name    = "gpt-35-turbo"
    version = "0301"
  }

  scale {
    capacity = "120"
    type     = "Standard"
  }
}

resource "azurerm_cognitive_deployment" "embeddings" {
  cognitive_account_id = azurerm_cognitive_account.main.id
  name                 = "embeddings"
  rai_policy_name      = "Microsoft.Default"

  model {
    format  = "OpenAI"
    name    = "text-embedding-ada-002"
    version = "2"
  }

  scale {
    capacity = "120"
    type     = "Standard"
  }
}

resource "azurerm_key_vault_secret" "openai_primary_key" {
  key_vault_id = var.key_vault_id
  name         = "${var.resource_prefix}-${var.instance_id}-primarykey"
  tags         = var.tags
  value        = azurerm_cognitive_account.main.primary_access_key
}

resource "azurerm_key_vault_secret" "openai_secondary_key" {
  key_vault_id = var.key_vault_id
  name         = "${var.resource_prefix}-${var.instance_id}-secondarykey"
  tags         = var.tags
  value        = azurerm_cognitive_account.main.secondary_access_key
}

resource "azurerm_monitor_metric_alert" "alert" {
  depends_on = [module.diagnostics] // Delay to avoid race condition.
  for_each   = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-openai-openai${var.instance_id}-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_cognitive_account.main.id]
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
  name                = "${var.resource_prefix}-${var.instance_id}-openai-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "openai"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-${var.instance_id}-openai-connection"
    private_connection_resource_id = azurerm_cognitive_account.main.id
    subresource_names              = ["account"]
  }
}

# Modules
module "diagnostics" {
  source = "../../../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    "openai-${var.instance_id}" = {
      id = azurerm_cognitive_account.main.id
    }
  }
}