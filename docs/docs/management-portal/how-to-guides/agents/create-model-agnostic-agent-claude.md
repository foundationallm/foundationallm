# Create a Model Agnostic Agent with Claude

This step-by-step guide walks you through creating a model agnostic agent using Claude, with a code interpreter tool and a knowledge search tool for uploaded files.

## Overview

A model agnostic agent uses the `ExternalAgentWorkflow` pattern with the `FoundationaLLMFunctionCallingWorkflow` class. This architecture provides:

- **Code Interpreter**: Execute Python code in isolated containers
- **Knowledge Search**: Search and retrieve content from uploaded files
- **Flexible Routing**: Dynamic tool selection based on user queries

## Prerequisites

- A Claude model deployed and configured in your FoundationaLLM instance
- Azure Container Apps configured for code execution (for Code Interpreter)
- Access to the Management Portal with agent creation permissions

## Part 1: Create the Agent

1. Navigate to the **Management Portal**
2. Click **Create New Agent** in the sidebar

### Basic Configuration

| Field | Value |
|-------|-------|
| **Agent Name** | Your unique agent identifier (e.g., `claude-maa-agent`) |
| **Agent Display Name** | User-friendly name (e.g., `Claude Analysis Agent`) |
| **Description** | Purpose of the agent |

### User Portal Experience

Set **"Would you like to allow the user to upload files?"** to **Yes**

### Workflow Configuration

1. Select **ExternalAgentWorkflow** from the workflow dropdown
2. Click **Configure Workflow**
3. Enter the following values:

| Field | Value |
|-------|-------|
| **Workflow Name** | `MAA-Workflow` |
| **Workflow Package Name** | `foundationallm_agent_plugins` |
| **Workflow Class Name** | `FoundationaLLMFunctionCallingWorkflow` |
| **Workflow Host** | `LangChain` |
| **Workflow Main Model** | Select your Claude model |

4. Add a model parameter:
   - Click **Add Property**
   - **Property Key**: `temperature`
   - **Property Type**: `number`
   - **Property Value**: `0.5`
   - Click **Save**

5. For **Main Workflow Prompt**, use the prompt from:
   
   > **TODO**: Obtain the workflow main prompt content from the FoundationaLLM packages repository at: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Workflow-Main.txt

## Part 2: Create the Workflow Prompts

Open a new browser tab and navigate to **Prompts** in the Management Portal.

> **Note**: Replace `Your-Agent-Name` with your actual agent name in all prompt names below.

### Create Workflow-Files Prompt

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Workflow-Files` |
| **Description** | Provides instructions for identifying files relevant to the question |
| **Category** | Workflow |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Workflow-Files.txt

### Create Workflow-Final Prompt

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Workflow-Final` |
| **Description** | Instructions to build final response from tool results |
| **Category** | Workflow |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Workflow-Final.txt

### Create Workflow-Router Prompt

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Workflow-Router` |
| **Description** | Instructions for tool selection |
| **Category** | Workflow |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Workflow-Router.txt

## Part 3: Create the Tool Prompts

### Code Interpreter Prompts

**Tool-Code-Main:**

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Tool-Code-Main` |
| **Description** | Main instructions for the code interpreter tool |
| **Category** | Tool |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Tool-Code-Main.txt

**Tool-Code-Router:**

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Tool-Code-Router` |
| **Description** | Instructions for selecting the code interpreter tool |
| **Category** | Tool |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Tool-Code-Router.txt

### Knowledge Tool Prompts

**Tool-Knowledge-Main:**

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Tool-Knowledge-Main` |
| **Description** | Main instructions for the knowledge tool |
| **Category** | Tool |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Tool-Knowledge-Main.txt

**Tool-Knowledge-Router:**

| Field | Value |
|-------|-------|
| **Prompt Name** | `Your-Agent-Name-Tool-Knowledge-Router` |
| **Description** | Instructions for selecting the knowledge tool |
| **Category** | Tool |
| **Prompt Prefix** | See note below |

> **TODO**: Obtain prompt content from: https://github.com/foundationallm/foundationallm-packages/blob/main/ModelAgnosticAgent/artifacts/Agent-Tool-Knowledge-Router.txt

## Part 4: Configure Workflow Resources

Return to your agent configuration and add workflow resources:

### Add Router Prompt Resource

1. Click **Add Workflow Resource**
2. Configure:
   - **Resource Type**: Prompt
   - **Resource**: Select `Your-Agent-Name-Workflow-Router`
   - **Resource Role**: `router_prompt`
3. Click **Save**

### Add Files Prompt Resource

1. Click **Add Workflow Resource**
2. Configure:
   - **Resource Type**: Prompt
   - **Resource**: Select `Your-Agent-Name-Workflow-Files`
   - **Resource Role**: `files_prompt`
3. Click **Save**

### Add Final Prompt Resource

1. Click **Add Workflow Resource**
2. Configure:
   - **Resource Type**: Prompt
   - **Resource**: Select `Your-Agent-Name-Workflow-Final`
   - **Resource Role**: `final_prompt`
3. Click **Save**

## Part 5: Configure the Code Interpreter Tool

1. In the **Tools** section, click **Add New Tool**

### Basic Configuration

| Field | Value |
|-------|-------|
| **Tool Name** | `Code-01` (must match prompts exactly) |
| **Tool Description** | Answers questions that require dynamic generation of code |
| **Tool Package Name** | `foundationallm_agent_plugins` |
| **Tool Class Name** | `FoundationaLLMCodeInterpreterTool` |

### Add Tool Resources

**Main Model:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Model
3. Select your Claude model
4. **Resource Role**: `main_model`
5. Click **Save**
6. Expand the model resource and click **Add Property**:
   - **Property Key**: `model_parameters`
   - **Property Type**: Object / Array
   - **Property Value**: `{"temperature": 0.2}`
   - Click **Save**

**Main Prompt:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Prompt
3. Select `Your-Agent-Name-Tool-Code-Main`
4. **Resource Role**: `main_prompt`
5. Click **Save**

**Router Prompt:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Prompt
3. Select `Your-Agent-Name-Tool-Code-Router`
4. **Resource Role**: `router_prompt`
5. Click **Save**

### Add Tool Properties

Add these properties one at a time:

| Property Key | Property Type | Property Value |
|--------------|---------------|----------------|
| `code_session_required` | Boolean | `True` |
| `code_session_endpoint_provider` | String | `AzureContainerAppsCustomContainer` |
| `code_session_language` | String | `Python` |

Click **Save** in the Configure Tool dialog.

## Part 6: Configure the Knowledge Tool

1. Click **Add New Tool**

### Basic Configuration

| Field | Value |
|-------|-------|
| **Tool Name** | `Knowledge-Conversation-Files` (must match prompts exactly) |
| **Tool Description** | Retrieves content from files uploaded to conversations |
| **Tool Package Name** | `foundationallm_agent_plugins` |
| **Tool Class Name** | `FoundationaLLMKnowledgeTool` |

### Add Tool Resources

**Main Model:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Model
3. Select your Claude model
4. **Resource Role**: `main_model`
5. Click **Save**

**Main Prompt:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Prompt
3. Select `Your-Agent-Name-Tool-Knowledge-Main`
4. **Resource Role**: `main_prompt`
5. Click **Save**

**Router Prompt:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Prompt
3. Select `Your-Agent-Name-Tool-Knowledge-Router`
4. **Resource Role**: `router_prompt`
5. Click **Save**

**Data Pipeline:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Data Pipeline
3. Select `DefaultFileUpload`
4. **Resource Role**: `file_upload_data_pipeline`
5. Click **Save**

**Vector Database:**
1. Click **Add Tool Resource**
2. Select **Resource Type**: Vector Database
3. Select `ConversationFiles`
4. **Resource Role**: `vector_database`
5. Click **Save**

### Add Tool Properties

| Property Key | Property Type | Property Value |
|--------------|---------------|----------------|
| `embedding_model` | String | `text-embedding-3-large` |
| `embedding_dimensions` | Number | `2048` |

Click **Save** in the Configure Tool dialog.

## Part 7: Create the Agent

1. Review all configurations
2. Click **Create Agent**
3. Wait for confirmation

## Testing Your Agent

1. Open the **Chat User Portal**
2. Select your new agent
3. Test the capabilities:

### Test File Upload and Knowledge Search
1. Upload a document (PDF, Word, etc.)
2. Ask questions about the document content

### Test Code Interpreter
1. Ask for data analysis or calculations
2. Request charts or visualizations
3. Ask for code generation

## Troubleshooting

### Tool Not Being Selected
- Verify tool names match exactly: `Code-01` and `Knowledge-Conversation-Files`
- Check that router prompts are correctly assigned

### File Upload Not Working
- Ensure "Allow File Upload" is enabled in User Portal Experience
- Verify the `DefaultFileUpload` data pipeline exists

### Code Execution Failing
- Check Azure Container Apps custom container configuration
- Verify `code_session_endpoint_provider` is set correctly

## Related Topics

- [Create Model Agnostic Agent with GPT-4o](create-model-agnostic-agent-gpt4o.md)
- [Managing Prompts](prompts.md)
- [Agents & Workflows Reference](../../reference/concepts/agents-workflows.md)
