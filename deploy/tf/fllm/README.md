# Foundational LLM Standard Deployment

- [Foundational LLM Standard Deployment](#foundational-llm-standard-deployment)
  - [Pre-requisites](#pre-requisites)
  - [Required Inputs](#required-inputs)
    - [ environment](#-environment)
    - [ location](#-location)
    - [ project\_id](#-project_id)
    - [ public\_domain](#-public_domain)
    - [ sql\_admin\_ad\_group](#-sql_admin_ad_group)
  - [Optional Inputs](#optional-inputs)
    - [ client\_entra\_application](#-client_entra_application)
    - [ core\_entra\_application](#-core_entra_application)
    - [ namespace](#-namespace)
    - [ test\_db\_password](#-test_db_password)
  - [Outputs](#outputs)
  - [Resources](#resources)
  - [Requirements](#requirements)
  - [Providers](#providers)
  - [Modules](#modules)
    - [ aks\_backend](#-aks_backend)
    - [ aks\_frontend](#-aks_frontend)
    - [ application\_gateway](#-application_gateway)
    - [ content\_safety](#-content_safety)
    - [ cosmosdb](#-cosmosdb)
    - [ openai\_ha](#-openai_ha)
    - [ search](#-search)
    - [ storage](#-storage)

## Pre-requisites

You need to enable host-based encryption on your subscription before deploying this module. To do so, run the following command:

```bash
az feature register --namespace Microsoft.Compute --name EncryptionAtHost
```

Furthermore, we also need to enable the following provider to support Graphana dashboards.

```bash
az provider register "Microsoft.Dashboard"
az provider show --namespace "Microsoft.Dashboard" --query "registrationState"
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

The following input variables are optional (have default values):

### <a name="input_client_entra_application"></a> [client\_entra\_application](#input\_client\_entra\_application)

Description: The Chat Client Entra application.

Type: `string`

Default: `"FoundationaLLM-Client"`

### <a name="input_core_entra_application"></a> [core\_entra\_application](#input\_core\_entra\_application)

Description: The Core API Entra application.

Type: `string`

Default: `"FoundationaLLM-API"`

### <a name="input_namespace"></a> [namespace](#input\_namespace)

Description: The namespace to deploy Azure resources.

Type: `string`

Default: `"default"`

### <a name="input_test_db_password"></a> [test\_db\_password](#input\_test\_db\_password)

Description: The test database password.

Type: `string`

Default: `""`

## Outputs

No outputs.

## Resources

The following resources are used by this module:

- [azurerm_app_configuration_feature.feature](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_configuration_feature) (resource)
- [azurerm_app_configuration_key.config_key_kv](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_configuration_key) (resource)
- [azurerm_app_configuration_key.config_key_vault](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_configuration_key) (resource)
- [azurerm_federated_identity_credential.service_mi](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/federated_identity_credential) (resource)
- [azurerm_key_vault_secret.ai_connection_string](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.api_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.client_entra_clientsecret](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.content_safety_apikey](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.core_entra_clientsecret](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.cosmosdb_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.langchain_csvfile_url](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.langchain_sqldatabase_testdb_pw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.openai_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.search_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.storage_connection_string](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_resource_group.rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) (resource)
- [azurerm_role_assignment.app_config_service_mi](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) (resource)
- [azurerm_role_assignment.key_vault_service_mi](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) (resource)
- [azurerm_role_assignment.role_agw_mi](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) (resource)
- [azurerm_user_assigned_identity.agw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/user_assigned_identity) (resource)
- [azurerm_user_assigned_identity.service_mi](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/user_assigned_identity) (resource)
- [random_string.api_key](https://registry.terraform.io/providers/hashicorp/random/latest/docs/resources/string) (resource)
- [azapi_resource.amw](https://registry.terraform.io/providers/azure/azapi/latest/docs/data-sources/resource) (data source)
- [azurerm_app_configuration.appconfig](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/app_configuration) (data source)
- [azurerm_application_insights.ai](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/application_insights) (data source)
- [azurerm_client_config.current](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/client_config) (data source)
- [azurerm_dns_zone.public_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/dns_zone) (data source)
- [azurerm_key_vault.keyvault_ops](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/key_vault) (data source)
- [azurerm_key_vault_certificate.agw](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/key_vault_certificate) (data source)
- [azurerm_log_analytics_workspace.logs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/log_analytics_workspace) (data source)
- [azurerm_monitor_action_group.do_nothing](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/monitor_action_group) (data source)
- [azurerm_private_dns_zone.private_dns](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/private_dns_zone) (data source)
- [azurerm_resource_group.backend](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/resource_group) (data source)
- [azurerm_storage_account.storage_ops](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/storage_account) (data source)
- [azurerm_subnet.subnet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/subnet) (data source)
- [azurerm_virtual_network.network](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/virtual_network) (data source)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

- <a name="requirement_azapi"></a> [azapi](#requirement\_azapi) (~> 1.9)

- <a name="requirement_azuread"></a> [azuread](#requirement\_azuread) (~> 2.44)

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.65)

## Providers

The following providers are used by this module:

- <a name="provider_azapi"></a> [azapi](#provider\_azapi) (1.10.0)

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (3.80.0)

- <a name="provider_random"></a> [random](#provider\_random) (3.5.1)

## Modules

The following Modules are called:

### <a name="module_aks_backend"></a> [aks\_backend](#module\_aks\_backend)

Source: ../modules/aks

Version:

### <a name="module_aks_frontend"></a> [aks\_frontend](#module\_aks\_frontend)

Source: ../modules/aks

Version:

### <a name="module_application_gateway"></a> [application\_gateway](#module\_application\_gateway)

Source: ../modules/application-gateway-ingress-controller

Version:

### <a name="module_content_safety"></a> [content\_safety](#module\_content\_safety)

Source: ../modules/content-safety

Version:

### <a name="module_cosmosdb"></a> [cosmosdb](#module\_cosmosdb)

Source: ../modules/cosmosdb

Version:

### <a name="module_openai_ha"></a> [openai\_ha](#module\_openai\_ha)

Source: ../modules/ha-openai

Version:

### <a name="module_search"></a> [search](#module\_search)

Source: ../modules/search

Version:

### <a name="module_storage"></a> [storage](#module\_storage)

Source: ../modules/storage-account

Version:
<!-- END_TF_DOCS -->