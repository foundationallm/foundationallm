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

variable "sql_azuread_administrator" {
  description = "Azure AD group to be added as SQL Server administrator."
  type = object({
    name      = string
    object_id = string
  })
}