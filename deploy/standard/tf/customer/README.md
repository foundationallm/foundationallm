# Example customer data sources

<!-- BEGIN_TF_DOCS -->


## Required Inputs

The following input variables are required:

### <a name="input_environment"></a> [environment](#input\_environment)

Description: The environment name.

Type: `string`

### <a name="input_location"></a> [location](#input\_location)

Description: The location to deploy Azure resources.

Type: `string`

### <a name="input_project_id"></a> [project\_id](#input\_project\_id)

Description: The project identifier.

Type: `string`

### <a name="input_sql_azuread_administrator"></a> [sql\_azuread\_administrator](#input\_sql\_azuread\_administrator)

Description: Azure AD group to be added as SQL Server administrator.

Type:

```hcl
object({
    name      = string
    object_id = string
  })
```

## Optional Inputs

No optional inputs.

## Outputs

No outputs.

## Resources

The following resources are used by this module:

- [azurerm_resource_group.rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) (resource)
- [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) (data source)
- [azurerm_log_analytics_workspace.logs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/log_analytics_workspace) (data source)
- [azurerm_monitor_action_group.do_nothing](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/monitor_action_group) (data source)
- [azurerm_private_dns_zone.private_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) (data source)
- [azurerm_resource_group.backend](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/resource_group) (data source)
- [azurerm_subnet.subnet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/subnet) (data source)
- [azurerm_virtual_network.network](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/virtual_network) (data source)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

- <a name="requirement_azapi"></a> [azapi](#requirement\_azapi) (~> 1.9)

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.65)

## Providers

The following providers are used by this module:

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (3.79.0)

## Modules

The following Modules are called:

### <a name="module_cosmosdb_data"></a> [cosmosdb\_data](#module\_cosmosdb\_data)

Source: ../modules/cosmosdb

Version:

### <a name="module_sql"></a> [sql](#module\_sql)

Source: ../modules/mssql-server

Version:

### <a name="module_storage_data"></a> [storage\_data](#module\_storage\_data)

Source: ../modules/storage-account

Version:
<!-- END_TF_DOCS -->