# FoundationalLM Ops Enviornment

Bootstraps the Terraform Cloud Agent and other backoffice dependencies to 
support FoundationalLM deployments.
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

## Optional Inputs

No optional inputs.

## Outputs

No outputs.

## Resources

The following resources are used by this module:

- [azurerm_resource_group.rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) (resource)
- [azurerm_virtual_network.network](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/virtual_network) (resource)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

## Providers

The following providers are used by this module:

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (3.77.0)

## Modules

No modules.
<!-- END_TF_DOCS -->