# FoundationaLLM Semantic Kernel API

The Semantic Kernel API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Semantic Kernel API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Semantic Kernel API provides the following services:

- Handle user prompts for completion and summaries from the Agent Factory API

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIKey
- FoundationaLLM:DurableSystemPrompt
- FoundationaLLM:CognitiveSearchMemorySource
- FoundationaLLM:BlobStorageMemorySource
