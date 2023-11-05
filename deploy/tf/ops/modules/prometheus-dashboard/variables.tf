variable "azure_monitor_workspace_id" {
  description = "The ID of the Azure Monitor workspace to send prometheus data to."
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
  description = "The name prefix for the module resources."
  type        = string
}

variable "tags" {
  description = "A map of tags for the resource."
  type        = map(string)
}