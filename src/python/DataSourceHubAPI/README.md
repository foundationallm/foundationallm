# FoundationaLLM Data Source Hub API

The Data Source Hub API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Data Source Hub API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Data Source Hub API provides the following services:

- Resolves a request from the Agent Factory to return a target Data source configuration
- Data source configuration will be retrieved from the backing data source (currently Azure Blob Storage)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:DataSourceHubAPI:Key
- FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString
- FoundationaLLM:DataSourceHub:AgentMetadata:StorageContainer

### Data Source Hub not returning agents

Ensure that you have loaded at least one data source configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
