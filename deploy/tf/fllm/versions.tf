terraform {
  required_version = "~> 1.6"

  required_providers {

    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.65"
    }
  }

  cloud {
    organization = "FoundationaLLM"
    workspaces {
      name = "FoundationaLLM-Platform"
    }
  }
}

