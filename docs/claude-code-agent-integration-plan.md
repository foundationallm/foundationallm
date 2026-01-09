# Claude Code Agent Integration Plan for FoundationaLLM

## Executive Summary

This document outlines a comprehensive plan to integrate Claude Code agent capabilities into FoundationaLLM. The integration will allow users to access the full power of Claude Code through the familiar FoundationaLLM UI and APIs, with FoundationaLLM serving as the operating system for this AI agent.

The core approach leverages FoundationaLLM's existing infrastructure:
- **Dynamic Sessions Custom Containers** for secure, sandboxed Ubuntu Linux execution environments
- **Context API** for file management, code session lifecycle, and security
- **File Service** for secure per-user file persistence with RBAC
- **Existing tool patterns** from the Code Interpreter tool implementation

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Existing Infrastructure Analysis](#2-existing-infrastructure-analysis)
3. [Component Design](#3-component-design)
4. [Workflow Implementation](#4-workflow-implementation)
5. [Claude Code Tool Implementation](#5-claude-code-tool-implementation)
6. [Sandbox Container Infrastructure](#6-sandbox-container-infrastructure)
7. [File Storage & Security](#7-file-storage--security)
8. [Conversation History & Multi-Turn Support](#8-conversation-history--multi-turn-support)
9. [UI Integration](#9-ui-integration)
10. [Configuration & Deployment](#10-configuration--deployment)
11. [Testing Strategy](#11-testing-strategy)
12. [Implementation Phases](#12-implementation-phases)
13. [Open Questions & Decisions](#13-open-questions--decisions)

---

## 1. Architecture Overview

### 1.1 High-Level Architecture

The Claude Code Agent integrates into FoundationaLLM following the established patterns used by the Code Interpreter tool, leveraging the Context API as a secure middleware layer.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          FoundationaLLM Platform                            │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐    ┌──────────────┐    ┌─────────────────────────────────┐ │
│  │ User Portal │    │ Management   │    │           Core API              │ │
│  │    (Vue)    │◄──►│   Portal     │◄──►│    (Completions, Sessions)     │ │
│  └─────────────┘    └──────────────┘    └─────────────────────────────────┘ │
│         │                                              │                    │
│         ▼                                              ▼                    │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │                      Orchestration API                                  ││
│  │  ┌──────────────────────────────────────────────────────────────────┐   ││
│  │  │           OrchestrationBuilder (creates code sessions)           │   ││
│  │  │  ┌─────────────┐  ┌─────────────┐  ┌────────────────────────┐   │   ││
│  │  │  │ LangChain   │  │ Function    │  │  Claude Code Agent     │   │   ││
│  │  │  │ Workflow    │  │ Calling WF  │  │  Workflow (NEW)        │   │   ││
│  │  │  └─────────────┘  └─────────────┘  └────────────────────────┘   │   ││
│  │  └──────────────────────────────────────────────────────────────────┘   ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │                         Context API                                     ││
│  │  ┌──────────────────────────────────────────────────────────────────┐   ││
│  │  │ CodeSessionsController                                           │   ││
│  │  │  • POST /codeSessions              (create session)              │   ││
│  │  │  • POST /codeSessions/{id}/uploadFiles                           │   ││
│  │  │  • POST /codeSessions/{id}/downloadFiles                         │   ││
│  │  │  • POST /codeSessions/{id}/executeCode                           │   ││
│  │  └──────────────────────────────────────────────────────────────────┘   ││
│  │  ┌──────────────────────────────────────────────────────────────────┐   ││
│  │  │ CodeSessionService                                               │   ││
│  │  │  • Manages session lifecycle via CosmosDB                        │   ││
│  │  │  • Routes to appropriate provider (CodeInterpreter/CustomContainer)│  ││
│  │  │  • Handles file upload/download via FileService                  │   ││
│  │  └──────────────────────────────────────────────────────────────────┘   ││
│  │  ┌──────────────────────────────────────────────────────────────────┐   ││
│  │  │ FileService (Secured file operations)                            │   ││
│  │  │  • Per-user file isolation (UPN-based)                           │   ││
│  │  │  • Conversation-scoped storage                                   │   ││
│  │  │  • Owner verification on access                                  │   ││
│  │  └──────────────────────────────────────────────────────────────────┘   ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │         Azure Container Apps Dynamic Sessions (Custom Container)        ││
│  │  ┌────────────────────────────────────────────────────────────────────┐ ││
│  │  │         Claude Code Sandbox Container (Ubuntu Linux)               │ ││
│  │  │  ┌──────────────┐  ┌────────────────┐  ┌─────────────────────────┐ │ ││
│  │  │  │ Claude Agent │  │ Dev Tools      │  │ GitHub CLI              │ │ ││
│  │  │  │ SDK + API    │  │ (Python, git)  │  │ (authenticated)         │ │ ││
│  │  │  └──────────────┘  └────────────────┘  └─────────────────────────┘ │ ││
│  │  │  Files mounted at /mnt/data (standard Dynamic Sessions path)       │ ││
│  │  └────────────────────────────────────────────────────────────────────┘ ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │               Azure Blob Storage + CosmosDB                             ││
│  │           (Per-user file persistence with owner verification)           ││
│  └─────────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Key Design Principles

1. **Leverage Existing Infrastructure**: Use the same patterns as the Code Interpreter tool—Context API, CodeSessionService, FileService, and Azure Container Apps Custom Containers
2. **No Custom Web Server in Sandbox**: Follow the standard Dynamic Sessions API contract (`/code/execute`, `/files/upload`, etc.) rather than building a custom orchestrator
3. **Security Through Context API**: All file access flows through FileService with UPN-based owner verification
4. **Session-per-Tool Pattern**: Code sessions are created by OrchestrationBuilder based on tool configuration (`code_session_required: true`)
5. **Multi-Turn via Conversation Files**: Workspace state persisted to blob storage and restored on subsequent requests
6. **Configurable Internet Access**: Container network configuration via Azure Container Apps session pool settings

---

## 2. Existing Infrastructure Analysis

Understanding how the Code Interpreter tool works is essential for designing the Claude Code agent integration. This section documents the key patterns we will follow.

### 2.1 Code Session Lifecycle

The existing Code Interpreter follows this flow:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Code Session Lifecycle (Current Pattern)                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. OrchestrationBuilder.LoadAgent()                                        │
│     ├── Detects tool has `code_session_required: true`                      │
│     ├── Calls ContextServiceClient.CreateCodeSession()                      │
│     └── Stores session endpoint + ID in exploded objects dictionary         │
│                                                                             │
│  2. Tool Execution (e.g., FoundationaLLMCodeInterpreterTool)                │
│     ├── Retrieves session_id and endpoint from runnable_config              │
│     ├── Uploads files: POST /codeSessions/{id}/uploadFiles                  │
│     ├── Executes code: POST /codeSessions/{id}/executeCode                  │
│     └── Downloads new files: POST /codeSessions/{id}/downloadFiles          │
│                                                                             │
│  3. FileService handles file persistence                                    │
│     ├── Files stored with UPN, conversationId, instanceId                   │
│     ├── Owner verification on all file access                               │
│     └── Files accessible via Context API file endpoints                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Key Code References

**Session Creation** (`OrchestrationBuilder.cs` lines 390-426):
```csharp
if (tool.TryGetPropertyValue<bool>(AgentToolPropertyNames.CodeSessionRequired, out bool codeSessionRequired)
    && codeSessionRequired)
{
    var contextServiceResponse = await contextServiceClient.CreateCodeSession(
        instanceId, agentName, originalRequest.SessionId, tool.Name,
        codeSessionProvider, codeSessionLanguage);
    
    toolParameters.Add(AgentToolPropertyNames.CodeSessionEndpoint, codeSession.Endpoint);
    toolParameters.Add(AgentToolPropertyNames.CodeSessionId, codeSession.SessionId);
}
```

**File Upload/Download** (`CodeSessionService.cs`):
- `UploadFilesToCodeSession`: Gets file content from FileService, uploads to Dynamic Sessions
- `DownloadFilesFromCodeSession`: Gets files from Dynamic Sessions, saves via `FileService.CreateFileForConversation()`

**File Security** (`FileService.cs`):
```csharp
// Files are stored with user identity
var fileRecord = new ContextFileRecord(instanceId, origin, conversationId, agentName,
    fileName, contentType, content.Length, fileProcessingType, userIdentity, metadata);

// Owner verification on retrieval
var fileRecords = await _cosmosDBService.GetFileRecords(instanceId, conversationId,
    fileName, userIdentity.UPN!, bypassOwnerCheck);
```

### 2.3 Azure Container Apps Custom Container API

The custom container service (`AzureContainerAppsCustomContainerService.cs`) implements these endpoints:

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/files/upload?identifier={sessionId}` | POST | Upload file to session |
| `/files?identifier={sessionId}` | GET | List files in session |
| `/files/download?identifier={sessionId}` | POST | Download file from session |
| `/files/delete?identifier={sessionId}` | POST | Delete files from session |
| `/code/execute?identifier={sessionId}` | POST | Execute code in session |

The response format from `/code/execute`:
```json
{
  "detail": {
    "output": "stdout content",
    "error": "stderr content", 
    "results": "execution result"
  }
}
```

### 2.4 Implications for Claude Code Agent

Based on this analysis, the Claude Code agent should:

1. **Use the same tool pattern**: Create a `FoundationaLLMClaudeCodeTool` similar to `FoundationaLLMCodeInterpreterTool`
2. **Leverage existing session management**: Set `code_session_required: true` and let OrchestrationBuilder handle session creation
3. **Build a custom container image**: That accepts the standard Dynamic Sessions API and internally runs the Claude Agent SDK
4. **Use FileService for persistence**: Files flow through Context API, inheriting security model
5. **Add a new code session provider**: `AzureContainerAppsClaudeCodeService` or reuse `AzureContainerAppsCustomContainerService` with Claude-specific endpoints

---

## 3. Component Design

### 3.1 Option A: Claude Code as a Tool (Recommended)

Following the Code Interpreter pattern, implement Claude Code as a **tool** that can be added to agents using the `ExternalAgentWorkflow` or `LangChainAgentWorkflow`:

**Benefits**:
- Minimal changes to existing workflow/orchestration code
- Can be combined with other tools (Knowledge Search, etc.)
- Follows established patterns

**Structure**:
```
src/python/plugins/agent_claude_code/
├── pkg/
│   └── foundationallm_agent_plugins_claude_code/
│       ├── __init__.py
│       ├── _metadata/
│       │   └── foundationallm_manifest.json
│       ├── tool_plugin_manager.py
│       └── tools/
│           ├── __init__.py
│           ├── foundationallm_claude_code_tool.py
│           └── foundationallm_claude_code_tool_input.py
├── requirements.txt
└── test/
```

### 3.2 Option B: Claude Code as a Dedicated Workflow

Create a new workflow type where Claude is the primary orchestrator (not just a tool):

**Benefits**:
- Full control over the agentic loop
- Can implement Claude's native tool-use patterns directly
- Better for pure "Claude Code" experience

**Drawback**:
- More code to maintain
- Cannot easily combine with other FoundationaLLM tools

### 3.3 Recommended Approach: Hybrid

Implement **both**:
1. **FoundationaLLMClaudeCodeTool**: For use within existing workflows (like Function Calling Workflow)
2. **ClaudeCodeAgentWorkflow**: For dedicated Claude Code agent experience

This allows users to:
- Add Claude Code capabilities to existing agents
- Create dedicated Claude Code agents with full capabilities

### 3.4 New Tool: FoundationaLLMClaudeCodeTool

**Location**: `src/python/plugins/agent_claude_code/pkg/foundationallm_agent_plugins_claude_code/tools/`

Tool configuration properties:

| Property | Type | Description |
|----------|------|-------------|
| `code_session_required` | bool | Always `true` |
| `code_session_endpoint_provider` | string | `AzureContainerAppsClaudeCode` |
| `code_session_language` | string | `ClaudeCode` |
| `allow_internet_access` | bool | Enable web search capabilities |
| `allowed_capabilities` | list | `["bash", "file_read", "file_write", "github"]` |
| `max_turns` | int | Maximum agentic loop iterations |

### 3.5 New Code Session Provider

Add a new provider to handle Claude Code sessions:

**Location**: `src/dotnet/ContextEngine/Constants/CodeSessionProviderNames.cs`

```csharp
/// <summary>
/// Azure Container Apps Dynamic Sessions custom container for Claude Code.
/// </summary>
public const string AzureContainerAppsClaudeCode = "AzureContainerAppsClaudeCode";
```

**Location**: `src/dotnet/ContextEngine/Services/AzureContainerAppsClaudeCodeService.cs`

This can extend or adapt `AzureContainerAppsCustomContainerService` with Claude-specific execution logic.

---

## 4. Workflow Implementation

### 4.1 Using Existing Workflow with Claude Code Tool

The simplest integration path is to use the existing `FoundationaLLMFunctionCallingWorkflow` with a new `FoundationaLLMClaudeCodeTool`. This approach:

- Leverages existing router/tool-selection logic
- Can combine Claude Code with other tools (Knowledge Search, etc.)
- Follows established patterns

**Agent Configuration Example**:
```json
{
  "type": "agent",
  "name": "claude-code-assistant",
  "workflow": {
    "type": "external-agent-workflow",
    "package_name": "foundationallm_agent_plugins",
    "class_name": "FoundationaLLMFunctionCallingWorkflow"
  },
  "tools": [
    {
      "name": "Claude-Code",
      "description": "Executes complex coding tasks using Claude's agentic capabilities",
      "package_name": "foundationallm_agent_plugins_claude_code",
      "class_name": "FoundationaLLMClaudeCodeTool",
      "properties": {
        "code_session_required": true,
        "code_session_endpoint_provider": "AzureContainerAppsClaudeCode",
        "code_session_language": "ClaudeCode",
        "allow_internet_access": true,
        "max_turns": 20
      }
    }
  ]
}
```

### 4.2 Dedicated Claude Code Agent Workflow (Optional)

For a pure Claude Code experience, create a dedicated workflow:

```python
# foundationallm_claude_code_workflow.py
"""
Dedicated workflow where Claude is the primary orchestrator.
"""

from typing import Dict, List, Optional
import json

from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.orchestration import (
    CompletionResponse, ContentArtifact, OpenAITextMessageContentItem
)
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.operations import OperationsManager

class FoundationaLLMClaudeCodeWorkflow(FoundationaLLMWorkflowBase):
    """
    Dedicated Claude Code workflow where Claude handles all orchestration.
    """

    def __init__(
        self,
        workflow_config,
        objects: Dict,
        tools: List,  # Tools are built into Claude, not LangChain tools
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration
    ):
        super().__init__(workflow_config, objects, tools, 
                        operations_manager, user_identity, config)
        
        # Get session info from exploded objects (set by OrchestrationBuilder)
        self.session_id = objects.get('Claude-Code', {}).get('code_session_id')
        self.session_endpoint = objects.get('Claude-Code', {}).get('code_session_endpoint')
        
        self.__create_context_client()

    async def invoke_async(
        self,
        operation_id: str,
        user_prompt: str,
        user_prompt_rewrite: Optional[str],
        message_history: List,
        file_history: List,
        conversation_id: Optional[str] = None,
        objects: dict = None
    ) -> CompletionResponse:
        """
        Invoke the Claude Code workflow.
        
        This sends the user prompt to the Claude Code sandbox container,
        which runs the Claude Agent SDK in an agentic loop.
        """
        llm_prompt = user_prompt_rewrite or user_prompt
        
        # 1. Upload conversation files to the sandbox
        file_names = [f.original_file_name for f in file_history]
        await self._upload_files_to_session(file_names)
        
        # 2. Build the execution request for Claude
        execution_request = self._build_claude_request(
            llm_prompt, message_history, file_history, conversation_id
        )
        
        # 3. Execute in the Claude sandbox (this runs the full agentic loop)
        result = await self._execute_in_sandbox(execution_request)
        
        # 4. Download any new files created by Claude
        content_artifacts = await self._download_new_files(operation_id)
        
        # 5. Build response
        return CompletionResponse(
            operation_id=operation_id,
            content=[OpenAITextMessageContentItem(
                value=result.get('response', ''),
                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )],
            content_artifacts=content_artifacts,
            user_prompt=user_prompt,
            completion_tokens=result.get('output_tokens', 0),
            prompt_tokens=result.get('input_tokens', 0)
        )

    def _build_claude_request(self, prompt, history, files, conversation_id):
        """Build request for Claude sandbox execution."""
        return json.dumps({
            "prompt": prompt,
            "conversation_history": [
                {"role": "user" if m.sender == "User" else "assistant", 
                 "content": m.text}
                for m in history
            ],
            "conversation_id": conversation_id,
            "config": {
                "max_turns": self.workflow_config.properties.get('max_turns', 20),
                "allowed_capabilities": self.workflow_config.properties.get(
                    'allowed_capabilities', 
                    ['bash', 'file_read', 'file_write']
                )
            }
        })

    async def _execute_in_sandbox(self, request_json):
        """Execute request in Claude sandbox via Context API."""
        response = await self.context_api_client.post_async(
            endpoint=f"/instances/{self.instance_id}/codeSessions/{self.session_id}/executeCode",
            data=json.dumps({"code_to_execute": request_json})
        )
        return response
```

---

## 5. Claude Code Tool Implementation

### 5.1 Tool Input Model

**Location**: `src/python/plugins/agent_claude_code/pkg/foundationallm_agent_plugins_claude_code/tools/foundationallm_claude_code_tool_input.py`

```python
from typing import Optional, List
from pydantic import BaseModel, Field

class FoundationaLLMClaudeCodeToolInput(BaseModel):
    """Input data model for the Claude Code tool."""
    prompt: str = Field(
        description="The task description or question for Claude Code to process."
    )
    file_names: List[str] = Field(
        default=[],
        description="List of file names from the conversation to provide as context."
    )
```

### 5.2 Claude Code Tool

**Location**: `src/python/plugins/agent_claude_code/pkg/foundationallm_agent_plugins_claude_code/tools/foundationallm_claude_code_tool.py`

```python
"""
FoundationaLLMClaudeCodeTool: A tool for executing tasks using Claude's agentic capabilities.

This tool follows the same pattern as FoundationaLLMCodeInterpreterTool:
1. Uses code sessions managed by Context API
2. Uploads files via uploadFiles endpoint
3. Executes via executeCode endpoint (which runs Claude's agentic loop)
4. Downloads results via downloadFiles endpoint
"""

from typing import Optional, Tuple, Type, List, ClassVar, Any
from uuid import uuid4
import json

from langchain_core.callbacks import AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from pydantic import BaseModel

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMToolBase, FoundationaLLMToolResult
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import ContentArtifactTypeNames, RunnableConfigKeys
from foundationallm.models.orchestration import CompletionRequestObjectKeys, ContentArtifact
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService

from .foundationallm_claude_code_tool_input import FoundationaLLMClaudeCodeToolInput


class FoundationaLLMClaudeCodeTool(FoundationaLLMToolBase):
    """A tool for executing complex tasks using Claude's agentic capabilities."""
    
    args_schema: Type[BaseModel] = FoundationaLLMClaudeCodeToolInput
    DYNAMIC_SESSION_ENDPOINT: ClassVar[str] = "code_session_endpoint"
    DYNAMIC_SESSION_ID: ClassVar[str] = "code_session_id"

    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        super().__init__(tool_config, objects, user_identity, config)
        
        # Get Context API client (same pattern as Code Interpreter)
        context_api_endpoint_configuration = APIEndpointConfiguration(
            **objects.get(CompletionRequestObjectKeys.CONTEXT_API_ENDPOINT_CONFIGURATION, None)
        )
        if context_api_endpoint_configuration:
            self.context_api_client = HttpClientService(
                context_api_endpoint_configuration,
                user_identity,
                config
            )
        else:
            raise ToolException("Context API endpoint configuration is required.")
        
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID)
        
        # Tool configuration
        self.max_turns = tool_config.properties.get('max_turns', 20) if tool_config.properties else 20
        self.allowed_capabilities = tool_config.properties.get(
            'allowed_capabilities', 
            ['bash', 'file_read', 'file_write']
        ) if tool_config.properties else ['bash', 'file_read', 'file_write']

    def _run(self, *args, **kwargs):
        raise ToolException("This tool does not support synchronous execution.")

    async def _arun(
        self,
        prompt: str,
        file_names: Optional[List[str]] = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig = None,
        **kwargs
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """Execute Claude Code tool asynchronously."""
        
        file_names = file_names or []
        content_artifacts = []
        input_tokens = 0
        output_tokens = 0
        
        # Get session info from runnable_config (set by OrchestrationBuilder)
        session_id = runnable_config['configurable'][self.tool_config.name][self.DYNAMIC_SESSION_ID]
        
        # Get original prompts for context
        user_prompt = runnable_config['configurable'].get(RunnableConfigKeys.ORIGINAL_USER_PROMPT)
        user_prompt_rewrite = runnable_config['configurable'].get(RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE)
        conversation_id = runnable_config['configurable'].get(RunnableConfigKeys.CONVERSATION_ID)
        
        llm_prompt = prompt or user_prompt_rewrite or user_prompt
        
        # 1. Upload files to the code session
        self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
        
        operation_response = await self.context_api_client.post_async(
            endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/uploadFiles",
            data=json.dumps({"file_names": file_names})
        )
        operation_id = operation_response['operation_id']
        
        # Get initial file list
        beginning_files = await self.context_api_client.post_async(
            endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
            data=json.dumps({"operation_id": operation_id})
        )
        beginning_files_list = beginning_files.get('file_records', {})
        
        # 2. Build Claude execution request
        # The "code" sent to executeCode is actually a JSON payload for Claude
        claude_request = self._build_claude_request(
            llm_prompt, 
            conversation_id,
            file_names
        )
        
        try:
            # 3. Execute via Claude sandbox
            execution_response = await self.context_api_client.post_async(
                endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/executeCode",
                data=json.dumps({"code_to_execute": claude_request})
            )
            
            # 4. Get new files created by Claude
            files_response = await self.context_api_client.post_async(
                endpoint=f"/instances/{self.instance_id}/codeSessions/{session_id}/downloadFiles",
                data=json.dumps({"operation_id": operation_id})
            )
            files_list = files_response.get('file_records', {})
            
            # Filter to only new files
            new_files = {k: v for k, v in files_list.items() if k not in beginning_files_list}
            
            # Create content artifacts for new files
            for file_name, file_data in new_files.items():
                content_artifacts.append(ContentArtifact(
                    id=self.name,
                    title=f'{self.name} (file)',
                    type=ContentArtifactTypeNames.FILE,
                    filepath=file_name,
                    metadata={
                        'file_object_id': file_data['file_object_id'],
                        'original_file_name': file_data['file_name'],
                        'file_path': file_data['file_path'],
                        'file_size': str(file_data['file_size_bytes']),
                        'content_type': file_data['content_type'],
                        'conversation_id': file_data['conversation_id']
                    }
                ))
            
            # Extract response
            final_response = execution_response.get('execution_result', '')
            if execution_response.get('status') == 'Failed':
                final_response = f"Claude Code execution failed: {execution_response.get('error_output', 'Unknown error')}"
            
            # Extract token usage from response
            input_tokens = execution_response.get('input_tokens', 0)
            output_tokens = execution_response.get('output_tokens', 0)
            
        except Exception as e:
            final_response = f"Claude Code execution error: {str(e)}"
        
        # Create execution artifact
        content_artifacts.append(ContentArtifact(
            id=self.name,
            title=self.name,
            type=ContentArtifactTypeNames.TOOL_EXECUTION,
            filepath=str(uuid4()),
            metadata={
                'original_user_prompt': user_prompt_rewrite or user_prompt,
                'tool_input_prompt': prompt,
                'tool_input_files': ', '.join(file_names),
                'tool_result': final_response
            }
        ))
        
        return final_response, FoundationaLLMToolResult(
            content=final_response,
            content_artifacts=content_artifacts,
            input_tokens=input_tokens,
            output_tokens=output_tokens
        )

    def _build_claude_request(
        self, 
        prompt: str, 
        conversation_id: str,
        file_names: List[str]
    ) -> str:
        """Build the JSON request for Claude sandbox execution."""
        return json.dumps({
            "prompt": prompt,
            "conversation_id": conversation_id,
            "files": file_names,
            "config": {
                "max_turns": self.max_turns,
                "allowed_capabilities": self.allowed_capabilities
            }
        })
```

---

## 6. Sandbox Container Infrastructure

### 6.1 Container Design Philosophy

**Key Insight**: We do NOT need to build a custom web server in the container. Instead, we implement the **standard Azure Container Apps Dynamic Sessions API contract** that the existing `AzureContainerAppsCustomContainerService` already knows how to call.

The container must implement these endpoints:

| Endpoint | Method | Request | Response |
|----------|--------|---------|----------|
| `/files/upload` | POST | multipart/form-data | `{"success": true}` |
| `/files` | GET | `?identifier={sessionId}` | File listing JSON |
| `/files/download` | POST | `{"file_name": "..."}` | File content stream |
| `/files/delete` | POST | `?identifier={sessionId}` | `{"success": true}` |
| `/code/execute` | POST | `{"code": "..."}` | `{"detail": {"output": "...", "error": "...", "results": "..."}}` |

### 6.2 Custom Container Image

**Location**: `src/containers/claude-code-sandbox/`

```dockerfile
# Dockerfile
FROM ubuntu:22.04

# System dependencies
RUN apt-get update && apt-get install -y \
    python3 python3-pip python3-venv \
    nodejs npm \
    git curl wget \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# GitHub CLI
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg \
    | dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" \
    | tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
    && apt-get update \
    && apt-get install -y gh

# Create sandbox user and directories
RUN useradd -m -s /bin/bash sandbox \
    && mkdir -p /mnt/data \
    && chown sandbox:sandbox /mnt/data

WORKDIR /app

# Install Python dependencies
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt

# Copy the Dynamic Sessions API server
COPY app/ /app/

# Expose API port (standard Dynamic Sessions port)
EXPOSE 8080

# Run as non-root
USER sandbox

# Start the API server
CMD ["python3", "server.py"]
```

### 6.3 Dynamic Sessions API Implementation

The container implements the Dynamic Sessions API contract, with `/code/execute` running the Claude agentic loop:

```python
# app/server.py
"""
Claude Code Sandbox - Dynamic Sessions API Implementation

Implements the Azure Container Apps Dynamic Sessions API contract,
with /code/execute running Claude's agentic loop.
"""

from fastapi import FastAPI, File, UploadFile, Query, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from typing import List, Dict, Optional
import anthropic
import asyncio
import json
import os
from pathlib import Path

app = FastAPI(title="Claude Code Sandbox")

# Standard Dynamic Sessions file mount point
DATA_DIR = Path("/mnt/data")

# ============================================================
# File Operations (Standard Dynamic Sessions API)
# ============================================================

@app.post("/files/upload")
async def upload_file(
    file: UploadFile = File(...),
    identifier: str = Query(...)
):
    """Upload a file to the session workspace."""
    session_dir = DATA_DIR / identifier
    session_dir.mkdir(parents=True, exist_ok=True)
    
    file_path = session_dir / file.filename
    with open(file_path, "wb") as f:
        content = await file.read()
        f.write(content)
    
    return {"success": True, "filename": file.filename}

@app.get("/files")
async def list_files(
    identifier: str = Query(...),
    path: Optional[str] = Query(default="")
):
    """List files in the session workspace."""
    session_dir = DATA_DIR / identifier
    if path:
        session_dir = session_dir / path.lstrip("/")
    
    if not session_dir.exists():
        return {"value": []}
    
    items = []
    for item in session_dir.iterdir():
        items.append({
            "name": item.name,
            "type": "directory" if item.is_dir() else "file",
            "size": item.stat().st_size if item.is_file() else 0,
            "contentType": _get_content_type(item.name) if item.is_file() else None
        })
    
    return {"value": items}

@app.post("/files/download")
async def download_file(
    identifier: str = Query(...),
    body: dict = None
):
    """Download a file from the session workspace."""
    file_name = body.get("file_name", "")
    session_dir = DATA_DIR / identifier
    file_path = session_dir / file_name.lstrip("/")
    
    if not file_path.exists():
        raise HTTPException(404, f"File not found: {file_name}")
    
    return StreamingResponse(
        open(file_path, "rb"),
        media_type=_get_content_type(file_name)
    )

@app.post("/files/delete")
async def delete_files(identifier: str = Query(...)):
    """Delete all files from the session workspace."""
    import shutil
    session_dir = DATA_DIR / identifier
    if session_dir.exists():
        shutil.rmtree(session_dir)
        session_dir.mkdir(parents=True, exist_ok=True)
    return {"success": True}

# ============================================================
# Code Execution (Claude Agentic Loop)
# ============================================================

class CodeExecuteRequest(BaseModel):
    code: str  # JSON payload containing prompt and config

@app.post("/code/execute")
async def execute_code(
    request: CodeExecuteRequest,
    identifier: str = Query(...)
):
    """
    Execute Claude's agentic loop.
    
    The 'code' field contains a JSON payload with:
    - prompt: The user's task/question
    - conversation_id: For context
    - files: List of available files
    - config: max_turns, allowed_capabilities
    """
    try:
        payload = json.loads(request.code)
        prompt = payload.get("prompt", "")
        config = payload.get("config", {})
        
        session_dir = DATA_DIR / identifier
        
        # Run Claude agentic loop
        result = await run_claude_agent(
            prompt=prompt,
            workspace_dir=session_dir,
            max_turns=config.get("max_turns", 20),
            allowed_capabilities=config.get("allowed_capabilities", [])
        )
        
        # Return in Dynamic Sessions format
        return {
            "detail": {
                "output": result.get("stdout", ""),
                "error": result.get("stderr", ""),
                "results": result.get("response", "")
            },
            "input_tokens": result.get("input_tokens", 0),
            "output_tokens": result.get("output_tokens", 0)
        }
        
    except json.JSONDecodeError:
        # Fallback: treat as raw Python code (backwards compatible)
        # This allows the container to also work as a standard code interpreter
        return await execute_python_code(request.code, identifier)
    except Exception as e:
        return {
            "detail": {
                "output": "",
                "error": str(e),
                "results": ""
            }
        }

async def run_claude_agent(
    prompt: str,
    workspace_dir: Path,
    max_turns: int,
    allowed_capabilities: List[str]
) -> Dict:
    """Run Claude's agentic loop with tool use."""
    
    client = anthropic.Anthropic(
        api_key=os.environ.get("ANTHROPIC_API_KEY")
    )
    
    # Configure available tools
    tools = build_tools(allowed_capabilities, workspace_dir)
    
    # System prompt for Claude Code behavior
    system_prompt = build_system_prompt(workspace_dir, allowed_capabilities)
    
    messages = [{"role": "user", "content": prompt}]
    
    total_input_tokens = 0
    total_output_tokens = 0
    all_output = []
    
    for turn in range(max_turns):
        response = client.messages.create(
            model=os.environ.get("CLAUDE_MODEL", "claude-sonnet-4-20250514"),
            max_tokens=8192,
            system=system_prompt,
            messages=messages,
            tools=tools
        )
        
        total_input_tokens += response.usage.input_tokens
        total_output_tokens += response.usage.output_tokens
        
        if response.stop_reason == "end_turn":
            # Claude is done
            final_text = ""
            for block in response.content:
                if hasattr(block, 'text'):
                    final_text += block.text
            
            return {
                "response": final_text,
                "stdout": "\n".join(all_output),
                "stderr": "",
                "input_tokens": total_input_tokens,
                "output_tokens": total_output_tokens
            }
        
        elif response.stop_reason == "tool_use":
            # Execute tool calls
            tool_results = []
            
            for block in response.content:
                if block.type == "tool_use":
                    result = await execute_tool(
                        block.name, 
                        block.input, 
                        workspace_dir,
                        allowed_capabilities
                    )
                    all_output.append(f"[{block.name}] {result.get('output', '')[:500]}")
                    
                    tool_results.append({
                        "type": "tool_result",
                        "tool_use_id": block.id,
                        "content": result.get("output", "")
                    })
            
            # Add assistant response and tool results to messages
            messages.append({"role": "assistant", "content": response.content})
            messages.append({"role": "user", "content": tool_results})
    
    # Max turns reached
    return {
        "response": "Maximum turns reached without completion.",
        "stdout": "\n".join(all_output),
        "stderr": "",
        "input_tokens": total_input_tokens,
        "output_tokens": total_output_tokens
    }

def build_tools(capabilities: List[str], workspace_dir: Path) -> List[Dict]:
    """Build Claude tool definitions based on allowed capabilities."""
    tools = []
    
    if "bash" in capabilities:
        tools.append({
            "name": "bash",
            "description": "Execute a bash command in the workspace directory.",
            "input_schema": {
                "type": "object",
                "properties": {
                    "command": {"type": "string", "description": "The bash command to execute"}
                },
                "required": ["command"]
            }
        })
    
    if "file_read" in capabilities:
        tools.append({
            "name": "read_file",
            "description": "Read the contents of a file.",
            "input_schema": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Path to the file (relative to workspace)"}
                },
                "required": ["path"]
            }
        })
    
    if "file_write" in capabilities:
        tools.append({
            "name": "write_file",
            "description": "Write content to a file, creating directories as needed.",
            "input_schema": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Path to the file"},
                    "content": {"type": "string", "description": "Content to write"}
                },
                "required": ["path", "content"]
            }
        })
    
    if "github" in capabilities:
        tools.append({
            "name": "github",
            "description": "Interact with GitHub using the gh CLI.",
            "input_schema": {
                "type": "object",
                "properties": {
                    "command": {"type": "string", "description": "gh CLI command (e.g., 'repo clone owner/repo')"}
                },
                "required": ["command"]
            }
        })
    
    return tools

async def execute_tool(
    tool_name: str, 
    tool_input: Dict, 
    workspace_dir: Path,
    capabilities: List[str]
) -> Dict:
    """Execute a tool and return the result."""
    
    if tool_name == "bash" and "bash" in capabilities:
        command = tool_input.get("command", "")
        try:
            proc = await asyncio.create_subprocess_shell(
                command,
                stdout=asyncio.subprocess.PIPE,
                stderr=asyncio.subprocess.PIPE,
                cwd=workspace_dir
            )
            stdout, stderr = await asyncio.wait_for(proc.communicate(), timeout=300)
            return {
                "output": stdout.decode() + (f"\nSTDERR: {stderr.decode()}" if stderr else ""),
                "success": proc.returncode == 0
            }
        except asyncio.TimeoutError:
            return {"output": "Command timed out", "success": False}
        except Exception as e:
            return {"output": str(e), "success": False}
    
    elif tool_name == "read_file" and "file_read" in capabilities:
        path = workspace_dir / tool_input.get("path", "").lstrip("/")
        try:
            return {"output": path.read_text(), "success": True}
        except Exception as e:
            return {"output": str(e), "success": False}
    
    elif tool_name == "write_file" and "file_write" in capabilities:
        path = workspace_dir / tool_input.get("path", "").lstrip("/")
        try:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(tool_input.get("content", ""))
            return {"output": f"Wrote {len(tool_input.get('content', ''))} bytes to {path.name}", "success": True}
        except Exception as e:
            return {"output": str(e), "success": False}
    
    elif tool_name == "github" and "github" in capabilities:
        command = f"gh {tool_input.get('command', '')}"
        return await execute_tool("bash", {"command": command}, workspace_dir, ["bash"])
    
    return {"output": f"Unknown or disabled tool: {tool_name}", "success": False}

def build_system_prompt(workspace_dir: Path, capabilities: List[str]) -> str:
    """Build the system prompt for Claude."""
    cap_str = ", ".join(capabilities)
    return f"""You are Claude Code, an AI assistant that helps with software development tasks.

You have access to a workspace directory at {workspace_dir} where you can read and write files.

Available capabilities: {cap_str}

Guidelines:
- Use tools to explore and modify files as needed
- Write clean, well-documented code
- Test your changes when possible
- Explain what you're doing and why
- If you need to install packages, use pip or npm as appropriate
"""

def _get_content_type(filename: str) -> str:
    """Get MIME type from filename."""
    ext = Path(filename).suffix.lower()
    types = {
        ".py": "text/x-python",
        ".js": "application/javascript",
        ".json": "application/json",
        ".txt": "text/plain",
        ".md": "text/markdown",
        ".html": "text/html",
        ".css": "text/css",
        ".png": "image/png",
        ".jpg": "image/jpeg",
        ".gif": "image/gif",
    }
    return types.get(ext, "application/octet-stream")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8080)
```

---

## 7. File Storage & Security

### 7.1 Leveraging Existing FileService

The Claude Code tool uses the **existing FileService** for all file persistence. This provides:

1. **UPN-based ownership**: Files are tagged with user's UPN for access control
2. **Conversation scoping**: Files are associated with conversation IDs
3. **Automatic security**: Owner verification on all file retrieval operations

### 7.2 File Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         File Flow in Claude Code                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  UPLOAD (User files → Sandbox):                                             │
│  1. User uploads file via UI                                                │
│  2. FileService.CreateFileForConversation() stores to blob + CosmosDB       │
│  3. Tool calls Context API: POST /codeSessions/{id}/uploadFiles             │
│  4. CodeSessionService.UploadFilesToCodeSession():                          │
│     - Gets file content via FileService.GetFileContent()                    │
│     - Uploads to Dynamic Sessions container at /mnt/data/{sessionId}/       │
│                                                                             │
│  DOWNLOAD (Sandbox files → User):                                           │
│  1. Claude creates files at /mnt/data/{sessionId}/                          │
│  2. Tool calls Context API: POST /codeSessions/{id}/downloadFiles           │
│  3. CodeSessionService.DownloadFilesFromCodeSession():                      │
│     - Gets file list from Dynamic Sessions                                  │
│     - Filters to NEW files only (not originally uploaded)                   │
│     - For each new file:                                                    │
│       * Downloads from Dynamic Sessions container                           │
│       * Saves via FileService.CreateFileForConversation()                   │
│       * Returns ContextFileRecord with file_object_id                       │
│  4. Tool creates ContentArtifact with file metadata                         │
│  5. UI renders download links via Context API file endpoints                │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.3 File Record Structure

Files are stored in CosmosDB with this structure (from `ContextFileRecord`):

```json
{
  "id": "unique-file-id",
  "instanceId": "fllm-instance-id",
  "conversationId": "conversation-123",
  "agentName": "claude-code-assistant",
  "fileName": "output.py",
  "filePath": "file/conversations/conversation-123/output.py",
  "contentType": "text/x-python",
  "fileSizeBytes": 1234,
  "fileProcessingType": "None",
  "upn": "user@example.com",
  "origin": "CodeSession",
  "metadata": {
    "codeSessionFileUploadRecordId": "upload-record-id",
    "codeSessionFilePath": "/output"
  }
}
```

### 7.4 Security Enforcement

From `FileService.GetFileContent()`:

```csharp
// Owner verification (unless bypassed by admin role)
var bypassOwnerCheck = await ShouldBypassOwnerCheck(instanceId, userIdentity);

var fileRecords = await _cosmosDBService.GetFileRecords(
    instanceId,
    conversationId,
    fileName,
    userIdentity.UPN!,      // <-- Only returns files owned by this user
    bypassOwnerCheck);       // <-- Unless user is Data Pipeline Execution Manager
```

### 7.5 File Access Patterns

| Access Type | Security Check | Method |
|-------------|----------------|--------|
| User downloads own file | UPN match required | `FileService.GetFileContent()` |
| Admin downloads any file | Role-based bypass | `ShouldBypassOwnerCheck()` |
| Tool accesses file | X-USER-IDENTITY header | Context API middleware |
| UI downloads file | Session token + UPN | Core API → Context API |

---

## 8. Conversation History & Multi-Turn Support

### 8.1 Leveraging Existing Patterns

FoundationaLLM already provides robust conversation history via:

1. **MessageHistory**: Passed to workflows via `CompletionRequest.MessageHistory`
2. **FileHistory**: Files associated with the conversation via `CompletionRequest.FileHistory`
3. **Session ID**: Code sessions are scoped to conversation via `sessionId = conversationId-toolName`

### 8.2 Multi-Turn Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Multi-Turn Claude Code Conversation                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Turn 1: "Create a Python script to analyze sales data"                     │
│  ├─ OrchestrationBuilder creates code session: conv-123-Claude-Code         │
│  ├─ Files uploaded to /mnt/data/conv-123-Claude-Code/                       │
│  ├─ Claude creates analyze_sales.py                                         │
│  └─ New files downloaded & saved via FileService (owner = user UPN)         │
│                                                                             │
│  Turn 2: "Add visualization to that script"                                 │
│  ├─ Same session ID: conv-123-Claude-Code (session persists)                │
│  ├─ MessageHistory includes Turn 1 exchange                                 │
│  ├─ FileHistory includes analyze_sales.py from Turn 1                       │
│  ├─ Claude modifies analyze_sales.py, creates charts.png                    │
│  └─ Updated/new files saved via FileService                                 │
│                                                                             │
│  Turn 3: "Run the script and show me the results"                           │
│  ├─ Same session, same workspace state preserved                            │
│  ├─ Claude executes: python analyze_sales.py                                │
│  └─ Output and generated files returned                                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Session ID Strategy

From existing code (`AzureContainerAppsServiceBase.GetNewSessionId`):

```csharp
protected virtual string GetNewSessionId(string conversationId, string context) =>
    $"code-{conversationId}-{context}";  // e.g., "code-conv-123-Claude-Code"
```

This ensures:
- **Same conversation**: Same session, workspace persists
- **Different tools**: Each tool gets its own session
- **New conversation**: Fresh session with clean workspace

### 8.4 Conversation Context in Claude

The Claude Code tool passes conversation history to the sandbox:

```python
def _build_claude_request(self, prompt, conversation_id, file_names):
    """Build request including conversation context."""
    return json.dumps({
        "prompt": prompt,
        "conversation_id": conversation_id,
        "files": file_names,
        "config": {
            "max_turns": self.max_turns,
            "allowed_capabilities": self.allowed_capabilities
        }
        # Note: Full message history is optionally passed for context
        # But workspace state (files) is the primary form of memory
    })
```

### 8.5 Workspace as Memory

Claude Code's primary "memory" is the **workspace filesystem**:

- Files created in previous turns persist in `/mnt/data/{sessionId}/`
- Claude can read previous files to understand context
- This is more reliable than summarizing conversation history
- Matches how developers naturally work (code is the source of truth)

---

## 9. UI Integration

### 9.1 User Portal (Minimal Changes)

The Claude Code tool works within existing UI patterns:

- **File Downloads**: Already supported via ContentArtifact file type
- **Response Display**: Standard completion response rendering
- **File Uploads**: Existing file upload mechanism

**Optional Enhancements**:

```vue
<!-- Enhanced tool execution display in ChatMessage.vue -->
<template>
  <div v-if="hasToolExecutionArtifacts" class="tool-execution-details">
    <Accordion>
      <AccordionTab header="Claude Code Activity">
        <div v-for="artifact in toolExecutionArtifacts" :key="artifact.id">
          <div class="tool-input">
            <strong>Task:</strong> {{ artifact.metadata.tool_input_prompt }}
          </div>
          <div class="tool-result">
            <strong>Result:</strong>
            <pre>{{ artifact.metadata.tool_result }}</pre>
          </div>
        </div>
      </AccordionTab>
    </Accordion>
  </div>
</template>
```

### 9.2 Management Portal Changes

Add Claude Code tool configuration in agent editing:

**Tool Configuration Properties**:

| Property | Type | UI Control |
|----------|------|------------|
| `code_session_required` | bool | Hidden (always true) |
| `code_session_endpoint_provider` | string | Dropdown |
| `code_session_language` | string | Hidden (ClaudeCode) |
| `allow_internet_access` | bool | Toggle switch |
| `max_turns` | int | Number input (1-50) |
| `allowed_capabilities` | list | Checkbox group |

### 9.3 No New API Endpoints Required

Claude Code uses existing APIs:

| Operation | Existing Endpoint |
|-----------|-------------------|
| Create session | `POST /codeSessions` (Context API) |
| Upload files | `POST /codeSessions/{id}/uploadFiles` |
| Execute | `POST /codeSessions/{id}/executeCode` |
| Download files | `POST /codeSessions/{id}/downloadFiles` |
| Get file | `GET /files/{fileId}` (Context API) |

---

## 10. Configuration & Deployment

### 10.1 App Configuration Keys

Add Claude Code endpoints to existing Dynamic Sessions configuration:

**Location**: `src/dotnet/Common/Constants/Data/AppConfiguration.json`

```json
{
  "FoundationaLLM:Context:AzureContainerAppsCustomContainerService:Endpoints:ClaudeCode": [
    "https://{session-pool-endpoint}.{region}.azurecontainerapps.io"
  ]
}
```

### 10.2 Key Vault Secrets

| Secret Name | Description | Used By |
|-------------|-------------|---------|
| `AnthropicApiKey` | Anthropic API key for Claude | Container environment variable |
| `GitHubToken` | GitHub PAT for gh CLI (optional) | Container environment variable |

### 10.3 Container Deployment

Deploy the Claude Code sandbox container to Azure Container Apps Dynamic Sessions:

```bicep
// Add to existing session pool configuration or create new pool
resource claudeCodeSessionPool 'Microsoft.App/sessionPools@2024-02-02-preview' = {
  name: 'claude-code-sessions'
  location: location
  properties: {
    environmentId: existingContainerAppsEnv.id
    poolManagementType: 'Dynamic'
    containerType: 'CustomContainer'
    customContainerTemplate: {
      containers: [
        {
          name: 'claude-code-sandbox'
          image: '${containerRegistry}/claude-code-sandbox:${version}'
          resources: {
            cpu: json('1.0')
            memory: '2Gi'
          }
          env: [
            {
              name: 'ANTHROPIC_API_KEY'
              secretRef: 'anthropic-api-key'
            }
            {
              name: 'CLAUDE_MODEL'
              value: 'claude-sonnet-4-20250514'
            }
          ]
        }
      ]
    }
    secrets: [
      {
        name: 'anthropic-api-key'
        keyVaultUrl: '${keyVault.properties.vaultUri}secrets/AnthropicApiKey'
        identity: managedIdentity.id
      }
    ]
    scaleConfiguration: {
      maxConcurrentSessions: 50
      readySessionInstances: 2
    }
    sessionNetworkConfiguration: {
      status: 'EgressEnabled'  // Enable for web search capability
    }
  }
}
```

### 10.4 Code Session Provider Registration

Register the new provider in `CodeSessionService`:

```csharp
// Add to CodeSessionProviderNames.cs
public const string AzureContainerAppsClaudeCode = "AzureContainerAppsClaudeCode";

// Option A: Reuse existing CustomContainerService with different endpoint
// Option B: Create AzureContainerAppsClaudeCodeService extending base class
```

---

## 11. Testing Strategy

### 11.1 Unit Tests

**Python Tool Tests** (`src/python/plugins/agent_claude_code/test/`):

```python
# test_claude_code_tool.py
async def test_tool_builds_correct_request():
    """Verify Claude request payload structure."""
    tool = FoundationaLLMClaudeCodeTool(...)
    request = tool._build_claude_request("Write hello world", "conv-123", ["data.csv"])
    
    payload = json.loads(request)
    assert payload["prompt"] == "Write hello world"
    assert payload["conversation_id"] == "conv-123"
    assert "data.csv" in payload["files"]

async def test_tool_handles_execution_failure():
    """Verify graceful error handling."""
    # Mock Context API to return failure
    ...
```

### 11.2 Integration Tests

| Test Scenario | What It Validates |
|---------------|-------------------|
| Create agent with Claude Code tool | Tool config parsing, session provider lookup |
| Simple prompt execution | Full flow: upload → execute → download |
| Multi-turn conversation | Session persistence, file history |
| File creation and download | FileService integration, security |
| Error recovery | Timeout handling, container crashes |

### 11.3 Container Tests

```python
# Test the sandbox container directly
import httpx

async def test_execute_simple_bash():
    """Test bash execution via Dynamic Sessions API."""
    async with httpx.AsyncClient(base_url="http://localhost:8080") as client:
        response = await client.post(
            "/code/execute",
            params={"identifier": "test-session"},
            json={"code": json.dumps({"prompt": "Run ls -la", "config": {"allowed_capabilities": ["bash"]}})}
        )
        result = response.json()
        assert "detail" in result
        assert result["detail"]["error"] == ""
```

### 11.4 Security Tests

1. **File isolation**: Verify users cannot access other users' files
2. **Session isolation**: Verify sessions are truly isolated
3. **Network isolation**: Verify internet access is blocked when disabled
4. **Secret protection**: Verify API keys are not exposed in responses

---

## 12. Implementation Phases

### Phase 1: Foundation (Weeks 1-2)

- [ ] Create `FoundationaLLMClaudeCodeTool` Python class
- [ ] Create `FoundationaLLMClaudeCodeToolInput` model
- [ ] Add plugin manifest and registration
- [ ] Build basic sandbox container Dockerfile
- [ ] Implement Dynamic Sessions API in container

### Phase 2: Container & Integration (Weeks 3-4)

- [ ] Complete Claude agentic loop in container
- [ ] Add code session provider (or configure existing CustomContainer)
- [ ] Add App Configuration entries
- [ ] Deploy container to Azure Container Apps
- [ ] Integration testing with existing orchestration

### Phase 3: Capabilities & Polish (Weeks 5-6)

- [ ] Add GitHub CLI integration
- [ ] Add web search capability (when internet enabled)
- [ ] Management Portal UI for tool configuration
- [ ] User Portal enhancements for tool output display
- [ ] Documentation and examples

### Phase 4: Testing & Release (Week 7)

- [ ] Comprehensive test suite
- [ ] Performance testing
- [ ] Security audit
- [ ] Documentation finalization
- [ ] Release preparation

---

## 13. Open Questions & Decisions

### 13.1 Technical Decisions

| Question | Options | Recommendation |
|----------|---------|----------------|
| **Provider approach** | New provider vs. Reuse CustomContainer | Reuse with different endpoint |
| **Claude model** | Fixed vs. Configurable | Configurable via tool properties |
| **Max turns** | 10, 20, 50 | 20 default, configurable |
| **Default capabilities** | All vs. Limited | `[bash, file_read, file_write]` |
| **Internet access** | Default on/off | Default off (security) |

### 13.2 Product Decisions

1. **Token tracking**: How to track/report Claude API usage?
2. **Cost allocation**: How does this integrate with cost centers?
3. **Feature gating**: Is this available to all users or gated?

### 13.3 Dependencies

| Dependency | Status | Notes |
|------------|--------|-------|
| Anthropic API access | Required | Need API key provisioning |
| Azure Container Apps Dynamic Sessions | Required | Already used by Code Interpreter |
| Container registry | Required | For custom container image |
| GitHub App (optional) | Optional | For authenticated GitHub operations |

---

## Appendix A: Example Agent Configuration

### A.1 Agent with Claude Code Tool (Recommended)

Using existing workflow with Claude Code as a tool:

```json
{
  "type": "agent",
  "name": "claude-code-assistant",
  "display_name": "Claude Code Assistant",
  "description": "AI-powered software development assistant using Claude Code",
  "workflow": {
    "type": "external-agent-workflow",
    "name": "MAA-Workflow",
    "package_name": "foundationallm_agent_plugins",
    "class_name": "FoundationaLLMFunctionCallingWorkflow",
    "workflow_host": "LangChain",
    "resource_object_ids": {
      "main_model": {
        "object_id": "/instances/{id}/providers/FoundationaLLM.AIModel/aiModels/gpt-4o",
        "properties": {
          "object_role": "main_model",
          "model_parameters": { "temperature": 0.3 }
        }
      }
    }
  },
  "tools": [
    {
      "name": "Claude-Code",
      "description": "Executes complex coding tasks including file operations, bash commands, and code generation using Claude's agentic capabilities.",
      "package_name": "foundationallm_agent_plugins_claude_code",
      "class_name": "FoundationaLLMClaudeCodeTool",
      "properties": {
        "code_session_required": true,
        "code_session_endpoint_provider": "AzureContainerAppsClaudeCode",
        "code_session_language": "ClaudeCode",
        "max_turns": 20,
        "allowed_capabilities": ["bash", "file_read", "file_write", "github"]
      },
      "resource_object_ids": {
        "router_prompt": {
          "object_id": "/instances/{id}/providers/FoundationaLLM.Prompt/prompts/claude-code-router",
          "properties": { "object_role": "router_prompt" }
        }
      }
    }
  ],
  "conversation_history_settings": {
    "enabled": true,
    "max_history": 20
  }
}
```

### A.2 Claude Code Router Prompt Example

```text
Use the Claude-Code tool when the user's request involves any of the following:
- Writing, modifying, or analyzing code
- Running shell commands or scripts
- File system operations (create, read, modify files)
- Software development tasks
- Debugging or troubleshooting code
- Working with Git or GitHub repositories

Do NOT use Claude-Code for:
- Simple questions that don't require code execution
- General knowledge questions
- Tasks that other tools handle better (e.g., knowledge search for documents)
```

---

## Appendix B: Container API Reference

The Claude Code sandbox implements the Azure Container Apps Dynamic Sessions API:

### B.1 File Operations

```
POST /files/upload?identifier={sessionId}
  Body: multipart/form-data with 'file' field
  Response: {"success": true, "filename": "..."}

GET /files?identifier={sessionId}&path={optional_path}
  Response: {"value": [{"name": "...", "type": "file|directory", "size": 123}]}

POST /files/download?identifier={sessionId}
  Body: {"file_name": "..."}
  Response: File stream

POST /files/delete?identifier={sessionId}
  Response: {"success": true}
```

### B.2 Code Execution

```
POST /code/execute?identifier={sessionId}
  Body: {"code": "{JSON payload with prompt and config}"}
  Response: {
    "detail": {
      "output": "stdout content",
      "error": "stderr content",
      "results": "final response from Claude"
    },
    "input_tokens": 1234,
    "output_tokens": 567
  }
```

### B.3 JSON Payload Schema

```json
{
  "prompt": "The user's task description",
  "conversation_id": "conv-123",
  "files": ["file1.py", "data.csv"],
  "config": {
    "max_turns": 20,
    "allowed_capabilities": ["bash", "file_read", "file_write", "github"]
  }
}
```

---

## Appendix C: File Locations Summary

| Component | Location |
|-----------|----------|
| Claude Code Tool | `src/python/plugins/agent_claude_code/pkg/foundationallm_agent_plugins_claude_code/tools/` |
| Tool Input Model | `.../tools/foundationallm_claude_code_tool_input.py` |
| Plugin Manifest | `.../foundationallm_agent_plugins_claude_code/_metadata/foundationallm_manifest.json` |
| Sandbox Container | `src/containers/claude-code-sandbox/` |
| Container API Server | `src/containers/claude-code-sandbox/app/server.py` |
| Dockerfile | `src/containers/claude-code-sandbox/Dockerfile` |

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-09 | AI Assistant | Initial draft |
| 1.1 | 2026-01-09 | AI Assistant | Revised based on Code Interpreter analysis - aligned with existing patterns, removed custom API server approach, added existing infrastructure analysis |
