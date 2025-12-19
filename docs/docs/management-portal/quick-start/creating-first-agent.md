# Creating Your First Agent

A step-by-step guide to creating your first AI agent using the Management Portal.

## Prerequisites

Before creating an agent, ensure:

- You have access to the Management Portal with appropriate permissions
- At least one AI model is configured (under **Models and Endpoints > AI Models**)
- (Optional) Data sources are configured if your agent needs to access organizational data

## Overview of the Agent Creation Process

Creating an agent involves configuring several sections:

1. **General Information** - Name, description, and welcome message
2. **Agent Configuration** - Conversation history, gatekeeper, and display settings
3. **Workflow** - The AI workflow type and model settings
4. **Tools** - Additional capabilities like code interpreter or image generation
5. **Security** - Access tokens for API access (optional)

## Step-by-Step Guide

### Step 1: Navigate to Create New Agent

1. In the Management Portal sidebar, click **Create New Agent** under the **Agents** section
2. The agent creation form appears with all configuration sections

### Step 2: Enter General Information

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Agent Name** | Unique identifier for the agent | Letters, numbers, dashes, and underscores only. No spaces or special characters. |
| **Agent Display Name** | User-friendly name shown in the portal | Any text. This is what users will see. |
| **Description** | Purpose of the agent | Optional but recommended for discoverability |
| **Welcome Message** | Initial message shown to users | Supports rich text formatting |

The agent name field validates in real-time:
- ✔️ Green checkmark indicates the name is available
- ❌ Red X indicates the name is invalid or already taken

### Step 3: Configure Agent Behavior

#### Conversation History

| Setting | Default | Description |
|---------|---------|-------------|
| **Enabled** | No | When enabled, the agent remembers previous messages in the conversation |
| **Max Messages** | 5 | Number of previous messages to include in context |

Click the step item to expand and edit these settings. Toggle to **Yes** to enable conversation history, then set the maximum messages as needed.

#### Gatekeeper

The gatekeeper provides content safety and data protection:

| Setting | Options | Description |
|---------|---------|-------------|
| **Use system default** | Yes/No | Use instance-level gatekeeper settings |
| **Content Safety** | Azure Content Safety, Lakera Guard, Enkrypt Guardrails | AI safety platform for content moderation |
| **Data Protection** | Microsoft Presidio | PII detection and redaction |

If you disable "Use system default," you can select specific content safety and data protection options.

#### User Prompt Rewrite

| Setting | Description |
|---------|-------------|
| **Enabled** | When enabled, rewrites user prompts before processing |
| **Rewrite Model** | AI model to use for rewriting |
| **Rewrite Prompt** | Prompt template for rewriting |
| **Rewrite Window Size** | Number of messages to consider (default: 3) |

#### Semantic Cache

| Setting | Description |
|---------|-------------|
| **Enabled** | When enabled, caches semantically similar responses |
| **Model** | Embedding model for similarity comparison |
| **Embedding Dimensions** | Vector dimensions (default: 2048) |
| **Minimum Similarity Threshold** | Similarity score required for cache hit (default: 0.97) |

#### Cost Center and Expiration

| Field | Description |
|-------|-------------|
| **Cost Center** | Assign to a department for cost tracking (optional) |
| **Expiration Date** | Date when the agent becomes inactive (optional) |

### Step 4: Configure User Portal Experience

These settings control what features are available to users in the Chat User Portal:

| Setting | Default | Description |
|---------|---------|-------------|
| **Show Message Tokens** | Yes | Display token consumption for each message |
| **Allow Rating** | Yes | Let users rate agent responses (thumbs up/down) |
| **Show View Prompt** | Yes | Allow users to view the full completion prompt |
| **Allow File Upload** | No | Enable file attachments in conversations |

### Step 5: Select and Configure Workflow

1. Select a **Workflow Type** from the dropdown:

| Workflow Type | Description | Best For |
|---------------|-------------|----------|
| **OpenAIAssistants** | Azure OpenAI Assistants API with Code Interpreter, File Search, and Function Calling | Complex tasks requiring code execution or file analysis |
| **LangGraphReactAgent** | LangGraph-based agent with dynamic tool selection | Flexible agents that choose tools based on context |
| **ExternalAgentWorkflow** | Custom Python workflows registered with the platform | Advanced custom logic |

2. Click **Configure Workflow** to expand the workflow settings

3. Configure workflow details:

| Field | Description |
|-------|-------------|
| **Workflow Name** | Identifier for this workflow configuration |
| **Workflow Package Name** | Python package name (for custom workflows) |
| **Workflow Class Name** | Python class name (for custom workflows) |
| **Workflow Host** | Orchestration framework (typically LangChain) |
| **Workflow Main Model** | Primary AI model for the agent |
| **Workflow Main Model Parameters** | Model settings (temperature, max_tokens, etc.) |
| **Main Workflow Prompt** | System prompt defining the agent's persona and behavior |

Example system prompt:
```
You are a helpful assistant named [Agent Name] that helps users with [specific purpose]. 
Provide concise, accurate answers. If you don't know something, say so clearly.
```

### Step 6: Add Tools (Optional)

Tools extend the agent's capabilities. Click **Add New Tool** to configure:

| Tool Property | Description |
|---------------|-------------|
| **Name** | Tool identifier (must be unique) |
| **Package Name** | Python package containing the tool |
| **Description** | What the tool does (helps the AI decide when to use it) |
| **Tool Resources** | Additional resources the tool needs (models, data sources) |

**Common Tools:**

- **DALLEImageGeneration** - Generate images using DALL-E
- **Code Interpreter** - Execute Python code for analysis
- **Knowledge Search** - Search configured knowledge sources

> **Note:** For DALL-E image generation, the tool name must be exactly `DALLEImageGeneration` and requires an AI model with the `main_model` object role in Tool Resources.

### Step 7: Create the Agent

1. Review all settings
2. Click **Create Agent** at the bottom of the page
3. Wait for the creation process to complete
4. Upon success, you'll see a confirmation message

## Testing Your Agent

After creation:

1. Open the **Chat User Portal**
2. Select your new agent from the agent dropdown
3. Send a test message to verify it responds correctly
4. Test any configured tools (file upload, code execution, etc.)

## Next Steps

- [Detailed Agent Creation Guide](../how-to-guides/agents/create-new-agent.md) - In-depth configuration options
- [Managing Prompts](../how-to-guides/agents/prompts.md) - Create reusable prompt templates
- [Agent Access Tokens](../reference/concepts/agent-access-tokens.md) - Configure API access
- [Agents & Workflows Reference](../reference/concepts/agents-workflows.md) - Technical details

## Troubleshooting

### Agent Name Validation Fails
- Ensure no spaces or special characters
- Check if the name is already in use
- Use only letters, numbers, dashes (-), and underscores (_)

### Workflow Model Not Available
- Verify AI models are configured under **Models and Endpoints > AI Models**
- Check that your account has access to the required models

### Agent Not Appearing in Chat Portal
- Verify the agent was created successfully
- Check user permissions for the agent
- Ensure the agent hasn't expired (if expiration was set)
