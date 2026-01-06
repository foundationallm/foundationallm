# Managing Prompts

Learn how to create and manage prompts in the Management Portal.

## Overview

Prompts are reusable text templates that define agent behavior, tool instructions, and data pipeline processing. They provide a centralized way to manage the instructions that guide AI model responses.

## Accessing Prompts

1. In the Management Portal sidebar, click **Prompts** under the **Agents** section
2. The prompt list loads, showing all available prompts

## Prompt List

The table displays:

| Column | Description |
|--------|-------------|
| **Name** | Unique identifier for the prompt |
| **Category** | Type of prompt (Workflow, Tool, DataPipeline) |
| **Description** | Purpose of the prompt |
| **Edit** | Settings icon to modify the prompt |

### Searching and Sorting

- Use the search box to filter by name or description
- Click column headers to sort
- Use pagination controls to navigate large lists

## Prompt Categories

| Category | Usage |
|----------|-------|
| **Agent Workflow** | Main prompts for agent orchestration, routing, and response generation |
| **Agent Tool** | Instructions for specific tools (code interpreter, knowledge search) |
| **Data Pipeline** | Prompts used in data processing workflows |

## Creating a Prompt

1. Click **Create Prompt** at the top right of the page
2. Configure the prompt settings:

### Prompt Configuration Fields

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Prompt Name** | Unique identifier | Letters, numbers, dashes, underscores only. No spaces. |
| **Description** | Purpose and usage notes | Recommended for discoverability |
| **Category** | Type of prompt | Select from dropdown |
| **Prompt Prefix** | Main prompt content | The actual instructions for the AI |

3. Click **Create Prompt** to save

### Name Validation

The prompt name validates in real-time:
- ✔️ Green checkmark: Name is available
- ❌ Red X: Name is invalid or already taken

## Editing Prompts

1. Locate the prompt in the list
2. Click the **Settings** icon (⚙️) in the Edit column
3. Modify the configuration
4. Click **Save Changes**

> **Note:** The prompt name cannot be changed after creation. To rename, create a new prompt and update references.

## Writing Effective Prompts

### Best Practices

1. **Be specific**: Clearly define the expected behavior
2. **Include context**: Specify the persona, tone, and constraints
3. **Use examples**: Include sample inputs and outputs when helpful
4. **Structure clearly**: Use formatting (headers, lists) for complex prompts

### Example Workflow Prompt

```
You are a helpful assistant that provides accurate information about company policies.

Guidelines:
- Answer questions concisely and professionally
- If information is not available, clearly state so
- Cite relevant policy documents when applicable
- Do not make assumptions beyond the provided data

Response Format:
- Start with a direct answer
- Provide supporting details
- Include relevant references
```

### Example Tool Prompt

```
You are a code interpreter tool. Generate Python code to answer the user's question.

Requirements:
- Write clean, well-commented code
- Handle edge cases appropriately
- Return results in a clear format
- Do not execute dangerous operations
```

## Using Prompts in Agents

Prompts are referenced in agent configurations:

### Workflow Main Prompt
Defined directly in the agent's workflow configuration as the primary system prompt.

### Additional Workflow Resources
Added via the "Add Workflow Resource" dialog:
- **Resource Type**: Prompt
- **Resource**: Select the prompt
- **Resource Role**: Purpose identifier (e.g., `router_prompt`, `final_prompt`)

### Tool Prompts
Added via the "Add Tool Resource" dialog when configuring agent tools:
- **main_prompt**: Primary tool instructions
- **router_prompt**: Instructions for tool selection

## Common Prompt Roles

| Role | Purpose |
|------|---------|
| `main_prompt` | Primary instructions for the workflow or tool |
| `router_prompt` | Instructions for selecting tools or routing |
| `files_prompt` | Instructions for file identification |
| `final_prompt` | Instructions for generating final responses |

## Access Control

Prompt permissions can be managed for each prompt:

1. Open the prompt for editing
2. Click **Access Control** at the top right
3. Add or remove role assignments

| Permission | Description |
|------------|-------------|
| `FoundationaLLM.Prompt/prompts/read` | View the prompt |
| `FoundationaLLM.Prompt/prompts/write` | Edit the prompt |

## Related Topics

- [Prompts & Resources Reference](../../reference/concepts/prompts-resources.md)
- [Agents & Workflows](../../reference/concepts/agents-workflows.md)
- [Create New Agent](create-new-agent.md)
