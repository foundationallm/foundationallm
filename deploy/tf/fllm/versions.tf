terraform {
  required_version = "~> 1.6"

  required_providers {

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

