locals {
  services = {
    "orchestration-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "agent-hub-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "core-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "core-job" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "data-source-hub-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "gatekeeper-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "langchain-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "prompt-hub-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "semantic-kernel-api" = {
      issuer = module.aks_backend.oidcIssuerUrl
    }
    "chat-ui" = {
      issuer = module.aks_frontend.oidcIssuerUrl
    }
  }
}

resource "azurerm_user_assigned_identity" "service_mi" {
  for_each = local.services

  location            = var.location
  name                = "${local.resource_prefix["app"]}-${each.key}-mi"
  resource_group_name = azurerm_resource_group.rg["app"].name
}

resource "azurerm_federated_identity_credential" "service_mi" {
  for_each = azurerm_user_assigned_identity.service_mi

  audience            = ["api://AzureADTokenExchange"]
  issuer              = module.aks_backend.oidcIssuerUrl
  name                = each.key
  parent_id           = each.value.id
  resource_group_name = each.value.resource_group_name
  subject             = "system:serviceaccount:${var.namespace}:${each.key}"
}

resource "azurerm_role_assignment" "app_config_service_mi" {
  for_each = azurerm_user_assigned_identity.service_mi

  principal_id         = each.value.principal_id
  role_definition_name = "App Configuration Data Reader"
  scope                = data.azurerm_app_configuration.appconfig.id
}

resource "azurerm_role_assignment" "key_vault_service_mi" {
  for_each = azurerm_user_assigned_identity.service_mi

  principal_id         = each.value.principal_id
  role_definition_name = "Key Vault Secrets User"
  scope                = data.azurerm_key_vault.keyvault_ops.id
}

resource "azurerm_role_assignment" "cosmos_service_mi" {
  principal_id         = azurerm_user_assigned_identity.service_mi["core-job"].principal_id
  role_definition_name = "Contributor"
  scope                = module.cosmosdb.id
}