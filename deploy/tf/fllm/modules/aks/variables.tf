variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "agw_id" {
  description = "Specify the application gateway ID for incoming traffic."
  type        = string
}

variable "administrator_object_ids" {
  description = "Groups or users that should be granted admin access to the cluster."
  type        = list(string)
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
}

variable "private_endpoint" {
  description = "The private endpoint configuration."
  type = object({
    subnet = object({
      id   = string
      name = string
    })
    private_dns_zone_ids = map(list(string))
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
  description = "The name prefix for the module resources."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}

variable "tenant_id" {
  description = "The ID of the tenant to use for RBAC."
  type        = string
}