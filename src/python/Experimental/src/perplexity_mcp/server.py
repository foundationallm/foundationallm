"""FastMCP server exposing Perplexity tools."""

from __future__ import annotations

from dataclasses import dataclass
from typing import Any, Callable, Iterable, Literal, Optional, Sequence

import anyio
from mcp.server.fastmcp import FastMCP
from typing_extensions import Annotated, TypedDict

from perplexity import Perplexity

from .config import PerplexityConfig

DEFAULT_DEPENDENCIES = ("perplexityai>=0.16.0", "mcp>=1.17.0")


class ChatMessage(TypedDict):
    """Minimal structure for Perplexity chat messages."""

    role: Literal["system", "user", "assistant", "tool"]
    content: str


def _merge_dependencies(user_dependencies: Optional[Sequence[str]]) -> list[str]:
    seen: set[str] = set()
    ordered: list[str] = []
    for value in list(user_dependencies or ()) + list(DEFAULT_DEPENDENCIES):  # type: ignore[list-item]
        if value not in seen:
            ordered.append(value)
            seen.add(value)
    return ordered


@dataclass(slots=True)
class PerplexityMCPServer:
    """Container bundling the FastMCP server and Perplexity client."""

    config: PerplexityConfig
    fastmcp: FastMCP
    client: Perplexity
    search_callable: Callable[..., Any]
    chat_callable: Callable[..., Any]

    def run(self, transport: Literal["stdio", "sse", "http", "streamable-http"] = "stdio", *, mount_path: Optional[str] = None) -> None:
        """Run the underlying FastMCP server using the requested transport."""

        if transport == "http":
            original_stateless = self.fastmcp.settings.stateless_http
            original_json = self.fastmcp.settings.json_response
            try:
                self.fastmcp.settings.stateless_http = True
                self.fastmcp.settings.json_response = True
                self.fastmcp.run(transport="streamable-http")
            finally:  # pragma: no branch - restoration is always required
                self.fastmcp.settings.stateless_http = original_stateless
                self.fastmcp.settings.json_response = original_json
            return

        if transport not in {"stdio", "sse", "streamable-http"}:
            raise ValueError(f"Unsupported transport '{transport}'")
        self.fastmcp.run(transport=transport, mount_path=mount_path)

    # internal helpers -------------------------------------------------
    def _perform_search(
        self,
        query: str,
        *,
        max_results: Optional[int],
        search_mode: Optional[str],
        display_server_time: Optional[bool],
        max_tokens: Optional[int],
        max_tokens_per_page: Optional[int],
        search_domain_filter: Optional[Sequence[str]],
    ) -> dict[str, Any]:
        request: dict[str, Any] = {"query": query}
        request["max_results"] = max_results if max_results is not None else self.config.default_search_max_results
        request["search_mode"] = search_mode or self.config.default_search_mode
        if display_server_time is not None:
            request["display_server_time"] = display_server_time
        if max_tokens is not None:
            request["max_tokens"] = max_tokens
        if max_tokens_per_page is not None:
            request["max_tokens_per_page"] = max_tokens_per_page
        if search_domain_filter:
            request["search_domain_filter"] = list(search_domain_filter)

        response = self.client.search.create(**request)
        return response.model_dump()

    def _perform_chat_completion(
        self,
        messages: Iterable[ChatMessage],
        *,
        model: Optional[str],
        temperature: Optional[float],
        top_p: Optional[float],
        max_tokens: Optional[int],
        num_search_results: Optional[int],
        search_mode: Optional[str],
        search_domain_filter: Optional[Sequence[str]],
        return_related_questions: Optional[bool],
        return_images: Optional[bool],
        stream: bool,
    ) -> Any:
        payload: dict[str, Any] = {
            "messages": list(messages),
            "model": model or self.config.default_chat_model,
            "search_mode": search_mode or self.config.default_search_mode,
        }
        if temperature is not None:
            payload["temperature"] = temperature
        if top_p is not None:
            payload["top_p"] = top_p
        if max_tokens is not None:
            payload["max_tokens"] = max_tokens
        if num_search_results is not None:
            payload["num_search_results"] = num_search_results
        elif self.config.default_search_max_results:
            payload["num_search_results"] = self.config.default_search_max_results
        if search_domain_filter:
            payload["search_domain_filter"] = list(search_domain_filter)
        if return_related_questions is not None:
            payload["return_related_questions"] = return_related_questions
        if return_images is not None:
            payload["return_images"] = return_images
        if stream:
            payload["stream"] = True

        response = self.client.chat.completions.create(**payload)
        if stream:
            return [chunk.model_dump() for chunk in response]
        if hasattr(response, "model_dump"):
            return response.model_dump()
        return response


def create_perplexity_mcp_server(
    config: PerplexityConfig,
    *,
    client_factory: Callable[[PerplexityConfig], Perplexity] | None = None,
    dependencies: Optional[Sequence[str]] = None,
    name: Optional[str] = None,
    instructions: Optional[str] = None,
    **fastmcp_kwargs: Any,
) -> PerplexityMCPServer:
    """Create a fully configured Perplexity MCP server."""

    client_factory = client_factory or _default_client_factory
    client = client_factory(config)

    deps = _merge_dependencies(dependencies)
    fastmcp = FastMCP(
        name=name or "Perplexity MCP Server",
        instructions=(
            instructions
            or "Access the Perplexity search and chat APIs. The `Search` tool performs one-shot web searches;"
            " `ChatCompletions` starts an agentic conversation that can automatically trigger web searches."
        ),
        dependencies=deps,
        **fastmcp_kwargs,
    )

    server = PerplexityMCPServer(
        config=config,
        fastmcp=fastmcp,
        client=client,
        search_callable=lambda *args, **kwargs: None,
        chat_callable=lambda *args, **kwargs: None,
    )

    @fastmcp.tool(
        name="Search",
        title="Perplexity Web Search",
        description="Search the public web using Perplexity's search API.",
    )
    async def search_tool(
        query: Annotated[str, "Search query text to forward to Perplexity."],
        max_results: Annotated[Optional[int], "Override the configured maximum number of results."] = None,
        search_mode: Annotated[Optional[Literal["web", "academic", "sec"]], "Select the Perplexity search vertical."] = None,
        display_server_time: Annotated[Optional[bool], "Include server processing time in the response metadata."] = None,
        max_tokens: Annotated[Optional[int], "Limit the total number of tokens returned by Perplexity."] = None,
        max_tokens_per_page: Annotated[Optional[int], "Maximum tokens to extract per crawled page."] = None,
        search_domain_filter: Annotated[Optional[Sequence[str]], "Restrict results to specific domains."] = None,
    ) -> dict[str, Any]:
        return await anyio.to_thread.run_sync(
            lambda: server._perform_search(
                query,
                max_results=max_results,
                search_mode=search_mode,
                display_server_time=display_server_time,
                max_tokens=max_tokens,
                max_tokens_per_page=max_tokens_per_page,
                search_domain_filter=search_domain_filter,
            )
        )

    server.search_callable = search_tool

    @fastmcp.tool(
        name="ChatCompletions",
        title="Perplexity Chat Completions",
        description="Hold a conversational search session using Perplexity's agentic chat API.",
    )
    async def chat_completions_tool(
        messages: Annotated[Sequence[ChatMessage], "Conversation history passed directly to Perplexity."],
        model: Annotated[Optional[str], "Override the default chat model identifier." ] = None,
        temperature: Annotated[Optional[float], "Sampling temperature for creative responses."] = None,
        top_p: Annotated[Optional[float], "Nucleus sampling parameter." ] = None,
        max_tokens: Annotated[Optional[int], "Cap the number of tokens generated per response."] = None,
        num_search_results: Annotated[Optional[int], "Limit documents returned when web search is triggered."] = None,
        search_mode: Annotated[Optional[Literal["web", "academic", "sec"]], "Search vertical used for agentic lookups."] = None,
        search_domain_filter: Annotated[Optional[Sequence[str]], "Restrict agentic lookups to specific domains."] = None,
        return_related_questions: Annotated[Optional[bool], "Request related question suggestions."] = None,
        return_images: Annotated[Optional[bool], "Ask Perplexity to return supporting images when available."] = None,
        stream: Annotated[bool, "When true, stream partial responses back to the caller."] = False,
    ) -> Any:
        payload = list(messages)
        return await anyio.to_thread.run_sync(
            lambda: server._perform_chat_completion(
                payload,
                model=model,
                temperature=temperature,
                top_p=top_p,
                max_tokens=max_tokens,
                num_search_results=num_search_results,
                search_mode=search_mode,
                search_domain_filter=search_domain_filter,
                return_related_questions=return_related_questions,
                return_images=return_images,
                stream=stream,
            )
        )

    server.chat_callable = chat_completions_tool
    return server


def _default_client_factory(config: PerplexityConfig) -> Perplexity:
    kwargs: dict[str, Any] = {"api_key": config.api_key}
    if config.request_timeout is not None:
        kwargs["timeout"] = config.request_timeout
    return Perplexity(**kwargs)


__all__ = ["PerplexityMCPServer", "create_perplexity_mcp_server", "ChatMessage"]
