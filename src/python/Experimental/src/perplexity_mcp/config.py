"""Configuration helpers for the Perplexity MCP server."""

from __future__ import annotations

import os
from dataclasses import dataclass
from typing import Optional


@dataclass(slots=True)
class PerplexityConfig:
    """Runtime configuration for the Perplexity MCP server."""

    api_key: str
    default_search_mode: str = "web"
    default_search_max_results: int = 5
    default_chat_model: str = "sonar"
    request_timeout: Optional[float] = None

    @classmethod
    def from_env(cls) -> "PerplexityConfig":
        """Build a :class:`PerplexityConfig` from environment variables.

        The following variables are recognised:

        ``PERPLEXITY_API_KEY`` (required)
            API key used to authenticate with Perplexity.
        ``PERPLEXITY_DEFAULT_SEARCH_MODE`` (optional)
            Defaults to ``"web"`` when not supplied.
        ``PERPLEXITY_DEFAULT_MAX_RESULTS`` (optional)
            Maximum number of documents returned by the search tool. Must be an integer.
        ``PERPLEXITY_DEFAULT_CHAT_MODEL`` (optional)
            Chat model identifier used by the chat completions tool.
        ``PERPLEXITY_REQUEST_TIMEOUT`` (optional)
            Timeout (seconds) applied to outbound Perplexity requests.
        """

        api_key = os.getenv("PERPLEXITY_API_KEY")
        if not api_key:
            raise RuntimeError("PERPLEXITY_API_KEY environment variable is required")

        search_mode = os.getenv("PERPLEXITY_DEFAULT_SEARCH_MODE", "web")
        chat_model = os.getenv("PERPLEXITY_DEFAULT_CHAT_MODEL", "sonar")

        max_results_raw = os.getenv("PERPLEXITY_DEFAULT_MAX_RESULTS")
        if max_results_raw:
            try:
                max_results = int(max_results_raw)
            except ValueError as exc:  # pragma: no cover - defensive branch
                raise RuntimeError("PERPLEXITY_DEFAULT_MAX_RESULTS must be an integer") from exc
        else:
            max_results = 5

        timeout_raw = os.getenv("PERPLEXITY_REQUEST_TIMEOUT")
        timeout: Optional[float]
        if timeout_raw:
            try:
                timeout = float(timeout_raw)
            except ValueError as exc:  # pragma: no cover - defensive branch
                raise RuntimeError("PERPLEXITY_REQUEST_TIMEOUT must be numeric") from exc
        else:
            timeout = None

        return cls(
            api_key=api_key,
            default_search_mode=search_mode,
            default_search_max_results=max_results,
            default_chat_model=chat_model,
            request_timeout=timeout,
        )


__all__ = ["PerplexityConfig"]
