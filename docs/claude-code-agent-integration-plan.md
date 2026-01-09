# Claude Code Agent Integration Plan for FoundationaLLM

## Executive Summary

This document outlines a comprehensive plan to integrate Claude Code agent capabilities into FoundationaLLM. The integration will allow users to access the full power of Claude Code through the familiar FoundationaLLM UI and APIs, with FoundationaLLM serving as the operating system for this AI agent.

The core approach leverages FoundationaLLM's existing Dynamic Sessions Custom Containers infrastructure to provide a secure, sandboxed Ubuntu Linux environment where the Claude Agent SDK runs in service of user requests.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Component Design](#2-component-design)
3. [Workflow Implementation](#3-workflow-implementation)
4. [Sandbox Container Infrastructure](#4-sandbox-container-infrastructure)
5. [Claude Agent SDK Integration](#5-claude-agent-sdk-integration)
6. [File Storage & Management](#6-file-storage--management)
7. [Conversation History & Multi-Turn Support](#7-conversation-history--multi-turn-support)
8. [Security & Permissions](#8-security--permissions)
9. [UI Integration](#9-ui-integration)
10. [API Integration](#10-api-integration)
11. [Configuration & Deployment](#11-configuration--deployment)
12. [Testing Strategy](#12-testing-strategy)
13. [Implementation Phases](#13-implementation-phases)
14. [Open Questions & Decisions](#14-open-questions--decisions)

---

## 1. Architecture Overview

### 1.1 High-Level Architecture

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
│  │  │                    Agent Orchestration                           │   ││
│  │  │  ┌─────────────┐  ┌─────────────┐  ┌────────────────────────┐   │   ││
│  │  │  │ LangChain   │  │ OpenAI      │  │  Claude Code Agent     │   │   ││
│  │  │  │ Workflow    │  │ Assistants  │  │  Workflow (NEW)        │   │   ││
│  │  │  └─────────────┘  └─────────────┘  └────────────────────────┘   │   ││
│  │  └──────────────────────────────────────────────────────────────────┘   ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │              Azure Container Apps Dynamic Sessions                      ││
│  │  ┌────────────────────────────────────────────────────────────────────┐ ││
│  │  │         Claude Code Agent Sandbox Container (Ubuntu Linux)         │ ││
│  │  │  ┌──────────────┐  ┌────────────────┐  ┌─────────────────────────┐ │ ││
│  │  │  │ Claude Agent │  │ File System    │  │ Development Tools       │ │ ││
│  │  │  │ SDK (Python) │  │ (git, bash)    │  │ (Python, Node, etc.)    │ │ ││
│  │  │  └──────────────┘  └────────────────┘  └─────────────────────────┘ │ ││
│  │  │  ┌──────────────┐  ┌────────────────┐  ┌─────────────────────────┐ │ ││
│  │  │  │ Web Search   │  │ GitHub CLI     │  │ Code Execution          │ │ ││
│  │  │  │ (Optional)   │  │ Integration    │  │ Environment             │ │ ││
│  │  │  └──────────────┘  └────────────────┘  └─────────────────────────┘ │ ││
│  │  └────────────────────────────────────────────────────────────────────┘ ││
│  └─────────────────────────────────────────────────────────────────────────┘│
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐│
│  │                    Azure Blob Storage                                   ││
│  │           (Per-user file persistence with RBAC)                         ││
│  └─────────────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Key Design Principles

1. **Leverage Existing Infrastructure**: Build on top of the existing Dynamic Sessions Custom Container infrastructure
2. **Maintain Security Model**: All file access and permissions flow through FoundationaLLM's security and RBAC system
3. **Seamless User Experience**: Users select Claude Code agent like any other agent from the dropdown
4. **Multi-Turn Conversation Support**: Conversation history persisted in FoundationaLLM and passed to Claude agent
5. **Persistent File Storage**: Files generated by Claude Code persist in Azure Blob Storage, secured per-user
6. **Configurable Internet Access**: Sandbox can be configured to allow/disallow outbound internet access

---

## 2. Component Design

### 2.1 New Agent Workflow Type

Create a new workflow type: `ClaudeCodeAgentWorkflow`

**Location**: `src/dotnet/Common/Models/ResourceProviders/Agent/AgentWorkflows/`

```csharp
// ClaudeCodeAgentWorkflow.cs
namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows
{
    /// <summary>
    /// Provides an agent workflow configuration for Claude Code Agent.
    /// </summary>
    public class ClaudeCodeAgentWorkflow : AgentWorkflowBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Type => AgentWorkflowTypes.ClaudeCodeAgent;

        /// <summary>
        /// Whether to allow outbound internet access from the sandbox.
        /// </summary>
        [JsonPropertyName("allow_internet_access")]
        public bool AllowInternetAccess { get; set; } = false;

        /// <summary>
        /// Maximum execution time for Claude Code operations (in seconds).
        /// </summary>
        [JsonPropertyName("max_execution_time_seconds")]
        public int MaxExecutionTimeSeconds { get; set; } = 300;

        /// <summary>
        /// GitHub authentication configuration for code operations.
        /// </summary>
        [JsonPropertyName("github_config")]
        public ClaudeCodeGitHubConfig? GitHubConfig { get; set; }

        /// <summary>
        /// Memory limit for the sandbox container (in MB).
        /// </summary>
        [JsonPropertyName("memory_limit_mb")]
        public int MemoryLimitMb { get; set; } = 2048;

        /// <summary>
        /// List of allowed tools for this workflow.
        /// </summary>
        [JsonPropertyName("allowed_tools")]
        public List<string> AllowedTools { get; set; } = new()
        {
            "read_file",
            "write_file",
            "bash",
            "search",
            "github"
        };
    }

    public class ClaudeCodeGitHubConfig
    {
        /// <summary>
        /// Resource object ID for GitHub credentials in Key Vault.
        /// </summary>
        [JsonPropertyName("credentials_object_id")]
        public string? CredentialsObjectId { get; set; }

        /// <summary>
        /// Whether to enable PR creation capabilities.
        /// </summary>
        [JsonPropertyName("enable_pr_creation")]
        public bool EnablePRCreation { get; set; } = false;
    }
}
```

### 2.2 Update AgentWorkflowTypes

**Location**: `src/dotnet/Common/Models/ResourceProviders/Agent/AgentWorkflows/AgentWorkflowTypes.cs`

```csharp
/// <summary>
/// The Claude Code Agent workflow.
/// </summary>
public const string ClaudeCodeAgent = "claude-code-agent-workflow";
```

### 2.3 New Service: Claude Code Agent Service

**Location**: `src/dotnet/ContextEngine/Services/ClaudeCodeAgentService.cs`

This service manages the lifecycle of Claude Code agent sessions:

```csharp
public interface IClaudeCodeAgentService
{
    Task<ClaudeCodeSession> CreateSessionAsync(
        string instanceId,
        string conversationId,
        string userId,
        ClaudeCodeAgentWorkflow workflow,
        UnifiedUserIdentity userIdentity);
    
    Task<ClaudeCodeResponse> ExecuteAsync(
        string sessionId,
        string userPrompt,
        List<MessageHistoryItem> messageHistory,
        List<FileHistoryItem> fileHistory);
    
    Task<Stream?> DownloadFileAsync(
        string sessionId,
        string filePath);
    
    Task TerminateSessionAsync(string sessionId);
}
```

### 2.4 Python Plugin: Claude Code Workflow

**Location**: `src/python/plugins/agent_claude_code/`

Create a new Python plugin package for the Claude Code workflow:

```
src/python/plugins/agent_claude_code/
├── pkg/
│   ├── foundationallm_agent_plugins_claude_code/
│   │   ├── __init__.py
│   │   ├── _metadata/
│   │   │   └── foundationallm_manifest.json
│   │   ├── workflow_plugin_manager.py
│   │   └── workflows/
│   │       ├── __init__.py
│   │       └── claude_code_agent_workflow.py
│   ├── pyproject.toml
│   └── README.md
├── requirements.txt
└── test/
```

---

## 3. Workflow Implementation

### 3.1 Claude Code Agent Workflow (Python)

```python
# claude_code_agent_workflow.py
"""
Class: ClaudeCodeAgentWorkflow
Description: FoundationaLLM workflow that wraps the Claude Agent SDK
             running in a secure sandbox container.
"""

from dataclasses import dataclass
from typing import Dict, List, Optional
import json
import httpx

from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.orchestration import (
    CompletionResponse,
    ContentArtifact,
    OpenAITextMessageContentItem
)
from foundationallm.operations import OperationsManager


class ClaudeCodeAgentWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow that orchestrates Claude Code agent 
    operations within a secure sandbox container.
    """

    def __init__(
        self,
        workflow_config,
        objects: Dict,
        tools: List,
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration
    ):
        super().__init__(
            workflow_config, objects, tools, 
            operations_manager, user_identity, config
        )
        self.sandbox_endpoint = self._get_sandbox_endpoint()
        self.allow_internet = workflow_config.properties.get(
            'allow_internet_access', False
        )
        self.max_execution_time = workflow_config.properties.get(
            'max_execution_time_seconds', 300
        )

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
        Invoke the Claude Code agent workflow.
        """
        # 1. Prepare the request for the sandbox
        sandbox_request = self._prepare_sandbox_request(
            operation_id=operation_id,
            user_prompt=user_prompt_rewrite or user_prompt,
            message_history=message_history,
            file_history=file_history,
            conversation_id=conversation_id
        )

        # 2. Execute in sandbox container
        async with httpx.AsyncClient() as client:
            response = await client.post(
                f"{self.sandbox_endpoint}/execute",
                json=sandbox_request,
                timeout=self.max_execution_time
            )
            result = response.json()

        # 3. Process response and extract artifacts
        content_artifacts = self._extract_artifacts(result)
        
        # 4. Handle file outputs
        file_outputs = await self._process_file_outputs(
            result.get('files', []),
            conversation_id
        )
        content_artifacts.extend(file_outputs)

        # 5. Build completion response
        return CompletionResponse(
            operation_id=operation_id,
            content=[OpenAITextMessageContentItem(
                value=result.get('response', ''),
                agent_capability_category='ClaudeCodeAgent'
            )],
            content_artifacts=content_artifacts,
            user_prompt=user_prompt,
            completion_tokens=result.get('tokens', {}).get('output', 0),
            prompt_tokens=result.get('tokens', {}).get('input', 0)
        )

    def _prepare_sandbox_request(
        self,
        operation_id: str,
        user_prompt: str,
        message_history: List,
        file_history: List,
        conversation_id: str
    ) -> dict:
        """Prepare the request payload for the sandbox container."""
        return {
            'operation_id': operation_id,
            'conversation_id': conversation_id,
            'user_prompt': user_prompt,
            'message_history': [
                {'role': m.sender.lower(), 'content': m.text}
                for m in message_history
            ],
            'files': [
                {'name': f.file_name, 'path': f.file_path}
                for f in file_history
            ],
            'config': {
                'allow_internet': self.allow_internet,
                'max_execution_time': self.max_execution_time,
                'tools': self.workflow_config.properties.get('allowed_tools', [])
            }
        }
```

---

## 4. Sandbox Container Infrastructure

### 4.1 Custom Container Image

Create a new Docker image for the Claude Code agent sandbox:

**Location**: `src/python/ClaudeCodeSandbox/`

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
RUN curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
    && apt-get update \
    && apt-get install -y gh

# Create sandbox user
RUN useradd -m -s /bin/bash sandbox
WORKDIR /home/sandbox

# Install Claude Agent SDK
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt

# Copy sandbox orchestrator
COPY sandbox_orchestrator/ /app/
WORKDIR /app

# Expose API port
EXPOSE 8080

# Run as non-root user
USER sandbox

CMD ["python3", "main.py"]
```

### 4.2 Sandbox Orchestrator

The sandbox orchestrator manages Claude SDK execution within the container:

```python
# sandbox_orchestrator/main.py
"""
Claude Code Sandbox Orchestrator
Manages Claude SDK execution within the secure container environment.
"""

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Dict, Optional
import anthropic
import asyncio
import os

app = FastAPI(title="Claude Code Sandbox")

class ExecuteRequest(BaseModel):
    operation_id: str
    conversation_id: str
    user_prompt: str
    message_history: List[Dict[str, str]]
    files: List[Dict[str, str]]
    config: Dict

class ExecuteResponse(BaseModel):
    response: str
    files: List[Dict[str, str]]
    tokens: Dict[str, int]
    tool_calls: List[Dict]

@app.post("/execute", response_model=ExecuteResponse)
async def execute(request: ExecuteRequest):
    """Execute a Claude Code agent request."""
    try:
        # Initialize Claude client
        client = anthropic.Anthropic(
            api_key=os.environ.get("ANTHROPIC_API_KEY")
        )
        
        # Build conversation history
        messages = _build_messages(
            request.message_history,
            request.user_prompt
        )
        
        # Configure tools based on request config
        tools = _configure_tools(request.config)
        
        # Execute Claude agent loop
        result = await _run_agent_loop(
            client=client,
            messages=messages,
            tools=tools,
            files=request.files,
            config=request.config
        )
        
        return ExecuteResponse(
            response=result['response'],
            files=result['generated_files'],
            tokens=result['tokens'],
            tool_calls=result['tool_calls']
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

async def _run_agent_loop(
    client,
    messages: List,
    tools: List,
    files: List,
    config: Dict
) -> Dict:
    """Run the Claude agent agentic loop with tool use."""
    generated_files = []
    tool_calls = []
    total_input_tokens = 0
    total_output_tokens = 0
    
    while True:
        response = client.messages.create(
            model="claude-sonnet-4-20250514",  # or configured model
            max_tokens=8192,
            system=_get_system_prompt(config),
            messages=messages,
            tools=tools
        )
        
        total_input_tokens += response.usage.input_tokens
        total_output_tokens += response.usage.output_tokens
        
        # Check if we need to execute tools
        if response.stop_reason == "tool_use":
            for block in response.content:
                if block.type == "tool_use":
                    tool_result = await _execute_tool(
                        block.name,
                        block.input,
                        files,
                        config
                    )
                    tool_calls.append({
                        'name': block.name,
                        'input': block.input,
                        'result': tool_result
                    })
                    
                    # Track generated files
                    if block.name in ['write_file', 'bash'] and tool_result.get('files'):
                        generated_files.extend(tool_result['files'])
                    
                    # Add tool result to messages
                    messages.append({
                        'role': 'assistant',
                        'content': response.content
                    })
                    messages.append({
                        'role': 'user',
                        'content': [{
                            'type': 'tool_result',
                            'tool_use_id': block.id,
                            'content': tool_result['output']
                        }]
                    })
        else:
            # Final response
            final_response = ""
            for block in response.content:
                if hasattr(block, 'text'):
                    final_response += block.text
            
            return {
                'response': final_response,
                'generated_files': generated_files,
                'tokens': {
                    'input': total_input_tokens,
                    'output': total_output_tokens
                },
                'tool_calls': tool_calls
            }

def _configure_tools(config: Dict) -> List:
    """Configure available tools based on request config."""
    all_tools = {
        'read_file': {
            'name': 'read_file',
            'description': 'Read the contents of a file at the specified path.',
            'input_schema': {
                'type': 'object',
                'properties': {
                    'path': {'type': 'string', 'description': 'File path to read'}
                },
                'required': ['path']
            }
        },
        'write_file': {
            'name': 'write_file',
            'description': 'Write content to a file at the specified path.',
            'input_schema': {
                'type': 'object',
                'properties': {
                    'path': {'type': 'string', 'description': 'File path to write'},
                    'content': {'type': 'string', 'description': 'Content to write'}
                },
                'required': ['path', 'content']
            }
        },
        'bash': {
            'name': 'bash',
            'description': 'Execute a bash command in the sandbox environment.',
            'input_schema': {
                'type': 'object',
                'properties': {
                    'command': {'type': 'string', 'description': 'Bash command to execute'}
                },
                'required': ['command']
            }
        },
        'search': {
            'name': 'web_search',
            'description': 'Search the web for information.',
            'input_schema': {
                'type': 'object',
                'properties': {
                    'query': {'type': 'string', 'description': 'Search query'}
                },
                'required': ['query']
            }
        },
        'github': {
            'name': 'github',
            'description': 'Interact with GitHub repositories.',
            'input_schema': {
                'type': 'object',
                'properties': {
                    'action': {'type': 'string', 'enum': ['clone', 'commit', 'push', 'create_pr']},
                    'repo': {'type': 'string'},
                    'params': {'type': 'object'}
                },
                'required': ['action']
            }
        }
    }
    
    allowed = config.get('tools', list(all_tools.keys()))
    return [all_tools[t] for t in allowed if t in all_tools]
```

### 4.3 Container Configuration

**Location**: `src/dotnet/ContextEngine/Models/Configuration/ClaudeCodeContainerServiceSettings.cs`

```csharp
namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Settings for the Claude Code Agent container service.
    /// </summary>
    public class ClaudeCodeContainerServiceSettings : AzureContainerAppsServiceSettings
    {
        /// <summary>
        /// Whether containers should allow outbound internet access by default.
        /// </summary>
        public bool DefaultAllowInternetAccess { get; set; } = false;

        /// <summary>
        /// Default memory limit for containers in MB.
        /// </summary>
        public int DefaultMemoryLimitMb { get; set; } = 2048;

        /// <summary>
        /// Default CPU allocation for containers.
        /// </summary>
        public double DefaultCpuCores { get; set; } = 1.0;

        /// <summary>
        /// Maximum execution time before container is terminated (seconds).
        /// </summary>
        public int MaxExecutionTimeSeconds { get; set; } = 600;

        /// <summary>
        /// Container image name for Claude Code sandbox.
        /// </summary>
        public string ContainerImage { get; set; } = "foundationallm/claude-code-sandbox:latest";
    }
}
```

---

## 5. Claude Agent SDK Integration

### 5.1 SDK Configuration

The Claude Agent SDK will be integrated within the sandbox container with the following capabilities:

| Capability | Description | Configuration |
|------------|-------------|---------------|
| **Read Files** | Read file contents from workspace | Always enabled |
| **Write Files** | Create/modify files in workspace | Always enabled |
| **Bash Commands** | Execute shell commands | Always enabled |
| **Code Execution** | Run Python/Node/etc. code | Always enabled |
| **Web Search** | Search the internet | Requires `allow_internet_access: true` |
| **GitHub Operations** | Clone, commit, push, create PRs | Requires GitHub config |

### 5.2 Tool Implementation

Each Claude Code tool maps to sandbox operations:

```python
# sandbox_orchestrator/tools.py

async def execute_read_file(params: dict) -> dict:
    """Read a file from the sandbox filesystem."""
    path = params['path']
    try:
        with open(f"/workspace/{path}", 'r') as f:
            content = f.read()
        return {'output': content, 'success': True}
    except Exception as e:
        return {'output': str(e), 'success': False}

async def execute_write_file(params: dict) -> dict:
    """Write a file to the sandbox filesystem."""
    path = params['path']
    content = params['content']
    try:
        full_path = f"/workspace/{path}"
        os.makedirs(os.path.dirname(full_path), exist_ok=True)
        with open(full_path, 'w') as f:
            f.write(content)
        return {
            'output': f'Successfully wrote to {path}',
            'success': True,
            'files': [{'path': path, 'size': len(content)}]
        }
    except Exception as e:
        return {'output': str(e), 'success': False}

async def execute_bash(params: dict, config: dict) -> dict:
    """Execute a bash command in the sandbox."""
    command = params['command']
    timeout = config.get('max_execution_time', 300)
    
    # Security: Block dangerous commands if internet disabled
    if not config.get('allow_internet', False):
        blocked = ['curl', 'wget', 'nc', 'netcat']
        if any(cmd in command for cmd in blocked):
            return {
                'output': 'Network commands are disabled for this agent.',
                'success': False
            }
    
    try:
        result = await asyncio.create_subprocess_shell(
            command,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE,
            cwd='/workspace'
        )
        stdout, stderr = await asyncio.wait_for(
            result.communicate(),
            timeout=timeout
        )
        
        return {
            'output': stdout.decode() + stderr.decode(),
            'success': result.returncode == 0,
            'return_code': result.returncode
        }
    except asyncio.TimeoutError:
        return {'output': 'Command timed out', 'success': False}
    except Exception as e:
        return {'output': str(e), 'success': False}

async def execute_github(params: dict, config: dict) -> dict:
    """Execute GitHub operations using gh CLI."""
    action = params['action']
    
    if action == 'clone':
        repo = params['repo']
        cmd = f"gh repo clone {repo} /workspace/repo"
    elif action == 'create_pr':
        title = params['params'].get('title', 'PR from Claude Code')
        body = params['params'].get('body', '')
        cmd = f'gh pr create --title "{title}" --body "{body}"'
    else:
        return {'output': f'Unknown action: {action}', 'success': False}
    
    return await execute_bash({'command': cmd}, config)
```

---

## 6. File Storage & Management

### 6.1 File Persistence Strategy

Files created by Claude Code agent are persisted to Azure Blob Storage with the following structure:

```
{storage-account}/
└── claude-code-files/
    └── {instance-id}/
        └── {user-id}/
            └── {conversation-id}/
                ├── workspace/
                │   ├── file1.py
                │   ├── file2.txt
                │   └── subdirectory/
                │       └── file3.json
                └── metadata.json
```

### 6.2 File Service Integration

Extend the existing `FileService` to handle Claude Code files:

**Location**: `src/dotnet/ContextEngine/Services/ClaudeCodeFileService.cs`

```csharp
public interface IClaudeCodeFileService
{
    /// <summary>
    /// Persist files from a Claude Code session to blob storage.
    /// </summary>
    Task<List<ClaudeCodeFileRecord>> PersistSessionFilesAsync(
        string instanceId,
        string userId,
        string conversationId,
        string sessionId,
        List<string> filePaths);
    
    /// <summary>
    /// Restore files from blob storage to a new Claude Code session.
    /// </summary>
    Task RestoreSessionFilesAsync(
        string instanceId,
        string userId,
        string conversationId,
        string sessionId);
    
    /// <summary>
    /// Get download URL for a specific file.
    /// </summary>
    Task<string> GetFileDownloadUrlAsync(
        string instanceId,
        string userId,
        string conversationId,
        string filePath);
    
    /// <summary>
    /// Delete all files for a conversation.
    /// </summary>
    Task DeleteConversationFilesAsync(
        string instanceId,
        string userId,
        string conversationId);
}
```

### 6.3 File Access Security

Files are secured using FoundationaLLM's existing security model:

1. **User Isolation**: Each user can only access their own files
2. **Conversation Scope**: Files are scoped to specific conversations
3. **RBAC Integration**: Admin users can access all files for audit purposes
4. **SAS Token Generation**: Secure, time-limited access URLs for file downloads

---

## 7. Conversation History & Multi-Turn Support

### 7.1 Conversation Persistence

Claude Code agent conversations use the existing FoundationaLLM conversation storage:

```csharp
// Extended MessageHistoryItem for Claude Code
public class ClaudeCodeMessageHistoryItem : MessageHistoryItem
{
    /// <summary>
    /// Tool calls made during this message.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ClaudeCodeToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Files created/modified during this message.
    /// </summary>
    [JsonPropertyName("files_modified")]
    public List<string>? FilesModified { get; set; }
}

public class ClaudeCodeToolCall
{
    [JsonPropertyName("tool_name")]
    public string ToolName { get; set; }

    [JsonPropertyName("input")]
    public Dictionary<string, object> Input { get; set; }

    [JsonPropertyName("output")]
    public string Output { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
```

### 7.2 Context Management

The workflow manages conversation context:

1. **Message History**: All previous messages passed to Claude
2. **File History**: List of files in the workspace with their current state
3. **Tool History**: Previous tool calls and their results (summarized)
4. **Workspace State**: Current state of the workspace directory

### 7.3 Context Window Management

For long conversations, implement context windowing:

```python
def _prepare_context(
    self,
    message_history: List[MessageHistoryItem],
    max_messages: int = 20,
    max_tokens: int = 100000
) -> List[Dict]:
    """Prepare conversation context within token limits."""
    
    # Always include system context
    context = [self._get_workspace_context()]
    
    # Add recent messages, respecting token limits
    recent_messages = message_history[-max_messages:]
    
    # Summarize older messages if needed
    if len(message_history) > max_messages:
        summary = self._summarize_older_messages(
            message_history[:-max_messages]
        )
        context.insert(0, {'role': 'system', 'content': summary})
    
    # Add recent messages
    for msg in recent_messages:
        context.append({
            'role': msg.sender.lower(),
            'content': msg.text
        })
    
    return context
```

---

## 8. Security & Permissions

### 8.1 Security Model

| Layer | Security Measure |
|-------|------------------|
| **Authentication** | FoundationaLLM user authentication (Entra ID) |
| **Authorization** | RBAC for agent access, file access |
| **Container Isolation** | Each session runs in isolated container |
| **Network Isolation** | Optional internet access per agent config |
| **File Isolation** | Per-user, per-conversation file storage |
| **Secrets Management** | API keys stored in Key Vault |

### 8.2 Agent Permissions

New permissions for Claude Code agents:

```csharp
public static class ClaudeCodePermissions
{
    /// <summary>
    /// Permission to use Claude Code agents.
    /// </summary>
    public const string UseClaudeCodeAgent = "ClaudeCode.Use";

    /// <summary>
    /// Permission to configure internet access.
    /// </summary>
    public const string ConfigureInternetAccess = "ClaudeCode.ConfigureInternet";

    /// <summary>
    /// Permission to configure GitHub integration.
    /// </summary>
    public const string ConfigureGitHub = "ClaudeCode.ConfigureGitHub";

    /// <summary>
    /// Permission to access files from all users (admin).
    /// </summary>
    public const string AccessAllFiles = "ClaudeCode.AccessAllFiles";
}
```

### 8.3 Sandbox Security

Container security configuration:

```yaml
# Azure Container Apps configuration
properties:
  configuration:
    secrets:
      - name: anthropic-api-key
        keyVaultUrl: https://{keyvault}.vault.azure.net/secrets/AnthropicApiKey
    activeRevisionsMode: Single
    ingress:
      external: false  # Internal only
      targetPort: 8080
  template:
    containers:
      - name: claude-code-sandbox
        image: foundationallm/claude-code-sandbox:latest
        resources:
          cpu: 1.0
          memory: 2Gi
        securityContext:
          runAsNonRoot: true
          readOnlyRootFilesystem: false  # Needed for file operations
          capabilities:
            drop:
              - ALL
    scale:
      minReplicas: 0
      maxReplicas: 10
```

---

## 9. UI Integration

### 9.1 User Portal Changes

Update the User Portal to support Claude Code agents:

**File**: `src/ui/UserPortal/components/ChatMessage.vue`

Add rendering for Claude Code specific content:

```vue
<template>
  <!-- Existing message rendering -->
  
  <!-- Claude Code specific content -->
  <div v-if="isClaudeCodeMessage" class="claude-code-content">
    <!-- Tool calls accordion -->
    <Accordion v-if="message.toolCalls?.length">
      <AccordionTab header="Tool Calls">
        <div v-for="call in message.toolCalls" :key="call.id" class="tool-call">
          <Tag :value="call.toolName" :severity="call.success ? 'success' : 'danger'" />
          <pre v-if="call.output">{{ call.output }}</pre>
        </div>
      </AccordionTab>
    </Accordion>
    
    <!-- Generated files -->
    <div v-if="message.filesModified?.length" class="files-section">
      <h4>Files Modified</h4>
      <ul>
        <li v-for="file in message.filesModified" :key="file">
          <a @click="downloadFile(file)">{{ file }}</a>
        </li>
      </ul>
    </div>
  </div>
</template>
```

### 9.2 Management Portal Changes

Update agent creation to support Claude Code workflow:

**File**: `src/ui/ManagementPortal/pages/agents/edit/[agentName].vue`

Add Claude Code specific configuration options:

- Toggle for internet access
- Memory limit slider
- Tool selection checkboxes
- GitHub integration configuration
- Anthropic model selection

### 9.3 New Components

Create new UI components:

1. **ClaudeCodeFileExplorer.vue**: Browse and manage workspace files
2. **ClaudeCodeToolCallViewer.vue**: Display tool call details
3. **ClaudeCodeConfigPanel.vue**: Agent configuration panel

---

## 10. API Integration

### 10.1 New API Endpoints

**Core API Extensions**:

```
POST   /instances/{instanceId}/claude-code/sessions
       Create a new Claude Code session

GET    /instances/{instanceId}/claude-code/sessions/{sessionId}
       Get session status and details

DELETE /instances/{instanceId}/claude-code/sessions/{sessionId}
       Terminate a session

GET    /instances/{instanceId}/claude-code/sessions/{sessionId}/files
       List files in session workspace

GET    /instances/{instanceId}/claude-code/sessions/{sessionId}/files/{path}
       Download a specific file

POST   /instances/{instanceId}/claude-code/sessions/{sessionId}/files
       Upload a file to session workspace
```

### 10.2 Completion Request Extension

Extend the completion request to support Claude Code specific options:

```csharp
public class ClaudeCodeCompletionRequest : CompletionRequest
{
    /// <summary>
    /// Whether to restore previous workspace state.
    /// </summary>
    [JsonPropertyName("restore_workspace")]
    public bool RestoreWorkspace { get; set; } = true;

    /// <summary>
    /// Additional context files to include in the session.
    /// </summary>
    [JsonPropertyName("context_files")]
    public List<string>? ContextFiles { get; set; }

    /// <summary>
    /// Override allowed tools for this request.
    /// </summary>
    [JsonPropertyName("allowed_tools")]
    public List<string>? AllowedTools { get; set; }
}
```

### 10.3 Completion Response Extension

Extend the completion response:

```csharp
public class ClaudeCodeCompletionResponse : CompletionResponse
{
    /// <summary>
    /// Tool calls made during this completion.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public List<ClaudeCodeToolCall>? ToolCalls { get; set; }

    /// <summary>
    /// Files created or modified.
    /// </summary>
    [JsonPropertyName("files_modified")]
    public List<ClaudeCodeFileInfo>? FilesModified { get; set; }

    /// <summary>
    /// Session ID for subsequent requests.
    /// </summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }
}
```

---

## 11. Configuration & Deployment

### 11.1 App Configuration Keys

Add new configuration keys:

```json
{
  "FoundationaLLM:ClaudeCode:ContainerEndpoint": "https://{container-apps-env}.{region}.azurecontainerapps.io",
  "FoundationaLLM:ClaudeCode:DefaultMemoryLimitMb": "2048",
  "FoundationaLLM:ClaudeCode:DefaultCpuCores": "1.0",
  "FoundationaLLM:ClaudeCode:MaxExecutionTimeSeconds": "600",
  "FoundationaLLM:ClaudeCode:DefaultAllowInternetAccess": "false",
  "FoundationaLLM:ClaudeCode:ContainerImage": "foundationallm/claude-code-sandbox:latest"
}
```

### 11.2 Key Vault Secrets

Required secrets:

| Secret Name | Description |
|-------------|-------------|
| `AnthropicApiKey` | Anthropic API key for Claude |
| `GitHubAppPrivateKey` | GitHub App private key (optional) |
| `GitHubAppId` | GitHub App ID (optional) |

### 11.3 Infrastructure Requirements

```bicep
// Azure Container Apps Environment for Claude Code
resource claudeCodeContainerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'claude-code-env'
  location: location
  properties: {
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

// Session Pool for Claude Code containers
resource claudeCodeSessionPool 'Microsoft.App/sessionPools@2024-02-02-preview' = {
  name: 'claude-code-sessions'
  location: location
  properties: {
    environmentId: claudeCodeContainerAppsEnv.id
    poolManagementType: 'Dynamic'
    containerType: 'CustomContainer'
    customContainerTemplate: {
      containers: [
        {
          name: 'claude-code-sandbox'
          image: 'foundationallm/claude-code-sandbox:latest'
          resources: {
            cpu: 1
            memory: '2Gi'
          }
        }
      ]
    }
    scaleConfiguration: {
      maxConcurrentSessions: 100
      readySessionInstances: 5
    }
    sessionNetworkConfiguration: {
      status: 'EgressEnabled'  // For internet access
    }
  }
}
```

---

## 12. Testing Strategy

### 12.1 Unit Tests

| Component | Test Focus |
|-----------|------------|
| `ClaudeCodeAgentWorkflow` | Request preparation, response parsing |
| `ClaudeCodeFileService` | File persistence, retrieval, deletion |
| `SandboxOrchestrator` | Tool execution, error handling |
| `SecurityValidation` | Permission checks, isolation |

### 12.2 Integration Tests

1. **End-to-End Flow**: Create agent → Send message → Receive response
2. **File Operations**: Create files → Persist → Restore → Download
3. **Multi-Turn Conversations**: Context preservation across turns
4. **Tool Execution**: Each tool type individually and in combination
5. **Error Handling**: Timeout, OOM, permission denied scenarios

### 12.3 Performance Tests

| Test | Target |
|------|--------|
| Session creation | < 5 seconds |
| Simple completion | < 30 seconds |
| File operations | < 2 seconds |
| Container scaling | Handle 100 concurrent sessions |

### 12.4 Security Tests

1. **Container escape attempts**
2. **Unauthorized file access**
3. **Network isolation verification**
4. **API key exposure prevention**

---

## 13. Implementation Phases

### Phase 1: Foundation (Weeks 1-3)

- [ ] Create `ClaudeCodeAgentWorkflow` model classes
- [ ] Update `AgentWorkflowTypes` and `AgentWorkflowBase`
- [ ] Create sandbox container Dockerfile
- [ ] Implement basic sandbox orchestrator
- [ ] Add configuration settings

### Phase 2: Core Functionality (Weeks 4-6)

- [ ] Implement Python workflow plugin
- [ ] Integrate with existing orchestration service
- [ ] Implement file persistence service
- [ ] Add tool implementations (read/write/bash)
- [ ] Basic conversation history support

### Phase 3: Advanced Features (Weeks 7-9)

- [ ] GitHub integration
- [ ] Web search capability (when internet enabled)
- [ ] Enhanced context management
- [ ] File explorer UI component
- [ ] Tool call visualization

### Phase 4: Security & Polish (Weeks 10-11)

- [ ] Security hardening
- [ ] Permission model implementation
- [ ] Error handling improvements
- [ ] Documentation
- [ ] Performance optimization

### Phase 5: Testing & Release (Week 12)

- [ ] Comprehensive testing
- [ ] Bug fixes
- [ ] Documentation finalization
- [ ] Release preparation

---

## 14. Open Questions & Decisions

### 14.1 Technical Decisions Needed

| Question | Options | Recommendation |
|----------|---------|----------------|
| **Session persistence** | Per-request vs. Long-lived | Long-lived (within conversation) |
| **File size limits** | 10MB, 50MB, 100MB | 50MB per file, 500MB per conversation |
| **Container timeout** | 5min, 10min, 30min | 10min default, configurable |
| **Default internet access** | Enabled, Disabled | Disabled (opt-in) |
| **Model selection** | Fixed, Configurable | Configurable per agent |

### 14.2 Product Decisions Needed

1. **Pricing model**: How to charge for Claude Code agent usage?
2. **Rate limiting**: What limits to apply to container usage?
3. **Feature flags**: Which features are available at which tier?
4. **Audit logging**: What level of detail for Claude Code operations?

### 14.3 Dependencies

1. **Anthropic API**: Access to Claude API with agent capabilities
2. **Azure Container Apps**: Dynamic Sessions feature availability
3. **Container Registry**: Storage for custom sandbox image

---

## Appendix A: Claude Agent SDK Reference

The Claude Agent SDK provides the following tool types:

| Tool | Description |
|------|-------------|
| `computer` | Screen interaction (not applicable for server) |
| `text_editor` | File editing with view/create/replace |
| `bash` | Shell command execution |
| `web_search` | Internet search capability |

For FoundationaLLM integration, we'll implement equivalent functionality:

- `text_editor` → `read_file` + `write_file`
- `bash` → `bash` (with restrictions)
- `web_search` → `web_search` (when internet enabled)

---

## Appendix B: Example Agent Configuration

```json
{
  "type": "agent",
  "name": "claude-code-developer",
  "display_name": "Claude Code Developer",
  "description": "AI-powered software development assistant",
  "workflow": {
    "type": "claude-code-agent-workflow",
    "name": "Claude-Code-Workflow",
    "package_name": "foundationallm_agent_plugins_claude_code",
    "class_name": "ClaudeCodeAgentWorkflow",
    "workflow_host": "LangChain",
    "properties": {
      "allow_internet_access": true,
      "max_execution_time_seconds": 600,
      "memory_limit_mb": 4096,
      "allowed_tools": ["read_file", "write_file", "bash", "search", "github"]
    },
    "resource_object_ids": {
      "main_model": {
        "object_id": "/instances/{id}/providers/FoundationaLLM.AIModel/aiModels/claude-sonnet",
        "properties": {
          "model_parameters": {
            "temperature": 0.3,
            "max_tokens": 8192
          }
        }
      }
    }
  },
  "conversation_history_settings": {
    "enabled": true,
    "max_history": 50
  },
  "portal_display_options": {
    "show_file_explorer": true,
    "show_tool_calls": true,
    "allow_file_upload": true,
    "allow_file_download": true
  }
}
```

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-09 | AI Assistant | Initial draft |
