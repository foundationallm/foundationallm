terraform {
  required_version = "~> 1.6"

  required_providers {
    azapi = {
      source  = "azure/azapi"
      version = "~> 1.9"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.65"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.44"
    }
  }

  cloud {
    organization = "FoundationaLLM"
    workspaces {
      name = "FoundationaLLM-Platform"
    }
  }
}

