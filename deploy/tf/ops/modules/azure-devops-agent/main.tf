locals {
  alert = {
    cpu = {
      aggregation = "Average"
      description = "Alert on VMSS Threshold - Average CPU greater than 75% for 5 minutes"
      frequency   = "PT1M"
      metric_name = "Percentage CPU"
      operator    = "GreaterThan"
      severity    = 2
      threshold   = 75
      window_size = "PT5M"
    }
    disk = {
      aggregation = "Average"
      description = "Alert on VMSS Threshold - Average Disk Queue greater than 8 for 5 minutes"
      frequency   = "PT1M"
      metric_name = "OS Disk Queue Depth"
      operator    = "GreaterThan"
      severity    = 2
      threshold   = 8
      window_size = "PT5M"
    }
  }
}

resource "azurerm_linux_virtual_machine_scale_set" "main" {
  admin_password                  = random_password.password.result
  admin_username                  = random_id.user.hex
  computer_name_prefix            = "agent"
  disable_password_authentication = false
  encryption_at_host_enabled      = true
  instances                       = 0
  location                        = var.resource_group.location
  name                            = "${var.resource_prefix}-vmss"
  overprovision                   = false
  resource_group_name             = var.resource_group.name
  sku                             = "Standard_DS3_v2"
  tags                            = var.tags
  upgrade_mode                    = "Manual"

  boot_diagnostics {}

  identity {
    type = "SystemAssigned"
  }

  network_interface {
    enable_accelerated_networking = true
    name                          = "primary"
    primary                       = true

    ip_configuration {
      name      = "internal"
      primary   = true
      subnet_id = var.subnet_id
    }
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    offer     = "0001-com-ubuntu-server-focal"
    publisher = "Canonical"
    sku       = "20_04-lts"
    version   = "latest"
  }

  lifecycle {
    ignore_changes = [tags, instances]
  }
}

resource "azurerm_monitor_metric_alert" "alert" {
  for_each = local.alert

  description         = each.value.description
  frequency           = each.value.frequency
  name                = "${var.resource_prefix}-vmss-${each.key}-alert"
  resource_group_name = var.resource_group.name
  scopes              = [azurerm_linux_virtual_machine_scale_set.main.id]
  severity            = each.value.severity
  tags                = var.tags
  window_size         = each.value.window_size

  action {
    action_group_id = var.action_group_id
  }

  criteria {
    aggregation      = each.value.aggregation
    metric_name      = each.value.metric_name
    metric_namespace = "Microsoft.Compute/virtualmachinescalesets"
    operator         = each.value.operator
    threshold        = each.value.threshold
  }
}

resource "azurerm_virtual_machine_scale_set_extension" "azure_monitor_agent" {
  name                         = "AzureMonitorLinuxAgent"
  publisher                    = "Microsoft.Azure.Monitor"
  type                         = "AzureMonitorLinuxAgent"
  type_handler_version         = "1.0"
  virtual_machine_scale_set_id = azurerm_linux_virtual_machine_scale_set.main.id
}

resource "azurerm_virtual_machine_scale_set_extension" "dependency_agent" {
  name                         = "DependencyAgentLinux"
  publisher                    = "Microsoft.Azure.Monitoring.DependencyAgent"
  settings                     = jsonencode({ enableAMA = true })
  type                         = "DependencyAgentLinux"
  type_handler_version         = "9.10"
  virtual_machine_scale_set_id = azurerm_linux_virtual_machine_scale_set.main.id
}

resource "azurerm_monitor_data_collection_rule_association" "dcr_to_vmss" {
  data_collection_rule_id = var.data_collection_rule_id
  description             = "Associate DCR to VMSS."
  name                    = "${var.resource_prefix}-dcra"
  target_resource_id      = azurerm_linux_virtual_machine_scale_set.main.id
}

# No one should need to login to the VMSS, so we'll use a random password
resource "random_id" "user" {
  byte_length = 8
}

resource "random_password" "password" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

module "diagnostics" {
  source = "../diagnostics"

  log_analytics_workspace_id = var.log_analytics_workspace_id

  monitored_services = {
    vmss = {
      id = azurerm_linux_virtual_machine_scale_set.main.id
    }
  }
}
