variable "core_entra_application" {
  description = "The Core API Entra application."
  type        = string
  default     = "FoundationaLLM-API"
}

variable "client_entra_application" {
  description = "The Chat Client Entra application."
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

variable "namespace" {
  description = "The namespace to deploy Azure resources."
  type        = string
  default     = "default"
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

variable "test_db_password" {
  description = "The test database password."
  type        = string
  default     = ""
  sensitive   = true
}