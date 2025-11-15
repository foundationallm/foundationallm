"""Azure Identity helpers for acquiring management API tokens."""

from __future__ import annotations

import asyncio
from functools import partial
from typing import Callable

from azure.identity import (
    AzureCliCredential,
    DefaultAzureCredential,
    ManagedIdentityCredential,
)
from azure.identity import TokenCredential

from .config import Settings


class TokenProvider:
    """Wraps Azure Identity credentials and exposes an async token helper."""

    def __init__(self, settings: Settings):
        self._settings = settings
        self._credential: TokenCredential = self._build_credential()

    def _build_credential(self) -> TokenCredential:
        tenant_kwargs = {}
        if self._settings.tenant_id:
            tenant_kwargs["tenant_id"] = self._settings.tenant_id

        if self._settings.auth_mode == "cli":
            return AzureCliCredential(**tenant_kwargs)
        if self._settings.auth_mode == "managed_identity":
            return ManagedIdentityCredential(**tenant_kwargs)
        return DefaultAzureCredential(
            exclude_interactive_browser_credential=True,
            **tenant_kwargs,
        )

    async def get_token(self, scope: str) -> str:
        """Retrieve a fresh access token for the requested scope."""

        loop = asyncio.get_running_loop()
        bound_get_token: Callable[[], str] = partial(
            self._credential.get_token, scope
        )  # type: ignore[arg-type]
        token = await loop.run_in_executor(None, bound_get_token)
        return token.token

