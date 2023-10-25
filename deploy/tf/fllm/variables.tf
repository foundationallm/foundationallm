variable "chat_entra_application" {
  description = "The Chat Entra application."
  type        = string
  default     = "FoundationaLLM"
}

variable "client_entra_application" {
  description = "The Client Entra application."
  type        = string
  default     = "FoundationaLLM-Client"
}

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

variable "sql_admin_ad_group" {
  description = "SQL Admin AD group"
  type = object({
    name      = string
    object_id = string
  })
}