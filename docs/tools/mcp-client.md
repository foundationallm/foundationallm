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
pytest src/python/FoundationaLLMAgentPlugins/test/foundationallm_mcp_client_tool
```

> **Note:** The integration test issues live requests; ensure the endpoint is reachable and stable before enabling it in automated pipelines.

