# Agents and Workflows

Agents are the core of FoundationaLLM, providing users with customized AI-powered conversational experiences based on their configuration. This reference documents the agent resource structure and workflow configuration options.

## Agent Resource Structure

An agent consists of several configuration sections:

| Section | Purpose |
|---------|---------|
| **General** | Basic information (name, description, welcome message) |
| **Agent Configuration** | Behavior settings (history, gatekeeper, cost center, expiration, portal displays) |
| **User Portal Experience** | Portal feature visibility settings |
| **Workflow** | Orchestration and model configuration |
| **Tools** | Tool capabilities (Code Interpreter, Knowledge Search, DALL-E) |
| **Security** | Access tokens and virtual security groups |

## Agent JSON Structure

```json
{
  "type": "agent",
  "name": "my-agent",
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/my-agent",
  "display_name": "My Agent",
  "description": "Agent description",
  "inline_context": false,
  "conversation_history_settings": {
    "enabled": true,
    "max_history": 5
  },
  "gatekeeper_settings": {
    "use_system_setting": false,
    "options": ["ContentSafety", "Presidio"]
  },
  "orchestration_settings": {
    "agent_parameters": {}
  },
  "workflow_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/workflows/my-workflow",
  "tool_object_ids": [],
  "cost_center": "",
  "expiration_date": null
}
```

---

## Workflow Types

Workflows define how agents process requests and generate responses.

### OpenAIAssistants

Uses Azure OpenAI Assistants API for orchestration.

| Feature | Description |
|---------|-------------|
| Code Interpreter | Execute Python code for data analysis |
| File Search | Search through uploaded documents |
| Function Calling | Call external functions/tools |

**Best For:** Complex multi-step tasks, code execution, file analysis

### LangGraphReactAgent

Uses LangGraph for dynamic tool selection with ReAct pattern.

| Feature | Description |
|---------|-------------|
| Dynamic Tool Selection | Agent chooses tools based on context |
| ReAct Pattern | Reasoning + Acting loop |
| Tool Orchestration | Multiple tools in sequence |

**Best For:** Dynamic multi-tool scenarios, reasoning chains

### ExternalAgentWorkflow

Enables custom Python-based workflow implementations.

| Feature | Description |
|---------|-------------|
| Custom Logic | Implement any workflow pattern |
| Plugin-Based | Registered as workflow plugins |
| Full Control | Complete control over orchestration |

**Best For:** Custom business logic, specialized workflows

---

## Workflow Configuration

### Workflow Resource Structure

```json
{
  "type": "workflow",
  "name": "my-workflow",
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/workflows/my-workflow",
  "workflow_type": "OpenAIAssistants",
  "workflow_host": "LangChain",
  "package_name": "FoundationaLLM.Workflow",
  "main_ai_model_object_id": "/instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/gpt-4o",
  "main_prompt_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/system-prompt",
  "ai_model_object_ids": {},
  "prompt_object_ids": {},
  "resource_object_ids": {}
}
```

### Key Workflow Settings

| Setting | Description |
|---------|-------------|
| `workflow_type` | One of: `OpenAIAssistants`, `LangGraphReactAgent`, `ExternalAgentWorkflow` |
| `workflow_host` | Currently `LangChain` for all workflows |
| `package_name` | Plugin package name for external workflows |
| `main_ai_model_object_id` | Primary model for generation |
| `main_prompt_object_id` | System prompt defining agent persona |
| `ai_model_object_ids` | Additional models by role |
| `prompt_object_ids` | Additional prompts by role |
| `resource_object_ids` | External resources (e.g., Azure AI Project) |

---

## Agent Configuration Options

### Conversation History

Controls context retention across conversation turns.

| Setting | Type | Description |
|---------|------|-------------|
| `enabled` | boolean | Enable conversation history |
| `max_history` | integer | Number of message pairs to retain (default: 5) |

### Gatekeeper

Controls content moderation and safety features.

| Option | Description |
|--------|-------------|
| `ContentSafety` | Azure Content Safety integration |
| `ContentSafetyPromptShield` | Azure Content Safety with prompt injection detection |
| `LakeraGuard` | Lakera Guard integration |
| `EnkryptGuardrails` | Enkrypt Guardrails integration |
| `Presidio` | Microsoft Presidio for PII detection |

### User Prompt Rewrite

Enables query transformation for improved retrieval.

| Setting | Type | Description |
|---------|------|-------------|
| `enabled` | boolean | Enable prompt rewriting |
| `ai_model_object_id` | string | Model for rewriting |
| `prompt_object_id` | string | Prompt for rewriting instructions |

### Semantic Cache

Caches responses for similar queries.

| Setting | Type | Description |
|---------|------|-------------|
| `enabled` | boolean | Enable semantic caching |
| `score_threshold` | float | Similarity threshold (0-1) |

### Cost Center & Expiration

| Setting | Type | Description |
|---------|------|-------------|
| `cost_center` | string | Cost allocation identifier |
| `expiration_date` | datetime | Agent expiration date (null = no expiration) |

---

## User Portal Experience Settings

Control what features are visible in the Chat User Portal.

| Setting | Description |
|---------|-------------|
| Show Token Usage | Display token counts for requests/responses |
| Show Prompt | Allow users to view the compiled prompt |
| Enable Rating | Allow users to rate responses |
| Enable File Upload | Allow users to upload files |

---

## Tool Configuration

### Tool Resource Structure

```json
{
  "type": "tool",
  "name": "code-interpreter",
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/tools/code-interpreter",
  "tool_type": "code-interpreter",
  "description": "Execute Python code",
  "ai_model_object_ids": {},
  "prompt_object_ids": {},
  "resource_object_ids": {},
  "properties": {}
}
```

### Built-in Tool Types

| Tool Type | Description | Configuration |
|-----------|-------------|---------------|
| `code-interpreter` | Python code execution | Conversation file scope |
| `knowledge-search` | Vector search through knowledge sources | Indexing profile, data sources |
| `dalle-image-generation` | Generate images with DALL-E | AI model with `main_model` role |

### Tool Resource Mapping

Tools can reference models, prompts, and resources by role:

```json
{
  "ai_model_object_ids": {
    "main_model": "/instances/.../aiModels/dalle-3"
  },
  "prompt_object_ids": {
    "tool_prompt": "/instances/.../prompts/tool-instructions"
  },
  "resource_object_ids": {
    "indexing_profile": "/instances/.../indexingProfiles/default"
  }
}
```

---

## Security Configuration

### Virtual Security Group ID

Identifies the agent for external access scenarios.

```json
{
  "virtual_security_group_id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
}
```

### Agent Access Tokens

Enable unauthenticated access to specific agents. See [Agent Access Tokens](agent-access-tokens.md) for details.

---

## Related Topics

- [Create New Agent](../../how-to-guides/agents/create-new-agent.md)
- [Agent Access Tokens](agent-access-tokens.md)
- [Prompts Resources](prompts-resources.md)
- [Resource Management](resource-management.md)
