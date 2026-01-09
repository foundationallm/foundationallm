## FoundationaLLM “Claude Code” Agent — detailed implementation plan

### 0) Executive summary
This plan describes how to add a new **first-class Agent option** to FoundationaLLM: a **Claude Code** agent that provides “Claude Code”-style capabilities (read/edit files, run bash commands, author/execute code, GitHub operations, optional web access) **through the existing FoundationaLLM UI + APIs**.

Key idea: instead of running a LangChain workflow, this agent runs inside a **sandboxed Ubuntu environment** provisioned via **Azure Container Apps Dynamic Sessions (Custom Containers)**. FoundationaLLM becomes the “operating system” that:

- Authenticates/authorizes the user and the agent
- Stores durable artifacts using the **Context API file store** (Cosmos file records + Blob payloads), scoped per user/conversation
- Persists multi-turn conversation history (Cosmos) and supplies it to the agent each turn
- Controls sandbox limits, egress policy, and audit logging

---

### 1) Goals, non-goals, and success criteria

#### Goals
- **G1 — Selectable agent**: “Claude Code” appears in the agent dropdown like any other agent (User Portal).
- **G2 — Runs in sandbox**: completion requests invoke the Claude Agent SDK inside a Dynamic Sessions custom container (Ubuntu).
- **G3 — Multi-turn**: conversation history is persisted by FoundationaLLM and provided to the agent on each invocation.
- **G4 — Durable artifacts**: files to keep (patches, generated code, reports, logs) are saved as **Context files** (backed by Blob) with access restricted by FoundationaLLM’s file record + authorization rules.
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
  - Already supports Dynamic Sessions “code sessions” via `CodeSessionsController` + `CodeSessionService`
  - Handles **secure file upload into** and **secure file download out of** a code session, using per-user Cosmos records and the file store
- **Dynamic Sessions Custom Container image (Ubuntu)**:
  - **No custom web server required** (preferred): rely on the existing Dynamic Sessions “execute code + file store” APIs that FoundationaLLM already calls via the Context API.
  - Image responsibility is primarily **packaging**: Claude Agent SDK + git/gh + runtimes/tools needed by the agent.
  - The “tool adapter” can live as a local library invoked by the executed code (not a network service).

#### Data flow (single turn)
- User message → Orchestration API `/completions` (existing)
- OrchestrationBuilder loads agent + conversation history + attachments
- ClaudeCodeOrchestrationService:
  - Ensures/creates sandbox session (Dynamic Sessions)
  - Restores persisted workspace snapshot into sandbox (if configured)
  - Uploads required conversation files into the session via Context API (same mechanism as Code Interpreter tool)
  - Executes a “runner” program inside the session (via Dynamic Sessions code execution API)
  - Downloads produced artifacts from the session via Context API, which materializes them as **new Context file records**
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

Supporting client work in .NET:
- Extend `IContextServiceClient`/`ContextServiceClient` (or add a dedicated client) to call:
  - `POST /codeSessions/{sessionId}/uploadFiles`
  - `POST /codeSessions/{sessionId}/executeCode`
  - `POST /codeSessions/{sessionId}/downloadFiles`
  so the orchestrator can follow the same secure pattern as the existing Python Code Interpreter tool plugin.

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

### 5) Sandbox runtime (Dynamic Sessions Code Sessions + Custom Containers)

#### Key design principle: reuse the existing Code Interpreter “code session” contract
FoundationaLLM already has a proven pattern for running code in Dynamic Sessions **and** moving files in/out securely:

- **Upload**: Context API `POST /instances/{instanceId}/codeSessions/{sessionId}/uploadFiles` takes *conversation file names*. The Context API reads file content via `FileService.GetFileContent(...)` using the **current user identity**, then uploads to the Dynamic Session.
- **Execute**: Context API `POST /instances/{instanceId}/codeSessions/{sessionId}/executeCode` runs code inside the Dynamic Session.
- **Download**: Context API `POST /instances/{instanceId}/codeSessions/{sessionId}/downloadFiles` downloads newly-created session files and persists them using `FileService.CreateFileForConversation(...)`, producing **new file records** (and thus preserving access control).

Additional mechanics worth copying directly:
- `uploadFiles` returns an **operation id**; that operation id is then passed into `downloadFiles`, allowing the Context API to correlate “inputs uploaded for this run” vs “outputs produced by this run”.
- `uploadFiles` currently performs a best-effort upload and also **cleans the session file store first**, so a multi-turn design should assume it needs to re-upload required inputs each turn (or change that behavior explicitly).

For the Claude Code agent we should mirror this end-to-end flow, which means we can likely **avoid implementing a bespoke HTTP server inside the sandbox**.

#### Container responsibilities (what the image needs to provide)
Build a container image (Ubuntu base) that:
- Installs:
  - Claude Agent SDK (pinned version)
  - git + gh CLI (recommended)
  - language runtimes as needed by the SDK (python/node)
  - any OS packages needed for common coding tasks (ripgrep, unzip, etc.)
- Provides a runnable entrypoint/program that can be invoked via “execute code” to:
  - Accept a request payload (prompt + conversation transcript + policy + file list)
  - Run the Claude Agent SDK and capture:
    - final assistant text output
    - tool event summaries (optional)
    - artifacts (patches, archives, logs) written to the session file store for download

Implementation note (based on current platform constraints):
- Code sessions are validated to a small set of languages (currently `Python` and `CSharp`), so the Claude Code agent should start with a **Python-based runner** (which can still execute shell commands via `subprocess` inside the sandbox).
- Prefer using the existing provider name `AzureContainerAppsCustomContainer` so we can supply an image that contains the Claude Agent SDK and tooling.

#### How “instructions” and “code” should be delivered (copying the Code Interpreter ergonomics)
To minimize the amount of “inline code” we send over `executeCode`, follow a two-part pattern:
- **Part A — upload a request payload file** (JSON) to the conversation file store (Context API), then include its filename in the `uploadFiles` list for the session (so the sandbox receives it securely).
- **Part B — execute a small, stable bootstrap snippet** via `executeCode` whose job is just to run the preinstalled runner, e.g.:
  - read `/mnt/data/<request>.json`
  - run Claude Agent SDK
  - write outputs (summary, patch, logs, artifact manifest) back to the session file store

This mirrors how the existing Code Interpreter tool sends “the real work” as code to execute plus a curated list of input files, and then retrieves outputs through `downloadFiles`.

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

### 6) Durable storage model (Context API file store + Blob)

#### What to persist
Per conversation (and optionally per user), persist:
- **Workspace snapshot** (tar/zip) OR **patch set** (preferred if stable)
- **Produced artifacts** (files, reports, images)
- **Execution logs** (tool events, command transcripts) with redaction
- **Optional agent state** (if the Claude Agent SDK supports resumable state)

#### How this is secured (important)
The existing file pipeline already provides a strong security boundary:
- Files are stored with a Cosmos **file record** whose partition key is the user’s UPN, and the Blob path includes `file/users/{upn}/{conversationId}/...`.
- When the sandbox needs a file, the Context API fetches it using the **current user identity** and conversation/file record checks.
- When the sandbox produces a file, the Context API writes it back as a **new conversation file record** owned by (or associated with) the initiating user, so subsequent retrieval honors the same access control.
- Code sessions themselves are also tracked in Cosmos as per-user records (partitioned by UPN), so a caller cannot access another user’s code session record even if they guess a session id.

#### Access control
Use FoundationaLLM’s existing security model:
- Writes performed by the service identity
- Reads gated by the calling user’s permissions (virtual security groups / conversation ACLs)
- Ensure artifacts are not cross-tenant or cross-user accessible

#### Important constraint discovered (affects “edit repo” scenarios)
The current `downloadFiles` implementation in the Context API is optimized for the Code Interpreter pattern of “upload inputs → produce new outputs”:
- It **filters out** files that were uploaded successfully (by name) when those files remain in the root directory, which means it may **not** automatically retrieve “modified in-place” files with the same names as uploaded inputs.

Implication for Claude Code agent:
- Prefer producing outputs as **new files** (so they are always downloadable), e.g.:
  - `changes.patch` / `changes.diff`
  - `workspace.tar.gz` (new name per turn)
  - `run.log`, `commands.jsonl`, `artifacts.json`
- If we want “download modified files by same name” semantics, plan a Context API enhancement (e.g., `downloadFiles` supporting `include_uploaded_files=true` or hash-based change detection).

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
- Ensure produced Context file records are user/conversation-scoped and access-controlled

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
- Build sandbox container image that can run the Claude Agent SDK when invoked via Dynamic Sessions “execute code”.
- Implement “runner invocation” + artifacts capture:
  - Orchestrator uploads inputs via Context API `uploadFiles`
  - Orchestrator executes the runner via Context API `executeCode`
  - Orchestrator downloads outputs via Context API `downloadFiles` (which persists them as Context files)
- Implement workspace persistence using **Context files** (e.g. `workspace-<turn>.tar.gz`) + patch artifacts (e.g. `changes.patch`).
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
- **Dynamic Sessions API shape**: can the Claude Code agent rely exclusively on the existing Context API code-session endpoints (`uploadFiles`, `executeCode`, `downloadFiles`) and Dynamic Sessions platform APIs, with **no sandbox web server**?
- **Workspace persistence**: snapshot vs diff. (Snapshot is simpler; diff is cheaper and easier to review.)
- **Credential strategy**: direct Anthropic key vs Gateway facade; PAT vs GitHub App.
- **Streaming UX**: do we need live tool event streaming for MVP, or only final response + artifacts?
- **Egress architecture**: separate container apps env for internet mode vs runtime toggles.

