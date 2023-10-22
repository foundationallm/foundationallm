# FoundationaLLM Prompt Hub API

The Prompt Hub API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Prompt Hub API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Prompt Hub API provides the following services:

- Resolves a request from the Agent Factory to return a target prompt
- Prompt configuration will be retrieved from the backing data source (currently Azure Blob Storage)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:PromptHubAPI:Key
- FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString
- FoundationaLLM:PromptHub:AgentMetadata:StorageContainer

### Prompt Hub not returning agents

Ensure that you have loaded at least one prompt configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
