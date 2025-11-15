# MCP Tool for FoundationaLLM Agent Management - Implementation Plan

## Overview

This document outlines the plan to create a Model Context Protocol (MCP) tool that enables users to create and configure agents in FoundationaLLM through an MCP server interface. The tool will leverage the existing FoundationaLLM Management API to provide agent lifecycle management capabilities.

## Background

FoundationaLLM provides a comprehensive platform for deploying, scaling, securing, and governing generative AI agents in the enterprise. The Management API exposes endpoints for:
- Creating agents from templates
- Retrieving and updating agent configurations
- Managing agent workflows, prompts, and AI models
- Configuring agent capabilities, tools, and vectorization settings

The MCP (Model Context Protocol) is a protocol that enables AI assistants to interact with external tools and services. By creating an MCP server for FoundationaLLM agent management, we enable AI assistants to programmatically create and configure agents.

## Architecture

### Components

1. **MCP Server** (`src/python/MCPAgentManagement/`)
   - Python-based MCP server using the `mcp` package (already in requirements.txt)
   - Exposes tools for agent management operations
   - Handles authentication and API communication

2. **Management API Client**
   - Wraps FoundationaLLM Management API calls
   - Handles authentication (Azure CLI credentials or API keys)
   - Provides typed interfaces for agent operations

3. **Agent Models**
   - Leverages existing Python SDK models (`foundationallm.models.agents`)
   - Supports all agent types (GenericAgent, KnowledgeManagementAgent, etc.)

### Technology Stack

- **Language**: Python 3.x
- **MCP SDK**: `mcp==1.17.0` (already in requirements.txt)
- **HTTP Client**: `requests` or `aiohttp` (for async operations)
- **Authentication**: Azure Identity (`azure-identity`) or API key authentication
- **Models**: Existing `foundationallm.models` from PythonSDK

## MCP Tools to Implement

### 1. `create_agent`
Creates a new agent from the BasicAgentTemplate.

**Parameters:**
- `agent_name` (string, required): Unique name for the agent (slug format)
- `display_name` (string, required): Human-readable display name
- `description` (string, optional): Agent description
- `welcome_message` (string, optional): Welcome message shown to users
- `expiration_date` (string, optional): ISO 8601 formatted expiration date

**Returns:**
- Created agent object with full configuration

**API Endpoint:**
```
POST /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agentTemplates/BasicAgentTemplate/create-new
```

**Request Body:**
```json
{
  "template_parameters": {
    "AGENT_NAME": "agent-name",
    "AGENT_DISPLAY_NAME": "Display Name",
    "AGENT_DESCRIPTION": "Description",
    "AGENT_WELCOME_MESSAGE": "Welcome!",
    "AGENT_EXPIRATION_DATE": "2025-12-31T00:00:00+00:00"
  }
}
```

### 2. `get_agent`
Retrieves an agent by name.

**Parameters:**
- `agent_name` (string, required): Name of the agent to retrieve

**Returns:**
- Agent object with full configuration

**API Endpoint:**
```
GET /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

### 3. `list_agents`
Lists all available agents.

**Parameters:**
- None

**Returns:**
- Array of agent objects

**API Endpoint:**
```
GET /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents
```

### 4. `update_agent_prompt`
Updates the main system prompt for an agent.

**Parameters:**
- `agent_name` (string, required): Name of the agent
- `prompt` (string, required): New system prompt text

**Returns:**
- Updated prompt object

**API Endpoint:**
```
POST /management/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/{promptId}
```

### 5. `update_agent_model`
Updates the AI model used by an agent.

**Parameters:**
- `agent_name` (string, required): Name of the agent
- `model_object_id` (string, required): Object ID of the new AI model

**Returns:**
- Updated agent object

**API Endpoint:**
```
POST /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

### 6. `configure_agent_vectorization`
Configures vectorization settings for a Knowledge Management agent.

**Parameters:**
- `agent_name` (string, required): Name of the agent
- `indexing_profile_name` (string, required): Name of the indexing profile
- `text_embedding_profile_name` (string, optional): Name of the text embedding profile
- `text_partitioning_profile_name` (string, optional): Name of the text partitioning profile

**Returns:**
- Updated agent object

**API Endpoint:**
```
POST /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

### 7. `add_agent_tool`
Adds a tool/capability to an agent.

**Parameters:**
- `agent_name` (string, required): Name of the agent
- `tool_name` (string, required): Name of the tool to add
- `tool_configuration` (object, optional): Tool-specific configuration

**Returns:**
- Updated agent object

**API Endpoint:**
```
POST /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

### 8. `delete_agent`
Deletes an agent.

**Parameters:**
- `agent_name` (string, required): Name of the agent to delete

**Returns:**
- Success confirmation

**API Endpoint:**
```
DELETE /management/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}
```

### 9. `get_agent_workflows`
Retrieves available agent workflow types.

**Parameters:**
- None

**Returns:**
- Array of available workflow types

**API Endpoint:**
```
GET /management/instances/{instanceId}/providers/FoundationaLLM.Agent/workflows
```

### 10. `get_ai_models`
Retrieves available AI models for agent configuration.

**Parameters:**
- None

**Returns:**
- Array of available AI models

**API Endpoint:**
```
GET /management/instances/{instanceId}/providers/FoundationaLLM.AzureAI/aiModels
```

## Implementation Steps

### Phase 1: Project Setup

1. **Create MCP Server Directory Structure**
   ```
   src/python/MCPAgentManagement/
   ├── __init__.py
   ├── main.py                 # MCP server entry point
   ├── server.py              # MCP server implementation
   ├── tools/
   │   ├── __init__.py
   │   ├── agent_tools.py     # Agent management tools
   │   └── resource_tools.py # Resource listing tools
   ├── clients/
   │   ├── __init__.py
   │   └── management_client.py # Management API client
   ├── config/
   │   ├── __init__.py
   │   └── settings.py         # Configuration management
   └── requirements.txt
   ```

2. **Dependencies**
   - Add to `requirements.txt`:
     - `mcp==1.17.0` (already in LangChainAPI requirements)
     - `requests` or `aiohttp`
     - `azure-identity` (for Azure authentication)
     - `pydantic` (for validation)
     - Reference to `foundationallm` PythonSDK models

3. **Configuration**
   - Environment variables:
     - `FOUNDATIONALLM_MANAGEMENT_API_ENDPOINT`: Management API endpoint URL
     - `FOUNDATIONALLM_INSTANCE_ID`: FoundationaLLM instance ID
     - `FOUNDATIONALLM_API_SCOPE`: Azure AD scope for API access
     - `FOUNDATIONALLM_API_KEY` (optional): API key if using key-based auth
     - `FOUNDATIONALLM_USE_AZURE_CLI`: Boolean to use Azure CLI credentials

### Phase 2: Management API Client

1. **Create ManagementClient**
   - Implement HTTP client for Management API
   - Support both Azure CLI and API key authentication
   - Handle token refresh for Azure CLI credentials
   - Implement methods for all agent operations:
     - `create_agent_from_template()`
     - `get_agent()`
     - `list_agents()`
     - `update_agent()`
     - `delete_agent()`
     - `update_agent_prompt()`
     - `update_agent_model()`
     - `get_agent_workflows()`
     - `get_ai_models()`

2. **Error Handling**
   - Handle API errors gracefully
   - Provide meaningful error messages
   - Support retry logic for transient failures

### Phase 3: MCP Server Implementation

1. **MCP Server Setup**
   - Initialize MCP server using `mcp` package
   - Register all tools
   - Handle tool execution requests
   - Return structured responses

2. **Tool Implementations**
   - Implement each tool as a function
   - Validate input parameters
   - Call Management API client methods
   - Format responses appropriately
   - Handle errors and return error messages

3. **Input Validation**
   - Use Pydantic models for parameter validation
   - Validate agent names (slug format)
   - Validate dates (ISO 8601 format)
   - Validate object IDs

### Phase 4: Integration with Python SDK

1. **Model Integration**
   - Import agent models from `foundationallm.models.agents`
   - Use `AgentBase`, `KnowledgeManagementAgent`, etc.
   - Leverage existing model validation

2. **Type Safety**
   - Use type hints throughout
   - Ensure compatibility with MCP protocol types

### Phase 5: Testing

1. **Unit Tests**
   - Test Management API client methods
   - Test tool implementations
   - Test error handling
   - Test input validation

2. **Integration Tests**
   - Test against real Management API (if available)
   - Test end-to-end agent creation flow
   - Test agent configuration updates

3. **MCP Protocol Tests**
   - Test MCP server initialization
   - Test tool registration
   - Test tool execution
   - Test response formatting

### Phase 6: Documentation

1. **API Documentation**
   - Document all tools and parameters
   - Provide usage examples
   - Document configuration requirements

2. **Setup Guide**
   - Installation instructions
   - Configuration guide
   - Authentication setup
   - Usage examples

3. **README**
   - Overview of the MCP server
   - Quick start guide
   - Link to detailed documentation

## Configuration Example

```python
# config/settings.py
import os
from pydantic import BaseSettings

class Settings(BaseSettings):
    management_api_endpoint: str
    instance_id: str
    api_scope: str = "https://management.azure.com/.default"
    api_key: str | None = None
    use_azure_cli: bool = True
    
    class Config:
        env_prefix = "FOUNDATIONALLM_"
```

## Usage Example

```python
# Example MCP tool usage
{
  "name": "create_agent",
  "arguments": {
    "agent_name": "customer-support-agent",
    "display_name": "Customer Support Agent",
    "description": "AI agent for handling customer support inquiries",
    "welcome_message": "Hello! How can I help you today?",
    "expiration_date": "2025-12-31T00:00:00+00:00"
  }
}
```

## Error Handling

The MCP server should handle:
- Invalid agent names
- Missing required parameters
- API authentication failures
- Agent not found errors
- Validation errors
- Network/timeout errors

All errors should be returned in a format that the MCP client can understand and display to the user.

## Security Considerations

1. **Authentication**
   - Support Azure CLI credentials (recommended for development)
   - Support API key authentication (for production)
   - Never log or expose credentials

2. **Authorization**
   - Rely on FoundationaLLM's RBAC for authorization
   - The Management API will enforce permissions
   - Return appropriate error messages for unauthorized access

3. **Input Validation**
   - Validate all inputs before making API calls
   - Sanitize agent names and other user inputs
   - Prevent injection attacks

## Future Enhancements

1. **Additional Tools**
   - Configure agent conversation history settings
   - Configure agent gatekeeper settings
   - Manage agent role assignments
   - Upload agent files
   - Configure agent capabilities

2. **Advanced Features**
   - Batch operations
   - Agent templates management
   - Workflow configuration
   - Vectorization pipeline management

3. **Monitoring**
   - Logging and telemetry
   - Performance metrics
   - Error tracking

## References

- FoundationaLLM Management API endpoints (from `src/ui/UserPortal/js/api.ts`)
- Agent models (`src/python/PythonSDK/foundationallm/models/agents/`)
- Management client example (`samples/python/clients/management_client.py`)
- MCP Python SDK documentation

## Success Criteria

1. ✅ MCP server successfully initializes and registers all tools
2. ✅ Can create agents using the `create_agent` tool
3. ✅ Can retrieve agents using `get_agent` and `list_agents`
4. ✅ Can update agent prompts and models
5. ✅ Can configure agent vectorization settings
6. ✅ Can delete agents
7. ✅ Proper error handling and validation
8. ✅ Comprehensive documentation
9. ✅ Unit and integration tests pass

## Timeline Estimate

- **Phase 1**: 1-2 days (Project setup)
- **Phase 2**: 3-4 days (Management API client)
- **Phase 3**: 3-4 days (MCP server implementation)
- **Phase 4**: 1-2 days (Python SDK integration)
- **Phase 5**: 2-3 days (Testing)
- **Phase 6**: 1-2 days (Documentation)

**Total**: ~11-17 days

## Notes

- The implementation should follow existing code patterns in the FoundationaLLM codebase
- Reference the `samples/python/clients/management_client.py` for API client patterns
- Use the existing Python SDK models for type safety
- Ensure compatibility with the existing Management API endpoints
- Consider async/await patterns for better performance if needed
