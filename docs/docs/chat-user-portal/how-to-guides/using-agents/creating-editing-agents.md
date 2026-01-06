> **This article is still being authored.** Some sections contain placeholder content that requires additional information.

# Creating and Editing Custom Agents

Learn how to create and customize your own agents in the Chat User Portal when self-service agent creation is enabled.

## Overview

When your organization enables the Agent Self-Service feature, you can create custom agents tailored to your specific needs directly from the Chat User Portal. Custom agents allow you to:

- Define specialized behaviors with custom prompts
- Select appropriate AI models for your use case
- Configure tools like image generation and file upload
- Share agents with colleagues

> **Note:** This feature must be enabled by your administrator. If you don't see agent creation options, contact your IT department.

## Prerequisites

Before creating custom agents:

- Your organization must have **Agent Self-Service** enabled
- You need appropriate permissions (typically the **Agent Contributor** role)
- At least one AI model must be available for selection

## Accessing Agent Creation

### From Settings

1. Click the **Settings** button (gear icon ⚙️) in the bottom-left corner of the sidebar
2. Click the **Manage Agents** link at the bottom of the dialog
3. Click **Create New Agent**

### From Agent Management

1. Open the Agent Management page (if available)
2. Click **Create New Agent** button
3. The agent creation form opens

## Agent Configuration Options

### Basic Information

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Agent Display Name** | The name users see when selecting the agent | Required. Any text. |
| **Description** | Brief description of what the agent does | Recommended for discoverability |
| **Welcome Message** | Greeting shown when starting a conversation | Optional. Supports rich text. |

### Prompt Definition

The prompt defines your agent's personality, knowledge focus, and behavior:

| Setting | Description |
|---------|-------------|
| **System Prompt** | Instructions that guide the agent's responses |
| **Prompt Style** | Formal, conversational, technical, etc. |
| **Focus Area** | What topics the agent specializes in |

**Best practices for prompts:**

- Be specific about the agent's role and expertise
- Include any constraints or boundaries
- Specify the tone and style of responses
- Define how to handle off-topic questions

### Model Selection

Choose the AI model that powers your agent:

| Consideration | Guidance |
|---------------|----------|
| **Capability** | More capable models handle complex tasks better |
| **Speed** | Faster models provide quicker responses |
| **Cost** | Different models have different usage costs |
| **Context Length** | Larger context windows support longer conversations |

> **Note:** Available models depend on what your administrator has configured.

### Temperature Setting

Temperature controls the randomness/creativity of responses:

| Temperature | Behavior | Best For |
|-------------|----------|----------|
| **Low (0.0-0.3)** | Consistent, focused responses | Factual Q&A, data analysis |
| **Medium (0.4-0.7)** | Balanced creativity | General assistance |
| **High (0.8-1.0)** | More creative, varied responses | Creative writing, brainstorming |

### Agent Status

| Status | Description |
|--------|-------------|
| **Active** | Agent is available for use |
| **Inactive** | Agent exists but cannot be selected |
| **Expired** | Agent has passed its expiration date (if set) |

You can set an **Expiration Date** to automatically disable the agent after a certain date.

## Tool Selection

Enable tools to extend your agent's capabilities:

### Image Generation

Enable agents to create images using AI:

- Powered by DALL-E or similar models
- Users can request image creation in conversations
- Generated images can be downloaded

### Upload from Computer

Allow users to upload files for analysis:

- Supports common document formats (PDF, DOCX, etc.)
- Files are processed for the current conversation
- Enable for document analysis, data processing tasks

### Private Storage Knowledge Source

Connect to agent-specific private storage:

- Access documents stored in the agent's dedicated storage
- Provides persistent knowledge across conversations
- Configured separately in Private Storage settings

### Website Crawler

> **TODO**: This feature is planned for future release. Website crawler tool will allow agents to access and search web content.

## Accessibility Best Practices

When creating agents, follow accessibility guidelines:

### Explainer Text

Provide clear, accessible descriptions:

- Use plain language in agent descriptions
- Explain what the agent does in the welcome message
- Include guidance on how to interact effectively

### Accessibility Considerations

| Element | Guidance |
|---------|----------|
| **Display Name** | Use clear, descriptive names |
| **Welcome Message** | Provide context for screen reader users |
| **Tool Descriptions** | Explain what each enabled tool does |
| **Error Messages** | Ensure errors are clearly communicated |

## Saving Your Agent

1. Review all configuration settings
2. Click **Create Agent** or **Save Changes**
3. Wait for the creation process to complete
4. Your agent appears in your agent list

## Editing Existing Agents

### Accessing Edit Mode

1. Open **Settings** > **Agents** tab
2. Find your agent in the list
3. Click the **Edit** button (pencil icon)

### Editable Properties

You can modify:

- Display name (not the internal agent name)
- Welcome message
- Prompt definition
- Model selection
- Temperature
- Tool configuration
- Agent status and expiration

> **Note:** The internal agent name cannot be changed after creation.

### Saving Changes

1. Make your modifications
2. Click **Save Changes**
3. Changes take effect immediately for new conversations

## Agent Visibility

### Who Can See Your Agent

Custom agents you create are:

- Visible to you by default
- Shareable with others through the sharing model
- Subject to your organization's visibility policies

### Making Agents Available

See [Sharing Agents](sharing-agents.md) for information on sharing your custom agents with others.

## Limitations

| Limitation | Description |
|------------|-------------|
| **Model Access** | Only models configured by admins are available |
| **Tool Access** | Only enabled tools can be selected |
| **Storage** | Private storage requires separate configuration |
| **Permissions** | Some settings may require elevated permissions |

## Troubleshooting

### Can't Create Agents

- Verify Agent Self-Service is enabled for your organization
- Check you have the Agent Contributor role
- Contact your administrator for permission

### Model Not Available

- The model may not be configured in your deployment
- Your permissions may not include that model
- Contact your administrator

### Tools Not Working

- Verify the tool is properly enabled
- Check your agent configuration
- Some tools require additional setup

### Agent Not Appearing

- Ensure the agent status is Active
- Check the agent hasn't expired
- Verify sharing settings if others can't see it

## Related Topics

- [Sharing Agents](sharing-agents.md) — Share your agents with others
- [Managing Available Agents](managing-available-agents.md) — Control which agents you see
- [Selecting an Agent](selecting-agent.md) — Choose an agent for conversations
