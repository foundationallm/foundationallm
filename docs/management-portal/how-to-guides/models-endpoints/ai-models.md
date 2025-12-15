# AI Models

Learn how to configure and manage AI model deployments in the Management Portal.

## Overview

AI Models define the large language models (LLMs) and other AI models available to your FoundationaLLM deployment. These models power agent conversations, embeddings, and specialized capabilities like code interpretation and image generation.

## Accessing AI Models

1. In the Management Portal sidebar, click **AI Models** under the **Models and Endpoints** section
2. The models list loads, showing all configured models

## Model List

The table displays:

| Column | Description |
|--------|-------------|
| **Name** | Model identifier |
| **Source Type** | Model provider (Azure OpenAI, Anthropic, etc.) |
| **Edit** | Settings icon to modify configuration |
| **Delete** | Trash icon to remove the model |

### Searching and Managing

- Use the search box to filter by name
- Click the refresh button to reload the list
- Click column headers to sort

## Model Types

| Type | Description | Use Cases |
|------|-------------|-----------|
| **Chat/Completion** | Text generation models | Agent conversations, responses |
| **Embedding** | Vector embedding models | Semantic search, similarity |
| **Image Generation** | Image creation models | DALL-E, image generation tools |
| **Vision** | Image understanding models | Image analysis, OCR |

## Creating a Model

1. Click **Create Model** at the top right of the page
2. Configure the model settings

### Model Configuration

> **TODO**: Document specific model configuration fields when available in the UI, including:

| Field | Description |
|-------|-------------|
| **Model Name** | Unique identifier |
| **Source Type** | Provider/platform (Azure OpenAI, Anthropic, etc.) |
| **Deployment Name** | Cloud deployment identifier |
| **API Endpoint** | Endpoint URL reference |
| **Model Parameters** | Default parameters (temperature, max tokens, etc.) |

### Azure OpenAI Models

For Azure OpenAI deployments:

1. Select **Azure OpenAI** as the source type
2. Configure:
   - API endpoint reference
   - Deployment name
   - Model version

### Other Model Providers

For other providers (Anthropic, custom):

1. Select the appropriate source type
2. Configure provider-specific settings
3. Enter authentication details

## Model Configuration Details

### API Endpoint Association

Models are associated with API endpoint configurations:

1. Create or select an existing API endpoint
2. Link the model to the endpoint
3. The endpoint provides connection details

### Model Parameters

Configure default model behavior:

| Parameter | Description | Typical Range |
|-----------|-------------|---------------|
| **temperature** | Response randomness | 0.0 - 2.0 |
| **max_tokens** | Maximum response length | 1 - model limit |
| **top_p** | Nucleus sampling | 0.0 - 1.0 |

## Editing Models

1. Locate the model in the list
2. Click the **Settings** icon (⚙️)
3. Modify settings as needed
4. Click **Save Changes**

## Deleting Models

1. Click the **Trash** icon (🗑️) for the model
2. Confirm deletion in the dialog

> **Warning:** Deleting a model affects any agents using it. Verify dependencies before deleting.

## Using Models in Agents

Models are referenced in agent configurations:

### Workflow Main Model

The primary model for agent conversations:

1. In agent creation/editing, find the Workflow section
2. Select **Workflow Main Model** from the dropdown
3. Only compatible models appear

### Tool Models

Models assigned to specific tools:

1. In tool configuration, add a Model resource
2. Select the model
3. Assign a role (e.g., `main_model`)

## Access Control

Configure who can access and manage models:

| Permission | Description |
|------------|-------------|
| `FoundationaLLM.AIModel/aiModels/read` | View models |
| `FoundationaLLM.AIModel/aiModels/write` | Edit models |
| `FoundationaLLM.AIModel/aiModels/delete` | Delete models |

## Best Practices

### Naming Conventions

- Use descriptive names indicating model type and purpose
- Include version information when relevant
- Example: `gpt-4o-chat-main`, `text-embedding-3-large`

### Model Selection

- Use appropriate models for each task (chat vs. embedding)
- Consider cost and performance tradeoffs
- Test models before production use

### Parameter Configuration

- Set reasonable defaults
- Override at agent/tool level when needed
- Document parameter choices

## Troubleshooting

### Model Not Available in Dropdown

- Verify the model exists and is active
- Check your permissions
- Ensure the model type is compatible with the selection

### Model Responses Failing

- Verify API endpoint configuration
- Check authentication credentials
- Review Azure OpenAI deployment status

### Performance Issues

- Review token limits and quotas
- Check for rate limiting
- Consider model tier/capacity

## Related Topics

- [API Endpoints](api-endpoints.md)
- [Create New Agent](../agents/create-new-agent.md)
- [Configuration Reference](../../reference/configuration-reference.md)
