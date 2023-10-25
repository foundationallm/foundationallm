variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "agw_id" {
  description = "The ID of the application gateway to use for ingress."
  type        = string
}

variable "aks_admin_object_id" {
  description = "The object ID of the AKS administrator."
  type        = string
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