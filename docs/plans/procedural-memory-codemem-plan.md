# Procedural Memory for Agents via CodeMem Architecture

## Executive Summary

This document outlines a plan to add procedural memory capabilities to FoundationaLLM agents, inspired by the [CodeMem paper](https://arxiv.org/html/2512.15813v1). The CodeMem approach enables agents to save successful code executions as reusable "skills" that can be retrieved and executed deterministically, solving the probabilistic instability inherent in LLM-based agents.

**Design Goal:** Minimize platform changes by leveraging the existing code interpreter infrastructure (`FoundationaLLMCodeInterpreterTool`) and extending it with skill persistence capabilities.

**Key Benefits:**
- **Reproducibility**: Once a successful approach is discovered, it can be frozen as a deterministic skill
- **Efficiency**: Skip LLM inference for previously-solved tasks by reusing stored skills
- **Reliability**: Complex multi-step operations execute as deterministic code, not probabilistic chat
- **Learning**: Agents accumulate expertise over time through skill acquisition

---

## Table of Contents

1. [CodeMem Architecture Overview](#1-codemem-architecture-overview)
2. [Current FoundationaLLM Architecture](#2-current-foundationallm-architecture)
3. [Proposed Implementation](#3-proposed-implementation)
4. [Design Decisions](#4-design-decisions)
5. [Data Model](#5-data-model)
6. [API Design](#6-api-design)
7. [Implementation Phases](#7-implementation-phases)
8. [Alternative Approaches](#8-alternative-approaches)

---

## 1. CodeMem Architecture Overview

### 1.1 Core Concepts from the Paper

The CodeMem paper identifies four key problems with vanilla CodeAct (code-executing agents):

| Problem | Description | CodeMem Solution |
|---------|-------------|------------------|
| **Probabilistic Instability** | Same task produces different code each time | `register_skill` - freeze successful code as reusable skills |
| **Redundant Computation** | LLM regenerates similar code repeatedly | Skill retrieval bypasses LLM inference |
| **Context Drift** | Agent forgets goals during long tasks | `write_todos` - structured state persistence |
| **Context Bloat** | Too many tools in context | `search` / `load_functions` - Just-in-Time tool loading |

### 1.2 CodeMem Core Toolset

The paper proposes these core tools:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         CodeMem Core Toolset                             │
├─────────────────────────────────────────────────────────────────────────┤
│  execute_code      Execute Python code in a sandboxed environment        │
│  register_skill    Save successful code as a reusable named skill        │
│  search            Search for relevant skills by description             │
│  load_functions    Load skill code into the execution context            │
│  write_todos       Persist structured task state                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Skill Formation Pathways

The paper describes two pathways for skill creation:

1. **Exploration-Driven ("The Scientist")**: Agent discovers successful approaches through trial and error, then registers working code as a skill
2. **Instruction-Driven ("The Architect")**: Human provides working code directly, which is registered as a skill

---

## 2. Current FoundationaLLM Architecture

### 2.1 Existing Code Interpreter Infrastructure

FoundationaLLM already has code interpreter capabilities:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Current Architecture                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Agent (with Tools)                                                     │
│      │                                                                   │
│      ▼                                                                   │
│   FoundationaLLMCodeInterpreterTool (LangChain Tool)                     │
│      │                                                                   │
│      ├─► LLM generates Python code                                       │
│      │                                                                   │
│      ▼                                                                   │
│   Context API → Code Sessions                                            │
│      │                                                                   │
│      ▼                                                                   │
│   PythonCodeSessionAPI (FastAPI container)                               │
│      │                                                                   │
│      ├─► /code/execute - Execute Python code                             │
│      ├─► /files/upload - Upload files to session                         │
│      ├─► /files/download - Download generated files                      │
│      └─► /files/delete - Clean up session                                │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Key Files:**
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py` - The LangChain tool
- `src/python/PythonCodeSessionAPI/app/main.py` - The code execution service
- `src/dotnet/ContextEngine/Services/CodeSessionService.cs` - Session management

### 2.2 Existing Agent Tool Configuration

Agents are configured with tools via `AgentTool`:

```csharp
// src/dotnet/Common/Models/ResourceProviders/Agent/AgentTool.cs
public class AgentTool
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string Category { get; set; }
    public required string PackageName { get; set; }
    public string? ClassName { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}
```

---

## 3. Proposed Implementation

### 3.1 Minimal Changes Approach

The goal is to add procedural memory with minimal platform changes. The proposed approach:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Proposed Architecture                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│   Agent (with Tools)                                                     │
│      │                                                                   │
│      ├─► FoundationaLLMCodeInterpreterTool (ENHANCED)                    │
│      │      │                                                            │
│      │      ├─► search_skills() - Find relevant skills                   │
│      │      ├─► execute_skill() - Run a saved skill                      │
│      │      └─► register_skill() - Save code as skill                    │
│      │                                                                   │
│      └─► (Existing tools: Knowledge, KQL, SQL, etc.)                     │
│                                                                          │
│   NEW: Skill Resource Provider                                           │
│      │                                                                   │
│      ├─► Store skill definitions (code + metadata)                       │
│      ├─► Skill search via embedding similarity                           │
│      └─► Skill versioning and lifecycle                                  │
│                                                                          │
│   Existing: PythonCodeSessionAPI (UNCHANGED)                             │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Key Design Decisions

#### Option A: Extend Code Interpreter Tool (Recommended)

Add skill management methods directly to `FoundationaLLMCodeInterpreterTool`:

**Pros:**
- Minimal new components
- Natural integration with existing code execution flow
- Agent can seamlessly switch between generating new code and using skills

**Cons:**
- Larger tool surface area

#### Option B: Separate Skill Tool

Create a new `FoundationaLLMSkillTool` alongside the code interpreter:

**Pros:**
- Clean separation of concerns
- Can be used independently

**Cons:**
- More components to maintain
- Context overhead (two tools instead of one)

**Recommendation:** Option A - Extend the existing code interpreter tool, as it keeps the agent's tool set lean and provides a natural workflow.

### 3.3 Backwards Compatibility Requirement

**Critical Requirement:** The Code Interpreter tool must remain fully backwards compatible. If `ProceduralMemorySettings` is not enabled on an agent, the tool must work exactly as it does today with no changes in behavior.

**Implementation Strategy:**
1. All skill-related operations are gated by checking `procedural_memory_settings.enabled`
2. If disabled, the tool ignores any skill-related parameters and executes code directly
3. The default `operation` value of `"execute"` ensures existing integrations work unchanged
4. No new required parameters are added to the tool interface

```python
# Backwards compatibility check in the tool
async def _arun(self, prompt, file_names=[], operation="execute", ...):
    # Check if procedural memory is enabled for this agent
    procedural_memory_enabled = self._get_procedural_memory_enabled()
    
    if not procedural_memory_enabled or operation == "execute":
        # Original behavior - generate and execute code
        return await self._execute_code(prompt, file_names)
    
    # Procedural memory operations only if enabled
    if operation == "search_skills":
        return await self._search_skills(prompt)
    elif operation == "use_skill":
        return await self._use_skill(skill_name, skill_parameters, file_names)
    elif operation == "register_skill":
        return await self._register_skill(skill_name, skill_description, prompt)
```

### 3.4 Component Overview

| Component | Change Type | Description |
|-----------|-------------|-------------|
| `FoundationaLLMCodeInterpreterTool` | **Modify** | Add skill search, execution, and registration methods |
| `FoundationaLLM.Skill` Resource Provider | **New** | Store and retrieve skill definitions |
| `Skill` Resource Model | **New** | Data model for skill code and metadata |
| `SkillSearchService` | **New** | Vector search for skill discovery |
| Agent Configuration | **Modify** | Add procedural memory settings |

---

## 4. Design Decisions

This section documents the finalized design decisions for the procedural memory implementation.

### 4.1 Skill Scoping: Agent-User Combination

**Decision:** Skills are scoped to the **agent-user combination**. A skill created by user `zoinertejada@foundationallm.ai` for agent `MAA-02` is only visible and usable by that specific user when interacting with that specific agent.

**Rationale:**
- Ensures skills are personalized to each user's workflow and preferences
- Prevents skill pollution across users (one user's specialized skills don't affect others)
- Allows users to build up agent-specific expertise over time
- Maintains data isolation for multi-tenant deployments

**Implementation:**
- Skills are stored with composite key: `{agent_object_id}:{user_id}`
- Skill search automatically filters by current agent and user context
- Management Portal shows skills grouped by agent-user combination

**Example:** A skill `calculate_revenue_growth` for agent `MAA-02` and user `zoinertejada@foundationallm.ai` would have:
```json
{
    "owner_agent_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/MAA-02",
    "owner_user_id": "zoinertejada@foundationallm.ai"
}
```

### 4.2 Skill Approval Workflow: Auto-Approve by Default (Configurable)

**Decision:** Skills registered by agents are **auto-approved by default**, but this behavior is configurable via `ProceduralMemorySettings.require_skill_approval`.

**Rationale:**
- Auto-approval provides a seamless experience for skill learning
- Configurability allows organizations with stricter policies to require review
- Aligns with the CodeMem paper's approach of immediate skill availability

**Configuration Options:**
| Setting | Value | Behavior |
|---------|-------|----------|
| `require_skill_approval` | `false` (default) | Skills are immediately available after registration |
| `require_skill_approval` | `true` | Skills enter a "pending" state and require admin approval |

**Approval Workflow (when enabled):**
1. Agent registers skill → Skill created with `status: "pending_approval"`
2. Admin reviews skill in Management Portal
3. Admin approves or rejects → Skill becomes `status: "active"` or `status: "rejected"`
4. Only `active` skills are returned in skill searches

### 4.3 Skill Execution Security: Sandbox Only

**Decision:** Skill code executes **only within the existing sandboxed PythonCodeSessionAPI container**. No additional security layers are required.

**Rationale:**
- The existing sandbox provides process isolation and resource limits
- Skills are just Python code, same as dynamically generated code
- No need for static analysis or code review for security (approval workflow is for quality/policy, not security)
- Simplifies implementation by reusing existing infrastructure

**Security Properties (inherited from existing sandbox):**
- Isolated container per code session
- Network restrictions (configurable)
- File system isolation to `/mnt/data`
- Execution timeout limits
- Memory and CPU limits

### 4.4 Skill Discovery and Management UX

**Decision:** 
- **Management Portal** for administration (create, edit, delete, approve skills)
- **Agent transparency** for end users (agent explains when it uses a skill vs. generating new code)

**Rationale:**
- Keeps the User Portal simple and focused on conversations
- Provides clear audit trail of skill usage via agent responses
- Allows admins to curate and manage the skill library

**Agent Transparency Implementation:**
When an agent uses a skill, it should include context in its response:
```
I found an existing skill "calculate_revenue_growth" that matches your request. 
Using this skill to analyze your data...

[Results from skill execution]
```

When an agent creates a new skill:
```
I've solved your problem and created a reusable skill "calculate_revenue_growth" 
so I can help you with similar requests faster in the future.
```

### 4.5 Backwards Compatibility

**Decision:** The Code Interpreter tool must be **100% backwards compatible**. When `ProceduralMemorySettings.enabled` is `false` (or not set), the tool behaves exactly as it does today.

**Implementation:**
- All skill operations are gated by `procedural_memory_settings.enabled` check
- Default operation is `"execute"` (original behavior)
- No new required parameters
- Existing integrations and prompts continue to work unchanged

---

## 5. Data Model

### 5.1 Skill Resource Model

```csharp
// src/dotnet/Common/Models/ResourceProviders/Skill/Skill.cs (NEW)
namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Represents a reusable code skill that can be executed deterministically.
    /// Skills are scoped to an agent-user combination.
    /// </summary>
    public class Skill : ResourceBase
    {
        /// <summary>
        /// The Python code that implements this skill.
        /// </summary>
        [JsonPropertyName("code")]
        public required string Code { get; set; }

        /// <summary>
        /// A detailed description of what this skill does and when to use it.
        /// Used for semantic search to find relevant skills.
        /// </summary>
        [JsonPropertyName("description")]
        public required string Description { get; set; }

        /// <summary>
        /// Example prompts that this skill can handle.
        /// Improves skill discovery via semantic matching.
        /// </summary>
        [JsonPropertyName("example_prompts")]
        public List<string> ExamplePrompts { get; set; } = [];

        /// <summary>
        /// Input parameters expected by the skill code.
        /// </summary>
        [JsonPropertyName("parameters")]
        public List<SkillParameter> Parameters { get; set; } = [];

        /// <summary>
        /// Tags for categorization and filtering.
        /// </summary>
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = [];

        /// <summary>
        /// The object ID of the agent that this skill belongs to.
        /// Required - skills are always scoped to an agent.
        /// </summary>
        [JsonPropertyName("owner_agent_object_id")]
        public required string OwnerAgentObjectId { get; set; }

        /// <summary>
        /// The user ID (UPN) of the user that this skill belongs to.
        /// Required - skills are scoped to the agent-user combination.
        /// Example: "zoinertejada@foundationallm.ai"
        /// </summary>
        [JsonPropertyName("owner_user_id")]
        public required string OwnerUserId { get; set; }

        /// <summary>
        /// The approval status of the skill.
        /// Skills with status "active" are available for use.
        /// </summary>
        [JsonPropertyName("status")]
        public SkillStatus Status { get; set; } = SkillStatus.Active;

        /// <summary>
        /// Number of times this skill has been successfully executed.
        /// </summary>
        [JsonPropertyName("execution_count")]
        public int ExecutionCount { get; set; }

        /// <summary>
        /// Success rate (0.0 to 1.0) based on execution history.
        /// </summary>
        [JsonPropertyName("success_rate")]
        public double SuccessRate { get; set; } = 1.0;

        /// <summary>
        /// The version of this skill (for skill evolution).
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;
    }

    /// <summary>
    /// Skill approval status values.
    /// </summary>
    public enum SkillStatus
    {
        /// <summary>
        /// Skill is active and available for use.
        /// </summary>
        Active,

        /// <summary>
        /// Skill is pending approval (when require_skill_approval is enabled).
        /// </summary>
        PendingApproval,

        /// <summary>
        /// Skill was rejected by an administrator.
        /// </summary>
        Rejected,

        /// <summary>
        /// Skill has been disabled but not deleted.
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Defines an input parameter for a skill.
    /// </summary>
    public class SkillParameter
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; } = true;

        [JsonPropertyName("default_value")]
        public object? DefaultValue { get; set; }
    }
}
```

### 5.2 Agent Procedural Memory Settings

```csharp
// src/dotnet/Common/Models/ResourceProviders/Agent/ProceduralMemorySettings.cs (NEW)
namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Configuration for agent procedural memory capabilities.
    /// When enabled is false, the Code Interpreter tool behaves exactly as before.
    /// </summary>
    public class ProceduralMemorySettings
    {
        /// <summary>
        /// Whether procedural memory (skill learning) is enabled.
        /// When false, the Code Interpreter tool works in backwards-compatible mode.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Whether the agent can automatically register new skills.
        /// If false, skills must be registered manually/by admins.
        /// </summary>
        [JsonPropertyName("auto_register_skills")]
        public bool AutoRegisterSkills { get; set; } = true;

        /// <summary>
        /// Whether newly registered skills require admin approval before becoming active.
        /// Default is false (auto-approve).
        /// When true, new skills are created with status "PendingApproval".
        /// </summary>
        [JsonPropertyName("require_skill_approval")]
        public bool RequireSkillApproval { get; set; } = false;

        /// <summary>
        /// Maximum number of skills per agent-user combination.
        /// 0 = unlimited.
        /// </summary>
        [JsonPropertyName("max_skills_per_user")]
        public int MaxSkillsPerUser { get; set; } = 0;

        /// <summary>
        /// Similarity threshold for skill retrieval (0.0 to 1.0).
        /// Higher values = more precise matching.
        /// </summary>
        [JsonPropertyName("skill_search_threshold")]
        public double SkillSearchThreshold { get; set; } = 0.8;

        /// <summary>
        /// Whether to prefer using existing skills over generating new code.
        /// When true, agent will search for skills first before generating code.
        /// </summary>
        [JsonPropertyName("prefer_skills")]
        public bool PreferSkills { get; set; } = true;
    }
}
```

### 5.3 Add to AgentBase

```csharp
// Add to src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs

/// <summary>
/// Configuration for procedural memory capabilities (skill learning).
/// When null or when Enabled is false, the Code Interpreter tool works in 
/// backwards-compatible mode without any skill functionality.
/// </summary>
[JsonPropertyName("procedural_memory_settings")]
public ProceduralMemorySettings? ProceduralMemorySettings { get; set; }
```

---

## 6. API Design

### 6.1 Enhanced Code Interpreter Tool Interface

The enhanced code interpreter will support these operations:

```python
# src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """Input schema for the enhanced code interpreter tool."""
    
    # Existing parameters
    prompt: str = Field(description="The task description or code to execute")
    file_names: Optional[List[str]] = Field(default=[], description="Files to make available")
    
    # NEW: Skill-related parameters
    operation: str = Field(
        default="execute",
        description="Operation: 'execute' (run code), 'search_skills' (find skills), 'use_skill' (run a skill), 'register_skill' (save code as skill)"
    )
    skill_name: Optional[str] = Field(default=None, description="Name of skill to use or register")
    skill_description: Optional[str] = Field(default=None, description="Description for skill registration")
    skill_parameters: Optional[Dict[str, Any]] = Field(default=None, description="Parameters to pass to a skill")
```

### 6.2 Tool Operations

#### Search Skills
```python
# Agent calls tool with:
{
    "operation": "search_skills",
    "prompt": "calculate monthly revenue growth from sales data"
}

# Returns:
{
    "skills": [
        {
            "name": "calculate_revenue_growth",
            "description": "Calculates month-over-month revenue growth percentage",
            "similarity": 0.92,
            "parameters": [
                {"name": "data_file", "type": "str", "description": "Path to CSV with sales data"},
                {"name": "date_column", "type": "str", "description": "Name of date column"}
            ]
        }
    ]
}
```

#### Use Skill
```python
# Agent calls tool with:
{
    "operation": "use_skill",
    "skill_name": "calculate_revenue_growth",
    "skill_parameters": {
        "data_file": "sales_2024.csv",
        "date_column": "transaction_date"
    },
    "file_names": ["sales_2024.csv"]
}

# Returns: Same as regular code execution (results, output, files)
```

#### Register Skill
```python
# Agent calls tool with:
{
    "operation": "register_skill",
    "skill_name": "calculate_revenue_growth",
    "skill_description": "Calculates month-over-month revenue growth percentage from sales data",
    "prompt": '''
def calculate_revenue_growth(data_file: str, date_column: str) -> dict:
    """Calculate month-over-month revenue growth."""
    import pandas as pd
    df = pd.read_csv(data_file)
    df[date_column] = pd.to_datetime(df[date_column])
    monthly = df.groupby(df[date_column].dt.to_period('M'))['revenue'].sum()
    growth = monthly.pct_change() * 100
    return {"growth_rates": growth.to_dict()}
'''
}

# Returns:
{
    "status": "registered",
    "skill_name": "calculate_revenue_growth",
    "skill_id": "/instances/{id}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth"
}
```

### 6.3 Skill Resource Provider API

```
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
PUT    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
DELETE /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
POST   /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/search
```

### 6.4 Skill Search Request/Response

```csharp
// Search Request
public class SkillSearchRequest
{
    /// <summary>
    /// The search query (natural language description of desired skill).
    /// </summary>
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    /// <summary>
    /// The agent object ID. Required - skills are scoped to agent-user combination.
    /// </summary>
    [JsonPropertyName("agent_object_id")]
    public required string AgentObjectId { get; set; }

    /// <summary>
    /// The user ID (UPN). Required - skills are scoped to agent-user combination.
    /// </summary>
    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    /// <summary>
    /// Optional tags to filter results.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Minimum similarity score (0.0 to 1.0). Default: 0.7
    /// </summary>
    [JsonPropertyName("min_similarity")]
    public double MinSimilarity { get; set; } = 0.7;

    /// <summary>
    /// Maximum number of results to return. Default: 5
    /// </summary>
    [JsonPropertyName("max_results")]
    public int MaxResults { get; set; } = 5;

    /// <summary>
    /// Only return skills with this status. Default: Active only.
    /// </summary>
    [JsonPropertyName("status_filter")]
    public SkillStatus? StatusFilter { get; set; } = SkillStatus.Active;
}

// Search Response
public class SkillSearchResult
{
    [JsonPropertyName("skill")]
    public required Skill Skill { get; set; }

    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }
}
```

---

## 7. Implementation Phases

### Phase 1: Skill Resource Provider (Foundation)

**Goal:** Create the infrastructure to store and retrieve skills with agent-user scoping.

**Tasks:**
1. Create `Skill` resource model with agent-user scoping fields
2. Create `SkillStatus` enum and `SkillParameter` class
3. Create `FoundationaLLM.Skill` resource provider
4. Implement skill CRUD operations with agent-user filtering
5. Add skill storage (Cosmos DB with partition key on agent-user combination)
6. Add Management Portal UI for viewing/managing skills (grouped by agent-user)

**Estimated Effort:** 1-2 weeks

**Files to Create/Modify:**
- `src/dotnet/Common/Models/ResourceProviders/Skill/Skill.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillParameter.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillStatus.cs` (NEW)
- `src/dotnet/Common/Constants/ResourceProviders/SkillResourceProviderMetadata.cs` (NEW)
- `src/dotnet/Skill/ResourceProviders/SkillResourceProviderService.cs` (NEW)
- `deploy/standard/data/resource-provider/FoundationaLLM.Skill/` (NEW)

### Phase 2: Skill Search Service

**Goal:** Enable semantic search to find relevant skills.

**Tasks:**
1. Create skill embedding service (embed skill descriptions + example prompts)
2. Implement vector search for skill discovery
3. Add skill search API endpoint
4. Index existing skills with embeddings

**Estimated Effort:** 1 week

**Files to Create/Modify:**
- `src/dotnet/Skill/Services/SkillSearchService.cs` (NEW)
- `src/dotnet/Skill/Services/SkillEmbeddingService.cs` (NEW)
- Reuse existing embedding infrastructure from vectorization

### Phase 3: Code Interpreter Enhancement

**Goal:** Add skill operations to the code interpreter tool while maintaining 100% backwards compatibility.

**Tasks:**
1. Add procedural memory enabled check at tool initialization
2. Update `FoundationaLLMCodeInterpreterToolInput` with optional skill parameters
3. Implement backwards-compatible operation routing (default to `execute`)
4. Implement `search_skills` operation (only when enabled)
5. Implement `use_skill` operation (only when enabled)
6. Implement `register_skill` operation with approval workflow support
7. Update tool prompts to guide LLM on skill usage (conditional on enabled)
8. Add comprehensive tests for backwards compatibility

**Estimated Effort:** 1-2 weeks

**Files to Modify:**
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py`
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool_input.py`

**Backwards Compatibility Test Cases:**
- Agent without `procedural_memory_settings` → tool works as before
- Agent with `procedural_memory_settings.enabled = false` → tool works as before
- Existing prompts without `operation` parameter → default to `execute`
- Skill parameters ignored when procedural memory disabled

### Phase 4: Agent Configuration

**Goal:** Allow agents to be configured with procedural memory settings.

**Tasks:**
1. Add `ProceduralMemorySettings` to agent model
2. Update Management Portal agent configuration UI with:
   - Enable/disable toggle
   - Auto-register skills toggle
   - Require approval toggle
   - Max skills per user setting
   - Skill search threshold slider
   - Prefer skills toggle
3. Add skill management section in Management Portal (view skills by agent-user)
4. Add skill approval workflow UI (when require_skill_approval is enabled)

**Estimated Effort:** 1 week

**Files to Modify:**
- `src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs`
- `src/dotnet/Common/Models/ResourceProviders/Agent/ProceduralMemorySettings.cs` (NEW)
- `src/ui/ManagementPortal/pages/agents/create.vue`
- `src/ui/ManagementPortal/pages/skills/index.vue` (NEW)
- `src/ui/ManagementPortal/pages/skills/[skillId].vue` (NEW)

### Phase 5: Skill Lifecycle & Analytics (Optional Enhancement)

**Goal:** Track skill usage and improve skill quality over time.

**Tasks:**
1. Track skill execution count and success rate
2. Implement skill versioning
3. Add skill analytics dashboard
4. Implement skill pruning (remove unused skills)

**Estimated Effort:** 1-2 weeks

---

## 8. Alternative Approaches

### 8.1 Approach A: Enhanced Code Interpreter (Recommended)

**Description:** Extend the existing code interpreter tool with skill management capabilities.

| Pros | Cons |
|------|------|
| Minimal new components | Larger tool interface |
| Natural workflow integration | All operations through one tool |
| Lean agent context | |
| Leverages existing infrastructure | |

### 8.2 Approach B: Separate Skill Tool

**Description:** Create a dedicated `FoundationaLLMSkillTool` for skill management.

| Pros | Cons |
|------|------|
| Clean separation of concerns | More context overhead |
| Independent tool lifecycle | Two tools to manage |
| Clearer tool descriptions | More complex agent prompts |

### 8.3 Approach C: MCP-Based Skills (Future)

**Description:** Implement skills as MCP (Model Context Protocol) servers that can be dynamically loaded.

| Pros | Cons |
|------|------|
| Standard protocol | Requires MCP infrastructure |
| Dynamic tool discovery | More complex deployment |
| Cross-platform compatibility | Overhead for simple skills |

**Recommendation:** Start with Approach A (Enhanced Code Interpreter) and consider evolving to Approach C as MCP adoption grows.

---

## Appendix A: Example Agent Prompt with Skills

**Note:** This prompt is only used when `procedural_memory_settings.enabled = true`. When disabled, the standard code interpreter prompt is used unchanged.

```
You are an AI assistant with access to a code interpreter tool that supports procedural memory.

Your skills are personal to you and this user - they remember successful code patterns 
that have worked well in past conversations.

When given a task:
1. First, search for relevant skills that might help: use operation="search_skills"
2. If a suitable skill exists with similarity > 0.8, use it: use operation="use_skill"
   - Let the user know you're using a previously learned skill
3. If no suitable skill exists, generate and execute code: use operation="execute"
4. If your code works well and could be reused, consider registering it as a skill: 
   use operation="register_skill"
   - Let the user know you've learned a new skill for future use

Available operations:
- search_skills: Find skills matching a description (searches your skill library for this user)
- use_skill: Execute a registered skill with parameters
- execute: Generate and run Python code (default behavior)
- register_skill: Save working code as a reusable skill for future conversations
```

---

## Appendix B: Skill Storage Schema

```json
{
    "id": "calculate_revenue_growth_MAA-02_zoinertejada",
    "type": "FoundationaLLM.Skill/skills",
    "name": "calculate_revenue_growth",
    "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth_MAA-02_zoinertejada",
    "display_name": "Calculate Revenue Growth",
    "description": "Calculates month-over-month revenue growth percentage from sales data",
    "code": "def calculate_revenue_growth(data_file: str, date_column: str) -> dict:\n    import pandas as pd\n    df = pd.read_csv(data_file)\n    df[date_column] = pd.to_datetime(df[date_column])\n    monthly = df.groupby(df[date_column].dt.to_period('M'))['revenue'].sum()\n    growth = monthly.pct_change() * 100\n    return {\"growth_rates\": growth.to_dict()}",
    "example_prompts": [
        "Calculate the monthly revenue growth from my sales data",
        "What was the month-over-month growth in revenue?"
    ],
    "parameters": [
        {
            "name": "data_file",
            "type": "str",
            "description": "Path to CSV file with sales data",
            "required": true
        },
        {
            "name": "date_column",
            "type": "str",
            "description": "Name of the date column",
            "required": true
        }
    ],
    "tags": ["analytics", "revenue", "growth"],
    "owner_agent_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/MAA-02",
    "owner_user_id": "zoinertejada@foundationallm.ai",
    "status": "Active",
    "execution_count": 47,
    "success_rate": 0.98,
    "version": 2,
    "created_on": "2024-01-15T10:30:00Z",
    "updated_on": "2024-02-20T14:45:00Z"
}
```

**Note:** The skill ID incorporates both the agent name and a sanitized user identifier to ensure uniqueness within the agent-user scope.

---

## Appendix C: Related CodeMem Paper Concepts

### State Anchoring via write_todos

The paper describes using `write_todos` to prevent goal drift in long tasks. This is similar to FoundationaLLM's existing conversation state management, but could be enhanced for complex agent workflows.

### Dynamic MCP

The paper references "Dynamic MCP" for just-in-time tool loading. This aligns with FoundationaLLM's plugin architecture and could be a future enhancement.

### Skill Formation Metrics

From the paper's evaluation:
- Gemini 3 Full achieved 96% minimum correctness with CodeMem
- Average of ~7 assistant calls to resolve tasks
- Skills reduce latency and token usage for repeated tasks

---

*Document Version: 1.1*
*Created: January 2025*
*Last Updated: January 2025*
*Status: Design Decisions Finalized*

**Revision History:**
- v1.1 (Jan 2025): Finalized design decisions for skill scoping (agent-user), approval workflow (auto-approve by default, configurable), security (sandbox only), and backwards compatibility requirement
- v1.0 (Jan 2025): Initial plan draft
