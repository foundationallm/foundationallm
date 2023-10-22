# FoundationaLLM Agent Hub API

The Agent Hub API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Agent Hub API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Agent Hub API provides the following services:

- Resolves a request from the Agent Factory to return a target Agent configuration
- Agent configuration will be retrieved from the backing data source (currently Azure Blob Storage)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:AgentHubAPI:Key
- FoundationaLLM:AgentHub:StorageManager:BlobStorage:ConnectionString
- FoundationaLLM:AgentHub:AgentMetadata:StorageContainer

### Agent Hub not returning agents

Ensure that you have loaded at least one agent configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
