terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.65"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
}

