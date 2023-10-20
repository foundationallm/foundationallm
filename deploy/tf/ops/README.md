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

### <a name="input_tfc_agent_token"></a> [tfc\_agent\_token](#input\_tfc\_agent\_token)

Description: The token used by the agent to authenticate with Terraform Cloud.

Type: `string`

## Optional Inputs

No optional inputs.

## Outputs

No outputs.

## Resources

The following resources are used by this module:

- [azurerm_monitor_action_group.do_nothing](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_action_group) (resource)
- [azurerm_private_dns_zone.private_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_dns_zone) (resource)
- [azurerm_private_dns_zone_virtual_network_link.link](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_dns_zone_virtual_network_link) (resource)
- [azurerm_resource_group.rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) (resource)
- [azurerm_subnet.subnet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/subnet) (resource)
- [azurerm_virtual_network.network](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/virtual_network) (resource)
- [tfe_ip_ranges.tfc](https://registry.terraform.io/providers/hashicorp/tfe/latest/docs/data-sources/ip_ranges) (data source)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.65)

- <a name="requirement_tfe"></a> [tfe](#requirement\_tfe) (~> 0.49)

## Providers

The following providers are used by this module:

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (3.77.0)

- <a name="provider_tfe"></a> [tfe](#provider\_tfe) (0.49.2)

## Modules

The following Modules are called:

### <a name="module_ado_agent"></a> [ado\_agent](#module\_ado\_agent)

Source: ./modules/azure-devops-agent

Version:

### <a name="module_ampls"></a> [ampls](#module\_ampls)

Source: ./modules/monitor-private-link-scope

Version:

### <a name="module_appconfig"></a> [appconfig](#module\_appconfig)

Source: ./modules/app-config

Version:

### <a name="module_application_insights"></a> [application\_insights](#module\_application\_insights)

Source: ./modules/application-insights

Version:

### <a name="module_keyvault"></a> [keyvault](#module\_keyvault)

Source: ./modules/keyvault

Version:

### <a name="module_logs"></a> [logs](#module\_logs)

Source: ./modules/log-analytics-workspace

Version:

### <a name="module_nsg"></a> [nsg](#module\_nsg)

Source: ./modules/nsg

Version:

### <a name="module_tfc_agent"></a> [tfc\_agent](#module\_tfc\_agent)

Source: ./modules/tfc-agent

Version:
<!-- END_TF_DOCS -->