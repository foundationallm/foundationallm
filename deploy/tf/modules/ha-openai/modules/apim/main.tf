locals {
  alert = {
    capacity = {
      aggregation = "Average"
      description = "Service capacity greater than 75% for 1 hour"
      frequency   = "PT1M"
      metric_name = "Capacity"
      operator    = "GreaterThan"
      threshold   = 75
      window_size = "PT1H"
      severity    = 0
    }
  }

  inbound_choices = join("",
    [for index, backend in azurerm_api_management_backend.backends : <<XML
      <when condition="@(context.Variables.GetValueOrDefault<int>("backendId") == ${index + 1})">
        <set-backend-service backend-id="${backend.name}"/>
      </when>
XML
    ]
  )
}

# Data Sources
# Resources
resource "azurerm_api_management" "main" {
  location             = var.resource_group.location
  name                 = "${var.resource_prefix}-apim"
  public_ip_address_id = azurerm_public_ip.pip.id
  publisher_email      = var.publisher.email
  publisher_name       = var.publisher.name
  resource_group_name  = var.resource_group.name
  sku_name             = "Developer_1" # TODO: Parameterize
  tags                 = var.tags
  virtual_network_type = "Internal"

  identity {
    type = "SystemAssigned"
  }

  virtual_network_configuration {
    subnet_id = var.subnet_id
  }
}

resource "azurerm_api_management_api" "api" {
  api_management_name   = azurerm_api_management.main.name
  display_name          = "HA OpenAI"
  name                  = "${var.resource_prefix}-api"
  path                  = "openai"
  protocols             = ["https"]
  resource_group_name   = var.resource_group.name
  revision              = "1"
  subscription_required = false

  import {
    content_format = "openapi+json-link"
    content_value  = "https://raw.githubusercontent.com/Azure/azure-rest-api-specs/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/stable/2023-05-15/inference.json"
  }
}

# TODO: does this resource support tags. If so, add them.
resource "azurerm_api_management_api_policy" "api_policy" {
  api_management_name = azurerm_api_management_api.api.api_management_name
  api_name            = azurerm_api_management_api.api.name
  resource_group_name = var.resource_group.name

  xml_content = <<XML
  <policies>
  <inbound>
    <base/>
    <set-variable name="backendId" value="@(new Random(context.RequestId.GetHashCode()).Next(1, ${length(var.cognitive_account_endpoint) + 1}))" />
    <choose>
${local.inbound_choices}
      <otherwise>
      <!-- Should never happen, but you never know ;) -->
        <return-response>
          <set-status code="500" reason="InternalServerError" />
          <set-header name="Microsoft-Azure-Api-Management-Correlation-Id" exists-action="override">
            <value>@{return Guid.NewGuid().ToString();}</value>
          </set-header>
          <set-body>A gateway-related error occurred while processing the request.</set-body>
        </return-response>
      </otherwise>
    </choose>
  </inbound>
  <backend>
    <base/>
  </backend>
  <outbound>
    <base/>
  </outbound>
  <on-error>
    <base/>
  </on-error>
</policies>
XML
}

# TODO: does this resource support tags. If so, add them.
resource "azurerm_api_management_backend" "backends" {
  count = length(var.cognitive_account_endpoint) # TODO: Use for_each because the account collection is a map.

  api_management_name = azurerm_api_management.main.name
  name                = "${var.resource_prefix}-${count.index}-backend"
  protocol            = "http"
  resource_group_name = var.resource_group.name
  url                 = join("", [var.cognitive_account_endpoint[count.index].endpoint, "openai"])

  credentials {
    header = {
      "api-key" = join(",", [
        "{{${azurerm_api_management_named_value.openai_primary_key[count.index].name}}}",
        "{{${azurerm_api_management_named_value.openai_secondary_key[count.index].name}}}"
      ])
    }
  }

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

# TODO: does this resource support tags. If so, add them.
resource "azurerm_api_management_named_value" "openai_primary_key" {
  count = length(var.openai_primary_key) # TODO: Use for_each because the secret collection is a map.

  api_management_name = azurerm_api_management.main.name
  display_name        = var.openai_primary_key[count.index].name
  name                = var.openai_primary_key[count.index].name
  resource_group_name = var.resource_group.name
  secret              = true

  value_from_key_vault {
    secret_id = var.openai_primary_key[count.index].id
  }

  depends_on = [
    azurerm_role_assignment.role
  ]
}

# TODO: does this resource support tags. If so, add them.
resource "azurerm_api_management_named_value" "openai_secondary_key" {
  count = length(var.openai_secondary_key) # TODO: Use for_each because the secret collection is a map.

  api_management_name = azurerm_api_management.main.name
  display_name        = var.openai_secondary_key[count.index].name
  name                = var.openai_secondary_key[count.index].name
  resource_group_name = var.resource_group.name
  secret              = true

  value_from_key_vault {
    secret_id = var.openai_secondary_key[count.index].id
  }

  depends_on = [
    azurerm_role_assignment.role
  ]
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-apim-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_api_management.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ApiManagement/service"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_dns_a_record" "apim" {
  count = length(var.private_dns_zones) # TODO: Can we use for_each

  name                = lower(azurerm_api_management.main.name)
  records             = azurerm_api_management.main.private_ip_addresses
  resource_group_name = var.private_dns_zones[count.index].resource_group_name
  ttl                 = 0
  zone_name           = var.private_dns_zones[count.index].name
}

resource "azurerm_public_ip" "pip" {
  allocation_method   = "Static"
  domain_name_label   = "${lower(var.resource_prefix)}-apim"
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-pip"
  resource_group_name = var.resource_group.name
  sku                 = "Standard"
  tags                = var.tags
}

resource "azurerm_role_assignment" "role" {
  principal_id         = azurerm_api_management.main.identity.0.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = var.key_vault_id
}

# Modules
module "diagnostics" {
  source = "../../../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    apim = {
      id = azurerm_api_management.main.id
    }
  }
}