# AI Models

Learn how to manage AI model configurations in the Management Portal.

## Overview

AI Models define the language models available for use by agents.

## Accessing AI Models

1. Navigate to **Models and Endpoints** in the sidebar
2. Click **AI Models**

## Model Configuration

Each model configuration includes:
- **Model Name**: Identifier for the model
- **Provider**: Microsoft (Azure OpenAI), OpenAI, etc.
- **Deployment Name**: The deployment identifier
- **Endpoint**: API endpoint URL
- **Authentication**: Key or token-based

## Adding a Model

1. Click **Add Model**
2. Select the provider
3. Configure connection settings
4. Set default parameters (temperature, etc.)
5. Save the configuration

## Model Parameters

### Temperature
Controls response randomness (0.0 to 1.0)

### Max Tokens
Maximum response length

### Top P
Alternative to temperature for sampling

## Using Models in Agents

Agents reference models through:
- Workflow main model configuration
- Tool resource assignments

## Related Topics

- [API Endpoints](api-endpoints.md)
- [Agents & Workflows](../../reference/concepts/agents-workflows.md)
