from __future__ import annotations

from collections.abc import Sequence
from pathlib import Path
import sys

PACKAGE_SRC = Path(__file__).resolve().parents[3] / "src" / "python" / "Experimental" / "src"
if str(PACKAGE_SRC) not in sys.path:
    sys.path.insert(0, str(PACKAGE_SRC))

import anyio
import pytest

from perplexity_mcp import PerplexityConfig, create_perplexity_mcp_server


class DummySearchResponse:
    def __init__(self, payload: dict[str, object]) -> None:
        self._payload = payload

    def model_dump(self) -> dict[str, object]:
        return self._payload


class DummyStreamChunk:
    def __init__(self, index: int) -> None:
        self._payload = {"chunk": index}

    def model_dump(self) -> dict[str, int]:
        return self._payload


class DummyChatResponse:
    def __init__(self, payload: dict[str, object]) -> None:
        self._payload = payload

    def model_dump(self) -> dict[str, object]:
        return self._payload


class DummySearchResource:
    def __init__(self) -> None:
        self.calls: list[dict[str, object]] = []

    def create(self, **kwargs: object) -> DummySearchResponse:
        self.calls.append(kwargs)
        return DummySearchResponse({"results": ["example"]})


class DummyCompletionsResource:
    def __init__(self) -> None:
        self.calls: list[dict[str, object]] = []

    def create(self, **kwargs: object):  # noqa: ANN001 - signature matches SDK expectations
        self.calls.append(kwargs)
        if kwargs.get("stream"):
            return [DummyStreamChunk(0), DummyStreamChunk(1)]
        return DummyChatResponse({"choices": [{"message": {"content": "hi"}}]})


class DummyChatResource:
    def __init__(self) -> None:
        self.completions = DummyCompletionsResource()


class DummyPerplexityClient:
    def __init__(self) -> None:
        self.search = DummySearchResource()
        self.chat = DummyChatResource()


@pytest.fixture()
def dummy_client() -> DummyPerplexityClient:
    return DummyPerplexityClient()


def test_config_from_env(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("PERPLEXITY_API_KEY", "test-key")
    monkeypatch.setenv("PERPLEXITY_DEFAULT_SEARCH_MODE", "academic")
    monkeypatch.setenv("PERPLEXITY_DEFAULT_MAX_RESULTS", "7")
    monkeypatch.setenv("PERPLEXITY_DEFAULT_CHAT_MODEL", "pplx-70b")
    monkeypatch.setenv("PERPLEXITY_REQUEST_TIMEOUT", "15.5")

    config = PerplexityConfig.from_env()
    assert config.api_key == "test-key"
    assert config.default_search_mode == "academic"
    assert config.default_search_max_results == 7
    assert config.default_chat_model == "pplx-70b"
    assert config.request_timeout == pytest.approx(15.5)


def test_server_registers_tools(dummy_client: DummyPerplexityClient) -> None:
    config = PerplexityConfig(
        api_key="abc",
        default_search_mode="web",
        default_search_max_results=5,
        default_chat_model="sonar",
    )
    server = create_perplexity_mcp_server(config, client_factory=lambda _: dummy_client)
    tool_names = {tool.name for tool in anyio.run(server.fastmcp.list_tools)}
    assert {"Search", "ChatCompletions"} <= tool_names


def test_search_tool_uses_defaults(dummy_client: DummyPerplexityClient) -> None:
    config = PerplexityConfig(
        api_key="abc",
        default_search_mode="web",
        default_search_max_results=3,
        default_chat_model="sonar",
    )
    server = create_perplexity_mcp_server(config, client_factory=lambda _: dummy_client)
    async def invoke() -> dict[str, object]:
        return await server.search_callable("python", search_domain_filter=("example.com",))

    result = anyio.run(invoke)

    assert result["results"] == ["example"]
    call = dummy_client.search.calls[0]
    assert call["query"] == "python"
    assert call["search_mode"] == "web"
    assert call["max_results"] == 3
    assert call["search_domain_filter"] == ["example.com"]


def test_chat_completions_respects_parameters(dummy_client: DummyPerplexityClient) -> None:
    config = PerplexityConfig(
        api_key="abc",
        default_search_mode="web",
        default_search_max_results=2,
        default_chat_model="sonar-small",
    )
    server = create_perplexity_mcp_server(config, client_factory=lambda _: dummy_client)

    messages: Sequence[dict[str, str]] = [{"role": "user", "content": "Hello"}]
    async def invoke() -> list[dict[str, int]]:
        return await server.chat_callable(
            messages,
            temperature=0.2,
            search_mode="academic",
            num_search_results=4,
            stream=True,
        )

    streamed = anyio.run(invoke)

    call = dummy_client.chat.completions.calls[0]
    assert call["model"] == "sonar-small"
    assert call["temperature"] == 0.2
    assert call["search_mode"] == "academic"
    assert call["num_search_results"] == 4
    assert call["stream"] is True

    assert streamed == [{"chunk": 0}, {"chunk": 1}]


def test_chat_completions_applies_defaults(dummy_client: DummyPerplexityClient) -> None:
    config = PerplexityConfig(
        api_key="abc",
        default_search_mode="web",
        default_search_max_results=6,
        default_chat_model="sonar-large",
    )
    server = create_perplexity_mcp_server(config, client_factory=lambda _: dummy_client)

    async def invoke() -> dict[str, object]:
        return await server.chat_callable([
            {"role": "user", "content": "Summarise MCP."},
        ])

    response = anyio.run(invoke)

    call = dummy_client.chat.completions.calls[-1]
    assert call["model"] == "sonar-large"
    assert call["search_mode"] == "web"
    assert call["num_search_results"] == 6
    assert "stream" not in call
    assert response == {"choices": [{"message": {"content": "hi"}}]}
