# MCP Client Tool Implementation Plan

## 1. Goals and Non-Goals
- **Goals**
  - Create an MCP tool plugin for FoundationalLM that exposes a configurable client capable of connecting to any MCP server using the official MCP Python SDK.
  - Support synchronous and asynchronous invocation pathways within the agent runtime.
  - Provide transport-agnostic configuration covering STDIO, streamable HTTP, and HTTP+SSE transports supported by MCP.
  - Supply thorough configuration documentation and developer-facing usage examples.
  - Deliver automated tests exercising core functionality, including integration with the Microsoft Learn MCP Server (anonymous auth via endpoint URL).
- **Non-Goals**
  - Implement new MCP servers or modify existing agent frameworks beyond the FoundationalLM plugin surface.
  - Reimplement Microsoft Agent Framework samples; instead, rely solely on the official MCP Python SDK.

## 2. Context and Existing Patterns
- Review existing FoundationalLM MCP-related plugins (e.g., code interpreter, knowledge search, no-op tool) to align with established plugin interfaces and configuration patterns.
- Study the official MCP Python SDK documentation and sample clients: <https://github.com/modelcontextprotocol/python-sdk#writing-mcp-clients>.
- Inspect `agent-mcp-mslearn.py` from the NextGen AI Agents workshop for guidance on configuring the Microsoft Learn MCP server while adapting the approach to the Python SDK and FoundationalLM patterns.

## 3. High-Level Architecture
1. **Plugin Entry Point**
   - Implement a new tool module (e.g., `src/foundationallm/agents/tools/mcp_client`) exporting the tool metadata and factory consistent with other tools.
2. **Configurable MCP Client Wrapper**
   - Encapsulate the official MCP SDK client creation, supporting:
     - Transport selection (STDIO, HTTP stream, HTTP+SSE) with associated parameters.
     - Authentication options (anonymous, API key/token placeholder for future extensions).
     - Tool invocation options (timeout, concurrency limits).
   - Provide both synchronous (`invoke`) and asynchronous (`ainvoke`) execution paths by leveraging the SDK's async primitives.
3. **Request Routing**
   - Accept generic MCP requests (method name + params) and proxy them to the target server.
   - Handle response normalization to conform with FoundationalLM tool response schema (success payload, error handling, metadata).
   - Populate FoundationalLM content artifacts with detailed request, transport, and response diagnostics to aid downstream tooling.
4. **Resource Management**
   - Manage client lifecycle (connect, teardown) with context managers.
   - Establish per-invocation ephemeral connections only; do not maintain persistent connections across tool sessions.

## 4. Detailed Work Breakdown
1. **Design**
   - Draft configuration schema (Pydantic or equivalent) capturing transport, endpoint, authentication, and execution settings, including HTTP streaming vs. SSE toggles.
   - Encode lifecycle controls that enforce ephemeral connections and prohibit cross-session persistence.
   - Define internal helper classes for STDIO and HTTP transport initialisation using the MCP SDK.
2. **Implementation**
   - Create module structure and implement the tool factory aligning with existing tools.
   - Implement synchronous and asynchronous invocation wrappers, ensuring consistent OpenTelemetry logging, content artifact enrichment, and error translation.
   - Add configuration-driven selection logic for transport initialisation across STDIO, streamable HTTP, and HTTP+SSE, explicitly excluding unsupported transports.
   - Implement lifecycle management (connect/disconnect) with graceful cleanup on errors, ensuring connections are torn down after each invocation.
3. **Documentation**
   - Produce README-style documentation detailing setup, configuration examples, and usage patterns (see Section 6 below for outline).
4. **Testing & Validation**
   - Implement unit tests with mocked MCP client to validate configuration parsing, transport selection, and sync/async flows.
   - Add integration test targeting the Microsoft Learn MCP server via HTTP transport (requires endpoint environment variable or test fixture configuration).
   - Ensure tests run within existing CI (pytest or relevant test harness).
5. **Tool Registration**
   - Update any relevant registries or manifests so the new tool is discoverable within FoundationalLM.
   - Provide sample configuration snippet in repository docs/tests for easier adoption.

## 5. Risk Mitigation
- **Transport Compatibility**: Validate STDIO, HTTP stream, and HTTP+SSE transports across platforms with unit tests simulating subprocess/HTTP clients.
- **Asynchronous Handling**: Use `asyncio` event loop safe patterns, ensuring compatibility with existing agent runtime concurrency model.
- **External Dependency Stability**: Pin MCP SDK version compatible with project dependencies; document upgrade strategy.
- **Integration Flakiness**: Wrap integration tests with retry or mark them as optional if remote service availability is intermittent; provide mock server fallback instructions.

## 6. Documentation Deliverables
Create a new documentation page (e.g., `docs/tools/mcp-client.md`) containing:
- Overview and capabilities.
- Installation and dependency requirements (including MCP SDK version).
- Configuration reference:
  - Transport options and fields (STDIO, HTTP streaming, HTTP+SSE) with parameter tables and when to choose each option.
  - Authentication and headers support.
  - Invocation modes (sync vs. async) with code snippets.
  - Lifecycle expectations (ephemeral per-call connections with mandatory teardown).
- Step-by-step guide for registering the tool within FoundationalLM config files.
- Example using the Microsoft Learn MCP server, including sample configuration and expected responses.
- Troubleshooting section (common errors, debugging tips, and guidance on interpreting generated content artifacts).
- Clarify that the tool supports only STDIO, HTTP streaming, and HTTP+SSE transports per the MCP specification.
- Note that connections are ephemeral and closed after each invocation to align with FoundationaLLM tooling conventions.

## 7. Testing Strategy
- **Unit Tests**
  - Configuration schema validation (required/optional fields, defaults).
  - Transport factory selection logic (STDIO vs. HTTP streaming vs. HTTP+SSE) with mocked dependencies.
  - Content artifact builders ensuring request/response metadata is captured for FoundationaLLM integrations.
  - Synchronous invocation flow verifying request translation and response handling.
  - Asynchronous invocation flow ensuring event loop management and concurrency handling.
  - Error propagation and cleanup behaviour.
- **Integration Tests**
  - Use Microsoft Learn MCP server endpoint (configured via environment variable like `MSLEARN_MCP_URL`).
  - Verify both sync and async invocation pathways produce expected structured responses for sample queries and emit rich content artifacts summarizing requests/responses.
  - Assert that OpenTelemetry spans/logs are emitted for connect, request, and teardown lifecycle phases.
  - Include network availability guard/skip to prevent CI failures if endpoint unreachable.
- **Mock Server Tests (Optional)**
  - Implement lightweight local MCP server fixture using SDK to simulate responses for deterministic testing.

## 8. Dependencies and Tooling
- Add official MCP Python SDK dependency to project requirements (setup or poetry/requirements file).
- Ensure test utilities accommodate async tests (pytest-asyncio or equivalent).
- Integrate OpenTelemetry for lifecycle logging consistent with other tools.

## 9. Timeline & Milestones (Indicative)
1. **Day 1-2**: Finalize configuration design, update dependency management, scaffold module structure.
2. **Day 3-4**: Implement client wrapper, sync/async invocation logic, and transport handling.
3. **Day 5**: Draft documentation page, sample configs, and developer guide.
4. **Day 6**: Implement unit tests and integrate with CI.
5. **Day 7**: Add integration test with Microsoft Learn MCP server, finalize PR with documentation and testing artifacts.

## 10. Open Questions
- None at this time.

## 11. Acceptance Criteria
- New MCP client tool module merged and registered with FoundationalLM agents.
- Tool supports STDIO, streamable HTTP, and HTTP+SSE transports only, with documented configuration options.
- Both synchronous and asynchronous invocation pathways implemented and tested.
- Documentation describing setup and usage is complete and linked from primary tool docs.
- Unit and integration tests passing, including verification against Microsoft Learn MCP server (or documented fallback).
- OpenTelemetry logging spans recorded for key lifecycle events (connect, request, teardown).
