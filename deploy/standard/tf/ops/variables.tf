variable "environment" {
  description = "The environment name."
  type        = string
}

variable "location" {
  description = "The location to deploy Azure resources."
  type        = string
}

variable "project_id" {
  description = "The project identifier."
  type        = string
}

variable "public_domain" {
  description = "Public DNS domain"
  type        = string
}

variable "tfc_agent_token" {
  description = "The token used by the agent to authenticate with Terraform Cloud."
  sensitive   = true
  type        = string
}
