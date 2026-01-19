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
7. [Management Portal Configuration UI](#7-management-portal-configuration-ui)
8. [Implementation Phases](#8-implementation-phases)
9. [Alternative Approaches](#9-alternative-approaches)
10. [Manual Testing and Verification Guide](#10-manual-testing-and-verification-guide)
11. [Storage Options Analysis](#11-storage-options-analysis)
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

#### 2.2.1 Code Interpreter Tool Properties

The Code Interpreter tool currently uses these properties in the Management Portal:

| Property Name | Type | Description |
|---------------|------|-------------|
| `code_session_required` | `bool` | Whether the tool requires a code session |
| `code_session_endpoint_provider` | `string` | The code session endpoint provider name |
| `code_session_language` | `string` | The programming language (e.g., "python") |

**Example Tool Configuration:**
```json
{
  "name": "code-interpreter",
  "description": "Execute Python code in a sandboxed environment",
  "package_name": "foundationallm_agent_plugins",
  "class_name": "FoundationaLLMCodeInterpreterTool",
  "properties": {
    "code_session_required": true,
    "code_session_endpoint_provider": "AzureContainerAppsCustomContainer",
    "code_session_language": "Python"
  }
}
```

**Management Portal UI:** These properties are configured in the "Configure Tool" dialog when adding or editing the Code Interpreter tool on an agent.

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
│      │      │ User provides prompt                                       │
│      │      │                                                            │
│      │      ├─► INTERNAL: Search for relevant skills                     │
│      │      ├─► INTERNAL: Use skill if found and suitable                 │
│      │      ├─► INTERNAL: Generate new code if no skill found             │
│      │      └─► INTERNAL: Register successful code as skill (optional)    │
│      │                                                                   │
│      └─► (Existing tools: Knowledge, KQL, SQL, etc.)                     │
│                                                                          │
│   NEW: Skill Storage (Cosmos DB via Core API)                            │
│      │                                                                   │
│      ├─► Store skill definitions (code + metadata)                       │
│      ├─► Skill search via embedding similarity                           │
│      └─► Skill versioning and lifecycle                                  │
│                                                                          │
│   Existing: PythonCodeSessionAPI (UNCHANGED)                             │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Key Design Principle:** The Code Interpreter Tool manages skill operations internally. The agent simply provides a prompt, and the tool autonomously decides whether to search for skills, use existing skills, generate new code, or register successful code as a skill. This keeps the tool interface simple and backwards-compatible.

**Important:** Even though skill operations are internal, the tool still returns content artifacts (`skill_saved` and `skill_used`) that allow users to review, approve, or reject skills directly from the User Portal. This provides transparency and user control over the skill library.

### 3.2 Key Design Decisions

#### Internal Skill Management (Selected Approach)

The Code Interpreter Tool handles all skill operations internally. When procedural memory is enabled, the tool:

1. **Automatically searches for relevant skills** when given a prompt
2. **Uses a skill if found** and the similarity exceeds the threshold
3. **Generates new code** if no suitable skill is found
4. **Optionally registers successful code** as a skill after execution

**Key Benefits:**
- **Simple interface**: Agent only needs to provide a prompt (backwards compatible)
- **Autonomous decision-making**: Tool decides when to use skills vs. generate code
- **No agent orchestration required**: Agent doesn't need to explicitly call search/use/register operations
- **Backwards compatible**: When procedural memory is disabled, tool works exactly as before

**Implementation:**
- Tool interface remains unchanged: `prompt` and `file_names` parameters only
- Skill operations (`search_skills`, `use_skill`, `register_skill`) are internal methods
- Tool uses procedural memory settings to determine behavior
- Agent prompt doesn't need to mention skill operations (tool handles them automatically)

### 3.3 Backwards Compatibility Requirement

**Critical Requirement:** The Code Interpreter tool must remain fully backwards compatible. If `ProceduralMemorySettings` is not enabled on an agent, the tool must work exactly as it does today with no changes in behavior.

**Implementation Strategy:**
1. All skill-related operations are gated by checking `procedural_memory_settings.enabled`
2. If disabled, the tool executes code directly (original behavior)
3. **No new parameters are added to the tool interface** - only `prompt` and `file_names` (existing parameters)
4. Skill operations are internal implementation details, not exposed in the tool interface

```python
# Backwards compatibility check in the tool
async def _arun(self, prompt, file_names=[]):
    # Check if procedural memory is enabled for this agent
    procedural_memory_enabled = self._get_procedural_memory_enabled()
    
    if not procedural_memory_enabled:
        # Original behavior - generate and execute code
        return await self._execute_code(prompt, file_names)
    
    # Procedural memory enabled - internal skill management
    # 1. Search for relevant skills
    matching_skills = await self._search_skills_internal(prompt)
    
    # 2. Use skill if found and similarity > threshold
    if matching_skills and matching_skills[0]['similarity'] > threshold:
        return await self._use_skill_internal(matching_skills[0], prompt, file_names)
    
    # 3. Generate and execute new code
    result = await self._execute_code(prompt, file_names)
    
    # 4. Optionally register successful code as skill
    if result.successful and auto_register_enabled:
        await self._register_skill_internal(prompt, result.code)
    
    return result
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

**Decision:** When a skill is registered (even though registration is handled internally by the tool), the tool returns a **new content artifact type (`skill_saved`)** that allows users to review and approve/reject the skill directly from the User Portal.

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
│  1. Tool internally registers a skill (after successful code execution)  │
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

**Decision:** When an existing skill is used (even though skill usage is handled internally by the tool), the tool returns a **`skill_used` content artifact** that allows users to see which skill was executed and approve or reject it.

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
│  1. Tool internally uses an existing skill (found via search)           │
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

### 5.3 Procedural Memory Settings in Tool Properties

**Decision:** Procedural memory settings are stored as a tool property on the Code Interpreter tool, not on the agent itself. This keeps skill learning configuration scoped to the specific tool.

**Implementation:** Add a new tool property `procedural_memory_settings` that contains a JSON object with the procedural memory configuration.

#### 5.3.1 Tool Property Structure

```json
{
  "name": "code-interpreter",
  "description": "Execute Python code in a sandboxed environment",
  "package_name": "foundationallm_agent_plugins",
  "class_name": "FoundationaLLMCodeInterpreterTool",
  "properties": {
    "code_session_required": true,
    "code_session_endpoint_provider": "ContextAPI",
    "code_session_language": "python",
    "procedural_memory_settings": {
      "enabled": true,
      "auto_register_skills": true,
      "require_skill_approval": false,
      "max_skills_per_user": 0,
      "skill_search_threshold": 0.8,
      "prefer_skills": true
    }
  }
}
```

#### 5.3.2 Tool Property Constant

Add a constant for the new property name:

```csharp
// Add to src/dotnet/Common/Constants/Agents/AgentToolPropertyNames.cs

/// <summary>
/// Procedural memory settings for the Code Interpreter tool.
/// Contains a JSON object with ProceduralMemorySettings configuration.
/// </summary>
public const string ProceduralMemorySettings = "procedural_memory_settings";
```

#### 5.3.3 Tool Reading Settings

The `FoundationaLLMCodeInterpreterTool` reads procedural memory settings from tool properties:

```python
# In FoundationaLLMCodeInterpreterTool.__init__()
def _get_procedural_memory_settings(self) -> Optional[Dict[str, Any]]:
    """Get procedural memory settings from tool properties."""
    if not self.tool_config or not self.tool_config.properties:
        return None
    
    # Get procedural_memory_settings from tool properties
    pm_settings_json = self.tool_config.properties.get('procedural_memory_settings')
    if not pm_settings_json:
        return None
    
    # Parse JSON if it's a string, otherwise use directly
    if isinstance(pm_settings_json, str):
        import json
        pm_settings_json = json.loads(pm_settings_json)
    
    return pm_settings_json
```

**Note:** The `ProceduralMemorySettings` class on `AgentBase` (Section 5.2) is kept for reference but is not used. Settings come from tool properties instead.

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

### 6.1 Code Interpreter Tool Interface (Simplified)

The code interpreter tool maintains a simple interface - skill operations are handled internally:

```python
# src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py

class FoundationaLLMCodeInterpreterToolInput(BaseModel):
    """Input schema for the code interpreter tool.
    
    When procedural memory is enabled, the tool automatically:
    - Searches for relevant skills
    - Uses skills if found and suitable
    - Generates new code if no skill found
    - Optionally registers successful code as a skill
    """
    
    # Existing parameters (unchanged for backwards compatibility)
    prompt: str = Field(description="The task description or code to execute")
    file_names: Optional[List[str]] = Field(default=[], description="Files to make available")
    
    # Note: No skill-related parameters exposed in the interface
    # Skill operations (search, use, register) are internal implementation details
```

**Key Design:** The tool interface remains unchanged. Skill management is an internal implementation detail that doesn't affect the tool's external API.

**User Review via Content Artifacts:** Even though skill operations are internal, the tool returns content artifacts (`skill_saved` when a skill is registered, `skill_used` when a skill is executed) that enable users to review, approve, or reject skills directly from the User Portal. This provides transparency and user control without requiring the agent to explicitly manage skill operations.

### 6.2 Tool Internal Flow (When Procedural Memory Enabled)

The tool handles skill operations internally. The agent simply provides a prompt:

#### Example: Agent Calls Tool
```python
# Agent calls tool with (simple interface):
{
    "prompt": "calculate monthly revenue growth from sales data",
    "file_names": ["sales_2024.csv"]
}
```

#### Internal Tool Flow

**Step 1: Search for Skills (Internal)**
```python
# Tool internally searches for relevant skills
matching_skills = await self._search_skills_internal("calculate monthly revenue growth from sales data")
# Returns: [{"name": "calculate_revenue_growth", "similarity": 0.92, ...}]
```

**Step 2: Use Skill if Found (Internal)**
```python
# If similarity > threshold (e.g., 0.8), tool uses the skill
if matching_skills[0]['similarity'] > 0.8:
    result = await self._use_skill_internal(matching_skills[0], prompt, file_names)
    # Returns execution results + skill_used content artifact
```

**Step 3: Generate Code if No Skill (Internal)**
```python
# If no suitable skill found, tool generates new code
if not matching_skills or matching_skills[0]['similarity'] <= 0.8:
    result = await self._execute_code(prompt, file_names)
    # Standard code generation and execution
```

**Step 4: Register Successful Code (Internal, Optional)**
```python
# After successful execution, tool may register code as skill
if result.successful and auto_register_enabled:
    registration_result = await self._register_skill_internal(prompt, result.code, result.description)
    # Returns skill_saved content artifact which is merged into the tool result
    # This allows users to review and approve/reject the skill in the User Portal
    result.content_artifacts.extend(registration_result.content_artifacts)
```

#### Tool Response (Same for All Cases)

The tool returns a consistent response format regardless of whether it used a skill or generated new code:

```python
# Text response (execution results):
"The monthly revenue growth rates are: Jan: 5.2%, Feb: -1.3%, Mar: 8.7%..."

# Content Artifacts (if skill was used or registered):
[
    ContentArtifact(
        id="skill_used",  # or "skill_saved"
        title="Skill Used: calculate_revenue_growth",
        type="skill_used",
        filepath="calculate_revenue_growth_MAA-02_user",
        metadata={
            "skill_name": "calculate_revenue_growth",
            "skill_description": "Calculates month-over-month revenue growth...",
            "skill_code": "def calculate_revenue_growth(...):\n    ...",
            "skill_status": "Active",
            "execution_count": 48,
            "success_rate": 0.98,
            ...
        }
    )
]
```

**Key Points:**
1. The agent doesn't need to know whether a skill was used or new code was generated. The tool handles this internally.
2. **Content artifacts are always returned** when skills are used or registered, allowing users to review and approve/reject skills in the User Portal (see Sections 4.6 and 4.7).
3. The User Portal displays these artifacts with "Approve/Reject" or "Keep/Remove" buttons for user control.

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

## 7. Management Portal Configuration UI

This section provides detailed wireframes and UX specifications for configuring procedural memory settings on the Code Interpreter tool in the Management Portal.

### 7.1 Configure Tool Dialog - Procedural Memory Section

When editing the Code Interpreter tool in the Management Portal, a new "Procedural Memory" section appears below the existing tool properties.

#### 7.1.1 Tool Properties Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Configure Tool Dialog                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Tool name: [code-interpreter                          ]                │
│                                                                          │
│  Tool description: [Execute Python code in a sandboxed...]             │
│                                                                          │
│  Tool package name: [foundationallm_agent_plugins      ]                │
│                                                                          │
│  Tool class name: [FoundationaLLMCodeInterpreterTool   ]                │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│  Tool Properties:                                                        │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ Property Name              │ Property Value                      │  │
│  ├──────────────────────────────────────────────────────────────────┤  │
│  │ code_session_required      │ true                                │  │
│  │ code_session_endpoint_...  │ ContextAPI                          │  │
│  │ code_session_language       │ python                              │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  [Add Property]                                                          │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│  🔧 Procedural Memory Settings                                          │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │ ☑ Enable Procedural Memory                                       │  │
│  │                                                                   │  │
│  │ When enabled, the Code Interpreter tool will automatically:      │  │
│  │ • Search for relevant skills before generating new code          │  │
│  │ • Use existing skills when found and suitable                    │  │
│  │ • Optionally register successful code as reusable skills         │  │
│  │                                                                   │  │
│  │ ┌─────────────────────────────────────────────────────────────┐ │  │
│  │ │ ☑ Auto-Register Skills                                       │ │  │
│  │ │   Automatically save successful code executions as skills    │ │  │
│  │ └─────────────────────────────────────────────────────────────┘ │  │
│  │                                                                   │  │
│  │ ┌─────────────────────────────────────────────────────────────┐ │  │
│  │ │ ☐ Require Skill Approval                                    │ │  │
│  │ │   New skills require admin approval before becoming active  │ │  │
│  │ └─────────────────────────────────────────────────────────────┘ │  │
│  │                                                                   │  │
│  │ Max Skills Per User: [0        ] (0 = unlimited)                │  │
│  │                                                                   │  │
│  │ Skill Search Threshold: [━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━] 0.80 │  │
│  │   Minimum similarity score (0.0 to 1.0) for skill matching     │  │
│  │                                                                   │  │
│  │ ☑ Prefer Skills Over New Code                                   │  │
│  │   When enabled, tool will prefer using existing skills          │  │
│  │   over generating new code when similarity threshold is met      │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│  [Save]  [Close]                                                        │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

#### 7.1.2 Procedural Memory Section - Disabled State

When "Enable Procedural Memory" is unchecked, all other settings are disabled:

```
┌──────────────────────────────────────────────────────────────────┐
│ 🔧 Procedural Memory Settings                                    │
│                                                                   │
│ ☐ Enable Procedural Memory                                       │
│                                                                   │
│ [All other settings grayed out and disabled]                    │
└──────────────────────────────────────────────────────────────────┘
```

#### 7.1.3 Procedural Memory Section - Enabled State

When "Enable Procedural Memory" is checked, all settings become available:

```
┌──────────────────────────────────────────────────────────────────┐
│ 🔧 Procedural Memory Settings                                    │
│                                                                   │
│ ☑ Enable Procedural Memory                                       │
│                                                                   │
│ When enabled, the Code Interpreter tool will automatically:      │
│ • Search for relevant skills before generating new code          │
│ • Use existing skills when found and suitable                    │
│ • Optionally register successful code as reusable skills         │
│                                                                   │
│ ┌─────────────────────────────────────────────────────────────┐  │
│ │ ☑ Auto-Register Skills                                       │  │
│ │   Automatically save successful code executions as skills    │  │
│ └─────────────────────────────────────────────────────────────┘  │
│                                                                   │
│ ┌─────────────────────────────────────────────────────────────┐  │
│ │ ☐ Require Skill Approval                                    │  │
│ │   New skills require admin approval before becoming active  │  │
│ └─────────────────────────────────────────────────────────────┘  │
│                                                                   │
│ Max Skills Per User: [0        ] (0 = unlimited)                │
│                                                                   │
│ Skill Search Threshold: [━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━] 0.80 │
│   Minimum similarity score (0.0 to 1.0) for skill matching      │
│                                                                   │
│ ☑ Prefer Skills Over New Code                                   │
│   When enabled, tool will prefer using existing skills          │
│   over generating new code when similarity threshold is met      │
└──────────────────────────────────────────────────────────────────┘
```

### 7.2 Tool Properties JSON Structure

When saved, the procedural memory settings are stored as a JSON object in the tool properties:

```json
{
  "code_session_required": true,
  "code_session_endpoint_provider": "ContextAPI",
  "code_session_language": "python",
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

### 7.3 Implementation Details

#### 7.3.1 Component Detection

The `ConfigureToolDialog` component should detect when the Code Interpreter tool is being configured:

```typescript
// In ConfigureToolDialog.vue
const isCodeInterpreterTool = computed(() => {
  return toolObject.package_name === 'foundationallm_agent_plugins' &&
         toolObject.class_name === 'FoundationaLLMCodeInterpreterTool';
});
```

#### 7.3.2 Settings Initialization

When loading an existing tool, parse the `procedural_memory_settings` property:

```typescript
// In ConfigureToolDialog.vue
const proceduralMemorySettings = ref({
  enabled: false,
  auto_register_skills: true,
  require_skill_approval: false,
  max_skills_per_user: 0,
  skill_search_threshold: 0.8,
  prefer_skills: true
});

// On tool load
if (toolObject.properties?.procedural_memory_settings) {
  const settings = toolObject.properties.procedural_memory_settings;
  if (typeof settings === 'string') {
    proceduralMemorySettings.value = JSON.parse(settings);
  } else {
    proceduralMemorySettings.value = { ...proceduralMemorySettings.value, ...settings };
  }
}
```

#### 7.3.3 Settings Persistence

When saving the tool, serialize the settings to JSON:

```typescript
// In ConfigureToolDialog.vue - handleSave()
if (isCodeInterpreterTool.value) {
  toolObject.properties = toolObject.properties || {};
  toolObject.properties.procedural_memory_settings = JSON.stringify(proceduralMemorySettings.value);
}
```

### 7.4 Validation Rules

| Setting | Validation Rule | Error Message |
|---------|----------------|---------------|
| `skill_search_threshold` | Must be between 0.0 and 1.0 | "Skill search threshold must be between 0.0 and 1.0" |
| `max_skills_per_user` | Must be >= 0 | "Max skills per user must be 0 or greater (0 = unlimited)" |

### 7.5 User Experience Flow

1. **Admin opens agent configuration** in Management Portal
2. **Clicks "Edit" on Code Interpreter tool** (or adds new tool)
3. **ConfigureToolDialog opens** with tool details
4. **Scrolls to "Procedural Memory Settings" section** (appears only for Code Interpreter tool)
5. **Enables procedural memory** by checking the toggle
6. **Configures settings**:
   - Enables/disables auto-register
   - Sets approval requirement
   - Sets max skills limit
   - Adjusts search threshold slider
   - Sets prefer skills option
7. **Clicks "Save"** - settings are stored as JSON in tool properties
8. **Tool is ready** - Code Interpreter will use procedural memory when agent runs

### 7.6 Backwards Compatibility

- **Existing tools without `procedural_memory_settings`**: Work exactly as before (procedural memory disabled)
- **Tools with `procedural_memory_settings.enabled = false`**: Work exactly as before
- **Non-Code-Interpreter tools**: Don't show procedural memory section (not applicable)

---

## 8. Implementation Phases

> **Note:** This implementation uses **Cosmos DB storage** (Option A) instead of a separate Skill resource provider. Skills are stored in the existing Core API Cosmos DB database, eliminating the need for additional storage configuration.

### Phase 1: Skill Data Model & Cosmos DB Storage (Foundation)

**Goal:** Create the skill data model and Cosmos DB storage infrastructure.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Create `SkillReference` model for Cosmos DB storage
2. ✅ Create `SkillStatus` enum and `SkillParameter` class
3. ✅ Add skill methods to `IAzureCosmosDBService` interface
4. ✅ Implement skill CRUD operations in Core API

**Files Created/Modified:**
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillReference.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillParameter.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillStatus.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillSearchRequest.cs` (NEW)
- `src/dotnet/Common/Models/ResourceProviders/Skill/SkillSearchResult.cs` (NEW)
- `src/dotnet/Common/Interfaces/IAzureCosmosDBService.cs` (MODIFIED)

### Phase 2: Tool Configuration Structure

**Goal:** Define procedural memory settings structure for tool properties.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Create `ProceduralMemorySettings` class (data model reference)
2. ✅ Add `ProceduralMemorySettings` constant to `AgentToolPropertyNames`
3. ✅ Update tool to read settings from tool properties

**Files Created/Modified:**
- `src/dotnet/Common/Models/ResourceProviders/Agent/ProceduralMemorySettings.cs` (NEW - reference model)
- `src/dotnet/Common/Constants/Agents/AgentToolPropertyNames.cs` (MODIFIED - add constant)
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py` (MODIFIED - read from properties)

**Key Design:**
- Settings are stored in tool properties, not on the agent
- Tool reads `procedural_memory_settings` property from `tool_config.properties`
- Settings are scoped to the specific tool instance
- When `procedural_memory_settings` is missing or `enabled` is false, tool works in backwards-compatible mode

**Tool Property Reading Implementation:**
```python
# In FoundationaLLMCodeInterpreterTool.__init__()
# NOTE: Current implementation reads from agent object, but should be updated
# to read from tool_config.properties instead.

def _get_procedural_memory_enabled(self) -> bool:
    """Check if procedural memory is enabled from tool properties."""
    if not self.tool_config or not self.tool_config.properties:
        return False
    
    pm_settings = self.tool_config.properties.get('procedural_memory_settings')
    if not pm_settings:
        return False
    
    # Parse JSON if string, otherwise use directly
    if isinstance(pm_settings, str):
        import json
        pm_settings = json.loads(pm_settings)
    
    return pm_settings.get('enabled', False) if isinstance(pm_settings, dict) else False

def _get_procedural_memory_settings(self) -> Optional[Dict[str, Any]]:
    """Get procedural memory settings from tool properties."""
    if not self.tool_config or not self.tool_config.properties:
        return None
    
    pm_settings = self.tool_config.properties.get('procedural_memory_settings')
    if not pm_settings:
        return None
    
    # Parse JSON if string, otherwise use directly
    if isinstance(pm_settings, str):
        import json
        pm_settings = json.loads(pm_settings)
    
    if not isinstance(pm_settings, dict):
        return None
    
    return {
        'enabled': pm_settings.get('enabled', False),
        'auto_register_skills': pm_settings.get('auto_register_skills', True),
        'require_skill_approval': pm_settings.get('require_skill_approval', False),
        'max_skills_per_user': pm_settings.get('max_skills_per_user', 0),
        'skill_search_threshold': pm_settings.get('skill_search_threshold', 0.8),
        'prefer_skills': pm_settings.get('prefer_skills', True),
    }
```

**Migration Note:** The current implementation reads from `self.agent.procedural_memory_settings`. This should be updated to read from `self.tool_config.properties['procedural_memory_settings']` instead.

### Phase 3: Code Interpreter Enhancement

**Goal:** Add internal skill management to the code interpreter tool while maintaining 100% backwards compatibility.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Add procedural memory enabled check at tool initialization
2. ✅ Simplify `FoundationaLLMCodeInterpreterToolInput` - removed operation and skill parameters
3. ✅ Implement `_execute_with_procedural_memory()` method for internal skill management
4. ✅ Implement `_search_skills_internal()` - internal skill search (returns structured data)
5. ✅ Implement `_use_skill_internal()` - internal skill execution
6. ✅ Implement `_register_skill_internal()` - internal skill registration
7. ✅ Add helper methods for skill name/description generation
8. ✅ Add `skill_saved` and `skill_used` content artifacts

**Files Modified:**
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py`
- `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool_input.py`

**Key Design Change:**
- **Tool interface simplified**: Only `prompt` and `file_names` parameters (no `operation`, `skill_name`, etc.)
- **Skill operations are internal**: Tool automatically searches, uses, and registers skills
- **Agent doesn't need to orchestrate**: Agent just calls tool with prompt, tool handles skill management
- **Content artifacts still returned**: Tool returns `skill_saved` and `skill_used` artifacts for user review/approval in User Portal

**Backwards Compatibility Test Cases:**
- Agent without `procedural_memory_settings` → tool works as before
- Agent with `procedural_memory_settings.enabled = false` → tool works as before
- Existing tool calls with only `prompt` and `file_names` → work unchanged
- No new required parameters added to tool interface

### Phase 4: Core API Skills Controller

**Goal:** Provide REST API endpoints for skill management in User Portal.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Create `SkillsController` with Cosmos DB integration
2. ✅ Implement GET /skills (list user's skills)
3. ✅ Implement GET /skills/{skillId} (get skill details)
4. ✅ Implement POST /skills (create/update skill)
5. ✅ Implement POST /skills/{skillId}/approve (approve pending skill)
6. ✅ Implement DELETE /skills/{skillId} (reject/delete skill)
7. ✅ Implement POST /skills/{skillId}/execute (record execution stats)

**Files Created:**
- `src/dotnet/CoreAPI/Controllers/SkillsController.cs` (NEW)

### Phase 5: Cosmos DB Service Implementation

**Goal:** Implement the skill methods in `AzureCosmosDBService`.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Implement `GetSkillAsync` method
2. ✅ Implement `GetSkillsAsync` method
3. ✅ Implement `UpsertSkillAsync` method
4. ✅ Implement `DeleteSkillAsync` method
5. ✅ Implement `UpdateSkillExecutionAsync` method
6. ✅ Create `skills` container in Cosmos DB (created automatically on first use)

**Files Modified:**
- `src/dotnet/Common/Services/Azure/AzureCosmosDBService.cs`
- `src/dotnet/Common/Constants/AzureCosmosDBContainers.cs`

**Container Configuration:**
- Name: `Skills`
- Partition key: `/upn`
- Throughput: Autoscale 1000 RU/s
- Creation: Automatic on first use (no deployment prerequisite)

### Phase 6: Management Portal UI - Tool Configuration

**Goal:** Allow Code Interpreter tool to be configured with procedural memory settings in the Management Portal.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ **Update ConfigureToolDialog component** to show procedural memory settings when Code Interpreter tool is selected:
   - ✅ Detect when tool is Code Interpreter (package_name: "foundationallm_agent_plugins", class_name: "FoundationaLLMCodeInterpreterTool")
   - ✅ Show "Procedural Memory" section with visual divider
   - ✅ Add enable/disable toggle (InputSwitch)
   - ✅ Add auto-register skills toggle
   - ✅ Add require approval toggle
   - ✅ Add max skills per user input (InputNumber field)
   - ✅ Add skill search threshold slider (0.0 to 1.0) with live value display
   - ✅ Add prefer skills toggle
   - ✅ Store settings as JSON object in `procedural_memory_settings` tool property

2. ✅ **Add constant** for procedural memory settings property name:
   - ✅ Added `ProceduralMemorySettings` constant to `AgentToolPropertyNames.cs`

3. ✅ **Add validation** for procedural memory settings:
   - ✅ Skill search threshold must be between 0.0 and 1.0
   - ✅ Max skills per user must be >= 0 (0 = unlimited)
   - ✅ Validation only runs when procedural memory is enabled

4. ✅ **Implement settings initialization and persistence**:
   - ✅ Initialize settings from tool properties on component load
   - ✅ Handle both JSON string and object formats
   - ✅ Persist settings to tool properties on save
   - ✅ Preserve settings even when disabled (for future re-enabling)

**Files Modified:**
- ✅ `src/ui/ManagementPortal/components/ConfigureToolDialog.vue` - Added procedural memory settings UI section
- ✅ `src/dotnet/Common/Constants/Agents/AgentToolPropertyNames.cs` - Added `ProceduralMemorySettings` constant

**Implementation Details:**
- Component detection via computed property `isCodeInterpreterTool`
- Settings initialized from `toolObject.properties['procedural_memory_settings']`
- Settings persisted as JSON string in tool properties
- UI matches wireframes from Section 7.1
- All settings disabled when procedural memory is not enabled
- Backwards compatible: existing tools without settings work as before

**UX Wireframes:** See Section 7.1 for detailed wireframes of the Management Portal UI.

### Phase 7: User Portal Skill Review UI

**Goal:** Allow users to review, approve/reject, and remove skills directly from the conversation.

**Status:** ✅ COMPLETE

**Tasks:**
1. ✅ Create `SkillArtifact` component to render skill artifacts in conversation
   - ✅ Display skill name and appropriate icon (🔧 saved, ⚡ used)
   - ✅ "View Skill" button to open review modal
   - ✅ Different styling for saved vs. used artifacts
2. ✅ Create `SkillReviewModal` component for detailed skill review
   - ✅ Python code viewer (read-only)
   - ✅ Skill metadata display (name, description, parameters)
   - ✅ Execution statistics for used skills (execution count, success rate)
   - ✅ Context-appropriate buttons:
     - Skill Saved: "Approve" / "Reject"
     - Skill Used: "Keep Skill" / "Remove Skill"
3. ✅ Add CoreAPI endpoints for User Portal skill operations (already implemented in Phase 4)
   - ✅ `GET /instances/{instanceId}/skills/{skillId}` - Get skill for review
   - ✅ `POST /instances/{instanceId}/skills/{skillId}/approve` - Approve/keep skill
   - ✅ `DELETE /instances/{instanceId}/skills/{skillId}` - Reject/remove and delete skill
4. ✅ Add skill API methods to User Portal api.ts
   - ✅ `getSkill(skillId)` - Get skill details
   - ✅ `approveSkill(skillId)` - Approve a skill
   - ✅ `deleteSkill(skillId)` - Delete a skill
5. ✅ Implement skill actions with appropriate toast notifications
   - ✅ Saved + Approve: "Skill approved and ready to use"
   - ✅ Saved + Reject: "Skill rejected and removed"
   - ✅ Used + Keep: "Skill will continue to be used"
   - ✅ Used + Remove: "Skill removed - agent will generate new code next time"
6. ✅ Handle edge cases (skill already deleted, network errors, concurrent access)
   - ✅ Error handling in API methods
   - ✅ Fallback to metadata if API call fails
   - ✅ Loading and error states in modal

**Files Created/Modified:**
- ✅ `src/python/plugins/core/pkg/foundationallm/models/constants/content_artifact_type_names.py` - Added `SKILL_SAVED`, `SKILL_USED`
- ✅ `src/ui/UserPortal/js/types/index.ts` - Updated `ContentArtifact` interface to include `type` and `metadata`
- ✅ `src/ui/UserPortal/components/SkillArtifact.vue` (NEW - handles both types)
- ✅ `src/ui/UserPortal/components/SkillReviewModal.vue` (NEW)
- ✅ `src/ui/UserPortal/components/ChatMessage.vue` - Updated to render skill artifacts
- ✅ `src/ui/UserPortal/js/api.ts` - Added skill API methods (`getSkill`, `approveSkill`, `deleteSkill`)

**Implementation Details:**
- Skill artifacts are rendered inline in the message body using `SkillArtifact` component
- Clicking a skill artifact opens `SkillReviewModal` for detailed review
- Modal loads skill details from API (with fallback to artifact metadata)
- Actions (approve/reject/keep/remove) call appropriate API endpoints
- Toast notifications provide user feedback for all actions
- Error handling gracefully falls back to metadata if API calls fail
- Skill artifacts appear in content artifacts list with appropriate icons

### Phase 6: Skill Lifecycle & Analytics (Optional Enhancement)

**Goal:** Track skill usage and improve skill quality over time.

**Tasks:**
1. Track skill execution count and success rate
2. Implement skill versioning
3. Add skill analytics dashboard
4. Implement skill pruning (remove unused skills)

**Estimated Effort:** 1-2 weeks

---

## 9. Alternative Approaches

SECTION REMOVED 

---

## 10. Manual Testing and Verification Guide

This section provides step-by-step instructions to manually test and verify the procedural memory implementation. The testing approach uses **local development** for modified components (Core API, Management API, User Portal, Management Portal) while relying on the **deployed FoundationaLLM instance** for all other services.

### 10.1 Prerequisites

Before testing, ensure the following are in place:

1. **Deployed FoundationaLLM Instance**
   - FoundationaLLM platform is deployed and running in Azure
   - LangChain API, Context API, and other services are accessible
   - Azure Cosmos DB is configured and accessible (uses existing Core API Cosmos DB)
   - Storage account is accessible for test harness conversation data

2. **Local Development Environment**
   - .NET SDK installed (for Core API and Management API)
   - Node.js and npm installed (for User Portal and Management Portal)
   - Python 3.x installed (for Code Interpreter Sandbox and test harness)
   - Visual Studio Code or Cursor IDE with appropriate extensions
   - Access to the FoundationaLLM codebase

3. **Cosmos DB Configuration**
   Skills are stored in the existing Core API Cosmos DB database. No manual container setup is required:
   - The `AzureCosmosDBService` implementation includes the skill methods
   - The `skills` container is **automatically created on first use** via `CreateContainerIfNotExistsAsync`
   - Container is created with the following configuration:
     - Partition key: `/upn`
     - Autoscale throughput: 1000 RU/s
   - **Optional (future enhancement):** Enable vector indexing on `/embedding` for semantic search

4. **Test User**
   - Have a valid Entra ID user account for testing
   - Note the user's UPN (e.g., `testuser@contoso.com`) - **case sensitive**
   - Note the user's `user_id` (GUID format)

5. **Agent Configuration**
   - Have an agent with the Code Interpreter tool configured (via deployed Management Portal)
   - Have another agent without procedural memory for backwards compatibility testing

---

### 10.2 Local Development Setup

This section describes how to run the modified components locally while using the deployed instance for everything else.

#### 10.2.1 Build the .NET Solution

Before running any .NET services, rebuild the solution to include your changes:

```powershell
cd C:\Repos\foundationallm\src
dotnet build FoundationaLLM.sln --configuration Debug 2>&1 | Select-Object -Last 30
```

**Note:** This ensures all recent changes to Core API, Management API, and shared libraries are compiled.

#### 10.2.2 Run Core API Locally

The Core API handles skill management endpoints. Run it locally in debug mode:

```powershell
cd C:\Repos\foundationallm\src\dotnet\CoreAPI
dotnet run --configuration Debug --no-build 2>&1
```

**Expected:** Core API starts and listens on the configured port (typically `http://localhost:5001` or similar).

**Verify:** Check the console output for startup messages and ensure no errors occur.

#### 10.2.3 Run Management API Locally

The Management API handles agent configuration. Run it locally in debug mode:

```powershell
cd C:\Repos\foundationallm\src\dotnet\ManagementAPI
dotnet run --configuration Debug --no-build 2>&1
```

**Expected:** Management API starts and listens on the configured port.

**Verify:** Check the console output for startup messages and ensure no errors occur.

#### 10.2.4 Run User Portal Locally

The User Portal displays skill artifacts and allows skill review. Run it locally:

```powershell
cd C:\Repos\foundationallm\src\ui\UserPortal
npm run dev 2>&1
```

**Expected:** User Portal starts and is accessible (typically `http://localhost:3000` or similar).

**Note:** You can also use the "User Portal UI - Backend" configuration in VS Code/Cursor debug panel.

#### 10.2.5 Run Management Portal Locally

The Management Portal allows configuration of procedural memory settings. Run it locally:

```powershell
cd C:\Repos\foundationallm\src\ui\ManagementPortal
npm run dev
```

**Expected:** Management Portal starts and is accessible (typically `http://localhost:3001` or similar).

**Note:** You can also use the "Management Portal UI - Backend" configuration in VS Code/Cursor debug panel.

#### 10.2.6 Configuration for Local Services

Ensure your local services are configured to:
- Connect to the **deployed** Cosmos DB instance (for skill storage)
- Connect to the **deployed** storage account (for conversation data)
- Use the **deployed** LangChain API and Context API
- Authenticate using your Entra ID credentials

**Important:** Only the services you're modifying (Core API, Management API, portals) run locally. All other services use the deployed instance.

---

### 10.3 Running Code Interpreter Sandbox Locally

The Code Interpreter tool requires a local Python sandbox environment. Set it up as follows:

1. **Create Required Directories**
   - Create folder: `C:\mnt\data`
   - **Important:** This must be on the same drive as your repository (typically `C:`)

2. **Set Up Python Environment**
   - Option 1: Use the Python interpreter located under `src\Python\PythonCodeSessionAPI`
   - Option 2: Create a Python virtual environment at the project level
   - Ensure all required Python packages are installed

3. **Run PythonCodeSessionAPI in Debug**
   - In Cursor/VS Code, open the Debug panel
   - Select the **PythonCodeSessionAPI** configuration
   - Click the Run button (or press F5)

**Expected:** The Python sandbox service starts and is ready to execute Python code.

**Verify:** Check the console output for startup messages indicating the sandbox is running.

---

### 10.4 Using the Test Harness

The test harness allows you to test the Code Interpreter tool and procedural memory without running the entire platform. It uses a conversation JSON file from the deployed instance.

#### 10.4.1 Get a Conversation for Test Harness

1. **Create a Conversation in Deployed User Portal**
   - Login to the deployed Chat Portal (User Portal)
   - Create a new conversation with the agent you're testing
   - Send an initial message to create the conversation (e.g., "who are you")

2. **Get the Conversation JSON Representation**
   - Navigate to the Azure Portal
   - Go to the storage account backing your FoundationaLLM deployment
   - Select `Containers` → `orchestration-completion-requests`
   - Select the folder with your user ID
   - Select the latest folder (representing the conversation you just started)
   - Select the file ending in `completion-request-OUT.json`
   - Click `Edit` in the Azure Portal
   - Right-click in the text area and select `Format Document`
   - Select all JSON content and copy it

3. **Update the Test Harness File**
   - In the FoundationaLLM codebase, open `src\python\plugins\agent_core\test\full_request.json`
   - Replace the entire contents with the JSON you copied from Azure Portal
   - Save the file

#### 10.4.2 Configure the Test Harness

1. **Open the Test File**
   - Open `src\python\plugins\agent_core\test\foundationallm_function_calling_workflow\test_workflow.py`

2. **Update User Identity**
   - Locate the `user_identity_json` variable (around line 51)
   - Update it with your test user's information:
     ```python
     user_identity_json = {
         "name": "Experimental Test",
         "user_name": "yourname@foundationaLLM.ai",  # Your email
         "upn": "YourName@foundationaLLM.ai",        # Case-sensitive UPN matching Entra
         "user_id": "your-user-guid-here",            # Your user GUID
         "group_ids": []                              # Your group IDs if any
     }
     ```
   - **Important:** The `upn` is case-sensitive and must exactly match your Entra ID UPN

3. **Set the Test Prompt**
   - Locate the `user_prompt` variable (around line 75)
   - The prompt is read from `full_request.json`, but you can modify it directly in the file
   - For procedural memory testing, use prompts that will trigger code execution:
     - `"Calculate the factorial of 10 using Python"`
     - `"Generate a graph of y=mx+b where m=2 and b=3"`
     - `"Create a bar chart showing the average of 42, 84, and 168"`

#### 10.4.3 Run the Test Harness

1. **Start Required Services**
   - Ensure PythonCodeSessionAPI is running (see Section 10.3)
   - Ensure Core API is running locally (see Section 10.2.2) if testing skill endpoints

2. **Run the Test**
   - In Cursor/VS Code, open the Run and Debug panel
   - Select **"Agent Plugins - Debug Workflow"** from the dropdown
   - Click the Play icon (or press F5) to start debugging

3. **Observe the Output**
   - The test harness will execute the workflow
   - Check the console output for:
     - Code execution results
     - Content artifacts (including `skill_saved` or `skill_used` if procedural memory is enabled)
     - Any errors or warnings

4. **Verify Results**
   - Look for `skill_saved` content artifacts when code is successfully executed
   - Look for `skill_used` content artifacts when existing skills are reused
   - Check that the response content includes the expected results

**Example Output:**
```
++++++++++++++++++++++++++++++++++++++
Content artifacts:
[{'type': 'skill_saved', 'id': '...', 'title': '...', 'metadata': {...}}]
++++++++++++++++++++++++++++++++++++++

*********************************
The factorial of 10 is 3628800
*********************************
```

---

### 10.5 Test Case 1: Backwards Compatibility (Procedural Memory Disabled)

**Objective:** Verify the Code Interpreter works exactly as before when procedural memory is not enabled.

**Testing Approach:** Use the test harness (Section 10.4) or User Portal (Section 10.2.4).

**Steps:**

1. **Create or select an agent WITHOUT procedural memory**
   - In Management Portal (local or deployed), create/edit an agent
   - Ensure `procedural_memory_settings` is either:
     - Not present in the agent configuration, OR
     - Present with `enabled: false`
   - Add the Code Interpreter tool to the agent

2. **Test using Test Harness (Recommended)**
   - Update `test_workflow.py` with a prompt: `"Calculate the factorial of 10 using Python"`
   - Ensure PythonCodeSessionAPI is running locally (Section 10.3)
   - Run the test harness (Section 10.4.3)
   - **Expected:** Agent generates Python code, executes it, returns result (3628800)
   - **Verify:** No skill-related content artifacts appear in the response

3. **Test using User Portal (Alternative)**
   - Open User Portal (local or deployed)
   - Select the agent
   - Start a new conversation
   - Send: *"Calculate the factorial of 10 using Python"*
   - **Expected:** Agent generates Python code, executes it, returns result (3628800)
   - **Verify:** No skill-related content artifacts appear in the response

4. **Verify no skill operations available**
   - Send a message: *"Search for factorial skills"*
   - **Expected:** Agent treats this as a regular prompt, may generate code or respond conversationally
   - **Verify:** No skill search is performed (backwards compatible behavior)

**Pass Criteria:** Code Interpreter functions identically to pre-implementation behavior.

---

### 10.6 Test Case 2: Enable Procedural Memory on Agent

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

### 10.7 Test Case 3: Auto-Register a New Skill

**Objective:** Verify the Code Interpreter automatically registers successful code as a skill when `auto_register_skills` is enabled.

**Testing Approach:** Use the test harness (Section 10.4) for quick iteration, or User Portal (Section 10.2.4) for full UI testing.

**Steps:**

1. **Ensure agent has procedural memory enabled**
   - Complete Test Case 2 to configure procedural memory
   - Ensure `auto_register_skills: true` is set

2. **Test using Test Harness (Recommended)**
   - Update `test_workflow.py`:
     - Set `user_prompt` to: `"Calculate the factorial of 10 using Python"`
     - Ensure PythonCodeSessionAPI is running (Section 10.3)
     - Ensure Core API is running locally (Section 10.2.2)
   - Run the test harness (Section 10.4.3)
   - **Expected:** 
     - Code executes successfully
     - Response includes a `skill_saved` content artifact
     - Artifact metadata contains skill information

3. **Test using User Portal (Alternative)**
   - Open User Portal (local or deployed)
   - Select the agent with procedural memory enabled
   - Start a new conversation
   - Send: *"Calculate the factorial of 10 using Python"*
   - **Expected:** 
     - Agent generates and executes Python code successfully
     - Response includes a `skill_saved` content artifact
     - Artifact shows skill name, description, and code

4. **Verify skill_saved content artifact**
   - In the response, locate the skill artifact
   - **Expected fields in metadata:**
     - `skill_name`: Auto-generated name (e.g., "calculate_factorial")
     - `skill_description`: Auto-generated description
     - `skill_code`: The Python code that was executed
     - `skill_status`: "Active" (since `require_skill_approval` is false)
   - **Expected UI (User Portal):** Shows skill artifact with "View Skill" button, "Approve" and "Reject" buttons

5. **Verify skill stored in Cosmos DB**
   - Ensure Core API is running locally (Section 10.2.2)
   - Call `GET /instances/{instanceId}/skills` (as the test user)
   - **Expected:** Skill appears in the list with correct metadata
   - **Verify:** Skill ID includes agent name and user identifier

**Pass Criteria:** Skill is automatically registered, stored in Cosmos DB, and displayed in content artifact.

---

### 10.8 Test Case 4: Internal Skill Search and Use

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

### 10.9 Test Case 5: Skill Review UI in User Portal

**Objective:** Verify users can review, approve, and reject skills via the User Portal UI.

**Testing Approach:** Use local User Portal (Section 10.2.4) to test the skill review interface.

**Steps:**

1. **Ensure a skill exists**
   - Complete Test Case 3 to create a skill with `skill_saved` artifact

2. **View skill artifact in User Portal**
   - Open User Portal locally (Section 10.2.4)
   - Navigate to the conversation with the skill artifact
   - **Expected:** Skill artifact is displayed with appropriate icon (🔧 for saved, ⚡ for used)
   - **Expected:** "View Skill" button is visible

3. **Open skill review modal**
   - Click "View Skill" button on the artifact
   - **Expected:** SkillReviewModal opens showing:
     - Skill name and description
     - Python code (read-only, syntax-highlighted)
     - Skill metadata (status, execution count if used)
     - Action buttons (Approve/Reject for saved, Keep/Remove for used)

4. **Test approve action (for skill_saved)**
   - Click "Approve" button
   - **Expected:** 
     - Toast notification: "Skill approved and ready to use"
     - Modal closes
     - Skill status changes to "Active" (if it was pending)

5. **Test reject action**
   - Create another skill
   - Click "Reject" button on the skill artifact
   - **Expected:** 
     - Toast notification: "Skill rejected and removed"
     - Modal closes
     - Skill is deleted from Cosmos DB

6. **Test keep action (for skill_used)**
   - Use an existing skill (Test Case 4)
   - Click "Keep Skill" button on the `skill_used` artifact
   - **Expected:** 
     - Toast notification: "Skill will continue to be used"
     - Modal closes

7. **Test remove action (for skill_used)**
   - Use an existing skill
   - Click "Remove Skill" button
   - **Expected:** 
     - Toast notification: "Skill removed - agent will generate new code next time"
     - Modal closes
     - Skill is deleted from Cosmos DB

**Pass Criteria:** All skill review actions work correctly via the User Portal UI.

---

### 10.7 Test Case 6: Skill Approval Workflow

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

### 10.11 Test Case 7: API Endpoints for Skill Management

**Objective:** Verify all skill management API endpoints work correctly.

**Testing Approach:** Use local Core API (Section 10.2.2) and test with API calls.

**Steps:**

1. **Ensure Core API is running locally**
   - Start Core API (Section 10.2.2)
   - Verify it's accessible

2. **Test GET /instances/{instanceId}/skills**
   - Call the endpoint with your user identity
   - **Expected:** Returns list of skills for the current user
   - **Verify:** Skills are scoped to user and agent

3. **Test GET /instances/{instanceId}/skills/{skillId}**
   - Get a skill ID from the list
   - Call the endpoint
   - **Expected:** Returns full skill details including code, metadata, execution stats

4. **Test POST /instances/{instanceId}/skills/{skillId}/approve**
   - Create a skill with `require_skill_approval: true`
   - Call the approve endpoint
   - **Expected:** 
     - 200 OK response
     - Skill status changes to "Active"

5. **Test DELETE /instances/{instanceId}/skills/{skillId}**
   - Create a test skill
   - Get its skill ID
   - Call the delete endpoint
   - **Expected:** 
     - 200 OK response
     - Skill is removed from Cosmos DB
   - **Verify:** Skill no longer appears in GET /skills list

6. **Test error handling**
   - Call GET with non-existent skill ID
   - **Expected:** 404 Not Found
   - Call DELETE with non-existent skill ID
   - **Expected:** 404 Not Found

**Pass Criteria:** All skill API endpoints work correctly with proper error handling.

---

### 10.12 Test Case 8: Skill Scoping (Agent-User Combination)

**Objective:** Verify skills are correctly scoped to agent-user combination.

**Testing Approach:** Use test harness with different user identities and agents.

**Steps:**

1. **Create skill with User A on Agent 1**
   - Update `test_workflow.py` with User A's identity
   - Ensure agent configuration uses Agent 1
   - Run test harness to create a skill
   - Note the skill name/ID

2. **Verify User A can access on Agent 1**
   - Run test harness again with same user and agent
   - Use a prompt that should match the skill
   - **Expected:** Skill is found and used (if similarity > threshold)

3. **Verify User A cannot access on Agent 2**
   - Update `test_workflow.py` to use Agent 2 (different agent)
   - Keep User A's identity
   - Run test harness with similar prompt
   - **Expected:** Skill is NOT found (different agent scope)

4. **Verify User B cannot access User A's skill**
   - Update `test_workflow.py` with User B's identity
   - Use Agent 1 (same agent as step 1)
   - Run test harness
   - **Expected:** Skill is NOT found (different user scope)

5. **Verify via API**
   - Call `GET /instances/{instanceId}/skills` as User A
   - **Expected:** Only User A's skills appear
   - Call same endpoint as User B
   - **Expected:** Only User B's skills appear (or empty if none)

**Pass Criteria:** Skills are correctly scoped to agent-user combination and not accessible across boundaries.
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

### 10.13 Test Case 9: Error Handling

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

5. **Cosmos DB unavailable**
   - (If possible) Simulate Cosmos DB connection failure
   - Try skill operations
   - **Expected:** 503 Service Unavailable or appropriate error message

**Pass Criteria:** All error cases handled gracefully with informative messages.

### 10.14 Verification Checklist

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

## 11. Storage Options Analysis

> **✅ IMPLEMENTED:** This implementation uses **Option A (Cosmos DB)** for skill storage. The separate Skill resource provider has been removed in favor of storing skills directly in the existing Core API Cosmos DB database.

### 11.1 Option A: Store Skills in Cosmos DB (✅ IMPLEMENTED)

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

### 11.2 Option B: Add Skills as Agent Resource Provider Sub-Resource

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

### 11.3 Option C: Use Context Resource Provider Storage

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

### 11.4 Option D: Store Skills in User Profile (Cosmos DB)

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

### 11.5 Recommendation

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

### 11.6 Implementation Steps for Option A

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

## Appendix A: Example Agent Prompt with Skills

**Note:** This prompt is only used when `procedural_memory_settings.enabled = true`. When disabled, the standard code interpreter prompt is used unchanged.

```
You are an AI assistant with access to a code interpreter tool that supports procedural memory.

The code interpreter tool automatically manages skills for you. When you use the tool:
- The tool will automatically search for relevant skills that match the user's request
- If a suitable skill is found, the tool will use it automatically
- If no skill is found, the tool will generate new Python code
- After successful execution, the tool may automatically save the code as a skill for future use

You don't need to explicitly search for skills or register them - the tool handles this internally.

Simply use the code interpreter tool with the user's prompt, and the tool will:
1. Check for existing skills that match the request
2. Use a skill if found and suitable
3. Generate new code if no skill is available
4. Optionally save successful code as a skill

The tool will inform you (and the user) when it uses an existing skill or creates a new one through content artifacts in the response.
```

**Key Change:** The agent prompt no longer mentions explicit skill operations. The tool handles skill management internally, so the agent just needs to call the tool with a prompt.

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

*Document Version: 2.3*
*Created: January 2025*
*Last Updated: January 2025*
*Status: Core Implementation Complete (Simplified Interface - Internal Skill Management)*

**Revision History:**
- v2.3 (Jan 2025): **Management Portal configuration details**:
  - Added detailed section on tool property configuration (Section 11)
  - Settings stored in tool properties (`procedural_memory_settings` JSON object), not on agent
  - Added wireframes for ConfigureToolDialog with procedural memory settings
  - Updated Phase 2 and Phase 6 to reflect tool property approach
  - Documented existing tool properties (code_session_required, code_session_endpoint_provider, code_session_language)
  - Added implementation details for reading settings from tool_config.properties
- v2.2 (Jan 2025): **Simplified tool interface - internal skill management**:
  - Removed `operation`, `skill_name`, `skill_description`, and `skill_parameters` from tool input
  - Tool now handles skill search/use/register internally when procedural memory is enabled
  - Agent interface simplified: only `prompt` and `file_names` parameters
  - Added `_execute_with_procedural_memory()` method for internal skill orchestration
  - Added internal helper methods: `_search_skills_internal()`, `_use_skill_internal()`, `_register_skill_internal()`
  - Updated agent prompt examples to reflect simplified interface
  - Improved backwards compatibility - no new parameters required
- v2.1 (Jan 2025): **Migrated to Cosmos DB storage (Option A)** - Removed Skill resource provider:
  - Removed `FoundationaLLM.Skill` resource provider project and configuration
  - Added `SkillReference` model for Cosmos DB storage
  - Added skill methods to `IAzureCosmosDBService` interface
  - Updated `SkillsController` to use Cosmos DB directly
  - Updated Python code interpreter to use new Core API endpoints
  - Updated testing prerequisites to reflect Cosmos DB approach
  - No additional storage configuration required
- v2.0 (Jan 2025): **Initial implementation** - All core phases implemented:
  - Phase 1: Skill resource model (SkillReference.cs, SkillParameter.cs, SkillStatus.cs)
  - Phase 2: ProceduralMemorySettings added to AgentBase
  - Phase 3: Code Interpreter tool enhanced with skill operations
  - Phase 4: CoreAPI SkillsController for User Portal skill review
  - Content artifacts for skill_saved and skill_used implemented
  - Backwards compatibility maintained when procedural memory is disabled
- v1.3 (Jan 2025): Added `skill_used` content artifact for when existing skills are executed; users can review which skill was used and remove it if desired; updated Use Skill operation to return content artifact
- v1.2 (Jan 2025): Added skill registration content artifact with User Portal review UI; users can approve/reject skills directly from conversation; added Phase 5 for User Portal implementation; added CoreAPI endpoints for skill review
- v1.1 (Jan 2025): Finalized design decisions for skill scoping (agent-user), approval workflow (auto-approve by default, configurable), security (sandbox only), and backwards compatibility requirement
- v1.0 (Jan 2025): Initial plan draft
