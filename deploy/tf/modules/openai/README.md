# Azure: OpenAI

This module deploys Azure OpenAI.

- [Azure: OpenAI](#azure-openai)
  - [Default Example](#default-example)
  - [Customer Managed Key](#customer-managed-key)
  - [Required Inputs](#required-inputs)
    - [ action\_group\_id](#-action_group_id)
    - [ instance\_id](#-instance_id)
    - [ key\_vault\_id](#-key_vault_id)
    - [ log\_analytics\_workspace\_id](#-log_analytics_workspace_id)
    - [ private\_endpoint](#-private_endpoint)
    - [ resource\_group](#-resource_group)
    - [ resource\_prefix](#-resource_prefix)
    - [ tags](#-tags)
  - [Optional Inputs](#optional-inputs)
    - [ capacity](#-capacity)
    - [ customer\_managed\_key](#-customer_managed_key)
  - [Outputs](#outputs)
    - [ endpoint](#-endpoint)
    - [ key\_secret](#-key_secret)
  - [Resources](#resources)
  - [Requirements](#requirements)
  - [Providers](#providers)
  - [Modules](#modules)
    - [ diagnostics](#-diagnostics)
  - [Update Docs](#update-docs)

<!-- BEGIN_TF_DOCS -->

## Default Example

```hcl
locals {
  location        = "eastus"
  resource_prefix = local.test_namespace
  test_namespace  = random_pet.instance_id.id

  tags = {
    contact     = "nobody@example.com"
    environment = "sbx"
    location    = local.location
    repository  = "foundationallm/deploy/tf/modules/openai"
    workload    = "default"
  }
}

# Data Sources
data "azurerm_client_config" "current" {}

# Pre-requisite Resources
resource "azurerm_key_vault" "example" {
  enable_rbac_authorization   = true
  enabled_for_disk_encryption = true
  location                    = azurerm_resource_group.example.location
  name                        = "${local.test_namespace}-kv"
  purge_protection_enabled    = true # must be true for encryption
  resource_group_name         = azurerm_resource_group.example.name
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  tags                        = local.tags
  tenant_id                   = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_log_analytics_workspace" "example" {
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-la"
  resource_group_name = azurerm_resource_group.example.name
  retention_in_days   = 30
  sku                 = "PerGB2018"
  tags                = local.tags
}

resource "azurerm_monitor_action_group" "example" {
  name                = "Example Action Group"
  resource_group_name = azurerm_resource_group.example.name
  short_name          = "example"
  tags                = local.tags
}

resource "azurerm_private_dns_zone" "example" {
  for_each = {
    openai = "privatelink.openai.azure.com"
  }

  name                = each.value
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "azurerm_resource_group" "example" {
  location = local.location
  name     = "${local.test_namespace}-rg"
  tags     = local.tags
}

resource "azurerm_subnet" "example" {
  address_prefixes     = [cidrsubnet(azurerm_virtual_network.example.address_space.0, 1, 0)]
  name                 = "internal"
  resource_group_name  = azurerm_resource_group.example.name
  virtual_network_name = azurerm_virtual_network.example.name
}

resource "azurerm_virtual_network" "example" {
  address_space       = ["192.168.0.0/24"]
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-vnet"
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "random_pet" "instance_id" {}

# How to call the OpenAI module
module "example" {
  source = "../.."

  action_group_id            = azurerm_monitor_action_group.example.id
  instance_id                = 10
  key_vault_id               = azurerm_key_vault.example.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.example.id
  resource_group             = azurerm_resource_group.example
  resource_prefix            = local.resource_prefix
  tags                       = azurerm_resource_group.example.tags

  capacity = {
    completions = 1
    embeddings  = 1
  }

  private_endpoint = {
    subnet_id = azurerm_subnet.example.id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.example["openai"].id,
    ]
  }
}
```
## Customer Managed Key

```hcl
locals {
  location        = "eastus"
  resource_prefix = local.test_namespace
  test_namespace  = random_pet.instance_id.id

  tags = {
    contact     = "nobody@example.com"
    environment = "sbx"
    location    = local.location
    repository  = "foundationallm/deploy/tf/modules/openai"
    workload    = "cmk"
  }
}

# Data Sources
data "azurerm_client_config" "current" {}

# Pre-requisite Resources
resource "azurerm_key_vault" "example" {
  enable_rbac_authorization   = true
  enabled_for_disk_encryption = true
  location                    = azurerm_resource_group.example.location
  name                        = "${local.test_namespace}-kv"
  purge_protection_enabled    = true # must be true for disk encryption
  resource_group_name         = azurerm_resource_group.example.name
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  tags                        = local.tags
  tenant_id                   = data.azurerm_client_config.current.tenant_id
}

resource "azurerm_key_vault_key" "example" {
  name         = "${local.test_namespace}-key"
  key_vault_id = azurerm_key_vault.example.id
  key_type     = "RSA"
  key_size     = 2048
  tags         = local.tags

  key_opts = [
    "decrypt",
    "encrypt",
    "sign",
    "unwrapKey",
    "verify",
    "wrapKey",
  ]
}

resource "azurerm_log_analytics_workspace" "example" {
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-la"
  resource_group_name = azurerm_resource_group.example.name
  retention_in_days   = 30
  sku                 = "PerGB2018"
  tags                = local.tags
}

resource "azurerm_monitor_action_group" "example" {
  name                = "Example Action Group"
  resource_group_name = azurerm_resource_group.example.name
  short_name          = "example"
  tags                = local.tags
}

resource "azurerm_private_dns_zone" "example" {
  for_each = {
    openai = "privatelink.openai.azure.com"
  }

  name                = each.value
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "azurerm_resource_group" "example" {
  location = local.location
  name     = "${local.test_namespace}-rg"
  tags     = local.tags
}

resource "azurerm_subnet" "example" {
  address_prefixes     = [cidrsubnet(azurerm_virtual_network.example.address_space.0, 1, 0)]
  name                 = "internal"
  resource_group_name  = azurerm_resource_group.example.name
  virtual_network_name = azurerm_virtual_network.example.name
}

resource "azurerm_virtual_network" "example" {
  address_space       = ["192.168.0.0/24"]
  location            = azurerm_resource_group.example.location
  name                = "${local.test_namespace}-vnet"
  resource_group_name = azurerm_resource_group.example.name
  tags                = local.tags
}

resource "random_pet" "instance_id" {}

# How to call the OpenAI module
module "example" {
  source = "../.."

  action_group_id            = azurerm_monitor_action_group.example.id
  instance_id                = 10
  key_vault_id               = azurerm_key_vault.example.id
  log_analytics_workspace_id = azurerm_log_analytics_workspace.example.id
  resource_group             = azurerm_resource_group.example
  resource_prefix            = local.resource_prefix
  tags                       = azurerm_resource_group.example.tags

  capacity = {
    completions = 1
    embeddings  = 1
  }

  customer_managed_key = {
    enabled          = true
    key_vault_key_id = azurerm_key_vault_key.example.id
  }

  private_endpoint = {
    subnet_id = azurerm_subnet.example.id
    private_dns_zone_ids = [
      azurerm_private_dns_zone.example["openai"].id,
    ]
  }
}
```

## Required Inputs

The following input variables are required:

### <a name="input_action_group_id"></a> [action\_group\_id](#input\_action\_group\_id)

Description: The ID of the action group to send alerts to.

Type: `string`

### <a name="input_instance_id"></a> [instance\_id](#input\_instance\_id)

Description: The ID of the instance to deploy.

Type: `number`

### <a name="input_key_vault_id"></a> [key\_vault\_id](#input\_key\_vault\_id)

Description: The ID of the Key Vault to store secrets in.

Type: `string`

### <a name="input_log_analytics_workspace_id"></a> [log\_analytics\_workspace\_id](#input\_log\_analytics\_workspace\_id)

Description: The ID of the Log Analytics workspace to send diagnostics data to.

Type: `string`

### <a name="input_private_endpoint"></a> [private\_endpoint](#input\_private\_endpoint)

Description: The private endpoint configuration.

Type:

```hcl
object({
    subnet_id            = string
    private_dns_zone_ids = list(string)
  })
```

### <a name="input_resource_group"></a> [resource\_group](#input\_resource\_group)

Description: The resource group to deploy resources into

Type:

```hcl
object({
    id       = string
    location = string
    name     = string
  })
```

### <a name="input_resource_prefix"></a> [resource\_prefix](#input\_resource\_prefix)

Description: The name prefix for the resources.

Type: `string`

### <a name="input_tags"></a> [tags](#input\_tags)

Description: A map of tags for the resource.

Type: `map(string)`

## Optional Inputs

The following input variables are optional (have default values):

### <a name="input_capacity"></a> [capacity](#input\_capacity)

Description: The capacity of the OpenAI instance deployments.

Type:

```hcl
object({
    completions = number
    embeddings  = number
  })
```

Default:

```json
{
  "completions": 120,
  "embeddings": 120
}
```

### <a name="input_customer_managed_key"></a> [customer\_managed\_key](#input\_customer\_managed\_key)

Description: The ID of the customer managed key to use for encryption.

Type:

```hcl
object({
    enabled          = bool
    key_vault_key_id = string
  })
```

Default:

```json
{
  "enabled": false,
  "key_vault_key_id": null
}
```

## Outputs

The following outputs are exported:

### <a name="output_endpoint"></a> [endpoint](#output\_endpoint)

Description: The endpoint for the OpenAI account.

### <a name="output_key_secret"></a> [key\_secret](#output\_key\_secret)

Description: The key vault secrets for the OpenAI account access keys.

## Resources

The following resources are used by this module:

- [azurerm_cognitive_account.main](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cognitive_account) (resource)
- [azurerm_cognitive_account_customer_managed_key.cmk](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cognitive_account_customer_managed_key) (resource)
- [azurerm_cognitive_deployment.completions](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cognitive_deployment) (resource)
- [azurerm_cognitive_deployment.embeddings](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cognitive_deployment) (resource)
- [azurerm_key_vault_secret.openai_primary_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_key_vault_secret.openai_secondary_key](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/key_vault_secret) (resource)
- [azurerm_monitor_metric_alert.alert](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/monitor_metric_alert) (resource)
- [azurerm_private_endpoint.ple](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_endpoint) (resource)
- [azurerm_role_assignment.key_vault_crypto_user](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/role_assignment) (resource)

## Requirements

The following requirements are needed by this module:

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.80)

## Providers

The following providers are used by this module:

- <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) (~> 3.80)

## Modules

The following Modules are called:

### <a name="module_diagnostics"></a> [diagnostics](#module\_diagnostics)

Source: ../diagnostics

Version:
<!-- END_TF_DOCS -->

## Update Docs

Run this command:

```
terraform-docs markdown document --output-file README.md --output-mode inject .
```
