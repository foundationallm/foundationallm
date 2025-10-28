# FoundationaLLM MCP Client Tool

The FoundationaLLM MCP client tool allows agents to call any [Model Context Protocol](https://modelcontextprotocol.io/) server using the official Python SDK. It mirrors the configuration patterns used by other FoundationaLLM tools while adding transport-specific options and telemetry instrumentation.

## Capabilities

- Connects to arbitrary MCP servers over **STDIO**, **streamable HTTP**, or **HTTP + Server-Sent Events (SSE)** transports.
- Supports both synchronous (`_run`) and asynchronous (`_arun`) invocation paths.
- Emits rich content artifacts summarizing the invocation, including transport details, session identifiers, initialization metadata, and structured responses.
- Records OpenTelemetry spans for connection and request execution phases, aligning with existing FoundationaLLM tooling expectations.
- Provides per-invocation transport overrides so that agents can adjust endpoints or headers dynamically without redeploying the tool configuration.

## Installation

Add the `mcp` SDK dependency (already pinned in `FoundationaLLMAgentPlugins/requirements.txt`):

```bash
pip install mcp==1.17.0
```

## Configuration Reference

Register the tool in an agent definition using the `FoundationaLLMMCPClientTool` class. Tool properties are validated by a Pydantic model with the following top-level fields:

| Property | Type | Description |
| --- | --- | --- |
| `transport` | `"stdio" \| "streamable_http" \| "http_sse"` | Transport protocol to use. Defaults to `streamable_http`. |
| `stdio` | Object | Required when `transport` is `stdio`; see below. |
| `streamable_http` | Object | Required when `transport` is `streamable_http`; see below. |
| `http_sse` | Object | Required when `transport` is `http_sse`; see below. |
| `client_name` | String | Reported to the server as the MCP client name. |
| `client_version` | String | Reported to the server as the MCP client version. |
| `default_operation_timeout_seconds` | Number | Optional default timeout applied to operations that accept a `read_timeout_seconds` parameter (e.g., `call_tool`). |

### STDIO settings

```json
{
  "transport": "stdio",
  "stdio": {
    "command": "./my-mcp-server",
    "args": ["--flag"],
    "env": {"API_TOKEN": "config://my-secret"},
    "cwd": "/opt/mcp",
    "encoding": "utf-8",
    "encoding_error_handler": "strict"
  }
}
```

### Streamable HTTP settings

```json
{
  "transport": "streamable_http",
  "streamable_http": {
    "url": "https://example.com/mcp",
    "headers": {
      "Authorization": "config://my-service-token"
    },
    "timeout_seconds": 30,
    "sse_read_timeout_seconds": 300,
    "terminate_on_close": true
  }
}
```

### HTTP + SSE settings

```json
{
  "transport": "http_sse",
  "http_sse": {
    "url": "https://example.com/mcp",
    "headers": {},
    "timeout_seconds": 15,
    "sse_read_timeout_seconds": 120
  }
}
```

Configuration values that begin with `config://` are resolved through `Configuration.get_value`, allowing secrets to be stored in Azure App Configuration or Key Vault. Values beginning with `env://` are also resolved through `Configuration.get_value`, enabling environment variable lookups when the platform is configured to allow them.

## Invocation Schema

The tool exposes the following input schema (see `FoundationaLLMMCPClientToolInput`):

```json
{
  "operation": "list_tools",
  "arguments": {
    "cursor": null
  },
  "transport_overrides": {
    "headers": {"X-MCP-Trace": "123"}
  },
  "response_format": "json"
}
```

- `operation` must match one of the supported `ClientSession` coroutine names: `list_tools`, `list_resources`, `read_resource`, `list_prompts`, `get_prompt`, `call_tool`, `complete`, or `ping`.
- `arguments` is forwarded directly to the MCP SDK after basic validation (timeouts are converted to `timedelta`, URLs validated, etc.).
- `transport_overrides` applies per-call URL, header, or timeout adjustments on top of the configured transport. Unsupported fields are ignored for non-HTTP transports.
- `response_format` can be `json` (default) or `text`. JSON responses are pretty-printed before being returned.

## Content Artifacts

Each invocation produces a `TOOL_EXECUTION` artifact containing:

- The structured response body (JSON by default).
- Metadata describing the transport (`stdio`, `streamable_http`, or `http_sse`), target endpoint or command, resolved headers/env (with secrets redacted upstream), and initialization payload returned by the MCP server.
- A normalized record of the request arguments so downstream components can audit calls.

Errors generate an additional `TOOL_ERROR` artifact populated with the exception details.

## Example: Microsoft Learn MCP Server

To connect to the Microsoft Learn MCP server (anonymous HTTP access), configure the tool as follows:

```json
{
  "name": "mslearn_mcp",
  "description": "Call the Microsoft Learn MCP server for training content.",
  "package_name": "FoundationaLLM",
  "class_name": "FoundationaLLMMCPClientTool",
  "properties": {
    "transport": "streamable_http",
    "streamable_http": {
      "url": "https://learn.microsoft.com/api/mcp",
      "headers": {},
      "timeout_seconds": 30,
      "sse_read_timeout_seconds": 300,
      "terminate_on_close": true
    }
  }
}
```

Invoke the tool with the following payload to enumerate available tools:

```json
{
  "operation": "list_tools",
  "arguments": {}
}
```

## Testing

- Unit tests under `FoundationaLLMAgentPlugins/test/foundationallm_mcp_client_tool` validate configuration parsing, transport override merging, synchronous/asynchronous execution paths, and artifact generation using mocked sessions.
- An integration test exercises the Microsoft Learn MCP server when the `MSLEARN_MCP_ENDPOINT` environment variable is present. The test is skipped automatically when the endpoint is unavailable.

Run the tests with:

```bash
pytest test/foundationallm_mcp_client_tool
```

> **Note:** The integration test issues live requests; ensure the endpoint is reachable and stable before enabling it in automated pipelines.

### Running the Microsoft Learn search integration test

To invoke the `microsoft_docs_search` tool with the query "How does Azure use Model Context Protocol (MCP)?", set the endpoint and run the targeted test:

```powershell
$env:MSLEARN_MCP_ENDPOINT = "https://learn.microsoft.com/api/mcp"
pytest test/foundationallm_mcp_client_tool -k mslearn_docs_search -q
```

To also print request/response details to stdout, run the test in verbose mode:

```powershell
$env:MSLEARN_MCP_ENDPOINT = "https://learn.microsoft.com/api/mcp"
pytest test/foundationallm_mcp_client_tool -k mslearn_docs_search -v -q
```

### Showing print output

By default, pytest captures `print` output. To force printing to the console, add `-s` (disable capture). You can combine this with verbosity:

```powershell
$env:MSLEARN_MCP_ENDPOINT = "https://learn.microsoft.com/api/mcp"
pytest test/foundationallm_mcp_client_tool -k mslearn_docs_search -vv -s
```

Relevant switches:
- `-k pattern`: run tests matching the substring or expression (e.g., `mslearn_docs_search`).
- `-v`: verbose mode. Shows each test node id as it runs, includes deselected/xfail/xpass summaries, and provides richer assertion diffs.
- `-vv`: very verbose. Everything from `-v`, plus longer/full test node ids (paths and parametrization), more fixture/collection details, and chattier progress output.
- `-s`: disable output capturing so `print()` statements appear in real time.
- `-q`: quiet mode; reduces non-essential output (omit when you want more detail).

Alternative (logging instead of prints):

```powershell
$env:MSLEARN_MCP_ENDPOINT = "https://learn.microsoft.com/api/mcp"
pytest test/foundationallm_mcp_client_tool -k mslearn_docs_search -vv -o log_cli=true -o log_cli_level=INFO
```

## Agent prompt: Using the MCP Client Tool with Microsoft Learn MCP

Use this prompt snippet to guide the agent when answering Microsoft technology questions. It enforces correct use of the MCP Client Tool input schema (top-level `operation` and `arguments`) and dynamic tool discovery. Reference: Microsoft Learn MCP Server (`https://github.com/microsoftdocs/mcp`).

### System guidance for the agent

- Always consult Microsoft Learn via the MCP Client Tool first for Microsoft technologies.
- The MCP Client Tool requires top-level fields: `operation` and `arguments`.
  - `operation`: one of `list_tools`, `call_tool`, `list_resources`, `read_resource`, `list_prompts`, `get_prompt`, `complete`, `ping`.
  - `arguments`: object forwarded to the selected operation.
- Discover tools dynamically using `list_tools`; do not hard-code schemas. Read each tool’s `inputSchema` and shape your `arguments` accordingly.
- Prefer `microsoft_docs_search` to find relevant content; use `microsoft_docs_fetch` to retrieve full documents when needed. Optionally use `microsoft_code_sample_search` for code examples.
- Use `response_format: "json"` and set a reasonable `read_timeout_seconds` for long operations.
- If a call fails due to schema or availability, re-run `list_tools` and retry aligned to the latest `inputSchema`.

### Canonical invocation patterns

- List available tools

```json
{
  "operation": "list_tools",
  "arguments": {}
}
```

- Search Microsoft Learn

```json
{
  "operation": "call_tool",
  "arguments": {
    "name": "microsoft_docs_search",
    "arguments": {
      "query": "How does Azure use Model Context Protocol (MCP)?"
    },
    "read_timeout_seconds": 30
  },
  "response_format": "json"
}
```

- Fetch a full document by id (from search results)

```json
{
  "operation": "call_tool",
  "arguments": {
    "name": "microsoft_docs_fetch",
    "arguments": {
      "id": "<document-identifier>"
    },
    "read_timeout_seconds": 60
  },
  "response_format": "json"
}
```

- Search code samples (optional)

```json
{
  "operation": "call_tool",
  "arguments": {
    "name": "microsoft_code_sample_search",
    "arguments": {
      "query": "ASP.NET Core authentication",
      "language": "csharp"
    },
    "read_timeout_seconds": 30
  },
  "response_format": "json"
}
```

### Answering behavior

- Summarize the most relevant search hits with titles, links, and brief excerpts.
- When the user asks for details or full content, fetch the document via `microsoft_docs_fetch`.
- Cite Microsoft Learn links returned by the MCP server in answers.

