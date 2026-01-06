# Create a New Agent

This comprehensive guide walks you through creating a new agent using the Management Portal.

## Prerequisites

Before creating an agent, ensure:

- You have access to the Management Portal with appropriate permissions
- At least one AI model is configured under **Models and Endpoints > AI Models**
- (Optional) Prompts are created for workflow and tool configurations
- (Optional) Data sources are configured if your agent needs knowledge retrieval

## Accessing the Agent Creation Page

1. In the Management Portal sidebar, click **Create New Agent**
2. The agent creation form loads with all configuration sections

## Agent Configuration Sections

### 1. General Information

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Agent Name** | Unique identifier used internally | Letters, numbers, dashes, underscores only. No spaces or special characters. Validated in real-time. |
| **Agent Display Name** | User-friendly name shown in portals | Any text. This is what users see. |
| **Description** | Purpose of the agent | Recommended for discoverability |
| **Welcome Message** | Initial greeting shown to users | Supports rich text formatting via editor |

### 2. Agent Configuration

#### Conversation History

Controls whether the agent remembers previous messages in a conversation.

| Setting | Options | Description |
|---------|---------|-------------|
| **Enabled** | Yes/No | Toggle conversation memory |
| **Max Messages** | Number | How many previous messages to include (default: 5) |

#### Gatekeeper

The gatekeeper provides content moderation and data protection.

| Setting | Description |
|---------|-------------|
| **Use system default** | Apply instance-level gatekeeper settings |
| **Content Safety** | Select content moderation platforms |
| **Data Protection** | Select PII detection services |

**Content Safety Options:**
- Azure Content Safety
- Azure Content Safety Prompt Shield
- Lakera Guard
- Enkrypt Guardrails

**Data Protection Options:**
- Microsoft Presidio

#### User Prompt Rewrite

Optionally rewrite user prompts before processing.

| Setting | Description |
|---------|-------------|
| **Enabled** | Toggle prompt rewriting |
| **Rewrite Model** | AI model for rewriting |
| **Rewrite Prompt** | Prompt template for rewriting |
| **Rewrite Window Size** | Messages to consider (default: 3) |

#### Semantic Cache

Cache responses for semantically similar questions.

| Setting | Description |
|---------|-------------|
| **Enabled** | Toggle caching |
| **Model** | Embedding model for similarity |
| **Embedding Dimensions** | Vector size (default: 2048) |
| **Minimum Similarity Threshold** | Match threshold (default: 0.97) |

#### Cost Center and Expiration

| Field | Description |
|-------|-------------|
| **Cost Center** | Department for cost tracking (optional) |
| **Expiration Date** | Auto-disable date (optional) |

### 3. User Portal Experience

Control features available to users in the Chat User Portal.

| Setting | Default | Description |
|---------|---------|-------------|
| **Show Message Tokens** | Yes | Display token consumption |
| **Allow Rating** | Yes | Enable thumbs up/down ratings |
| **Show View Prompt** | Yes | Allow viewing completion prompts |
| **Allow File Upload** | No | Enable file attachments |

### 4. Workflow

The workflow defines how the agent processes requests and generates responses.

#### Selecting a Workflow Type

| Type | Description | Best For |
|------|-------------|----------|
| **OpenAIAssistants** | Azure OpenAI Assistants API | Code Interpreter, File Search, Function Calling |
| **LangGraphReactAgent** | LangGraph with dynamic tool selection | Flexible multi-tool agents |
| **ExternalAgentWorkflow** | Custom Python workflows | Advanced custom logic |

#### Workflow Configuration

Click **Configure Workflow** to expand settings:

| Field | Description |
|-------|-------------|
| **Workflow Name** | Identifier for this workflow |
| **Workflow Package Name** | Python package (for custom workflows) |
| **Workflow Class Name** | Python class (for custom workflows) |
| **Workflow Host** | Orchestration framework (e.g., LangChain) |
| **Workflow Main Model** | Primary AI model |
| **Workflow Main Model Parameters** | Model settings (temperature, etc.) |
| **Main Workflow Prompt** | System prompt defining behavior |

#### Adding Workflow Resources

Additional prompts and resources for the workflow:

1. Click **Add Workflow Resource**
2. Select **Resource Type**: Model, Prompt, or other
3. Select the specific **Resource**
4. Enter the **Resource Role** (e.g., `router_prompt`, `final_prompt`)
5. Click **Save**

### 5. Tools

Tools extend the agent's capabilities beyond text generation.

#### Adding a Tool

1. In the Tools section, click **Add New Tool**
2. Configure the tool:

| Field | Description |
|-------|-------------|
| **Tool Name** | Unique identifier |
| **Tool Description** | What the tool does (helps AI decide when to use it) |
| **Tool Package Name** | Python package containing the tool |
| **Tool Class Name** | Python class implementing the tool |

3. Add **Tool Resources** (models, prompts, data pipelines)
4. Add **Tool Properties** (configuration values)
5. Click **Save**

#### Common Tools

| Tool | Class Name | Purpose |
|------|------------|---------|
| **DALL-E Image Generation** | `DALLEImageGeneration` | Generate images |
| **Code Interpreter** | `FoundationaLLMCodeInterpreterTool` | Execute Python code |
| **Knowledge Search** | `FoundationaLLMKnowledgeTool` | Search knowledge sources |

### 6. Security (After Creation)

After creating an agent, you can configure security settings:

#### Virtual Security Group ID
A unique identifier for programmatic access to the agent.

#### Agent Access Tokens
Create tokens for API access without Entra ID authentication:

1. Access the agent edit page
2. Scroll to the Security section
3. Create and manage access tokens

See [Agent Access Tokens](../../reference/concepts/agent-access-tokens.md) for details.

## Creating the Agent

1. Review all configuration sections
2. Click **Create Agent** at the bottom of the page
3. Wait for the creation process to complete
4. Upon success, you'll be redirected or see a confirmation

## Editing Existing Agents

1. Navigate to **All Agents** or **My Agents**
2. Click the **Edit** icon for the agent
3. Modify settings as needed
4. Click **Save Changes**

> **Note:** The agent name cannot be changed after creation.

## Access Control

Configure who can access and manage the agent:

1. Open the agent for editing
2. Click **Access Control** at the top right
3. Add role assignments for:
   - **Agent scope**: Access to this specific agent
   - **Prompt scope**: Access to the agent's prompt

## Form Validation

The form validates required fields before allowing creation:

- Agent name must be unique and properly formatted
- Required workflow settings must be configured
- Model selections must be made where required

Validation errors appear as red text below the relevant field.

## Related Topics

- [Quick Start: Creating Your First Agent](../../quick-start/creating-first-agent.md)
- [Create Model Agnostic Agent with Claude](create-model-agnostic-agent-claude.md)
- [Create Model Agnostic Agent with GPT-4o](create-model-agnostic-agent-gpt4o.md)
- [Agents & Workflows Reference](../../reference/concepts/agents-workflows.md)
