# Changes to the 0.6.0 release

This document outlines the changes made to the FoundationaLLM project in the 0.6.0 release.

## Zero trust - removing dependencies on API keys

The following components are now added to the list of Entra ID managed identity-based authentication support:
- Azure CosmosDB service
- Azure OpenAI in LangChain
- AzureAIDirect orchestrator
- AzureOpenAIDirect orchestrator

## Citations

Citations (which means Explainability) is to be able to justify the responses returned by the agent and identify the source of where it based the response on.  This release include the API portion of this feature and in next releases we will include the UI portion of this feature.