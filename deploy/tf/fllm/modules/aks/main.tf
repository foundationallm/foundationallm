locals {
  alert = {
    cpu = {
      aggregation = "Average"
      description = "Node CPU utilization greater than 95% for 1 hour"
      frequency   = "PT5M"
      metric_name = "node_cpu_usage_percentage"
      operator    = "GreaterThan"
      threshold   = 95
      window_size = "PT5M"
      severity    = 3
    }
    mem = {
      aggregation = "Average"
      description = "Node memory utilization greater than 95% for 1 hour"
      frequency   = "PT5M"
      metric_name = "node_memory_working_set_percentage"
      operator    = "GreaterThan"
      severity    = 3
      threshold   = 100
      window_size = "PT5M"
    }
  }

  role_aks_mi = {
    dns = {
      role  = "Private DNS Zone Contributor"
      scope = var.private_endpoint.private_dns_zone_ids["aks"][0]
    }
    network = {
      role  = "Contributor"
      scope = var.application_gateway.id
    }
    subnet = {
      role  = "Network Contributor"
      scope = var.subnet_id
    }
  }

  role_agw_mi = {
    network = {
      role  = "Contributor"
      scope = var.application_gateway.id
    }
    identity = {
      role  = "Contributor"
      scope = var.application_gateway.identity_id
    }
    subnet = {
      role  = "Network Contributor"
      scope = var.application_gateway.agw_subnet_id
    }
  }
}

# Data Sources

# Resources
resource "azurerm_kubernetes_cluster" "main" {
  depends_on = [azurerm_role_assignment.role_aks_mi]

  automatic_channel_upgrade         = "stable"
  azure_policy_enabled              = true
  dns_prefix_private_cluster        = "${var.resource_prefix}-aks"
  local_account_disabled            = true
  location                          = var.resource_group.location
  name                              = "${var.resource_prefix}-aks"
  node_resource_group               = "${var.resource_prefix}-aks-mrg"
  oidc_issuer_enabled               = true
  private_cluster_enabled           = true
  private_dns_zone_id               = var.private_endpoint.private_dns_zone_ids["aks"][0]
  resource_group_name               = var.resource_group.name
  role_based_access_control_enabled = true
  sku_tier                          = "Standard"
  tags                              = var.tags
  workload_identity_enabled         = true

  azure_active_directory_role_based_access_control {
    admin_group_object_ids = var.administrator_object_ids
    azure_rbac_enabled     = true
    managed                = true
    tenant_id              = var.tenant_id
  }

  default_node_pool {
    enable_auto_scaling          = true
    max_count                    = 6
    min_count                    = 2
    name                         = "system"
    only_critical_addons_enabled = true
    os_disk_size_gb              = 1024
    tags                         = var.tags
    vm_size                      = "Standard_DS2_v2"
    vnet_subnet_id               = var.subnet_id

    upgrade_settings {
      max_surge = "200"
    }
  }

  identity {
    identity_ids = [azurerm_user_assigned_identity.aks_mi.id]
    type         = "UserAssigned"
  }

  ingress_application_gateway {
    gateway_id = var.application_gateway.id
  }

  key_vault_secrets_provider {
    secret_rotation_enabled  = true
    secret_rotation_interval = "2m"
  }

  microsoft_defender {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  monitor_metrics {
    annotations_allowed = null
    labels_allowed      = null
  }

  network_profile {
    dns_service_ip = "10.100.254.1"
    network_plugin = "azure"
    network_policy = "azure"
    service_cidr   = "10.100.0.0/16"
  }

  oms_agent {
    log_analytics_workspace_id      = var.log_analytics_workspace_id
    msi_auth_for_monitoring_enabled = true
  }
}

resource "azurerm_kubernetes_cluster_node_pool" "user" {
  enable_auto_scaling   = true
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  max_count             = 3
  min_count             = 1
  mode                  = "User"
  name                  = "user"
  os_disk_size_gb       = 1024
  tags                  = var.tags
  vm_size               = "Standard_DS2_v2"
  vnet_subnet_id        = var.subnet_id

  upgrade_settings {
    max_surge = "200"
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-aks-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_kubernetes_cluster.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.ContainerService/managedClusters"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_private_endpoint" "ple" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-aks-pe"
  resource_group_name = var.resource_group.name
  subnet_id           = var.private_endpoint.subnet_id
  tags                = var.tags

  private_dns_zone_group {
    name                 = "aks"
    private_dns_zone_ids = var.private_endpoint.private_dns_zone_ids.aks
  }

  private_service_connection {
    is_manual_connection           = false
    name                           = "${var.resource_prefix}-aks-connection"
    private_connection_resource_id = azurerm_kubernetes_cluster.main.id
    subresource_names              = ["management"]
  }
}

resource "azurerm_role_assignment" "role_aks_mi" {
  for_each = local.role_aks_mi

  principal_id         = azurerm_user_assigned_identity.aks_mi.principal_id
  role_definition_name = each.value.role
  scope                = each.value.scope
}

resource "azurerm_role_assignment" "role_agw_mi" {
  for_each = local.role_agw_mi

  principal_id         = azurerm_kubernetes_cluster.main.ingress_application_gateway[0].ingress_application_gateway_identity[0].object_id
  role_definition_name = each.value.role
  scope                = each.value.scope
}


resource "azurerm_user_assigned_identity" "aks_mi" {
  location            = var.resource_group.location
  name                = "${var.resource_prefix}-aks-mi"
  resource_group_name = var.resource_group.name
  tags                = var.tags
}

# Modules
module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    aks = {
      id = azurerm_kubernetes_cluster.main.id
    }
  }
}

module "prometheus_collector" {
  source = "./modules/prometheus-collector"

  azure_monitor_workspace_id = var.azure_monitor_workspace_id
  cluster                    = azurerm_kubernetes_cluster.main
  resource_group             = var.resource_group
  resource_prefix            = var.resource_prefix
}
