# Self-Service Agent Creation

FoundationaLLM enables users to create and customize their own agents directly from the User Portal. This self-service capability empowers users to build specialized agents tailored to their specific needs without requiring administrator intervention.

## Overview

Self-service agent creation allows users to:

- Create custom agents with personalized configurations
- Define agent prompts and behavior
- Select models and tools
- Share agents with colleagues
- Manage agent lifecycle (active/expired)

> [!NOTE]
> Self-service agent creation requires the `FoundationaLLM.Agent.SelfService` feature flag to be enabled. This flag is set to `true` by default.

## Prerequisites

Before creating a self-service agent, ensure:

1. You have appropriate permissions to create agents
2. The self-service feature is enabled in your instance
3. You understand the basic concepts of agents and prompts

## Creating a New Agent

### Step 1: Access Agent Creation

1. Open the User Portal
2. Click on your profile or Settings menu
3. Navigate to **My Agents**
4. Click **Create New Agent**

<!-- [TODO: Add screenshot of agent creation entry point] -->

### Step 2: Configure Basic Information

Provide the following basic information:

| Field | Description | Required |
|-------|-------------|----------|
| **Agent Display Name** | The name users see in the agent dropdown | Yes |
| **Description** | A brief description of the agent's purpose | Recommended |
| **Welcome Message** | The greeting shown when users select this agent | No |

#### Welcome Message

The welcome message supports HTML formatting and can include:
- Agent capabilities description
- Usage guidelines
- Important notices

Example:
```html
<p>Welcome! I can help you with:</p>
<ul>
  <li>Answering questions about our product documentation</li>
  <li>Generating reports from uploaded data</li>
  <li>Creating visualizations</li>
</ul>
<p><em>Note: Please avoid sharing sensitive information.</em></p>
```

### Step 3: Configure Prompt Definition

The prompt defines your agent's persona, instructions, and guardrails.

<!-- [TODO: Add screenshot of prompt configuration UI] -->

**Best Practices for Prompts:**

1. **Define a clear persona**: Describe who the agent is and what it specializes in
2. **Set boundaries**: Specify what the agent should and shouldn't do
3. **Provide examples**: Include example interactions when helpful
4. **Add guardrails**: Include instructions for handling inappropriate requests

For detailed guidance, see [Prompt Resources](../agents/prompt-resource.md).

### Step 4: Select Model

Choose the language model that powers your agent:

| Model | Best For |
|-------|----------|
| GPT-4o | Complex reasoning, analysis |
| GPT-4o-mini | General purpose, faster responses |
| Claude | Creative writing, nuanced responses |
| Gemini | Multimodal tasks |

> [!NOTE]
> Available models depend on your instance configuration.

**Temperature Setting:**

Adjust the temperature to control response creativity:

| Temperature | Behavior |
|-------------|----------|
| 0.0 - 0.3 | More factual, deterministic |
| 0.4 - 0.7 | Balanced |
| 0.8 - 1.0 | More creative, varied |

### Step 5: Configure Agent Status and Behavior

Configure the agent's operational status and behavioral settings.

#### Agent Status

| Status | Description |
|--------|-------------|
| **Active** | Agent is available for use |
| **Inactive** | Agent is hidden but can be reactivated |
| **Expiration Date** | Optional date when the agent automatically becomes inactive |

#### Conversation History

Configure how the agent remembers previous exchanges within a session:

| Setting | Description | Default |
|---------|-------------|---------|
| **Enable History** | Agent retains context from earlier messages | Enabled |
| **Max Messages** | Number of previous messages to include in context | 5 |

Enabling conversation history allows for more natural, contextual conversations but consumes additional tokens.

#### Content Safety (Gatekeeper)

If available for self-service agents, configure content moderation:

| Option | Description |
|--------|-------------|
| **Content Safety** | Filter harmful content (Azure Content Safety) |
| **Data Protection** | Mask PII in conversations (Microsoft Presidio) |

#### Cost Center (Optional)

Assign a cost center identifier for usage tracking and budget allocation. This helps organizations track AI costs by department or project.

For detailed information on all agent configuration options, see [Agents and Workflows](../agents/agents_workflows.md#agent-configuration-section).

### Step 6: Select Tools

Enable tools to extend your agent's capabilities:

#### Image Generation

Enable the DALL-E image generation tool to allow your agent to create images from text descriptions.

<!-- [TODO: Document specific configuration options] -->

#### Upload from Computer

Allow users to upload files when chatting with your agent. Supported file types include:
- Documents (PDF, DOCX, TXT)
- Spreadsheets (CSV, XLSX)
- Images (PNG, JPG, GIF)

#### Private Knowledge Source

Connect your agent to a private storage location containing knowledge documents. Files uploaded to private storage are automatically processed and made available for your agent's context. See [Private Storage for Custom Agent Owners](../agents/private-storage.md) for configuration details.

<!-- [TODO: Document private storage tool selection UI] -->

#### Website Crawler (Future)

<!-- [TODO: Document current behavior and future capabilities when website crawler tool becomes available] -->

> [!NOTE]
> Website crawler functionality is planned for a future release.

### Step 7: Accessibility Considerations

Ensure your agent is accessible to all users:

- Provide clear, descriptive welcome messages
- Use plain language in prompts
- Include alt text for any images referenced
- Test with screen readers when possible

See [Accessibility Features](accessibility.md) for more information.

## Editing an Agent

To edit an existing self-service agent:

1. Navigate to **My Agents** in the User Portal
2. Find the agent you want to edit
3. Click the **Edit** icon
4. Make your changes
5. Click **Save**

> [!NOTE]
> Some properties may be read-only after initial creation. Contact your administrator if you need to change these properties.

## Deleting an Agent

To delete a self-service agent:

1. Navigate to **My Agents**
2. Find the agent you want to delete
3. Click the **Delete** icon
4. Confirm the deletion

> [!WARNING]
> Deleting an agent is permanent. Any users who have access to the agent will lose access immediately.

## Editable Properties Summary

| Property | Description | Editable After Creation |
|----------|-------------|-------------------------|
| Display Name | User-facing name | Yes |
| Description | Agent description | Yes |
| Welcome Message | Greeting shown to users | Yes |
| Prompt Definition | System prompt | Yes |
| Model Selection | Language model | Yes |
| Temperature | Model creativity setting | Yes |
| Agent Status | Active/Inactive | Yes |
| Expiration Date | Auto-deactivation date | Yes |
| Tools | Enabled capabilities | Yes |

## Next Steps

- [Agent Sharing Model](agent-sharing-model.md) - Learn how to share your agent with others
- [Agent Creation Walkthrough](walkthroughs/agent-creation-walkthrough.md) - Step-by-step guide with screenshots
- [Prompt Resources](../agents/prompt-resource.md) - Deep dive into prompt configuration
