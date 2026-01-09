## FoundationaLLM “Claude Code” Agent — detailed implementation plan

### 0) Executive summary
This plan describes how to add a new **first-class Agent option** to FoundationaLLM: a **Claude Code** agent that provides “Claude Code”-style capabilities (read/edit files, run bash commands, author/execute code, GitHub operations, optional web access) **through the existing FoundationaLLM UI + APIs**.

Key idea: instead of running a LangChain workflow, this agent runs inside a **sandboxed Ubuntu environment** provisioned via **Azure Container Apps Dynamic Sessions (Custom Containers)**. FoundationaLLM becomes the “operating system” that:

- Authenticates/authorizes the user and the agent
- Stores durable artifacts in Blob Storage (secured per user/conversation)
- Persists multi-turn conversation history (Cosmos) and supplies it to the agent each turn
- Controls sandbox limits, egress policy, and audit logging

---

### 1) Goals, non-goals, and success criteria

#### Goals
- **G1 — Selectable agent**: “Claude Code” appears in the agent dropdown like any other agent (User Portal).
- **G2 — Runs in sandbox**: completion requests invoke the Claude Agent SDK inside a Dynamic Sessions custom container (Ubuntu).
- **G3 — Multi-turn**: conversation history is persisted by FoundationaLLM and provided to the agent on each invocation.
- **G4 — Durable artifacts**: files to keep (patches, generated code, reports, logs) are saved to **FoundationaLLM Blob Storage** with access restricted to the initiating user (or conversation ACL).
- **G5 — Tooling**: sandbox supports:
  - Read/edit workspace files
  - Run bash commands
  - Execute code
  - Git operations (clone, branch, commit) and PR creation (via GitHub App / token)
  - Optional outbound internet access (policy-controlled)
- **G6 — Observability & governance**: auditable tool usage (commands run, files changed, network usage, repo operations), with configurable retention.

#### Non-goals (initial release)
- **NG1**: Perfect feature parity with every Claude Code UI feature on day 1.
- **NG2**: Arbitrary long-lived, always-on sandboxes (we’ll support bounded session TTL and/or per-turn restore).
- **NG3**: Running untrusted binaries with unrestricted privileges (container remains locked down).

#### Success criteria (MVP)
- **S1**: A user can select the Claude Code agent, ask it to modify a repo, and receive:
  - A natural language summary
  - A list of changed files (as artifacts)
  - Optionally, a PR link (if GitHub integration enabled)
- **S2**: No durable data is stored outside FoundationaLLM-managed storage.
- **S3**: Egress is **off by default** and can be enabled per-agent (or per-request) with clear auditing.

---

### 2) Proposed architecture (high level)

#### Components
- **User Portal (UI)**: selects agent and displays responses + artifacts.
- **Management Portal (UI)**: creates/configures the Claude Code agent + its workflow settings.
- **Orchestration API (.NET)**:
  - Chooses orchestration service based on `agent.workflow.workflow_host`
  - Adds a new internal orchestration service: **ClaudeCodeOrchestrationService**
- **Context API (.NET)**:
  - Already supports Dynamic Sessions “code sessions” via `AzureContainerAppsCustomContainerService`
  - We will add (or reuse) a provider to create/manage a “Claude Code session” and transfer files in/out
- **Dynamic Sessions Custom Container image (Ubuntu)**:
  - Runs a small HTTP server (“sandbox control API”)
  - Embeds/installs **Claude Agent SDK** and a “tool adapter” that maps agent tool calls to:
    - Local FS operations (within workspace)
    - Bash command execution (restricted)
    - GitHub operations (restricted)
    - Optional web fetch/search (policy-controlled)
- **Blob Storage**:
  - Stores durable workspace snapshots, produced artifacts, and execution logs
  - Scoped by `instanceId / user / conversation / run`

#### Data flow (single turn)
- User message → Orchestration API `/completions` (existing)
- OrchestrationBuilder loads agent + conversation history + attachments
- ClaudeCodeOrchestrationService:
  - Ensures/creates sandbox session (Dynamic Sessions)
  - Restores persisted workspace snapshot into sandbox (if configured)
  - Sends prompt + conversation history to sandbox “run” endpoint
  - Streams/collects tool events and final output
  - Syncs modified/new files back to Blob and produces `ContentArtifacts`
  - Returns standard `CompletionResponse` (so existing UI/API clients work)

#### Data flow (multi-turn)
- FoundationaLLM persists conversation history as today.
- ClaudeCodeOrchestrationService also persists (depending on mode):
  - **Workspace snapshot** (tar/zip) and/or
  - **Patch/diff + artifact list**, and optionally
  - **Agent state** (Claude Agent SDK state, if needed)
- Next turn restores workspace and continues.

---

### 3) Agent and workflow modeling in FoundationaLLM

#### Current relevant behavior (existing)
- The agent “type” currently centers around `generic-agent`.
- The orchestrator used at runtime is chosen as:
  - `agent.workflow.workflow_host` if present
  - otherwise defaults to `LangChain`

#### Proposed: new workflow type
Add a new workflow type to represent Claude Code runtime requirements (so it’s configurable and discoverable):

- **New workflow type constant**: `claude-code-agent-workflow`
- **New workflow class**: `ClaudeCodeAgentWorkflow : AgentWorkflowBase`
- **WorkflowHost**: set to a new internal orchestrator service name, e.g. `ClaudeCode`

Workflow-specific `properties` (illustrative; versioned and extensible):
- **sandbox_profile**: `restricted` | `internet-enabled` | `custom`
- **sandbox_timeout_seconds**: e.g. 900
- **sandbox_cpu / sandbox_memory**: sizing hints
- **workspace_persistence_mode**: `snapshot` | `diff` | `none`
- **workspace_storage_scope**: `conversation` | `user`
- **allowed_repo_patterns**: allowlist for cloning (optional)
- **egress_policy**:
  - off by default
  - optional allowlist (domains, CIDRs) when enabled
- **github_integration_mode**: `none` | `github-app` | `user-pat` (MVP can start with `user-pat` stored securely)

#### How it becomes “an agent option in the dropdown”
No special UI work is required beyond creating an agent resource:
- Create a **built-in agent** (deployment template) named e.g. `claude-code`
- Mark it as featured/pinned in app config if desired
- The User Portal agent dropdown already reflects agents accessible to the user

---

### 4) Orchestration layer changes (Orchestration API)

#### Add a new internal orchestration service
Create `ClaudeCodeOrchestrationService : ILLMOrchestrationService` with:
- **Name**: `ClaudeCode`
- **GetCompletion**: orchestrates a single run in sandbox and returns `LLMCompletionResponse`
- **StartCompletionOperation / GetCompletionOperationStatus**:
  - Optional for long-running work; can be implemented early if needed (recommended)

#### Request/response mapping
- Inputs:
  - `LLMCompletionRequest` (already contains agent config, main AI model config, prompts, tool config, conversation history from upstream `CompletionRequest`)
- Outputs:
  - Standard `LLMCompletionResponse` fields:
    - `Completion` (final assistant message)
    - `ContentArtifacts` (files produced, PR link, logs)
    - `PromptTokens / CompletionTokens` (best-effort; can be filled once SDK exposes usage)

#### Conversation history injection
Use `AgentConversationHistorySettings` + inbound `MessageHistory` to build:
- A compact “chat transcript” for the Claude Agent SDK
- A stable system prompt describing environment/tooling + policy constraints

#### Artifact strategy (align with existing UI)
Represent Claude Code outputs as `ContentArtifact[]`:
- **File artifacts**: `type=file`, with `filepath`, optional `metadata` (size, hash, mime)
- **Git artifacts**: `type=git`, with `source=github`, `content` containing PR URL or branch/ref
- **Command log**: `type=log`, with `content` summarizing key commands/events
- **Web sources** (when egress enabled): `type=source`, with `source=url`, `filepath` optional, `content` snippet

---

### 5) Sandbox runtime (Dynamic Sessions Custom Container)

#### Container responsibilities
Build a new container image (Ubuntu base) that:
- Exposes a small HTTP API (internal to FoundationaLLM) such as:
  - `POST /agent/run` — run a single turn with transcript + constraints
  - `GET /agent/events` — (optional) streaming event channel
  - `POST /fs/upload` / `POST /fs/download` — bulk file transfer (or reuse existing endpoints)
  - `POST /cmd/run` — controlled command execution (if needed separately)
  - `GET /healthz`
- Installs:
  - Claude Agent SDK (pinned version)
  - git + gh CLI (optional, but recommended)
  - language runtimes as needed (python/node) for common tasks

#### Tool adapter (core)
Implement a tool adapter layer that the Claude Agent SDK calls for:
- **File read/write**:
  - Restrict to a workspace root (e.g. `/workspace`)
  - Enforce max file size and disallow binary reads by default (configurable)
- **Bash command execution**:
  - Run with timeouts, output size caps, and allow/deny list of commands
  - Capture stdout/stderr and return structured results
- **GitHub operations**:
  - Provide credentials only via short-lived tokens
  - Allowlist repository URLs/patterns when configured
  - Log PR creation and link as artifact
- **Web access** (optional):
  - Off by default
  - When enabled, route through an allowlist and log all outbound domains/URLs

#### Security posture (sandbox hardening)
- Run as non-root, read-only base FS where possible
- Drop Linux capabilities, seccomp/apparmor defaults
- CPU/memory/time quotas via container settings
- Default deny outbound network; enable only when explicitly allowed
- No access to instance metadata endpoints

---

### 6) Durable storage model (Blob Storage)

#### What to persist
Per conversation (and optionally per user), persist:
- **Workspace snapshot** (tar/zip) OR **patch set** (preferred if stable)
- **Produced artifacts** (files, reports, images)
- **Execution logs** (tool events, command transcripts) with redaction
- **Optional agent state** (if the Claude Agent SDK supports resumable state)

#### Naming scheme (example)
`{upn}/{instanceId}/{conversationId}/claude-code/{yyyy-mm-dd}/{runId}/...`

#### Access control
Use FoundationaLLM’s existing security model:
- Writes performed by the service identity
- Reads gated by the calling user’s permissions (virtual security groups / conversation ACLs)
- Ensure artifacts are not cross-tenant or cross-user accessible

---

### 7) Identity, secrets, and permissions

#### Anthropic/Claude model credentials
Options (choose one for MVP; support multiple later):
- **Option A (preferred)**: reuse FoundationaLLM AIModel + APIEndpointConfiguration so the orchestrator can pass the correct model + endpoint + auth to the sandbox in a controlled way.
- **Option B**: sandbox calls FoundationaLLM Gateway “model facade” (if available) so no direct Anthropic key is exposed to the sandbox.

#### GitHub credentials
Phased approach:
- **MVP**: user provides a GitHub PAT stored in FoundationaLLM secure storage; orchestrator injects it short-lived into sandbox.
- **Hardened**: GitHub App integration; mint user-scoped installation tokens; never store PATs.

#### Principle of least privilege
- Sandbox receives only the minimum tokens it needs for the task, scoped by:
  - conversation
  - repo allowlist
  - time

---

### 8) UX plan (User Portal + Management Portal)

#### User Portal
MVP (no major UI changes required):
- Agent selectable in dropdown
- Responses render as normal
- Artifacts appear via existing “sources/artifacts” UI affordances (where supported)

Incremental UX improvements:
- Live “activity” panel (tool events, commands, file edits)
- File tree viewer for produced artifacts
- “Open PR” button when PR artifact is present

#### Management Portal
Add/enable:
- A workflow entry representing `claude-code-agent-workflow` (so admins can create agents from UI)
- Workflow configuration fields for sandbox profile, persistence mode, egress policy, GitHub mode
- Admin-only “internet enabled” toggle + allowlist editor

---

### 9) Testing & validation plan

#### Unit tests
- Workflow deserialization/serialization (`ClaudeCodeAgentWorkflow`)
- Orchestration service request mapping (history → transcript)
- Artifact mapping (files/PR/log → `ContentArtifact[]`)

#### Integration tests
- Spin up a sandbox session and execute:
  - file write + readback
  - command execution (`git status`, `python -c ...`)
  - (optional) web request when enabled

#### End-to-end tests
- User Portal: select agent, ask it to modify a small repo, verify artifacts show up
- GitHub PR creation flow (in test org/repo)

#### Security tests
- Ensure sandbox cannot read outside workspace
- Ensure egress is blocked by default
- Ensure artifact blob paths are user-scoped and access-controlled

---

### 10) Observability & operations
- **Logging**:
  - Correlate runs by `operationId` / `conversationId` / `runId`
  - Store summarized tool events in Cosmos (optional) and full logs in Blob (configurable)
- **Metrics**:
  - session creation count, run durations, failure rates
  - bytes uploaded/downloaded
  - egress usage (when enabled)
- **Cost controls**:
  - enforce max runtime and max output sizes
  - cap concurrent sandboxes per user

---

### 11) Implementation phases (recommended)

#### Phase 1 — MVP (single-turn sandbox + durable artifacts)
- Add `claude-code-agent-workflow` and `ClaudeCodeAgentWorkflow` model.
- Add `ClaudeCodeOrchestrationService` (internal orchestrator).
- Build sandbox container image + minimal HTTP API: `POST /agent/run`.
- Implement workspace persistence (snapshot in Blob) + artifact extraction.
- Create a default “Claude Code” agent via deployment template (or documented setup).

#### Phase 2 — Multi-turn workspace continuity + improved artifact UX
- Persist workspace snapshot per conversation and restore on each turn.
- Add structured “file artifacts” summary and linkable downloads.
- Add optional streaming events (SSE) and UI rendering (activity log).

#### Phase 3 — GitHub PR flow + stronger security
- GitHub App integration (short-lived tokens).
- Repo allowlists and policy enforcement.
- Expand audit logging and redaction (secrets in logs).

#### Phase 4 — Internet-enabled mode (policy-driven)
- Add `internet-enabled` sandbox profile routed to a separate Dynamic Sessions environment or network policy.
- Add allowlisted outbound domains and full URL logging.

---

### 12) Key open questions / decisions to lock down early
- **Dynamic Sessions API shape**: do we standardize on the existing “custom container file/code endpoints”, or define a new sandbox API contract dedicated to Claude Code runs?
- **Workspace persistence**: snapshot vs diff. (Snapshot is simpler; diff is cheaper and easier to review.)
- **Credential strategy**: direct Anthropic key vs Gateway facade; PAT vs GitHub App.
- **Streaming UX**: do we need live tool event streaming for MVP, or only final response + artifacts?
- **Egress architecture**: separate container apps env for internet mode vs runtime toggles.

