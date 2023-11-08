variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "cognitive_account_endpoint" {
  description = "The endpoint for the Cognitive Services accounts."
  type = map(object({
    endpoint = string
  }))
}

variable "key_vault_id" {
  description = "The ID of the Key Vault to store secrets in."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "openai_primary_key" {
  description = "The primary key for the OpenAI account."
  type = map(object({
    id   = string
    name = string
  }))
}

variable "openai_secondary_key" {
  description = "The secondary key for the OpenAI account."
  type = map(object({
    id   = string
    name = string
  }))
}

variable "private_dns_zones" {
  description = "The private DNS zones to create A records in."
  type = list(object({
    name                = string
    resource_group_name = string
  }))
}

variable "publisher" {
  description = "The API publisher details"
  type = object({
    email = string
    name  = string
  })
}

variable "resource_group" {
  description = "The resource group to deploy resources into"

  type = object({
    location = string
    name     = string
  })
}

variable "resource_prefix" {
  description = "The name prefix for the Log Analytics workspace."
  type        = string
}

variable "subnet_id" {
  description = "The subnet ID to deploy the API Management instance into."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}