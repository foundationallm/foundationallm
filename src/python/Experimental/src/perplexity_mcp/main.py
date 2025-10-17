"""Command line entry-point for the Perplexity MCP server."""

from __future__ import annotations

import argparse
from typing import Sequence

from .config import PerplexityConfig
from .server import create_perplexity_mcp_server


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Run the Perplexity MCP server using FastMCP transports.",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )
    parser.add_argument(
        "--transport",
        choices=["stdio", "sse", "http", "streamable-http"],
        default="stdio",
        help="Transport protocol exposed by the server.",
    )
    parser.add_argument("--host", default="127.0.0.1", help="Bind address for HTTP-based transports.")
    parser.add_argument("--port", type=int, default=8000, help="Port used for HTTP-based transports.")
    parser.add_argument("--mount-path", default="/", help="Base mount path for SSE endpoints.")
    parser.add_argument("--message-path", default="/messages/", help="Endpoint used to post SSE messages.")
    parser.add_argument(
        "--streamable-http-path",
        default="/mcp",
        help="Path serving the Streamable HTTP endpoint.",
    )
    parser.add_argument(
        "--log-level",
        choices=["DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"],
        default="INFO",
        help="Logging level used by FastMCP.",
    )
    return parser


def main(argv: Sequence[str] | None = None) -> None:
    parser = build_arg_parser()
    args = parser.parse_args(argv)

    config = PerplexityConfig.from_env()
    server = create_perplexity_mcp_server(
        config,
        host=args.host,
        port=args.port,
        mount_path=args.mount_path,
        message_path=args.message_path,
        streamable_http_path=args.streamable_http_path,
        log_level=args.log_level,
    )
    server.run(transport=args.transport, mount_path=args.mount_path)


if __name__ == "__main__":  # pragma: no cover - exercised when executed as module
    main()
