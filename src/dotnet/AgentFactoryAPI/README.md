# FoundationaLLM Agent Factory API

The Agent Factory API facilities requests from the Gatekeeper API or other external applications.

## Overview

The Agent Factory API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The Agent Factory API provides the following services:

- Handles Completion and Summary requests from the Gatekeeper API
- Determines the best agent or agents for a user prompt
- Determines the best orchestrator (Semantic Kernel or LangChain)
- Builds a target(s) agent for the user prompt
  - Looks for a proper prompt to send to the sub-agents
  - Loads data sources for target agents (Data Source API)
- Performs IAM against requested entities (Agents, Data Sources, etc) based on user context
- Proxies the `resolve` requests for a user prompt and returns the completions to the Gatekeeper API
  
Downstream services that are called include:

- Agent Hub API
- Data Source API
- LangChain API
- Prompt Hub API

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.AgentFactoryAPI}:APIKey
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIKey
- FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIKey
- FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIKey
- FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIUrl
- FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIKey
- FoundationaLLM:AgentFactory

> NOTE: The APIUrl and APIKey (and most other values) are configured automatically for you via the deployment process, however, if the endpoints change due to some post configuration change, you will need to validate the urls and keys are still valid.
