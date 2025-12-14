# Create a New Agent

This guide walks you through creating a new agent using the Management Portal.

## Prerequisites

- Access to the FoundationaLLM Management Portal
- Appropriate permissions to create agents

## Steps to Create a New Agent

### 1. Navigate to Create New Agent

Navigate to the **Create New Agent** page using the side navigation bar.
    
![FLLM Create New Agent tab.](../../../setup-guides/media/fllm-management-interface.png "Create New Agent")

### 2. Select Agent Type

Set the agent type: **Knowledge Management** or **Analytics**. FoundationaLLM currently only supports Knowledge Management agents.

![Create New Agent select Agent Type.](../../../setup-guides/media/agent-type-selection.png "Agent Type")

### 3. Configure Knowledge Source

Set the agent Knowledge Source:

![Agent Knowledge Source four-tile view.](../../../setup-guides/media/agent-knowledge-source.png "Agent Knowledge Source")

- Expand the dropdown arrow next to the upper left box. Select the correct Content Source Profile.

    ![Agent Blob Storage Data Sources.](../../../setup-guides/media/agent-data-source-dropdown.png "Blob Storage Data Sources")

- Expand the dropdown arrow next to the upper right box to open the Indexing Profile dropdown. Select the correct Indexing Profile.

    ![Agent Knowledge Source Index Selection.](../../../setup-guides/media/aisearch-index-dropdown.png "Index Selection")
 
- Expand the dropdown arrow next to the lower left box. Set the **Chunk size** and **Overlap size** settings for text partitioning. Select **Done**.

    ![Agent Splitting & Chunking Configuration.](../../../setup-guides/media/set-splitting-and-chunking.png "Splitting & Chunking")

- Expand the dropdown arrow next to the lower right box. Set the trigger **Frequency**; FoundationaLLM currently only supports Manual triggers.

    ![Agent Vectorization Trigger Frequency.](../../../setup-guides/media/vectorization-trigger.png "Vectorization Trigger Frequency")

### 4. Configure User-Agent Interactions

Configure user-agent interactions.

![User-Agent Interactions & Gatekeeper Configuration.](../../../setup-guides/media/user-agent-interactions-config.png "User-Agent Interactions")

- Enable conversation history using the `Yes/No` Radio Button. Select **Done**.

    ![Agent Enable/Disable Conversation History.](../../../setup-guides/media/enable-disable-conversation-history.png "Enable/Disable Conversation History.")

- Configure the Gatekeeper. Then, select **Done**.
    - `Enable/Disable` the Gatekeeper using the Radio Button
    - Set the **Content Safety** platform to either `None` or `Azure Content Safety` using the dropdown menu
    - Set the **Data Protection** platform to either `None` or `Microsoft Presidio` using the dropdown menu

    ![Agent Gatekeeper Status, Content Safety Configuration, and Data Protection Configuration.](../../../setup-guides/media/gatekeeper-config.png "Gatekeeper Configuration")

### 5. Set the System Prompt

Set the **System Prompt**. The prompt prefixes users' requests to the agent, influencing the tone and functionality of the agent.

![Set Agent Prompt.](../../../setup-guides/media/set-system-prompt.png "Agent Prompt")

### 6. Create the Agent

After setting the desired agent configuration, select **Create Agent** at the bottom right-hand corner of the page. You will be able to edit the agent configuration after creation from the **Public Agents** page.

![Edit a newly-created agent in the Management UI.](../../../setup-guides/media/edit-agent-page.png "Edit agent")

## Next Steps

- [Create Model Agnostic Agent with Claude](create-model-agnostic-agent-claude.md)
- [Create Model Agnostic Agent with GPT-4o](create-model-agnostic-agent-gpt4o.md)
- [Agents & Workflows Reference](../../reference/concepts/agents-workflows.md)
