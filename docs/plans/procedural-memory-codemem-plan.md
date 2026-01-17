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
4. [Data Model](#4-data-model)
5. [API Design](#5-api-design)
6. [Implementation Phases](#6-implementation-phases)
7. [Alternative Approaches](#7-alternative-approaches)
8. [Open Questions](#8-open-questions)

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

### 3.3 Component Overview

| Component | Change Type | Description |
|-----------|-------------|-------------|
| `FoundationaLLMCodeInterpreterTool` | **Modify** | Add skill search, execution, and registration methods |
| `FoundationaLLM.Skill` Resource Provider | **New** | Store and retrieve skill definitions |
| `Skill` Resource Model | **New** | Data model for skill code and metadata |
| `SkillSearchService` | **New** | Vector search for skill discovery |
| Agent Configuration | **Modify** | Add procedural memory settings |

---

## 4. Data Model

### 4.1 Skill Resource Model

```csharp
// src/dotnet/Common/Models/ResourceProviders/Skill/Skill.cs (NEW)
namespace FoundationaLLM.Common.Models.ResourceProviders.Skill
{
    /// <summary>
    /// Represents a reusable code skill that can be executed deterministically.
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
        /// The object ID of the agent that created/owns this skill.
        /// Null for global skills available to all agents.
        /// </summary>
        [JsonPropertyName("owner_agent_object_id")]
        public string? OwnerAgentObjectId { get; set; }

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

### 4.2 Agent Procedural Memory Settings

```csharp
// src/dotnet/Common/Models/ResourceProviders/Agent/ProceduralMemorySettings.cs (NEW)
namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Configuration for agent procedural memory capabilities.
    /// </summary>
    public class ProceduralMemorySettings
    {
        /// <summary>
        /// Whether procedural memory (skill learning) is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Whether the agent can automatically register new skills.
        /// If false, skills must be registered manually/by admins.
        /// </summary>
        [JsonPropertyName("auto_register_skills")]
        public bool AutoRegisterSkills { get; set; } = false;

        /// <summary>
        /// Minimum confidence threshold for skill auto-registration.
        /// Agent must indicate high confidence in the code's correctness.
        /// </summary>
        [JsonPropertyName("skill_registration_confidence_threshold")]
        public double SkillRegistrationConfidenceThreshold { get; set; } = 0.9;

        /// <summary>
        /// Maximum number of skills the agent can store.
        /// 0 = unlimited.
        /// </summary>
        [JsonPropertyName("max_skills")]
        public int MaxSkills { get; set; } = 0;

        /// <summary>
        /// Similarity threshold for skill retrieval (0.0 to 1.0).
        /// Higher values = more precise matching.
        /// </summary>
        [JsonPropertyName("skill_search_threshold")]
        public double SkillSearchThreshold { get; set; } = 0.8;

        /// <summary>
        /// Whether to prefer using existing skills over generating new code.
        /// </summary>
        [JsonPropertyName("prefer_skills")]
        public bool PreferSkills { get; set; } = true;
    }
}
```

### 4.3 Add to AgentBase

```csharp
// Add to src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs

/// <summary>
/// Configuration for procedural memory capabilities (skill learning).
/// </summary>
[JsonPropertyName("procedural_memory_settings")]
public ProceduralMemorySettings? ProceduralMemorySettings { get; set; }
```

---

## 5. API Design

### 5.1 Enhanced Code Interpreter Tool Interface

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

### 5.2 Tool Operations

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

### 5.3 Skill Resource Provider API

```
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
PUT    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
DELETE /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillName}
POST   /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/search
```

### 5.4 Skill Search Request/Response

```csharp
// Search Request
public class SkillSearchRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("agent_object_id")]
    public string? AgentObjectId { get; set; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("min_similarity")]
    public double MinSimilarity { get; set; } = 0.7;

    [JsonPropertyName("max_results")]
    public int MaxResults { get; set; } = 5;
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

## 6. Implementation Phases

### Phase 1: Skill Resource Provider (Foundation)

**Goal:** Create the infrastructure to store and retrieve skills.

**Tasks:**
1. Create `Skill` resource model and related classes
2. Create `FoundationaLLM.Skill` resource provider
3. Implement skill CRUD operations
4. Add skill storage (Cosmos DB / Blob Storage)
5. Add Management Portal UI for viewing/managing skills

**Estimated Effort:** 1-2 weeks

**Files to Create/Modify:**
- `src/dotnet/Common/Models/ResourceProviders/Skill/Skill.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillParameter.cs` (NEW)
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

**Goal:** Add skill operations to the code interpreter tool.

**Tasks:**
1. Update `FoundationaLLMCodeInterpreterToolInput` with new parameters
2. Implement `search_skills` operation
3. Implement `use_skill` operation
4. Implement `register_skill` operation
5. Update tool prompts to guide LLM on skill usage

**Estimated Effort:** 1-2 weeks

**Files to Modify:**
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py`
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool_input.py`

### Phase 4: Agent Configuration

**Goal:** Allow agents to be configured with procedural memory settings.

**Tasks:**
1. Add `ProceduralMemorySettings` to agent model
2. Update Management Portal agent configuration UI
3. Update agent workflows to check for skills before code generation
4. Add agent-scoped skill visibility

**Estimated Effort:** 1 week

**Files to Modify:**
- `src/dotnet/Common/Models/ResourceProviders/Agent/AgentBase.cs`
- `src/dotnet/Common/Models/ResourceProviders/Agent/ProceduralMemorySettings.cs` (NEW)
- `src/ui/ManagementPortal/pages/agents/create.vue`

### Phase 5: Skill Lifecycle & Analytics (Optional Enhancement)

**Goal:** Track skill usage and improve skill quality over time.

**Tasks:**
1. Track skill execution count and success rate
2. Implement skill versioning
3. Add skill analytics dashboard
4. Implement skill pruning (remove unused skills)

**Estimated Effort:** 1-2 weeks

---

## 7. Alternative Approaches

### 7.1 Approach A: Enhanced Code Interpreter (Recommended)

**Description:** Extend the existing code interpreter tool with skill management capabilities.

| Pros | Cons |
|------|------|
| Minimal new components | Larger tool interface |
| Natural workflow integration | All operations through one tool |
| Lean agent context | |
| Leverages existing infrastructure | |

### 7.2 Approach B: Separate Skill Tool

**Description:** Create a dedicated `FoundationaLLMSkillTool` for skill management.

| Pros | Cons |
|------|------|
| Clean separation of concerns | More context overhead |
| Independent tool lifecycle | Two tools to manage |
| Clearer tool descriptions | More complex agent prompts |

### 7.3 Approach C: MCP-Based Skills (Future)

**Description:** Implement skills as MCP (Model Context Protocol) servers that can be dynamically loaded.

| Pros | Cons |
|------|------|
| Standard protocol | Requires MCP infrastructure |
| Dynamic tool discovery | More complex deployment |
| Cross-platform compatibility | Overhead for simple skills |

**Recommendation:** Start with Approach A (Enhanced Code Interpreter) and consider evolving to Approach C as MCP adoption grows.

---

## 8. Open Questions

### 8.1 Skill Scoping

**Question:** Should skills be scoped to individual agents, users, tenants, or global?

**Options:**
1. **Agent-scoped** (default): Each agent has its own skill library
2. **User-scoped**: Users can create personal skills across agents
3. **Tenant-scoped**: Shared skills within an organization
4. **Global**: Platform-wide skill library

**Recommendation:** Support agent-scoped by default, with optional tenant-scoped sharing.

### 8.2 Skill Approval Workflow

**Question:** Should agent-registered skills require human approval?

**Options:**
1. **Auto-approve**: Skills registered by agents are immediately available
2. **Review queue**: Skills require admin approval before use
3. **Confidence-based**: Auto-approve if agent confidence > threshold

**Recommendation:** Make configurable via `ProceduralMemorySettings`.

### 8.3 Skill Execution Security

**Question:** How to ensure skill code is safe to execute?

**Options:**
1. **Sandbox only**: Skills run in existing sandboxed environment
2. **Code review**: Required review before skill registration
3. **Static analysis**: Automated security scanning of skill code

**Recommendation:** Leverage existing sandbox security + optional code review.

### 8.4 Skill Discovery UX

**Question:** How should users discover and manage skills?

**Options:**
1. **Management Portal only**: Admin-level skill management
2. **User Portal visibility**: Users can see available skills
3. **Agent transparency**: Agent explains when it uses skills

**Recommendation:** Management Portal for administration, agent transparency for end users.

---

## Appendix A: Example Agent Prompt with Skills

```
You are an AI assistant with access to a code interpreter tool that supports procedural memory.

When given a task:
1. First, search for relevant skills that might help: use operation="search_skills"
2. If a suitable skill exists with similarity > 0.8, use it: use operation="use_skill"
3. If no suitable skill exists, generate and execute code: use operation="execute"
4. If your code works well and could be reused, consider registering it as a skill: use operation="register_skill"

Available operations:
- search_skills: Find skills matching a description
- use_skill: Execute a registered skill with parameters
- execute: Generate and run Python code
- register_skill: Save working code as a reusable skill
```

---

## Appendix B: Skill Storage Schema

```json
{
    "id": "calculate_revenue_growth",
    "type": "FoundationaLLM.Skill/skills",
    "name": "calculate_revenue_growth",
    "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth",
    "display_name": "Calculate Revenue Growth",
    "description": "Calculates month-over-month revenue growth percentage from sales data",
    "code": "def calculate_revenue_growth(data_file: str, date_column: str) -> dict:\n    ...",
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
    "owner_agent_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Agent/agents/sales-analyst",
    "execution_count": 47,
    "success_rate": 0.98,
    "version": 2,
    "created_on": "2024-01-15T10:30:00Z",
    "updated_on": "2024-02-20T14:45:00Z"
}
```

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

*Document Version: 1.0*
*Created: January 2025*
*Status: Draft - For Review*
