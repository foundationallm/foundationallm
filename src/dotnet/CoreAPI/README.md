# FoundationaLLM Core API

The Core API facilities requests from the Chat UI or other external applications.  

## Overview

The Core API is secured via Azure AD and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Core API provides the following services:

- Support custom branding of the Chat UI
- Create and load user chat sessions
- Pass user prompts for completion and summaries to the Gatekeeper API

## Instructions

Coming soon.

## Troubleshooting

### Chat UI will not load

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:CosmosDB
- FoundationaLLM:Branding
- FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIKey
- FoundationaLLM:CoreAPI:Entra:ClientSecret
- FoundationaLLM:CoreAPI:Entra:Instance
- FoundationaLLM:CoreAPI:Entra:TenantId
- FoundationaLLM:CoreAPI:Entra:ClientId
- FoundationaLLM:CoreAPI:Entra:CallbackPath
- FoundationaLLM:CoreAPI:Entra:Scopes

### Login is not working

The most common issue is the Azure Entra application client id and secret is not configured properly.  Verify your settings and restart the Chat UI.

### Chat sessions are not displaying

Ensure the Gatekeeper API service is configured, running and not erroring.
