"""Website crawling utilities built on top of Playwright."""

from __future__ import annotations

from collections import deque
from dataclasses import dataclass, asdict
from typing import Deque, Iterable
from urllib.parse import urlparse, urldefrag


@dataclass(slots=True)
class PageResult:
    """Information captured for a crawled page."""

    url: str
    title: str | None
    content: str | None
    links: list[str]

    def to_dict(self) -> dict[str, object]:
        """Return a JSON-serialisable representation."""

        return asdict(self)


@dataclass(slots=True)
class CrawlResult:
    """Aggregate crawl outcome."""

    pages: list[PageResult]
    errors: list[str]

    def to_dict(self) -> dict[str, object]:
        """Return a JSON-serialisable representation."""

        return {
            "pages": [page.to_dict() for page in self.pages],
            "errors": list(self.errors),
        }


@dataclass(slots=True)
class CrawlConfig:
    """Configuration knobs for the crawler."""

    max_depth: int = 1
    max_pages: int = 10
    same_origin: bool = True
    timeout_ms: int = 30_000
    wait_until: str = "load"
    headless: bool = True
    user_agent: str | None = None

    def validate(self) -> None:
        """Validate configuration values."""

        if self.max_depth < 0:
            raise ValueError("max_depth must be >= 0")
        if self.max_pages <= 0:
            raise ValueError("max_pages must be > 0")
        if self.timeout_ms <= 0:
            raise ValueError("timeout_ms must be > 0")


def _normalize_url(raw_url: str | None) -> str | None:
    """Return a canonicalised version of the provided URL."""

    if not raw_url:
        return None
    url, _ = urldefrag(raw_url)
    parsed = urlparse(url)
    if parsed.scheme not in {"http", "https"}:
        return None
    if not parsed.netloc:
        return None
    if not parsed.path:
        parsed = parsed._replace(path="/")
    return parsed.geturl()


def _should_enqueue(
    candidate_url: str,
    root_url: str,
    config: CrawlConfig,
    visited: set[str],
    queued: set[str],
) -> bool:
    """Determine whether the candidate URL should be crawled."""

    if candidate_url in visited or candidate_url in queued:
        return False
    parsed_candidate = urlparse(candidate_url)
    if parsed_candidate.scheme not in {"http", "https"}:
        return False
    if config.same_origin:
        parsed_root = urlparse(root_url)
        if parsed_candidate.netloc != parsed_root.netloc:
            return False
    return True


async def crawl_site_async(url: str, config: CrawlConfig | None = None) -> CrawlResult:
    """Crawl a site using Playwright's async API."""

    from playwright.async_api import async_playwright

    cfg = config or CrawlConfig()
    cfg.validate()
    start_url = _normalize_url(url)
    if not start_url:
        raise ValueError("A valid HTTP or HTTPS URL is required")

    visited: set[str] = set()
    queued: set[str] = {start_url}
    queue: Deque[tuple[str, int]] = deque([(start_url, 0)])
    pages: list[PageResult] = []
    errors: list[str] = []

    playwright = None
    browser = None
    context = None
    
    try:
        playwright = await async_playwright().start()
        browser = await playwright.chromium.launch(headless=cfg.headless)
        context = await browser.new_context(user_agent=cfg.user_agent)
        
        while queue and len(pages) < cfg.max_pages:
            current_url, depth = queue.popleft()
            if current_url in visited or depth > cfg.max_depth:
                continue
            visited.add(current_url)

            page = await context.new_page()
            try:
                await page.goto(
                    current_url,
                    wait_until=cfg.wait_until,
                    timeout=cfg.timeout_ms,
                )
                html = await page.content()
                title = await page.title()
                hrefs: Iterable[str] = await page.eval_on_selector_all(
                    "a[href]", "elements => elements.map(el => el.href)"
                )
                normalized_links: list[str] = []
                for href in hrefs:
                    normalized = _normalize_url(href)
                    if not normalized:
                        continue
                    normalized_links.append(normalized)
                    if depth < cfg.max_depth and _should_enqueue(
                        normalized, start_url, cfg, visited, queued
                    ):
                        queue.append((normalized, depth + 1))
                        queued.add(normalized)
                pages.append(
                    PageResult(
                        url=current_url,
                        title=title or None,
                        content=html,
                        links=normalized_links,
                    )
                )
            except Exception as exc:  # noqa: BLE001 - capture Playwright failures
                errors.append(f"{current_url}: {exc}")
            finally:
                await page.close()
    finally:
        # Clean up resources in reverse order
        if context:
            await context.close()
        if browser:
            await browser.close()
        if playwright:
            await playwright.stop()

    return CrawlResult(pages=pages, errors=errors)


def crawl_site_sync(url: str, config: CrawlConfig | None = None) -> CrawlResult:
    """Crawl a site using Playwright's synchronous API."""

    from playwright.sync_api import sync_playwright

    cfg = config or CrawlConfig()
    cfg.validate()
    start_url = _normalize_url(url)
    if not start_url:
        raise ValueError("A valid HTTP or HTTPS URL is required")

    visited: set[str] = set()
    queued: set[str] = {start_url}
    queue: Deque[tuple[str, int]] = deque([(start_url, 0)])
    pages: list[PageResult] = []
    errors: list[str] = []

    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(headless=cfg.headless)
        context = browser.new_context(user_agent=cfg.user_agent)
        try:
            while queue and len(pages) < cfg.max_pages:
                current_url, depth = queue.popleft()
                if current_url in visited or depth > cfg.max_depth:
                    continue
                visited.add(current_url)

                page = context.new_page()
                try:
                    page.goto(
                        current_url,
                        wait_until=cfg.wait_until,
                        timeout=cfg.timeout_ms,
                    )
                    html = page.content()
                    title = page.title()
                    hrefs: Iterable[str] = page.eval_on_selector_all(
                        "a[href]", "elements => elements.map(el => el.href)"
                    )
                    normalized_links: list[str] = []
                    for href in hrefs:
                        normalized = _normalize_url(href)
                        if not normalized:
                            continue
                        normalized_links.append(normalized)
                        if depth < cfg.max_depth and _should_enqueue(
                            normalized, start_url, cfg, visited, queued
                        ):
                            queue.append((normalized, depth + 1))
                            queued.add(normalized)
                    pages.append(
                        PageResult(
                            url=current_url,
                            title=title or None,
                            content=html,
                            links=normalized_links,
                        )
                    )
                except Exception as exc:  # noqa: BLE001 - capture Playwright failures
                    errors.append(f"{current_url}: {exc}")
                finally:
                    page.close()
        finally:
            context.close()
            browser.close()

    return CrawlResult(pages=pages, errors=errors)


__all__ = [
    "CrawlConfig",
    "CrawlResult",
    "PageResult",
    "crawl_site_async",
    "crawl_site_sync",
]
