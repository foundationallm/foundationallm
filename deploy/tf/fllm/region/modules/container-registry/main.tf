# TODO: Switch to Azure RM when 4.0 releases.
resource "azapi_resource" "main" {
  location  = var.resource_group.location
  name      = replace("${var.resource_prefix}-cr", "-", "")
  parent_id = var.resource_group.id
  type      = "Microsoft.ContainerRegistry/registries@2023-01-01-preview"
  tags      = var.tags

  body = jsonencode({
    sku = {
      name = "Premium"
    }
    properties = {
      adminUserEnabled = true
      policies = {
        quarantinePolicy = {
          status = "disabled"
        }
        trustPolicy = {
          type   = "Notary"
          status = "disabled"
        }
        retentionPolicy = {
          days   = 7
          status = "disabled"
        }
        exportPolicy = {
          status = "enabled"
        }
        azureADAuthenticationAsArmPolicy = {
          status = "enabled"
        }
        softDeletePolicy = {
          retentionDays = 7
          status        = "disabled"
        }
      }
      encryption = {
        status = "disabled"
      }
      dataEndpointEnabled      = false
      publicNetworkAccess      = "Enabled"
      networkRuleBypassOptions = "AzureServices"
      zoneRedundancy           = "Disabled"
      anonymousPullEnabled     = true
    }
  })


  response_export_values = ["id"]
}
