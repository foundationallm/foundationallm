"""FastMCP application exposing the Playwright crawler."""

from __future__ import annotations

from typing import Any

from mcp.server.fastmcp import FastMCP

from .crawler import CrawlConfig, crawl_site_async


def _build_config(**kwargs: Any) -> CrawlConfig:
    config = CrawlConfig(**kwargs)
    config.validate()
    return config


def create_app(
    *,
    name: str = "foundationallm-playwright-crawler",
    instructions: str | None = None,
    host: str = "127.0.0.1",
    port: int = 8000,
    mount_path: str = "/",
    sse_path: str = "/sse",
    message_path: str = "/messages/",
    streamable_http_path: str = "/mcp",
    stateless_http: bool = False,
    json_response: bool = False,
    debug: bool = False,
    log_level: str = "INFO",
) -> FastMCP:
    """Create the FastMCP server instance."""

    normalized_level = log_level.upper()
    allowed_levels = {"DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"}
    if normalized_level not in allowed_levels:
        raise ValueError(
            "log_level must be one of DEBUG, INFO, WARNING, ERROR, or CRITICAL"
        )

    app = FastMCP(
        name=name,
        instructions=(
            instructions
            or "Crawl websites using Playwright. All tools use the async API under the hood."
        ),
        host=host,
        port=port,
        mount_path=mount_path,
        sse_path=sse_path,
        message_path=message_path,
        streamable_http_path=streamable_http_path,
        stateless_http=stateless_http,
        json_response=json_response,
        debug=debug,
        log_level=normalized_level,  # FastMCP expects an upper-case value
    )

    @app.tool(
        name="crawl",
        description="Crawl a website using Playwright's asynchronous API.",
    )
    async def crawl_tool(
        url: str,
        max_depth: int = 1,
        max_pages: int = 10,
        same_origin: bool = True,
        timeout_ms: int = 30_000,
        wait_until: str = "load",
        headless: bool = True,
        user_agent: str | None = None,
    ) -> dict[str, Any]:
        """Run an asynchronous crawl and return the captured pages."""

        config = _build_config(
            max_depth=max_depth,
            max_pages=max_pages,
            same_origin=same_origin,
            timeout_ms=timeout_ms,
            wait_until=wait_until,
            headless=headless,
            user_agent=user_agent,
        )
        result = await crawl_site_async(url, config=config)
        return result.to_dict()

    return app


__all__ = ["create_app"]
