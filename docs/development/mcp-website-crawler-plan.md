# Standalone MCP Website Crawler Server Plan

## Overview

This document describes the plan for implementing a standalone Model Context Protocol (MCP) server that crawls websites using [Playwright](https://playwright.dev/python/). The server exposes a crawl tool capable of synchronous and asynchronous execution paths. The URL to crawl is supplied by the caller via tool parameters. The server is intended to run independently while remaining deployable inside the wider FoundationaLLM ecosystem.

## Goals

- Provide an MCP-compliant Python server package dedicated to web crawling.
- Rely on Playwright for page loading, rendering, and resource fetching.
- Offer a single crawl tool whose execution can be requested in both sync and async forms.
- Allow callers to pass the target URL at runtime with optional depth, scope, and throttling controls.
- Produce structured crawl results with metadata suitable for downstream embedding or analysis workflows.

## Non-Goals

- Building a general-purpose scraping framework beyond basic HTML extraction and link discovery.
- Providing persistence, scheduling, or distributed execution components.
- Handling authentication, CAPTCHA solving, or bypassing bot protections.

## High-Level Architecture

```
+-------------------------+
|  Client (MCP consumer)  |
+-----------+-------------+
            |
        MCP protocol
            |
+-----------v-------------+
|  Standalone MCP Server  |
|  (Python, asyncio)      |
+-----------+-------------+
            |
        Crawl Tool API
            |
+-----------v-------------+
| Crawl Coordinator       |
|  - validates request    |
|  - selects sync/async   |
+-----------+-------------+
            |
        Playwright Driver
            |
+-----------v-------------+
|  Browser Context        |
|  - page navigation      |
|  - content extraction   |
|  - link discovery       |
+-------------------------+
```

### Components

1. **Server Entrypoint (`mcp_server.py`)**
   - Implements the MCP server loop using `foundationallm.mcp` helpers or a lightweight protocol server.
   - Registers the crawl tool descriptor (parameters, description, return type).
   - Routes tool invocations to the crawl coordinator.

2. **Configuration Module (`config.py`)**
   - Reads environment variables or CLI flags for Playwright browser choice, concurrency limits, and default crawl settings.
   - Provides configuration dataclasses consumed by other modules.

3. **Crawl Coordinator (`crawler.py`)**
   - Validates and normalizes tool inputs (`url`, `max_depth`, `max_pages`, `async_mode`, etc.).
   - Orchestrates Playwright sessions by delegating to synchronous or asynchronous workers.
   - Handles logging, error translation to MCP-friendly failure responses, and timeout enforcement.

4. **Playwright Workers**
   - `sync_worker.py`: wraps Playwright's sync API (`playwright.sync_api`) for environments where blocking execution is acceptable.
   - `async_worker.py`: uses Playwright's async API (`playwright.async_api`) to integrate with asyncio-based MCP servers.
   - Both workers share utility functions for HTML extraction, link parsing, and respecting robots.txt (if included).

5. **Result Model (`models.py`)**
   - Defines Pydantic (or dataclass) schemas for crawl requests, page results, and aggregate response objects.
   - Ensures consistent serialization across sync and async paths.

## Tool Contract

- **Name:** `crawl_website`
- **Parameters:**
  - `url` *(string, required)*: Starting URL to crawl.
  - `max_depth` *(integer, optional, default 1)*: Depth of link traversal.
  - `max_pages` *(integer, optional, default 10)*: Upper bound on pages visited.
  - `async_mode` *(boolean, optional, default false)*: When true, schedules the crawl via the async worker and returns a job handle.
  - `wait_for` *(float, optional)*: Seconds to wait for network idle or selector completion per page.
  - `include_html` *(boolean, optional, default true)*: Whether to include raw HTML in results.
  - `selectors` *(list[string], optional)*: CSS selectors whose text content should be collected.
- **Response:**
  - In sync mode: immediate `CrawlResult` containing metadata and page summaries.
  - In async mode: initial `CrawlJob` handle; a separate `get_crawl_status` tool may be provided to poll for completion.

## Sync Execution Flow

1. Server receives `crawl_website` invocation with `async_mode=false` (default).
2. Crawl coordinator validates input, instantiates configuration, and launches the Playwright sync worker.
3. Sync worker uses `with sync_playwright()` to create a browser context, navigates to the requested URL, waits for page load, extracts data, and recursively visits internal links respecting limits.
4. Worker returns aggregated results to coordinator, which formats them as MCP tool output.

## Async Execution Flow

1. Server receives `crawl_website` invocation with `async_mode=true`.
2. Coordinator enqueues the request onto an asyncio task queue (e.g., `asyncio.create_task`).
3. Coordinator immediately returns a `CrawlJob` handle containing job ID and initial status.
4. A background async worker uses `async_playwright()` within a long-lived browser context pool to execute the crawl.
5. Results are stored in an in-memory cache or optional persistent store.
6. Client polls `get_crawl_status` (or receives notifications if supported) to retrieve final results.

## Data Structures

```python
@dataclass
class CrawlRequest:
    url: AnyHttpUrl
    max_depth: int = 1
    max_pages: int = 10
    async_mode: bool = False
    wait_for: Optional[float] = None
    include_html: bool = True
    selectors: Optional[list[str]] = None

@dataclass
class PageResult:
    url: AnyHttpUrl
    status: int
    title: str | None
    text: str | None
    html: str | None
    links: list[str]

@dataclass
class CrawlResult:
    pages: list[PageResult]
    total_pages: int
    warnings: list[str]

@dataclass
class CrawlJob:
    job_id: str
    status: Literal["pending", "running", "succeeded", "failed"]
    result: CrawlResult | None = None
    error: str | None = None
```

## Error Handling Strategy

- Normalize Playwright navigation and timeout errors into descriptive MCP tool failures.
- Detect invalid or disallowed URLs before launching Playwright to reduce wasted sessions.
- Provide clear warnings when robots.txt disallows crawling, when max limits are reached, or when content exceeds size thresholds.

## Testing Approach

- Unit tests for input validation and coordinator decision logic.
- Integration tests that spin up a local HTTP server (e.g., `pytest-playwright` fixtures) to verify both sync and async crawling paths.
- Smoke test script that runs the MCP server, invokes the tool via CLI, and confirms structured output.

## Deployment Considerations

- Package as a standalone Python module with console entry point: `python -m foundationallm.mcp_crawler`.
- Document prerequisites: Playwright installation (`playwright install`), environment variables for browser selection, and optional proxy configuration.
- Containerize using a slim Python base image with Playwright dependencies (Chromium-only by default).

## Next Steps

1. Scaffold the MCP server package structure under `src/foundationallm/mcp_crawler/` following the components outlined above.
2. Implement shared utilities for URL normalization and HTML extraction.
3. Build sync and async workers with corresponding tests.
4. Extend documentation with usage instructions and example requests.
5. Provide deployment artifacts (Dockerfile, Helm chart updates if necessary).

