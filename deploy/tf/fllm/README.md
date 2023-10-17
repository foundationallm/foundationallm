# Foundational LLM Standard Deployment

<!-- BEGIN_TF_DOCS -->
## Requirements

The following requirements are needed by this module:

- <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) (~> 1.6)

- <a name="requirement_acme"></a> [acme](#requirement\_acme) (~> 2.0)

- <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) (~> 3.65)

- <a name="requirement_tfe"></a> [tfe](#requirement\_tfe) (~> 0.49)

## Providers

No providers.

## Modules

The following Modules are called:

### <a name="module_global"></a> [global](#module\_global)

Source: ./global

Version:

### <a name="module_regions"></a> [regions](#module\_regions)

Source: ./region

Version:

## Resources

No resources.

## Required Inputs

The following input variables are required:

### <a name="input_azp_agent_name"></a> [azp\_agent\_name](#input\_azp\_agent\_name)

Description: n/a

Type: `string`

### <a name="input_azp_token"></a> [azp\_token](#input\_azp\_token)

Description: n/a

Type: `string`

### <a name="input_azp_url"></a> [azp\_url](#input\_azp\_url)

Description: n/a

Type: `string`

### <a name="input_tfc_agent_token"></a> [tfc\_agent\_token](#input\_tfc\_agent\_token)

Description: The token used by the agent to authenticate with Terraform Cloud.

Type: `string`

## Optional Inputs

The following input variables are optional (have default values):

### <a name="input_environment"></a> [environment](#input\_environment)

Description: The environment name

Type: `string`

Default: `"DEMO"`

### <a name="input_global_location"></a> [global\_location](#input\_global\_location)

Description: The global location

Type: `string`

Default: `"East US"`

### <a name="input_project_id"></a> [project\_id](#input\_project\_id)

Description: The project id

Type: `string`

Default: `"FLLM"`

### <a name="input_public_domain"></a> [public\_domain](#input\_public\_domain)

Description: Public DNS domain

Type: `string`

Default: `"internal.foundationallm.ai"`

### <a name="input_sql_admin_ad_group"></a> [sql\_admin\_ad\_group](#input\_sql\_admin\_ad\_group)

Description: SQL Admin AD group

Type:

```hcl
object({
    name      = string
    object_id = string
  })
```

Default:

```json
{
  "name": "FoundationaLLM SQL Admins",
  "object_id": "73d59f98-857b-45e7-950b-5ee30d289bc8"
}
```

### <a name="input_tags"></a> [tags](#input\_tags)

Description: The tags to use on each resource

Type: `map(string)`

Default: `{}`

## Outputs

No outputs.
<!-- END_TF_DOCS -->