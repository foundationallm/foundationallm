# Foundational LLM Standard Deployment

- [Foundational LLM Standard Deployment](#foundational-llm-standard-deployment)
  - [Pre-requisites](#pre-requisites)
  - [Requirements](#requirements)
  - [Providers](#providers)
  - [Modules](#modules)
    - [ global](#-global)
    - [ regions](#-regions)
  - [Resources](#resources)
  - [Required Inputs](#required-inputs)
    - [ tfc\_agent\_token](#-tfc_agent_token)
  - [Optional Inputs](#optional-inputs)
    - [ environment](#-environment)
    - [ global\_location](#-global_location)
    - [ project\_id](#-project_id)
    - [ public\_domain](#-public_domain)
    - [ sql\_admin\_ad\_group](#-sql_admin_ad_group)
    - [ tags](#-tags)
  - [Outputs](#outputs)

## Pre-requisites

You must enable host-based encryption on the subscription before deploying this module.  This is done by running the following command:

```bash
az feature register --namespace Microsoft.Compute --name EncryptionAtHost
```

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

### <a name="input_public_domain"></a> [public\_domain](#input\_public\_domain)

Description: Public DNS domain

Type: `string`

### <a name="input_sql_admin_ad_group"></a> [sql\_admin\_ad\_group](#input\_sql\_admin\_ad\_group)

Description: SQL Admin AD group

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
- [azurerm_role_assignment.keyvault_secrets_user_agw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) (resource)
- [azurerm_subnet.subnet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/subnet) (resource)
- [azurerm_user_assigned_identity.agw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/user_assigned_identity) (resource)
- [azurerm_dns_zone.public_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/dns_zone) (data source)
- [azurerm_key_vault.keyvault_ops](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/key_vault) (data source)
- [azurerm_key_vault_certificate.agw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/key_vault_certificate) (data source)
- [azurerm_log_analytics_workspace.logs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/log_analytics_workspace) (data source)
- [azurerm_monitor_action_group.do_nothing](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/monitor_action_group) (data source)
- [azurerm_private_dns_zone.private_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) (data source)
- [azurerm_resource_group.backend](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/resource_group) (data source)
- [azurerm_subnet.backend](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/subnet) (data source)
- [azurerm_virtual_network.network](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/virtual_network) (data source)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.65)

## Providers

The following providers are used by this module:

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (3.77.0)

## Modules

The following Modules are called:

### <a name="module_aks_backend"></a> [aks\_backend](#module\_aks\_backend)

Source: ./modules/aks

Version:

### <a name="module_application_gateway"></a> [application\_gateway](#module\_application\_gateway)

Source: ./modules/application-gateway

Version:

### <a name="module_nsg"></a> [nsg](#module\_nsg)

Source: ./modules/nsg

Version:

### <a name="module_storage_data"></a> [storage\_data](#module\_storage\_data)

Source: ./modules/storage-account

Version:
<!-- END_TF_DOCS -->