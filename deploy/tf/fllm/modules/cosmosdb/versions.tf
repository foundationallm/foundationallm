terraform {
  required_providers {
    azapi = {
      source  = "azure/azapi"
      version = "~> 1.9"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.65"
    }
  }
}

