"""Command line entrypoint for the Playwright MCP crawler server."""

from __future__ import annotations

import argparse
import anyio

from .app import create_app


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Run the Playwright-powered MCP crawler server.",
    )
    parser.add_argument(
        "--transport",
        choices=["stdio", "sse", "http", "streamable-http"],
        default="stdio",
        help="Transport protocol to expose.",
    )
    parser.add_argument("--host", default="127.0.0.1", help="Host binding for HTTP-based transports.")
    parser.add_argument("--port", type=int, default=8000, help="Port for HTTP-based transports.")
    parser.add_argument("--mount-path", default="/", help="Mount path for SSE transport.")
    parser.add_argument("--sse-path", default="/sse", help="SSE endpoint path.")
    parser.add_argument("--message-path", default="/messages/", help="Message endpoint for SSE transport.")
    parser.add_argument("--streamable-http-path", default="/mcp", help="Streamable HTTP endpoint path.")
    parser.add_argument("--json-response", action="store_true", help="Enable JSON responses for HTTP modes.")
    parser.add_argument("--debug", action="store_true", help="Enable FastMCP debug mode.")
    parser.add_argument(
        "--log-level",
        default="INFO",
        choices=["DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"],
        help="Log level for FastMCP.",
    )
    parser.add_argument("--name", default="foundationallm-playwright-crawler", help="Server display name.")
    parser.add_argument("--instructions", default=None, help="Optional custom instructions for clients.")
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    app = create_app(
        name=args.name,
        instructions=args.instructions,
        host=args.host,
        port=args.port,
        mount_path=args.mount_path,
        sse_path=args.sse_path,
        message_path=args.message_path,
        streamable_http_path=args.streamable_http_path,
        stateless_http=args.transport == "http",
        json_response=args.json_response,
        debug=args.debug,
        log_level=args.log_level,
    )

    if args.transport == "stdio":
        app.run("stdio")
    elif args.transport == "sse":
        app.run("sse", mount_path=args.mount_path)
    else:
        # Both HTTP and Streamable HTTP share the same runner with different statefulness settings.
        anyio.run(app.run_streamable_http_async)


if __name__ == "__main__":
    main()
