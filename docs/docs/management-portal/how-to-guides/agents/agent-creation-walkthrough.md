# Agent Creation UX Walkthrough

A step-by-step walkthrough of the agent creation experience in the Management Portal.

## Overview

This guide walks you through the complete user experience of creating a new agent, from initial navigation through all configuration options to final creation.

## Prerequisites

Before creating an agent, ensure you have:

- Access to the Management Portal with appropriate permissions
- At least one AI model configured under **Models and Endpoints > AI Models**
- (Optional) Prompts created for workflow and tool configurations
- (Optional) Data sources configured if your agent needs knowledge retrieval

## Starting Agent Creation

### Step 1: Navigate to Create New Agent

1. Log into the Management Portal
2. In the left sidebar, click **Create New Agent**
3. The agent creation form loads

### Step 2: Overview of the Form

The creation form is organized into expandable sections:

| Section | Purpose |
|---------|---------|
| **General Information** | Basic agent details |
| **Agent Configuration** | Behavior settings |
| **User Portal Experience** | Feature toggles |
| **Workflow** | Processing configuration |
| **Tools** | Extended capabilities |

## Section 1: General Information

### What You'll Configure

1. **Agent Name**
   - Type a unique identifier (e.g., `sales-assistant`)
   - Allowed: letters, numbers, dashes, underscores
   - Not allowed: spaces, special characters
   - Real-time validation shows if the name is valid
   - This name cannot be changed after creation

2. **Agent Display Name**
   - Type a user-friendly name (e.g., `Sales Assistant`)
   - This is what users see in the agent selector
   - Can include spaces and special characters
   - Can be changed later

3. **Description**
   - Describe what the agent does
   - Helps with discoverability
   - Displayed in agent lists

4. **Welcome Message**
   - Rich text editor opens when you click the field
   - Create a greeting for when users start conversations
   - Supports formatting (bold, italic, lists, links)
   - Preview shows how it will appear

### Validation

- Red error text appears below invalid fields
- Agent name must be unique — duplicates are rejected
- Required fields are marked with asterisks

## Section 2: Agent Configuration

### 2.1 Conversation History

1. Locate the **Conversation History** section
2. Toggle **Enabled** on or off
3. If enabled, set **Max Messages** (number field)
   - Default is 5 messages
   - Higher values provide more context but use more tokens

### 2.2 Gatekeeper Settings

1. Find the **Gatekeeper** section
2. Choose your approach:
   - **Use system default**: Applies instance-level settings
   - **Custom**: Select specific options

3. If using custom settings:
   - **Content Safety**: Check platforms to enable
     - Azure Content Safety
     - Azure Content Safety Prompt Shield
     - Lakera Guard
     - Enkrypt Guardrails
   - **Data Protection**: Check services to enable
     - Microsoft Presidio

### 2.3 User Prompt Rewrite

1. Find **User Prompt Rewrite** section
2. Toggle **Enabled** if you want prompts rewritten
3. If enabled, configure:
   - **Rewrite Model**: Select from dropdown
   - **Rewrite Prompt**: Select or create a prompt
   - **Rewrite Window Size**: Number of messages to consider

### 2.4 Semantic Cache

1. Find **Semantic Cache** section
2. Toggle **Enabled** for caching similar responses
3. If enabled, configure:
   - **Model**: Embedding model for similarity
   - **Embedding Dimensions**: Vector size (default: 2048)
   - **Minimum Similarity Threshold**: Match threshold (default: 0.97)

### 2.5 Cost Center and Expiration

1. **Cost Center**: Optional text field for department tracking
2. **Expiration Date**: Optional date picker
   - Click to open calendar
   - Select when the agent should auto-disable
   - Leave empty for no expiration

## Section 3: User Portal Experience

Configure what features users can access:

| Toggle | Default | What It Controls |
|--------|---------|------------------|
| **Show Message Tokens** | Yes | Display token consumption |
| **Allow Rating** | Yes | Enable thumbs up/down ratings |
| **Show View Prompt** | Yes | Allow viewing completion prompts |
| **Allow File Upload** | No | Enable file attachments |

1. Click each toggle to enable/disable
2. Changes are immediate (no save needed until final creation)

## Section 4: Workflow Configuration

### 4.1 Selecting Workflow Type

1. Find the **Workflow** dropdown
2. Select a workflow type:

| Type | Description |
|------|-------------|
| **OpenAIAssistants** | Azure OpenAI Assistants API |
| **LangGraphReactAgent** | LangGraph with dynamic tool selection |
| **ExternalAgentWorkflow** | Custom Python workflows |

### 4.2 Configuring Workflow

1. Click **Configure Workflow** to expand settings
2. Fill in required fields:

   **Workflow Name**: Identifier for this workflow

   **Workflow Host**: Select orchestration framework
   - LangChain
   - Other options depending on deployment

   **Workflow Main Model**: 
   - Click the dropdown
   - Select from available AI models
   - This is the primary model for responses

   **Main Workflow Prompt**:
   - Click the dropdown
   - Select an existing prompt
   - Or create new prompt (opens prompt editor)

3. **Model Parameters** (optional):
   - Expand to set temperature, max tokens, etc.

### 4.3 Adding Workflow Resources

For additional prompts needed by the workflow:

1. Click **Add Workflow Resource**
2. Select **Resource Type**: Model, Prompt, or other
3. Select the specific **Resource** from dropdown
4. Enter the **Resource Role** (e.g., `router_prompt`)
5. Click **Save**
6. Repeat for additional resources

## Section 5: Tools Configuration

### Adding a Tool

1. In the **Tools** section, click **Add New Tool**
2. A tool configuration panel opens

### Configuring a Tool

1. **Tool Name**: Enter unique identifier
2. **Tool Description**: Describe what it does (helps AI decide when to use it)
3. **Tool Package Name**: Python package (e.g., `foundationallm.tools`)
4. **Tool Class Name**: Python class (e.g., `DALLEImageGeneration`)

### Adding Tool Resources

1. Click **Add Resource** in the tool panel
2. Select resource type (Model, Prompt, Data Pipeline)
3. Select the specific resource
4. Set the resource role
5. Save

### Adding Tool Properties

1. Click **Add Property**
2. Enter property name
3. Enter property value
4. Save

### Common Tools

| Tool | Purpose | Package | Class |
|------|---------|---------|-------|
| DALL-E | Image generation | `foundationallm.tools` | `DALLEImageGeneration` |
| Code Interpreter | Python execution | `foundationallm.tools` | `FoundationaLLMCodeInterpreterTool` |
| Knowledge Search | Search data sources | `foundationallm.tools` | `FoundationaLLMKnowledgeTool` |

## Creating the Agent

### Final Review

1. Scroll through all sections
2. Verify required fields are completed:
   - Agent Name ✓
   - Agent Display Name ✓
   - Workflow Main Model ✓
   - Main Workflow Prompt ✓

3. Check for any validation errors (red text)

### Submitting

1. Click **Create Agent** at the bottom of the page
2. A loading indicator appears
3. Wait for the creation process
4. Success: Redirected or confirmation message
5. Failure: Error message with details

### After Creation

1. The new agent appears in All Agents list
2. Users with access can select it in the User Portal
3. You can edit settings at any time
4. Configure Access Control to grant permissions to others

## Validation Rules Summary

| Field | Rules |
|-------|-------|
| Agent Name | Required, unique, alphanumeric with dashes/underscores |
| Display Name | Required, any text |
| Workflow Main Model | Required |
| Main Workflow Prompt | Required |
| Expiration Date | Must be future date if set |
| Temperature | 0.0 to 1.0 |
| Max Messages | Positive integer |

## Common Issues and Solutions

### "Agent name already exists"

- Choose a different name
- Check All Agents for existing names
- Add a suffix to make unique

### "Model not found"

- Verify models are configured in Models and Endpoints
- Refresh the page to reload model list
- Contact administrator if no models appear

### "Workflow prompt required"

- Create a prompt first under Prompts
- Or use the inline prompt creation option

### Form not saving

- Check for validation errors
- Verify your session hasn't timed out
- Try a different browser

## Related Topics

- [Create New Agent](create-new-agent.md) — Detailed configuration reference
- [Agent Management Walkthrough](agent-management-walkthrough.md) — Managing existing agents
- [Prompts](prompts.md) — Creating and managing prompts
- [Agents & Workflows Reference](../../reference/concepts/agents-workflows.md) — Technical details
