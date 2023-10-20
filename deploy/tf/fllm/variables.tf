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

# variable "sql_admin_ad_group" {
#   description = "SQL Admin AD group"
#   type = object({
#     name      = string
#     object_id = string
#   })
#   default = {
#     name      = "FoundationaLLM SQL Admins"
#     object_id = "73d59f98-857b-45e7-950b-5ee30d289bc8"
#   }
# }

# variable "tags" {
#   description = "The tags to use on each resource"
#   type        = map(string)
#   default     = {}
# }

# variable "tfc_agent_token" {
#   description = "The token used by the agent to authenticate with Terraform Cloud."
#   sensitive   = true
#   type        = string
# }