variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "administrator_object_ids" {
  description = "Groups or users that should be granted admin access to the cluster."
  type        = list(string)
}

variable "application_gateway" {
  description = "Specify the application gateway for incoming traffic."
  type = object({
    id            = string
    identity_id   = string
    agw_subnet_id = string
  })
}

variable "azure_monitor_workspace_id" {
  description = "The ID of the Azure Monitor workspace to send prometheus data to."
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

variable "subnet_id" {
  description = "The ID of the subnet to deploy the cluster nodes into."
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