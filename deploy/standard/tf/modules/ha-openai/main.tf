locals {}

# Data Sources

# Resources

# Modules
module "apim" {
  source = "./modules/apim"

  action_group_id            = var.action_group_id
  key_vault_id               = module.keyvault.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  private_dns_zones          = var.private_endpoint.apim_private_dns_zones
  publisher                  = var.publisher
  resource_group             = var.resource_group
  resource_prefix            = var.resource_prefix
  subnet_id                  = var.subnet_id
  tags                       = var.tags

  cognitive_account_endpoint = {
    for k, v in module.openai : k => {
      endpoint = v.endpoint
    }
  }

  # TODO: merge once we have an idea of what the datastructure should look like.
  openai_primary_key = {
    for k, v in module.openai : k => {
      id   = v.key_secret.primary.id
      name = v.key_secret.primary.name
    }
  }

  openai_secondary_key = {
    for k, v in module.openai : k => {
      id   = v.key_secret.secondary.id
      name = v.key_secret.secondary.name
    }
  }
}

module "keyvault" {
  source = "../keyvault"

  action_group_id            = var.action_group_id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  resource_group             = var.resource_group
  resource_prefix            = var.resource_prefix
  tags                       = var.tags
  tenant_id                  = var.tenant_id

  private_endpoint = {
    subnet_id            = var.private_endpoint.subnet_id
    private_dns_zone_ids = var.private_endpoint.kv_private_dns_zone_ids
  }
}

module "openai" {
  source     = "../openai"
  count      = var.instance_count
  depends_on = [module.keyvault] # Make terraform wait for secrets to destroy before deleting the PLE

  action_group_id            = var.action_group_id
  instance_id                = count.index
  key_vault_id               = module.keyvault.id
  log_analytics_workspace_id = var.log_analytics_workspace_id
  resource_group             = var.resource_group
  resource_prefix            = var.resource_prefix
  tags                       = var.tags

  private_endpoint = {
    subnet_id            = var.private_endpoint.subnet_id
    private_dns_zone_ids = var.private_endpoint.openai_private_dns_zone_ids
  }
}
