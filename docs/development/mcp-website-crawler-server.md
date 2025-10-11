# Playwright MCP Website Crawler Server

This guide explains how to run and test the Playwright-powered Model Context Protocol (MCP) server that crawls websites. The server is implemented with the official MCP Python SDK using the FastMCP interface and exposes both synchronous and asynchronous crawl tools.

## Features

- Playwright-based crawler with synchronous (`crawl_sync`) and asynchronous (`crawl_async`) tools.
- Supports STDIO, Server-Sent Events (SSE), HTTP (stateless) and Streamable HTTP transports.
- Anonymous-only authentication; no credentials are required or accepted.
- Ships as a standalone Python module (`mcp_playwright_crawler`) that can be executed with `python -m`.
- Includes unit tests with Playwright stubs so test execution does not require browser binaries.

## Prerequisites

- Python 3.12 (the repository's development baseline).
- `pip` for dependency installation.
- Access to download Playwright browser binaries (for real crawls).

Install the Python dependencies:

```bash
python -m venv .venv
source .venv/bin/activate
pip install --upgrade pip
pip install mcp playwright anyio
# Install browsers required by Playwright
playwright install chromium
```

> **Tip:** When developing inside the repository, set `PYTHONPATH=src/python/Experimental` to allow Python to locate the `mcp_playwright_crawler` package without an install step.

## Running the Server

The server is exposed as a module with a command-line interface. The examples below assume you are in the repository root with the virtual environment activated and `PYTHONPATH` configured as described above.

### STDIO Transport

The STDIO transport is convenient for local MCP client development.

```bash
PYTHONPATH=src/python/Experimental python -m mcp_playwright_crawler --transport stdio
```

The process will block and wait for a client connection on STDIN/STDOUT.

### SSE Transport

The SSE transport exposes the server over HTTP using Server-Sent Events.

```bash
PYTHONPATH=src/python/Experimental python -m mcp_playwright_crawler \
  --transport sse \
  --host 0.0.0.0 \
  --port 8000 \
  --mount-path /crawler \
  --sse-path /events \
  --message-path /messages/
```

Clients should connect to `http://<host>:<port>/crawler/events` for the SSE stream and POST messages to `http://<host>:<port>/messages/`.

### Stateless HTTP Transport

This mode uses FastMCP's stateless HTTP handling where every request creates a fresh MCP session. It is useful when a simple request/response interaction is required.

```bash
PYTHONPATH=src/python/Experimental python -m mcp_playwright_crawler \
  --transport http \
  --host 0.0.0.0 \
  --port 8001 \
  --streamable-http-path /mcp \
  --json-response
```

With `--json-response` enabled, the server will respond with JSON payloads instead of streaming events.

### Streamable HTTP Transport

Streamable HTTP maintains MCP sessions across requests and supports resumable streams.

```bash
PYTHONPATH=src/python/Experimental python -m mcp_playwright_crawler \
  --transport streamable-http \
  --host 0.0.0.0 \
  --port 8002 \
  --streamable-http-path /mcp
```

This mode is stateful and is appropriate for long-running MCP sessions. You can optionally omit `--json-response` to keep streaming semantics.

## Tool Parameters

Both `crawl_sync` and `crawl_async` accept the same parameters:

| Parameter    | Description                                                      | Default |
|--------------|------------------------------------------------------------------|---------|
| `url`        | Starting URL for the crawl (required).                           | —       |
| `max_depth`  | Maximum link depth to follow.                                    | `1`     |
| `max_pages`  | Maximum number of pages to visit.                                | `10`    |
| `same_origin`| If `True`, restricts the crawl to the origin of the starting URL.| `True`  |
| `timeout_ms` | Navigation timeout per page in milliseconds.                     | `30000` |
| `wait_until` | Playwright wait condition (e.g. `load`, `domcontentloaded`).     | `load`  |
| `headless`   | Launch browsers in headless mode.                                | `True`  |
| `user_agent` | Optional custom user agent string.                               | `None`  |

## Running the Test Suite

Unit tests use Playwright stubs and therefore do not require browser binaries. Execute them with:

```bash
PYTHONPATH=src/python/Experimental pytest tests/python/Experimental/test_mcp_playwright_crawler.py
```

All tests should pass without launching actual browsers.

## Additional Notes

- The server currently permits only anonymous access. Authorization headers are ignored.
- When running real crawls, ensure outbound network access and respect the target website's robots.txt and terms of service.
- The CLI exposes `--debug` and `--log-level` switches that map directly to FastMCP's runtime settings if you need verbose logging during troubleshooting.
