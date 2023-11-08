variable "azure_monitor_workspace_id" {
  description = "The ID of the Azure Monitor workspace to send prometheus data to."
  type        = string
}

variable "cluster" {
  description = "The AKS Cluster Details."
  type = object({
    name = string
    id   = string
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
