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

## Intelligent Execute Operation

The MCP Client Tool supports an `intelligent_execute` operation that uses an LLM to orchestrate multi-turn MCP operations. This provides a simplified interface for complex MCP workflows without requiring the agent to manually discover and execute tools.

### Configuration Requirements

To use `intelligent_execute`, you must configure:

1. **Main Prompt Resource Object ID**: Set the `main_prompt` resource object ID in your FoundationaLLM instance configuration
2. **LLM Configuration**: Ensure a language model is configured for the tool

### Usage

The `intelligent_execute` operation accepts a user prompt and automatically:

1. **Discovers** available MCP tools via `list_tools`
2. **Plans** the execution strategy using the LLM
3. **Executes** the planned MCP operations
4. **Synthesizes** results into a coherent response

```json
{
  "operation": "intelligent_execute",
  "arguments": {
    "prompt": "Search Microsoft Learn for MCP documentation"
  }
}
```

### Example: Microsoft Learn Search

For MS Learn MCP server, the tool will:

1. Call `list_tools` to discover `microsoft_docs_search`
2. Use the LLM to map the user prompt to appropriate tool arguments
3. Execute `call_tool` with `{"name": "microsoft_docs_search", "arguments": {"query": "MCP documentation"}}`
4. Synthesize the search results into a comprehensive response

### Testing Intelligent Execute

Add tests for the `intelligent_execute` operation:

```python
def test_intelligent_execute():
    # Mock LLM responses for planning and synthesis
    # Mock MCP operations (list_tools, call_tool)
    # Verify full orchestration flow
    # Assert token tracking accuracy
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

## Agent prompt: Using the MCP Client Tool

Use this prompt snippet to guide the agent when working with MCP (Model Context Protocol) servers. 

The MCP Client Tool provides a simplified interface where agents can delegate tool discovery, selection, and execution to the tool itself.

### Primary Approach: Intelligent Execute

**Use `intelligent_execute` for most scenarios** - this allows the agent to provide a natural language prompt and let the MCP Client Tool handle:
- Discovering available tools on the MCP server
- Selecting appropriate tools based on the user's request
- Executing the tools with proper arguments
- Synthesizing results into a coherent response

#### Example Usage

```json
{
  "operation": "intelligent_execute",
  "arguments": {
    "prompt": "Search for information about Azure authentication best practices"
  }
}
```

For Microsoft Learn MCP server, this would automatically:
1. Discover the `microsoft_docs_search` tool
2. Execute the search with appropriate query parameters
3. Synthesize the results into a comprehensive response

#### More Examples

```json
{
  "operation": "intelligent_execute",
  "arguments": {
    "prompt": "Find code samples for ASP.NET Core web API development"
  }
}
```

```json
{
  "operation": "intelligent_execute",
  "arguments": {
    "prompt": "Get the latest documentation on Azure Functions deployment"
  }
}
```

### Manual Approach (Advanced)

When you need precise control over MCP operations, use the manual approach:

#### Discover Available Tools

```json
{
  "operation": "list_tools",
  "arguments": {}
}
```

#### Call Specific Tools

```json
{
  "operation": "call_tool",
  "arguments": {
    "name": "tool_name",
    "arguments": {
      "param1": "value1",
      "param2": "value2"
    },
    "read_timeout_seconds": 30
  },
  "response_format": "json"
}
```

#### Other Operations

```json
{
  "operation": "list_resources",
  "arguments": {}
}
```

```json
{
  "operation": "read_resource",
  "arguments": {
    "uri": "resource://path/to/resource"
  },
  "read_timeout_seconds": 60
}
```

### System Guidance for the Agent

- **Prefer `intelligent_execute`** for most user requests - it handles complexity automatically
- The MCP Client Tool requires top-level fields: `operation` and `arguments`
- For `intelligent_execute`, simply provide a clear, descriptive prompt in the `arguments.prompt` field
- Use manual operations only when you need specific control or when `intelligent_execute` doesn't meet your needs
- Use `response_format: "json"` and set reasonable timeouts for long operations
- **Token tracking**: The tool automatically tracks LLM token usage for `intelligent_execute` operations

### Configuration Requirements

For `intelligent_execute` to work properly, ensure the tool is configured with:
1. **Main Model Resource**: An LLM configured as `main_model` resource
2. **Main Prompt Resource**: An orchestration prompt configured as `main_prompt` resource

### Answering Behavior

- **Default to `intelligent_execute`** for user queries that involve MCP server interactions
- Provide clear, descriptive prompts that specify what information the user is seeking
- Let the tool handle tool discovery and execution - don't manually discover tools unless necessary
- Present the synthesized results in a user-friendly format
- Include relevant metadata and links returned by the MCP server when appropriate

