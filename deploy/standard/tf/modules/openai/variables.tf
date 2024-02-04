variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "capacity" {
  description = "The capacity of the OpenAI instance deployments."

  default = {
    completions = 120
    embeddings  = 120
  }

  type = object({
    completions = number
    embeddings  = number
  })
}

variable "customer_managed_key" {
  description = "The ID of the customer managed key to use for encryption."

  default = {
    enabled          = false
    key_vault_key_id = null
  }

  type = object({
    enabled          = bool
    key_vault_key_id = string
  })

  validation {
    condition     = var.customer_managed_key.enabled == false || var.customer_managed_key.key_vault_key_id != null
    error_message = "customer_managed_key.key_vault_key_id must be set when customer_managed_key.enabled is true."
  }
}


variable "instance_id" {
  description = "The ID of the instance to deploy."
  type        = number
}

variable "key_vault_id" {
  description = "The ID of the Key Vault to store secrets in."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "private_endpoint" {
  description = "The private endpoint configuration."
  type = object({
    subnet_id            = string
    private_dns_zone_ids = list(string)
  })
}

variable "resource_group" {
  description = "The resource group to deploy resources into"

  type = object({
    id       = string
    location = string
    name     = string
  })
}

variable "resource_prefix" {
  description = "The name prefix for the resources."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}