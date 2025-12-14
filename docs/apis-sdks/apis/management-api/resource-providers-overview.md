# Overview of Resource Providers

Learn about the resource providers available in FoundationaLLM.

## Overview

Resource providers are platform components that manage specific types of resources in FoundationaLLM.

> **Note:** This documentation can be generated from the resource provider metadata files in the source code.

## Available Resource Providers

### FoundationaLLM.Agent
Manages agents, agent workflows, agent tools, and agent access tokens.

**Resource Types:**
- `agents`
- `workflows`
- `tools`
- `accessTokens`

### FoundationaLLM.AIModel
Manages AI model configurations.

**Resource Types:**
- `aiModels`

### FoundationaLLM.Attachment
Manages file attachments uploaded by users.

**Resource Types:**
- `attachments`

### FoundationaLLM.Authorization
Manages authorization artifacts.

**Resource Types:**
- `roleAssignments`
- `roleDefinitions`

### FoundationaLLM.Configuration
Manages platform configuration settings.

**Resource Types:**
- `appConfigurations`
- `apiEndpoints`

### FoundationaLLM.DataPipeline
Manages data pipelines and related artifacts.

**Resource Types:**
- `dataPipelines`
- `dataPipelineRuns`

### FoundationaLLM.DataSource
Manages data source connections.

**Resource Types:**
- `dataSources`

### FoundationaLLM.Plugin
Manages plugin packages and plugins.

**Resource Types:**
- `pluginPackages`
- `plugins`

### FoundationaLLM.Prompt
Manages prompt templates.

**Resource Types:**
- `prompts`

## API Pattern

All resource providers follow a consistent API pattern:
```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

## Related Topics

- [Management API Overview](index.md)
- [Directly Calling Management API](directly-calling-management-api.md)
