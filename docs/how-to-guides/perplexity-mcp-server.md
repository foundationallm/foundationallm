# Perplexity MCP Server

This guide describes how to run the stand-alone Model Context Protocol (MCP) server that wraps the [Perplexity Python SDK](https://github.com/perplexityai/perplexity-py). The server is implemented with the official [`mcp`](https://pypi.org/project/mcp/) FastMCP framework and exposes two tools:

- **Search** – Performs focused web searches using `client.search.create`.
- **ChatCompletions** – Runs agentic conversations that can automatically consult the web through `client.chat.completions.create`.

The MCP server is designed to run independently of FoundationaLLM so that you can iterate locally before deploying anywhere else.

## Prerequisites

- Python 3.12 or newer.
- A Perplexity API key with search access.
- (Optional) `uv` or `pipx` if you prefer project-local tooling.

## Installation

1. Create and activate a virtual environment.

   ```bash
   python -m venv .venv
   source .venv/bin/activate
   ```

2. Install the Experimental Python dependencies (which now include `mcp` and `perplexityai`).

   ```bash
   pip install -r src/python/Experimental/requirements.txt
   ```

   Alternatively, if you only need the MCP server for development you can install just the dedicated requirements:

   ```bash
   pip install -r tests/python/PerplexityMCP.Tests/requirements.txt
   ```

3. Add the Experimental sources to `PYTHONPATH` so Python can resolve the package:

   ```bash
   export PYTHONPATH="$(pwd)/src/python/Experimental/src:${PYTHONPATH:-}"
   ```

4. Export your Perplexity API key and, optionally, override defaults:

   ```bash
   export PERPLEXITY_API_KEY="sk-..."
   # Optional configuration overrides
   export PERPLEXITY_DEFAULT_SEARCH_MODE="web"          # web | academic | sec
   export PERPLEXITY_DEFAULT_MAX_RESULTS="5"
   export PERPLEXITY_DEFAULT_CHAT_MODEL="sonar"
   export PERPLEXITY_REQUEST_TIMEOUT="30"               # seconds
   ```

## Running the server

The server exposes a small CLI located at `perplexity_mcp.main`. The commands below all run from the repository root after activating your virtual environment.

### STDIO transport

Use STDIO when driving the server from tools such as the MCP reference client or `mcp-cli`.

```bash
python -m perplexity_mcp.main --transport stdio
```

### Server-Sent Events (SSE)

SSE keeps a stateful connection open over HTTP.

```bash
python -m perplexity_mcp.main --transport sse --host 0.0.0.0 --port 8001
```

Clients should connect to `http://<host>:<port>/sse` and post outbound messages to `http://<host>:<port>/messages/`.

### Stateless HTTP transport

When you need classic request/response behaviour, run the server in stateless HTTP mode. This internally reuses FastMCP’s Streamable HTTP implementation but forces JSON responses so the server behaves like a pure REST endpoint.

```bash
python -m perplexity_mcp.main --transport http --host 0.0.0.0 --port 8002 --streamable-http-path /mcp
```

Requests are served from `http://<host>:<port>/mcp`.

### Streamable HTTP transport

Streamable HTTP keeps conversational state between requests while still using plain HTTP endpoints (ideal for modern MCP clients).

```bash
python -m perplexity_mcp.main --transport streamable-http --host 0.0.0.0 --port 8003 --streamable-http-path /mcp
```

### Additional options

The CLI exposes extra options should you need to tweak the endpoints:

```bash
python -m perplexity_mcp.main --help
```

All transports run anonymously; no OAuth or custom authentication is configured by default.

## Verifying the implementation

Automated tests ensure the tools call the Perplexity SDK with the correct payloads and that environment variables are respected.

```bash
pytest tests/python/PerplexityMCP.Tests
```

The test suite uses mocked Perplexity clients so that no live network access is required.

## Next steps

- Integrate the server into your MCP-capable client by pointing it to the desired transport.
- Adjust defaults via environment variables or by wrapping `create_perplexity_mcp_server` with custom factories.
- Extend the server with additional tools if you need richer Perplexity functionality.
