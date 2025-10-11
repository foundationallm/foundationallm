"""Unit tests for the Playwright MCP crawler."""

from __future__ import annotations

import sys
import types
from collections.abc import Callable

import anyio
import pytest

from mcp_playwright_crawler.app import create_app
from mcp_playwright_crawler.crawler import CrawlConfig, crawl_site_async, crawl_site_sync
from markdownify import markdownify as md


class _AsyncPageStub:
    def __init__(self, state: dict[str, object]):
        self._state = state
        self._current_url: str | None = None

    async def goto(self, url: str, wait_until: str, timeout: int) -> None:
        self._record_visit(url)
        data = self._lookup(url)
        error = data.get("error")
        if error:
            raise RuntimeError(error)
        self._current_url = url

    async def content(self) -> str:
        return self._lookup(self._current_url).get("html", "")

    async def title(self) -> str:
        return self._lookup(self._current_url).get("title", "")

    async def eval_on_selector_all(self, selector: str, script: str) -> list[str]:
        return list(self._lookup(self._current_url).get("links", []))

    async def close(self) -> None:
        return None

    def _lookup(self, url: str | None) -> dict[str, object]:
        if not url:
            raise RuntimeError("No page has been visited yet")
        site_map: dict[str, dict[str, object]] = self._state["site_map"]  # type: ignore[index]
        if url not in site_map:
            raise RuntimeError(f"Unexpected URL {url}")
        return site_map[url]

    def _record_visit(self, url: str) -> None:
        visits = self._state.setdefault("visited", [])  # type: ignore[assignment]
        visits.append(url)


class _AsyncContextStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    async def new_page(self) -> _AsyncPageStub:
        return _AsyncPageStub(self._state)

    async def close(self) -> None:
        return None


class _AsyncBrowserStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    async def new_context(self, user_agent: str | None = None) -> _AsyncContextStub:
        self._state["user_agent"] = user_agent
        return _AsyncContextStub(self._state)

    async def close(self) -> None:
        return None


class _AsyncChromiumStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    async def launch(self, headless: bool = True) -> _AsyncBrowserStub:
        self._state["headless"] = headless
        return _AsyncBrowserStub(self._state)


class _AsyncPlaywright:
    def __init__(self, state: dict[str, object]):
        self.chromium = _AsyncChromiumStub(state)

    async def stop(self) -> None:
        return None


class _AsyncPlaywrightManager:
    def __init__(self, state: dict[str, object]):
        self._state = state
        self._playwright = _AsyncPlaywright(state)

    async def __aenter__(self) -> _AsyncPlaywright:
        return self._playwright

    async def __aexit__(self, exc_type, exc, tb) -> bool:  # noqa: D401, ANN001
        return False

    async def start(self) -> _AsyncPlaywright:
        return self._playwright


class _SyncPageStub:
    def __init__(self, state: dict[str, object]):
        self._state = state
        self._current_url: str | None = None

    def goto(self, url: str, wait_until: str, timeout: int) -> None:
        self._record_visit(url)
        data = self._lookup(url)
        error = data.get("error")
        if error:
            raise RuntimeError(error)
        self._current_url = url

    def content(self) -> str:
        return self._lookup(self._current_url).get("html", "")

    def title(self) -> str:
        return self._lookup(self._current_url).get("title", "")

    def eval_on_selector_all(self, selector: str, script: str) -> list[str]:
        return list(self._lookup(self._current_url).get("links", []))

    def close(self) -> None:
        return None

    def _lookup(self, url: str | None) -> dict[str, object]:
        if not url:
            raise RuntimeError("No page has been visited yet")
        site_map: dict[str, dict[str, object]] = self._state["site_map"]  # type: ignore[index]
        if url not in site_map:
            raise RuntimeError(f"Unexpected URL {url}")
        return site_map[url]

    def _record_visit(self, url: str) -> None:
        visits = self._state.setdefault("visited", [])  # type: ignore[assignment]
        visits.append(url)


class _SyncContextStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    def new_page(self) -> _SyncPageStub:
        return _SyncPageStub(self._state)

    def close(self) -> None:
        return None


class _SyncBrowserStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    def new_context(self, user_agent: str | None = None) -> _SyncContextStub:
        self._state["user_agent"] = user_agent
        return _SyncContextStub(self._state)

    def close(self) -> None:
        return None


class _SyncChromiumStub:
    def __init__(self, state: dict[str, object]):
        self._state = state

    def launch(self, headless: bool = True) -> _SyncBrowserStub:
        self._state["headless"] = headless
        return _SyncBrowserStub(self._state)


class _SyncPlaywright:
    def __init__(self, state: dict[str, object]):
        self.chromium = _SyncChromiumStub(state)


class _SyncPlaywrightManager:
    def __init__(self, state: dict[str, object]):
        self._state = state

    def __enter__(self) -> _SyncPlaywright:
        return _SyncPlaywright(self._state)

    def __exit__(self, exc_type, exc, tb) -> bool:  # noqa: D401, ANN001
        return False


@pytest.fixture
def playwright_stubs(monkeypatch: pytest.MonkeyPatch) -> Callable[[dict[str, dict[str, object]]], dict[str, object]]:
    """Install Playwright stubs for the crawler module and return the shared state."""

    def installer(site_map: dict[str, dict[str, object]]) -> dict[str, object]:
        state: dict[str, object] = {"site_map": site_map}

        async_module = types.ModuleType("playwright.async_api")
        async_module.async_playwright = lambda: _AsyncPlaywrightManager(state)

        sync_module = types.ModuleType("playwright.sync_api")
        sync_module.sync_playwright = lambda: _SyncPlaywrightManager(state)

        playwright_pkg = types.ModuleType("playwright")
        playwright_pkg.async_api = async_module
        playwright_pkg.sync_api = sync_module

        monkeypatch.setitem(sys.modules, "playwright", playwright_pkg)
        monkeypatch.setitem(sys.modules, "playwright.async_api", async_module)
        monkeypatch.setitem(sys.modules, "playwright.sync_api", sync_module)

        return state

    return installer


def test_crawl_site_async_respects_same_origin(playwright_stubs: Callable[[dict[str, dict[str, object]]], dict[str, object]]) -> None:
    site_map = {
        "https://example.com/": {
            "html": "<a href='https://example.com/about'>About</a> <a href='https://other.com/'>Other</a>",
            "title": "Home",
            "links": ["https://example.com/about", "https://other.com/"]
        },
        "https://example.com/about": {
            "html": "<p>About us</p>",
            "title": "About",
            "links": []
        },
        "https://other.com/": {
            "html": "<p>External</p>",
            "title": "Other",
            "links": []
        },
    }
    state = playwright_stubs(site_map)

    result = anyio.run(
        crawl_site_async,
        "https://example.com/",
        CrawlConfig(max_depth=2, max_pages=5, same_origin=True),
    )

    assert [page.url for page in result.pages] == ["https://example.com/", "https://example.com/about"]
    assert [
        page.content for page in result.pages
    ] == [
        md(
            site_map["https://example.com/"]["html"],
            heading_style="ATX",
            bullets="-",
            strip=["script", "style"],
        ),
        md(
            site_map["https://example.com/about"]["html"],
            heading_style="ATX",
            bullets="-",
            strip=["script", "style"],
        ),
    ]
    assert result.errors == []
    assert state["visited"] == ["https://example.com/", "https://example.com/about"]


def test_crawl_site_sync_collects_errors(playwright_stubs: Callable[[dict[str, dict[str, object]]], dict[str, object]]) -> None:
    site_map = {
        "https://example.com/": {
            "html": "<a href='https://example.com/broken'>Broken</a>",
            "title": "Home",
            "links": ["https://example.com/broken"],
        },
        "https://example.com/broken": {
            "html": "",
            "title": "Broken",
            "links": [],
            "error": "navigation failed",
        },
    }
    state = playwright_stubs(site_map)

    result = crawl_site_sync(
        "https://example.com/",
        CrawlConfig(max_depth=1, max_pages=3, same_origin=True),
    )

    assert [page.url for page in result.pages] == ["https://example.com/"]
    assert result.pages[0].content == md(
        site_map["https://example.com/"]["html"],
        heading_style="ATX",
        bullets="-",
        strip=["script", "style"],
    )
    assert result.errors and "navigation failed" in result.errors[0]
    assert state["visited"] == ["https://example.com/", "https://example.com/broken"]


def test_create_app_validates_log_level() -> None:
    with pytest.raises(ValueError):
        create_app(log_level="invalid")


def test_create_app_exposes_tools() -> None:
    app = create_app(stateless_http=True)
    tool_list = anyio.run(app.list_tools)
    tools = {tool.name for tool in tool_list}
    assert "crawl" in tools
    assert app.settings.stateless_http is True
