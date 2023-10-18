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
      publicNetworkAccess      = "Enabled"
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
          days   = 7
          status = "disabled"
        }
        softDeletePolicy = {
          retentionDays = 7
          status        = "disabled"
        }
        trustPolicy = {
          type   = "Notary"
          status = "disabled"
        }
      }
    }

    sku = {
      name = "Premium"
    }
  })
}
