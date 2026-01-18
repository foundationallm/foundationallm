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

### 4.6 Skill Registration Content Artifact with User Review

**Decision:** When a skill is registered, the tool returns a **new content artifact type (`skill_saved`)** that allows users to review and approve/reject the skill directly from the User Portal.

**Rationale:**
- Provides transparency to users about what code is being saved on their behalf
- Gives users direct control over their skill library without requiring Management Portal access
- Creates a natural review point in the conversation flow
- Follows the existing content artifact pattern used for file outputs

**User Experience Flow:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Skill Registration UX Flow                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. Agent registers a skill                                              │
│     └─► Tool returns content artifact (type: skill_saved)               │
│                                                                          │
│  2. User Portal displays artifact in conversation                        │
│     ┌─────────────────────────────────────────────────────────────┐     │
│     │  🔧 Skill Saved: "calculate_revenue_growth"                  │     │
│     │  Click to review the code                                    │     │
│     └─────────────────────────────────────────────────────────────┘     │
│                                                                          │
│  3. User clicks to view skill details                                    │
│     ┌─────────────────────────────────────────────────────────────┐     │
│     │  Skill: calculate_revenue_growth                             │     │
│     │  Description: Calculates month-over-month revenue growth...  │     │
│     │                                                               │     │
│     │  ┌─────────────────────────────────────────────────────────┐ │     │
│     │  │ def calculate_revenue_growth(data_file, date_column):   │ │     │
│     │  │     import pandas as pd                                  │ │     │
│     │  │     df = pd.read_csv(data_file)                         │ │     │
│     │  │     ...                                                  │ │     │
│     │  └─────────────────────────────────────────────────────────┘ │     │
│     │                                                               │     │
│     │  [✓ Approve]  [✗ Reject]                                     │     │
│     └─────────────────────────────────────────────────────────────┘     │
│                                                                          │
│  4a. User clicks "Approve"                                               │
│      └─► Skill status remains Active (or changes from PendingApproval)  │
│      └─► Toast: "Skill approved and ready to use"                       │
│                                                                          │
│  4b. User clicks "Reject"                                                │
│      └─► Skill is DELETED from storage                                  │
│      └─► Toast: "Skill rejected and removed"                            │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Content Artifact Structure (Skill Saved):**

```python
ContentArtifact(
    id="skill_saved",
    title="Skill Saved: calculate_revenue_growth",
    type=ContentArtifactTypeNames.SKILL_SAVED,  # NEW type
    filepath=skill_object_id,  # Used for retrieval
    metadata={
        "skill_object_id": "/instances/{id}/providers/FoundationaLLM.Skill/skills/...",
        "skill_name": "calculate_revenue_growth",
        "skill_description": "Calculates month-over-month revenue growth...",
        "skill_code": "def calculate_revenue_growth(...):\n    ...",
        "skill_status": "Active",
        "agent_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/MAA-02",
        "user_id": "zoinertejada@foundationallm.ai"
    }
)
```

### 4.7 Skill Used Content Artifact with User Review

**Decision:** When an existing skill is used, the tool returns a **`skill_used` content artifact** that allows users to see which skill was executed and approve or reject it.

**Rationale:**
- Provides transparency when the agent uses learned skills
- Allows users to remove skills that aren't working well
- Creates an audit trail of skill usage in the conversation
- Enables users to understand agent behavior ("Why did it do that?")

**User Experience Flow:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Skill Used UX Flow                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. Agent uses an existing skill                                         │
│     └─► Tool returns content artifact (type: skill_used)                │
│                                                                          │
│  2. User Portal displays artifact in conversation                        │
│     ┌─────────────────────────────────────────────────────────────┐     │
│     │  ⚡ Skill Used: "calculate_revenue_growth"                   │     │
│     │  Click to review the skill                                   │     │
│     └─────────────────────────────────────────────────────────────┘     │
│                                                                          │
│  3. User clicks to view skill details (same modal as skill_saved)        │
│     ┌─────────────────────────────────────────────────────────────┐     │
│     │  Skill: calculate_revenue_growth                             │     │
│     │  Status: Active | Used 48 times | 98% success rate           │     │
│     │  Description: Calculates month-over-month revenue growth...  │     │
│     │                                                               │     │
│     │  ┌─────────────────────────────────────────────────────────┐ │     │
│     │  │ def calculate_revenue_growth(data_file, date_column):   │ │     │
│     │  │     import pandas as pd                                  │ │     │
│     │  │     ...                                                  │ │     │
│     │  └─────────────────────────────────────────────────────────┘ │     │
│     │                                                               │     │
│     │  [✓ Keep Skill]  [✗ Remove Skill]                            │     │
│     └─────────────────────────────────────────────────────────────┘     │
│                                                                          │
│  4a. User clicks "Keep Skill"                                            │
│      └─► No action needed (skill already Active)                        │
│      └─► Toast: "Skill will continue to be used"                        │
│                                                                          │
│  4b. User clicks "Remove Skill"                                          │
│      └─► Skill is DELETED from storage                                  │
│      └─► Toast: "Skill removed - agent will generate new code next time"│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Content Artifact Structure (Skill Used):**

```python
ContentArtifact(
    id="skill_used",
    title="Skill Used: calculate_revenue_growth",
    type=ContentArtifactTypeNames.SKILL_USED,  # NEW type
    filepath=skill_object_id,  # Used for retrieval
    metadata={
        "skill_object_id": "/instances/{id}/providers/FoundationaLLM.Skill/skills/...",
        "skill_name": "calculate_revenue_growth",
        "skill_description": "Calculates month-over-month revenue growth...",
        "skill_code": "def calculate_revenue_growth(...):\n    ...",
        "skill_status": "Active",
        "execution_count": 48,
        "success_rate": 0.98,
        "agent_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/MAA-02",
        "user_id": "zoinertejada@foundationallm.ai"
    }
)
```

**Differences from Skill Saved Artifact:**
| Aspect | Skill Saved | Skill Used |
|--------|-------------|------------|
| Icon | 🔧 (wrench) | ⚡ (lightning) |
| Title | "Skill Saved: {name}" | "Skill Used: {name}" |
| Button labels | "Approve" / "Reject" | "Keep Skill" / "Remove Skill" |
| Shows execution stats | No (new skill) | Yes (execution_count, success_rate) |
| Typical use case | First time skill is created | Subsequent uses of skill |

**User Portal Implementation:**

1. **Content Artifact Renderer**: New component to render `skill_saved` artifacts
   - Displays skill name and description inline in conversation
   - "View Code" button opens modal/drawer with full details
   
2. **Skill Review Modal**: Shows skill details with syntax-highlighted code
   - Read-only Python code viewer
   - Skill metadata (name, description, parameters)
   - Approve/Reject buttons
   
3. **Skill Actions**: API calls for user approval/rejection
   - `POST /skills/{skillId}/approve` → Sets status to Active
   - `DELETE /skills/{skillId}` → Removes skill from storage (on reject)

**Key Behaviors:**
- Skills are usable immediately after registration (status: Active)
- User can review and reject at any time, even after using the skill
- Rejected skills are permanently deleted (not just disabled)
- Approval action is idempotent (no-op if already Active)

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

### 5.4 Content Artifact Types for Skills

```csharp
// Add to src/dotnet/Common/Models/Constants/ContentArtifactTypeNames.cs

/// <summary>
/// Content artifact type for a skill that was saved by the agent.
/// Displayed in User Portal with review/approve/reject capabilities.
/// </summary>
public const string SKILL_SAVED = "skill_saved";

/// <summary>
/// Content artifact type for a skill that was used by the agent.
/// Displayed in User Portal with review/keep/remove capabilities.
/// </summary>
public const string SKILL_USED = "skill_used";
```

```python
# Add to foundationallm/models/constants.py (Python SDK)

class ContentArtifactTypeNames:
    # ... existing types ...
    SKILL_SAVED = "skill_saved"
    SKILL_USED = "skill_used"
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

# Returns: Execution results + Content Artifact for User Portal review
# Text response (same as regular code execution):
"The monthly revenue growth rates are: Jan: 5.2%, Feb: -1.3%, Mar: 8.7%..."

# Content Artifact (included in tool result):
ContentArtifact(
    id="skill_used",
    title="Skill Used: calculate_revenue_growth",
    type="skill_used",
    filepath="/instances/{id}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth_MAA-02_user",
    metadata={
        "skill_object_id": "/instances/{id}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth_MAA-02_user",
        "skill_name": "calculate_revenue_growth",
        "skill_description": "Calculates month-over-month revenue growth percentage from sales data",
        "skill_code": "def calculate_revenue_growth(data_file: str, date_column: str) -> dict:\n    ...",
        "skill_status": "Active",
        "execution_count": 48,
        "success_rate": 0.98,
        "agent_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/MAA-02",
        "user_id": "zoinertejada@foundationallm.ai"
    }
)
```

The content artifact allows users to see which skill was used and remove it if desired (see Section 4.7).

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

# Returns: Text response + Content Artifact for User Portal review
# Text response:
"Skill 'calculate_revenue_growth' has been saved and is ready to use."

# Content Artifact (included in tool result):
ContentArtifact(
    id="skill_saved",
    title="Skill Saved: calculate_revenue_growth",
    type="skill_saved",
    filepath="/instances/{id}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth_MAA-02_user",
    metadata={
        "skill_object_id": "/instances/{id}/providers/FoundationaLLM.Skill/skills/calculate_revenue_growth_MAA-02_user",
        "skill_name": "calculate_revenue_growth",
        "skill_description": "Calculates month-over-month revenue growth percentage from sales data",
        "skill_code": "def calculate_revenue_growth(data_file: str, date_column: str) -> dict:\n    ...",
        "skill_status": "Active",
        "agent_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/agents/MAA-02",
        "user_id": "zoinertejada@foundationallm.ai"
    }
)
```

The content artifact allows users to review and approve/reject the skill directly from the User Portal conversation view (see Section 4.6).

### 6.3 Skill Resource Provider API

**Management API (Management Portal / Admin):**
```
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills
GET    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillId}
PUT    /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillId}
DELETE /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/{skillId}
POST   /instances/{instanceId}/providers/FoundationaLLM.Skill/skills/search
```

**User Portal API (CoreAPI endpoints for user skill review):**
```
GET    /instances/{instanceId}/skills/{skillId}           # Get skill details for review
POST   /instances/{instanceId}/skills/{skillId}/approve   # Approve a skill (sets status to Active)
DELETE /instances/{instanceId}/skills/{skillId}           # Reject and delete a skill
```

### 6.4 User Portal Skill Review Endpoints

These endpoints are exposed via CoreAPI for the User Portal to review and manage skills:

```csharp
// GET /instances/{instanceId}/skills/{skillId}
// Returns skill details for the review modal
public class SkillReviewResponse
{
    [JsonPropertyName("skill_object_id")]
    public required string SkillObjectId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("parameters")]
    public List<SkillParameter> Parameters { get; set; } = [];

    [JsonPropertyName("status")]
    public SkillStatus Status { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("execution_count")]
    public int ExecutionCount { get; set; }
}

// POST /instances/{instanceId}/skills/{skillId}/approve
// Sets skill status to Active (idempotent)
// Returns: 200 OK with updated skill details

// DELETE /instances/{instanceId}/skills/{skillId}
// Permanently removes the skill from storage
// Returns: 204 No Content
```

**Authorization:**
- Users can only view/approve/reject skills where `owner_user_id` matches their UPN
- Admin users (via Management Portal) can manage any skill

### 6.5 Skill Search Request/Response

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

### Phase 4: Agent Configuration & Management Portal

**Goal:** Allow agents to be configured with procedural memory settings and provide admin skill management.

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

### Phase 5: User Portal Skill Review UI

**Goal:** Allow users to review, approve/reject, and remove skills directly from the conversation in the User Portal.

**Tasks:**
1. Add new content artifact types `SKILL_SAVED` and `SKILL_USED` to constants
2. Create `SkillArtifact` component to render both skill artifact types in conversation
   - Display skill name and appropriate icon (🔧 saved, ⚡ used)
   - "View Code" / "View Skill" button to open review modal
   - Different styling for saved vs. used artifacts
3. Create `SkillReviewModal` component for detailed skill review
   - Syntax-highlighted Python code viewer (read-only)
   - Skill metadata display (name, description, parameters)
   - Execution statistics for used skills (execution count, success rate)
   - Context-appropriate buttons:
     - Skill Saved: "Approve" / "Reject"
     - Skill Used: "Keep Skill" / "Remove Skill"
4. Add CoreAPI endpoints for User Portal skill operations
   - `GET /instances/{instanceId}/skills/{skillId}` - Get skill for review
   - `POST /instances/{instanceId}/skills/{skillId}/approve` - Approve/keep skill
   - `DELETE /instances/{instanceId}/skills/{skillId}` - Reject/remove and delete skill
5. Implement skill actions with appropriate toast notifications
   - Saved + Approve: "Skill approved and ready to use"
   - Saved + Reject: "Skill rejected and removed"
   - Used + Keep: "Skill will continue to be used"
   - Used + Remove: "Skill removed - agent will generate new code next time"
6. Handle edge cases (skill already deleted, network errors, concurrent access)

**Estimated Effort:** 1-2 weeks

**Files to Create/Modify:**
- `src/dotnet/Common/Constants/ContentArtifactTypeNames.cs` (add `SKILL_SAVED`, `SKILL_USED`)
- `src/dotnet/CoreAPI/Controllers/SkillsController.cs` (NEW)
- `src/ui/UserPortal/components/SkillArtifact.vue` (NEW - handles both types)
- `src/ui/UserPortal/components/SkillReviewModal.vue` (NEW)
- `src/ui/UserPortal/components/ChatMessage.vue` (update to render skill artifacts)
- `src/ui/UserPortal/js/api.ts` (add skill API methods)
- `src/ui/UserPortal/js/types/index.ts` (add skill types)

### Phase 6: Skill Lifecycle & Analytics (Optional Enhancement)

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

## 9. Manual Testing and Verification Guide

This section provides step-by-step instructions to manually test and verify the procedural memory implementation.

### 9.1 Prerequisites

Before testing, ensure the following are in place:

1. **Environment Setup**
   - FoundationaLLM platform is deployed and running
   - Core API, LangChain API, and Context API services are accessible
   - Azure Blob Storage is configured for the Skill resource provider
   - User Portal is accessible

2. **Configuration**
   - Add the Skill resource provider storage configuration:
     ```json
     "FoundationaLLM:ResourceProviders:Skill:Storage": {
       "AccountName": "<your-storage-account>",
       "ContainerName": "skills"
     }
     ```
   - Register the Skill resource provider in the relevant APIs

3. **Test User**
   - Have a valid Entra ID user account for testing
   - Note the user's UPN (e.g., `testuser@contoso.com`)

---

### 9.2 Test Case 1: Backwards Compatibility (Procedural Memory Disabled)

**Objective:** Verify the Code Interpreter works exactly as before when procedural memory is not enabled.

**Steps:**

1. **Create or select an agent WITHOUT procedural memory**
   - In Management Portal, create/edit an agent
   - Ensure `procedural_memory_settings` is either:
     - Not present in the agent configuration, OR
     - Present with `enabled: false`
   - Add the Code Interpreter tool to the agent

2. **Start a conversation**
   - Open User Portal
   - Select the agent
   - Start a new conversation

3. **Test code execution**
   - Send a message: *"Calculate the factorial of 10 using Python"*
   - **Expected:** Agent generates Python code, executes it, returns result (3628800)
   - **Verify:** No skill-related content artifacts appear in the response

4. **Verify no skill operations available**
   - Send a message: *"Search for factorial skills"*
   - **Expected:** Agent treats this as a regular prompt, may generate code or respond conversationally
   - **Verify:** No skill search is performed (backwards compatible behavior)

**Pass Criteria:** Code Interpreter functions identically to pre-implementation behavior.

---

### 9.3 Test Case 2: Enable Procedural Memory on Agent

**Objective:** Verify procedural memory can be enabled and configured on an agent.

**Steps:**

1. **Enable procedural memory on an agent**
   - In Management Portal, edit the test agent
   - Add procedural memory settings:
     ```json
     {
       "procedural_memory_settings": {
         "enabled": true,
         "auto_register_skills": true,
         "require_skill_approval": false,
         "max_skills_per_user": 0,
         "skill_search_threshold": 0.8,
         "prefer_skills": true
       }
     }
     ```
   - Save the agent

2. **Verify configuration saved**
   - Reload the agent in Management Portal
   - **Expected:** Procedural memory settings are persisted correctly

3. **Verify API reflects changes**
   - Call `GET /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}`
   - **Expected:** Response includes `procedural_memory_settings` with correct values

**Pass Criteria:** Agent configuration correctly saves and retrieves procedural memory settings.

---

### 9.4 Test Case 3: Register a New Skill

**Objective:** Verify a user can register code as a reusable skill.

**Steps:**

1. **Start a conversation with the procedural-memory-enabled agent**
   - Open User Portal
   - Select the agent with procedural memory enabled
   - Start a new conversation

2. **Generate and execute code**
   - Send: *"Write Python code to calculate compound interest given principal, rate, time, and compounding frequency"*
   - **Expected:** Agent generates and executes Python code successfully

3. **Register the code as a skill**
   - Send: *"Save that code as a skill called 'compound_interest_calculator' with description 'Calculates compound interest with configurable compounding frequency'"*
   - **Expected:** 
     - Agent registers the skill
     - Response includes a `skill_saved` content artifact
     - Artifact shows skill name, description, and code

4. **Verify skill_saved content artifact**
   - In the User Portal response, locate the skill artifact
   - **Expected fields:**
     - `skill_name`: "compound_interest_calculator"
     - `skill_description`: Contains the provided description
     - `skill_code`: The Python code
     - `skill_status`: "Active" (since `require_skill_approval` is false)
   - **Expected UI:** Shows "Approve" and "Reject" buttons

5. **Verify skill stored in resource provider**
   - Call `GET /instances/{instanceId}/skills` (as the test user)
   - **Expected:** Skill appears in the list with correct metadata

**Pass Criteria:** Skill is registered, stored, and displayed in content artifact.

---

### 9.5 Test Case 4: Search for Skills

**Objective:** Verify skill search functionality finds relevant skills.

**Steps:**

1. **Ensure at least one skill exists**
   - Complete Test Case 3 first, or verify a skill exists for the test user/agent

2. **Search for the skill**
   - In a new or existing conversation, send:
     *"Search for skills related to interest calculation"*
   - **Expected:** Response lists matching skills with similarity scores

3. **Search with no matches**
   - Send: *"Search for skills related to image processing"*
   - **Expected:** Response indicates no matching skills found

4. **Verify scoping**
   - Log in as a different user
   - Search for the same skill
   - **Expected:** Skill is NOT found (scoped to original user)

**Pass Criteria:** Skill search returns appropriate results based on query and user scoping.

---

### 9.6 Test Case 5: Use an Existing Skill

**Objective:** Verify a user can execute a previously saved skill.

**Steps:**

1. **Start with a registered skill**
   - Ensure "compound_interest_calculator" skill exists from Test Case 3

2. **Use the skill**
   - Send: *"Use the compound_interest_calculator skill with principal=1000, rate=0.05, time=10, frequency=12"*
   - **Expected:**
     - Skill code executes with the provided parameters
     - Result is returned (approximately $1647.01 for monthly compounding)
     - Response includes a `skill_used` content artifact

3. **Verify skill_used content artifact**
   - Locate the skill artifact in the response
   - **Expected fields:**
     - `skill_name`: "compound_interest_calculator"
     - `skill_code`: The stored Python code
     - `execution_count`: Incremented from previous value
   - **Expected UI:** Shows "Approve" (keep) and "Reject" (remove) buttons

4. **Verify execution count updated**
   - Call `GET /instances/{instanceId}/skills/{skillId}`
   - **Expected:** `execution_count` has increased by 1

**Pass Criteria:** Skill executes correctly and returns skill_used artifact.

---

### 9.7 Test Case 6: Skill Approval Workflow

**Objective:** Verify skill approval workflow when `require_skill_approval` is enabled.

**Steps:**

1. **Enable approval requirement**
   - Update agent configuration:
     ```json
     {
       "procedural_memory_settings": {
         "enabled": true,
         "require_skill_approval": true
       }
     }
     ```

2. **Register a new skill**
   - Send: *"Save this code as a skill called 'pending_test_skill': print('Hello World')"*
   - **Expected:**
     - Skill is created with status "PendingApproval"
     - Content artifact shows pending status
     - Message indicates skill is pending approval

3. **Verify skill cannot be used**
   - Send: *"Use the pending_test_skill skill"*
   - **Expected:** Error or message indicating skill is not active

4. **Approve the skill via User Portal**
   - Click "Approve" button on the skill_saved artifact
   - OR call `POST /instances/{instanceId}/skills/{skillId}/approve`
   - **Expected:** Skill status changes to "Active"

5. **Verify skill is now usable**
   - Send: *"Use the pending_test_skill skill"*
   - **Expected:** Skill executes successfully

**Pass Criteria:** Approval workflow correctly gates skill availability.

---

### 9.8 Test Case 7: Reject/Delete a Skill

**Objective:** Verify users can reject and remove skills.

**Steps:**

1. **Create a test skill**
   - Register a skill called "skill_to_delete"

2. **Reject via content artifact**
   - On the skill_saved or skill_used artifact, click "Reject"
   - **Expected:** Confirmation prompt appears

3. **Confirm rejection**
   - Confirm the rejection
   - **Expected:** 
     - Skill is deleted from storage
     - Success message displayed

4. **Verify skill removed**
   - Call `GET /instances/{instanceId}/skills`
   - **Expected:** "skill_to_delete" no longer appears

5. **Alternative: Reject via API**
   - Create another skill "skill_to_delete_via_api"
   - Call `DELETE /instances/{instanceId}/skills/{skillId}`
   - **Expected:** 200 OK with success message

**Pass Criteria:** Skills can be rejected/deleted via UI and API.

---

### 9.9 Test Case 8: Skill Scoping (Agent-User Combination)

**Objective:** Verify skills are correctly scoped to agent-user combination.

**Steps:**

1. **Create skill with User A on Agent 1**
   - Log in as User A
   - Use Agent 1 (procedural memory enabled)
   - Register skill "user_a_agent_1_skill"

2. **Verify User A can access on Agent 1**
   - Search for the skill
   - **Expected:** Skill is found

3. **Verify User A cannot access on Agent 2**
   - Switch to Agent 2 (also procedural memory enabled)
   - Search for "user_a_agent_1_skill"
   - **Expected:** Skill is NOT found (different agent)

4. **Verify User B cannot access User A's skill**
   - Log in as User B
   - Use Agent 1
   - Search for "user_a_agent_1_skill"
   - **Expected:** Skill is NOT found (different user)

5. **Verify User B can create own skill**
   - Register skill "user_b_agent_1_skill"
   - **Expected:** Skill is created successfully

6. **Verify both users' skills exist independently**
   - Check storage: both skills should exist with different `owner_user_id` values

**Pass Criteria:** Skills are isolated by agent-user combination.

---

### 9.10 Test Case 9: CoreAPI Skills Endpoints

**Objective:** Verify all SkillsController API endpoints function correctly.

**Steps:**

1. **GET /instances/{instanceId}/skills**
   - Call the endpoint as an authenticated user
   - **Expected:** Returns list of user's skills (empty or populated)

2. **GET /instances/{instanceId}/skills?agentObjectId={id}**
   - Call with agent filter
   - **Expected:** Returns only skills for that agent

3. **GET /instances/{instanceId}/skills/{skillId}**
   - Call with a valid skill ID
   - **Expected:** Returns skill details
   - Call with another user's skill ID
   - **Expected:** 403 Forbidden

4. **POST /instances/{instanceId}/skills/{skillId}/approve**
   - Call on a PendingApproval skill
   - **Expected:** Skill status becomes Active
   - Call on already Active skill
   - **Expected:** 400 Bad Request (not pending)

5. **DELETE /instances/{instanceId}/skills/{skillId}**
   - Call on own skill
   - **Expected:** Skill deleted, 200 OK
   - Call on another user's skill
   - **Expected:** 403 Forbidden

**Pass Criteria:** All API endpoints return correct responses and enforce authorization.

---

### 9.11 Test Case 10: Error Handling

**Objective:** Verify graceful error handling in edge cases.

**Steps:**

1. **Use non-existent skill**
   - Send: *"Use the skill called 'nonexistent_skill_12345'"*
   - **Expected:** Clear error message, no crash

2. **Register skill with invalid code**
   - Send: *"Save this as a skill: this is not valid python code {{{"*
   - **Expected:** Error during registration or execution, graceful handling

3. **Register duplicate skill name**
   - Register "duplicate_test" skill
   - Try to register another "duplicate_test" skill
   - **Expected:** Either overwrites (version increment) or returns conflict error

4. **Skill execution timeout**
   - Register a skill with an infinite loop: `while True: pass`
   - Try to use the skill
   - **Expected:** Execution times out gracefully, error message returned

5. **Resource provider unavailable**
   - (If possible) Disable Skill resource provider
   - Try skill operations
   - **Expected:** 503 Service Unavailable with clear message

**Pass Criteria:** All error cases handled gracefully with informative messages.

---

---

## 10. Storage Options Analysis

The current implementation creates a new Skill resource provider with its own storage configuration. However, to minimize configuration overhead, we can leverage existing storage infrastructure. This section analyzes the available options.

### 10.1 Option A: Store Skills in Cosmos DB (Recommended)

**Approach:** Add skill storage to the existing `IAzureCosmosDBService` interface, similar to how Attachments and Agent Files are already stored.

**Implementation:**
1. Add a new `SkillReference` model (similar to `AttachmentReference`)
2. Add methods to `IAzureCosmosDBService`:
   - `GetSkillAsync(string upn, string agentObjectId, string skillId)`
   - `GetSkillsAsync(string upn, string agentObjectId)` 
   - `UpsertSkillAsync(SkillReference skill)`
   - `DeleteSkillAsync(SkillReference skill)`
   - `SearchSkillsAsync(string upn, string agentObjectId, ReadOnlyMemory<float> embedding, double minSimilarity)`

3. Store skills in a `skills` container with partition key `/{upn}/{agentObjectId}`

**Pros:**
- Uses existing Cosmos DB already configured for Core API
- Supports vector search for semantic skill discovery (already has `CreateVectorSearchContainerAsync`)
- Follows established patterns (Attachments, Agent Files use same approach)
- No new storage account configuration required
- User-scoped data naturally partitioned by UPN

**Cons:**
- Requires changes to `IAzureCosmosDBService` and `AzureCosmosDBService`
- Skills not managed through resource provider pattern

**Required Changes:**
```csharp
// Add to IAzureCosmosDBService.cs
Task<SkillReference?> GetSkillAsync(string upn, string agentObjectId, string id, CancellationToken cancellationToken = default);
Task<List<SkillReference>> GetSkillsAsync(string upn, string agentObjectId, CancellationToken cancellationToken = default);
Task UpsertSkillAsync(SkillReference skill, CancellationToken cancellationToken = default);
Task DeleteSkillAsync(SkillReference skill, CancellationToken cancellationToken = default);
Task<List<SkillSearchResult>> SearchSkillsAsync(string upn, string agentObjectId, ReadOnlyMemory<float> embedding, double minSimilarity, CancellationToken cancellationToken = default);
```

---

### 10.2 Option B: Add Skills as Agent Resource Provider Sub-Resource

**Approach:** Add `skills` as a new resource type under the existing `FoundationaLLM.Agent` resource provider.

**Implementation:**
1. Add `AgentResourceTypeNames.Skills = "skills"`
2. Extend `AgentResourceProviderMetadata` with skills resource type
3. Store skills in the existing Agent provider blob storage
4. Skills accessed via `/instances/{id}/providers/FoundationaLLM.Agent/agents/{agentName}/skills/{skillId}`

**Pros:**
- Uses existing Agent resource provider storage (no new config)
- Follows resource provider pattern
- Skills naturally associated with agents

**Cons:**
- Skills conceptually belong to users, not agents
- Requires modifying Agent resource provider (more complex)
- Agent provider storage is agent-centric, not user-centric
- Would need custom logic for user-scoping within agent storage

**Required Changes:**
- Modify `AgentResourceTypeNames.cs`
- Modify `AgentResourceProviderMetadata.cs`
- Modify `AgentResourceProviderService.cs` (significant changes)

---

### 10.3 Option C: Use Context Resource Provider Storage

**Approach:** Store skills using the Context resource provider's storage, since Context already handles code-related resources.

**Implementation:**
1. Add `ContextResourceTypeNames.Skills = "skills"`
2. Extend Context provider to handle skill resources
3. Use existing Context provider blob storage

**Pros:**
- Context already handles code session data
- Conceptual alignment (code-related resources)
- No new storage configuration

**Cons:**
- Context focuses on knowledge units/sources, not procedural skills
- Would mix unrelated resource types
- Context provider may not be deployed in all configurations

---

### 10.4 Option D: Store Skills in User Profile (Cosmos DB)

**Approach:** Extend the existing user profile/data storage in Cosmos DB to include skills.

**Implementation:**
1. Add `Skills` property to `UserData` model
2. Skills stored as part of user's data document
3. Access via existing `GetUserDataAsync` / `UpsertUserDataAsync`

**Pros:**
- No new storage configuration
- User-centric storage (natural fit for user-owned skills)
- Very simple implementation

**Cons:**
- Limited scalability (all skills in one document)
- No vector search capability for semantic matching
- Document size limits could be hit with many skills
- Not suitable for large code blocks

---

### 10.5 Recommendation

**Option A (Cosmos DB with dedicated container)** is recommended for the following reasons:

1. **Vector Search Support:** Cosmos DB vector search is already configured and can be used for semantic skill discovery
2. **Scalability:** Dedicated container scales independently
3. **Existing Pattern:** Follows the same pattern as Attachments and Agent Files
4. **No New Configuration:** Uses the existing Core API Cosmos DB connection
5. **Natural Partitioning:** Partition by `{upn}/{agentObjectId}` for efficient queries

**Migration Path from Current Implementation:**

1. Remove the separate Skill resource provider project
2. Add skill methods to `IAzureCosmosDBService`
3. Implement skill storage in `AzureCosmosDBService`
4. Update `SkillsController` to use Cosmos DB service directly
5. Update Python code interpreter to call Core API skill endpoints
6. Remove skill storage configuration from `AppConfigurationKeySections`

**Storage Schema:**
```json
{
  "id": "compound_interest_MAA-02_user@example.com",
  "type": "skill",
  "upn": "user@example.com",
  "agentObjectId": "/instances/abc/providers/FoundationaLLM.Agent/agents/MAA-02",
  "name": "compound_interest_calculator",
  "description": "Calculates compound interest...",
  "code": "def calculate_compound_interest(...):\n    ...",
  "status": "Active",
  "executionCount": 15,
  "successRate": 0.98,
  "embedding": [0.123, -0.456, ...],
  "createdOn": "2025-01-17T10:00:00Z",
  "updatedOn": "2025-01-17T12:00:00Z"
}
```

---

### 10.6 Implementation Steps for Option A

1. **Create SkillReference model** (in Common project):
```csharp
public class SkillReference
{
    public string Id { get; set; }
    public string Type { get; set; } = "skill";
    public string UPN { get; set; }
    public string AgentObjectId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Code { get; set; }
    public SkillStatus Status { get; set; }
    public int ExecutionCount { get; set; }
    public double SuccessRate { get; set; }
    public float[]? Embedding { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
```

2. **Add methods to IAzureCosmosDBService** (interface changes)

3. **Implement in AzureCosmosDBService** (Core project)

4. **Update SkillsController** to inject `IAzureCosmosDBService` instead of resource provider

5. **Create Cosmos DB container** via configuration or startup:
   - Container name: `skills`
   - Partition key: `/upn`
   - Vector index on `/embedding` property

6. **Remove Skill resource provider** files and configuration

---

### 9.12 Verification Checklist

Use this checklist to confirm all functionality:

| Test | Status | Notes |
|------|--------|-------|
| Backwards compatibility (PM disabled) | ☐ | |
| Enable procedural memory on agent | ☐ | |
| Register new skill | ☐ | |
| skill_saved content artifact displays | ☐ | |
| Search for skills | ☐ | |
| Use existing skill | ☐ | |
| skill_used content artifact displays | ☐ | |
| Approval workflow (when enabled) | ☐ | |
| Reject skill via UI | ☐ | |
| Reject skill via API | ☐ | |
| Skill scoping by user | ☐ | |
| Skill scoping by agent | ☐ | |
| GET /skills endpoint | ☐ | |
| GET /skills/{id} endpoint | ☐ | |
| POST /skills/{id}/approve endpoint | ☐ | |
| DELETE /skills/{id} endpoint | ☐ | |
| Error handling | ☐ | |

---

*Document Version: 2.0*
*Created: January 2025*
*Last Updated: January 2025*
*Status: Implementation Complete*

**Revision History:**
- v2.0 (Jan 2025): **Implementation complete** - All phases implemented:
  - Phase 1: Skill resource model (Skill.cs, SkillParameter.cs, SkillStatus.cs, SkillSearchRequest.cs, SkillSearchResult.cs)
  - Phase 2: ProceduralMemorySettings added to AgentBase
  - Phase 3: Code Interpreter tool enhanced with skill operations (search_skills, use_skill, register_skill)
  - Phase 4: SkillResourceProviderService and DependencyInjection
  - Phase 5: CoreAPI SkillsController for User Portal skill review
  - Content artifacts for skill_saved and skill_used implemented
  - Backwards compatibility maintained when procedural memory is disabled
- v1.3 (Jan 2025): Added `skill_used` content artifact for when existing skills are executed; users can review which skill was used and remove it if desired; updated Use Skill operation to return content artifact
- v1.2 (Jan 2025): Added skill registration content artifact with User Portal review UI; users can approve/reject skills directly from conversation; added Phase 5 for User Portal implementation; added CoreAPI endpoints for skill review
- v1.1 (Jan 2025): Finalized design decisions for skill scoping (agent-user), approval workflow (auto-approve by default, configurable), security (sandbox only), and backwards compatibility requirement
- v1.0 (Jan 2025): Initial plan draft
