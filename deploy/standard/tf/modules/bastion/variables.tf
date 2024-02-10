variable "action_group_id" {
  description = "The ID of the action group to send alerts to."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace to send diagnostics data to."
  type        = string
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
  description = "The subnet id to deploy the Bastion Host into."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}