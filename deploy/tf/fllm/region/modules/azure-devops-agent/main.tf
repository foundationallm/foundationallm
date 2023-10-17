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

# No one should need to login to the VMSS, so we'll use a random password
resource "random_id" "user" {
  byte_length = 8
}

resource "random_password" "password" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}


