"""Input schema for the FoundationaLLM MCP client tool."""

from __future__ import annotations

from typing import Any, Dict, Optional

from pydantic import BaseModel, Field


class MCPTransportOverrides(BaseModel):
    """Per-invocation overrides for transport behaviour."""

    url: Optional[str] = Field(
        default=None,
        description="Override the base transport endpoint URL for this invocation.",
    )
    headers: Optional[Dict[str, str]] = Field(
        default=None,
        description="Headers to merge with the configured headers for this invocation.",
    )
    timeout_seconds: Optional[float] = Field(
        default=None,
        description="Override the general HTTP timeout (streamable HTTP and SSE only).",
    )
    sse_read_timeout_seconds: Optional[float] = Field(
        default=None,
        description="Override the SSE read timeout (streamable HTTP and SSE only).",
    )
    terminate_on_close: Optional[bool] = Field(
        default=None,
        description="Override terminate-on-close behaviour for streamable HTTP sessions.",
    )


class FoundationaLLMMCPClientToolInput(BaseModel):
    """Input payload accepted by the MCP client tool."""

    operation: str = Field(
        description=(
            "Name of the MCP client session coroutine to invoke, such as ``list_tools`` "
            "or ``call_tool``."
        )
    )
    arguments: Dict[str, Any] = Field(
        default_factory=dict,
        description="Arguments forwarded to the selected MCP operation.",
    )
    transport_overrides: Optional[MCPTransportOverrides] = Field(
        default=None,
        description=(
            "Optional overrides applied on top of the configured transport settings for "
            "this invocation."
        ),
    )
    response_format: str = Field(
        default="json",
        description=(
            "Hint describing how the response should be formatted. The tool currently "
            "supports ``json`` (default) and ``text`` outputs."
        ),
    )

