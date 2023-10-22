# FoundationaLLM LangChain API

The LangChain API facilities requests from the Gatekeeper API or other external applications.

## Overview

The LangChain API is secured via Azure AD and an `X-API-KEY` and requires a user context that is typically passed from the Azure AD/Identity platform secured UI/application.

The LangChain API provides the following services:

- Resolves a request from the Agent Factory to return user prompt completions
- LangChain based agents (anomly detection, conversational, csv, database, etc)
- Data source type handling (sql, csv, blob)
- Language model support
- Prompt orchesration (chaining agents, data sources and toolkits)
- Langchain Toolkits and custom FoundationaLLM toolkits (secured versions)

## Instructions

Coming soon.

## Troubleshooting

### Service is not starting

Ensure that all configuration values have been set in the Azure Key Vault along with the corresponding App Configuration settings. These include:

- FoundationaLLM:AppConfig:ConnectionString
- FoundationaLLM:APIs:LangChainAPI:Key
- FoundationaLLM:AzureOpenAI:API:Endpoint
- FoundationaLLM:AzureOpenAI:API:Version
- FoundationaLLM:AzureOpenAI:API:Key

### Data Source Hub not returning agents

Ensure that you have loaded at least one data source configuration into the blog storage container and that the search criteria matches what is being requested from the Agent Factory API.
