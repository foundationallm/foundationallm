# Agent Creation Walkthrough

This step-by-step walkthrough guides you through creating a self-service agent in the User Portal.

## Prerequisites

- Access to the FoundationaLLM User Portal
- Self-service agent creation enabled (`FoundationaLLM.Agent.SelfService` feature flag)
- Permissions to create agents

## Steps

### Step 1: Access My Agents

1. Log in to the User Portal
2. Click on your **profile icon** or navigate to **Settings**
3. Select **My Agents** from the menu

<!-- [TODO: Add screenshot of My Agents navigation] -->

### Step 2: Start Agent Creation

1. Click the **Create New Agent** button
2. The agent creation wizard opens

<!-- [TODO: Add screenshot of Create New Agent button] -->

### Step 3: Enter Basic Information

Fill in the basic agent details:

| Field | Action |
|-------|--------|
| **Agent Name** | Enter a unique, descriptive name |
| **Description** | Describe what your agent does |
| **Welcome Message** | Write a greeting for users (HTML supported) |

<!-- [TODO: Add screenshot of basic information form] -->

**Example Welcome Message:**
```html
<p>Hi! I'm your custom assistant.</p>
<ul>
  <li>Ask me questions about our documentation</li>
  <li>Upload files for analysis</li>
  <li>Request summaries and reports</li>
</ul>
```

### Step 4: Configure the Prompt

Define your agent's behavior:

1. Click on the **Prompt** section
2. Enter your system prompt in the text area
3. Use the preview feature to test your prompt

<!-- [TODO: Add screenshot of prompt configuration] -->

**Example Prompt:**
```
You are a helpful assistant specializing in technical documentation. 
You should:
- Provide clear, concise answers
- Include code examples when relevant
- Cite sources from uploaded documents
- Politely decline requests outside your expertise

Do not:
- Make up information
- Provide medical or legal advice
- Share confidential information
```

### Step 5: Select a Model

Choose the AI model for your agent:

1. Click on the **Model** section
2. Review available models
3. Select the model that best fits your use case
4. Adjust the **Temperature** slider if needed

<!-- [TODO: Add screenshot of model selection] -->

| Temperature | Use Case |
|-------------|----------|
| Low (0.0-0.3) | Factual responses, Q&A |
| Medium (0.4-0.7) | Balanced general use |
| High (0.8-1.0) | Creative writing, brainstorming |

### Step 6: Configure Agent Status

Set the operational status:

1. Click on the **Status** section
2. Choose **Active** to make the agent available
3. Optionally set an **Expiration Date**

<!-- [TODO: Add screenshot of status configuration] -->

### Step 7: Enable Tools

Select tools for your agent:

1. Click on the **Tools** section
2. Toggle on desired capabilities:

| Tool | Description |
|------|-------------|
| **Image Generation** | Enable DALL-E image creation |
| **File Upload** | Allow users to upload files |
| **Private Storage** | Connect to private knowledge source |

<!-- [TODO: Add screenshot of tools configuration] -->

### Step 8: Review Configuration

Before creating your agent:

1. Click **Review** or scroll through all sections
2. Verify all settings are correct
3. Check for any validation errors (shown in red)

<!-- [TODO: Add screenshot of review screen] -->

### Step 9: Create the Agent

1. Click the **Create Agent** button
2. Wait for the creation process to complete
3. Note any success or error messages

<!-- [TODO: Add screenshot of creation success message] -->

### Step 10: Test Your Agent

1. The new agent appears in your **My Agents** list
2. Click on the agent to select it
3. Start a chat conversation to test
4. Verify responses match your expected behavior

<!-- [TODO: Add screenshot of testing the new agent] -->

## Result

After completing this walkthrough, you have:

- ✅ Created a new self-service agent
- ✅ Configured the agent's prompt and behavior
- ✅ Selected an appropriate model
- ✅ Enabled desired tools
- ✅ Tested the agent

## Troubleshooting

### Agent Creation Button Missing

- Verify self-service is enabled for your instance
- Check your permissions with your administrator

### Validation Errors

- Ensure all required fields are completed
- Agent name must be unique
- Prompt cannot be empty

### Agent Not Appearing

- Wait a few minutes for propagation
- Refresh your browser
- Check the My Agents list

## Next Steps

- [Agent Sharing Model](../agent-sharing-model.md) - Share your agent with others
- [Managing Your Agent Catalog](../agent-management.md) - Organize your agents
- [Prompt Resources](../../agents/prompt-resource.md) - Advanced prompt configuration

## Video Tutorial

<!-- [TODO: Add link to video walkthrough when available] -->
