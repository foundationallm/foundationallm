"""HTTP client that wraps the FoundationaLLM management API."""

from __future__ import annotations

import asyncio
import json
import logging
from pathlib import Path
from typing import Any, Iterable

import httpx

from .auth import TokenProvider
from .config import Settings

LOGGER = logging.getLogger(__name__)


class ManagementApiClient:
    """Thin async wrapper around the management API endpoints used by the MCP tools."""

    def __init__(self, settings: Settings, token_provider: TokenProvider):
        self._settings = settings
        self._token_provider = token_provider
        self._client: httpx.AsyncClient | None = None

    async def __aenter__(self) -> "ManagementApiClient":
        await self._ensure_client()
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.close()

    async def _ensure_client(self) -> None:
        if self._client is None:
            timeout = httpx.Timeout(self._settings.request_timeout_seconds, connect=30.0)
            self._client = httpx.AsyncClient(
                base_url=str(self._settings.management_endpoint),
                verify=self._settings.verify_ssl,
                timeout=timeout,
            )

    async def close(self) -> None:
        if self._client is not None:
            await self._client.aclose()
            self._client = None

    async def _authorized_headers(self) -> dict[str, str]:
        token = await self._token_provider.get_token(self._settings.management_scope)
        return {"Authorization": f"Bearer {token}"}

    def _build_path(self, route: str, *, include_instance: bool = True) -> str:
        route = route.lstrip("/")
        if include_instance:
            return f"/instances/{self._settings.instance_id}/{route}"
        return f"/{route}"

    async def _request(
        self,
        method: str,
        route: str,
        *,
        include_instance: bool = True,
        include_api_version: bool = True,
        params: dict[str, Any] | None = None,
        json_body: Any | None = None,
        data: Any | None = None,
        files: Any | None = None,
        headers: dict[str, str] | None = None,
    ) -> Any:
        await self._ensure_client()
        assert self._client is not None

        params = dict(params or {})
        if include_api_version:
            params.setdefault("api-version", self._settings.api_version)

        request_headers = await self._authorized_headers()
        if headers:
            request_headers.update(headers)

        url = self._build_path(route, include_instance=include_instance)

        if self._settings.log_http:
            LOGGER.info("Management API %s %s params=%s", method, url, params or {})

        response = await self._client.request(
            method=method.upper(),
            url=url,
            params=params,
            json=json_body,
            data=data,
            files=files,
            headers=request_headers,
        )
        response.raise_for_status()

        if response.headers.get("Content-Type", "").startswith("application/json"):
            payload = response.json()
        else:
            payload = response.text

        if self._settings.log_http:
            LOGGER.debug(
                "Management API response %s %s status=%s body=%s",
                method,
                url,
                response.status_code,
                _truncate_json(payload),
            )
        return payload

    # -------------------------------------------------------------------------
    # Agent basics
    # -------------------------------------------------------------------------

    async def create_basic_agent(self, template_parameters: dict[str, Any]) -> Any:
        return await self._request(
            "POST",
            "providers/FoundationaLLM.Agent/agentTemplates/BasicAgentTemplate/create-new",
            json_body={"template_parameters": template_parameters},
        )

    async def get_agent(self, agent_name: str) -> dict[str, Any]:
        payload = await self._request(
            "GET",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}",
        )
        return self._unwrap_single(payload)

    async def update_agent(self, agent_name: str, agent_payload: dict[str, Any]) -> Any:
        return await self._request(
            "POST",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}",
            json_body=agent_payload,
        )

    async def list_agents(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.Agent/agents",
        )
        return self._extract_resources(results)

    # -------------------------------------------------------------------------
    # Dependency catalog helpers
    # -------------------------------------------------------------------------

    async def list_prompts(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.Prompt/prompts",
        )
        return self._extract_resources(results)

    async def list_ai_models(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.AIModel/aiModels",
        )
        return self._extract_resources(results)

    async def list_workflows(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.Agent/workflows",
        )
        return self._extract_resources(results)

    async def list_vectorization_profiles(self) -> dict[str, list[dict[str, Any]]]:
        embedding = await self._request(
            "GET",
            "providers/FoundationaLLM.Vectorization/textEmbeddingProfiles",
        )
        partitioning = await self._request(
            "GET",
            "providers/FoundationaLLM.Vectorization/textPartitioningProfiles",
        )
        indexing = await self._request(
            "GET",
            "providers/FoundationaLLM.Vectorization/indexingProfiles",
        )
        return {
            "text_embedding_profiles": self._extract_resources(embedding),
            "text_partitioning_profiles": self._extract_resources(partitioning),
            "indexing_profiles": self._extract_resources(indexing),
        }

    async def list_data_sources(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.DataSource/dataSources",
        )
        return self._extract_resources(results)

    async def list_agent_tools(self) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            "providers/FoundationaLLM.Agent/tools",
        )
        return self._extract_resources(results)

    # -------------------------------------------------------------------------
    # Private store helpers
    # -------------------------------------------------------------------------

    async def list_agent_files(self, agent_name: str) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentFiles",
        )
        return self._extract_resources(results, preserve_wrapper=True)

    async def upload_agent_file(
        self,
        agent_name: str,
        file_path: Path,
        *,
        file_name: str | None = None,
    ) -> Any:
        file_name = file_name or file_path.name
        file_bytes = await asyncio.to_thread(file_path.read_bytes)
        return await self._request(
            "POST",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentFiles/{file_name}",
            data=None,
            files={"file": (file_name, file_bytes)},
        )

    async def delete_agent_file(self, agent_name: str, file_name: str) -> Any:
        return await self._request(
            "DELETE",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentFiles/{file_name}",
        )

    async def list_file_tool_associations(self, agent_name: str) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentFileToolAssociations",
        )
        return self._extract_resources(results)

    async def update_file_tool_associations(
        self,
        agent_name: str,
        associations: dict[str, dict[str, bool]],
    ) -> Any:
        return await self._request(
            "POST",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentFileToolAssociations/__all__",
            json_body={"agent_file_tool_associations": associations},
        )

    # -------------------------------------------------------------------------
    # File upload policy
    # -------------------------------------------------------------------------

    async def set_agent_file_upload_flag(self, agent_name: str, enabled: bool) -> Any:
        agent_resource = await self.get_agent(agent_name)
        resource = agent_resource.get("resource", agent_resource)
        resource["show_file_upload"] = enabled
        return await self.update_agent(agent_name, resource)

    # -------------------------------------------------------------------------
    # Agent access tokens
    # -------------------------------------------------------------------------

    async def list_agent_access_tokens(self, agent_name: str) -> list[dict[str, Any]]:
        results = await self._request(
            "GET",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentAccessTokens",
        )
        return self._extract_resources(results)

    async def create_agent_access_token(
        self,
        agent_name: str,
        token_payload: dict[str, Any],
    ) -> Any:
        token_name = token_payload["name"]
        return await self._request(
            "POST",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentAccessTokens/{token_name}",
            json_body=token_payload,
        )

    async def delete_agent_access_token(
        self,
        agent_name: str,
        token_name: str,
    ) -> Any:
        return await self._request(
            "DELETE",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}/agentAccessTokens/{token_name}",
        )

    # -------------------------------------------------------------------------
    # Utility helpers
    # -------------------------------------------------------------------------

    @staticmethod
    def _unwrap_single(payload: Any) -> dict[str, Any]:
        if isinstance(payload, list):
            return payload[0] if payload else {}
        return payload or {}

    @staticmethod
    def _extract_resources(
        payload: Any,
        *,
        preserve_wrapper: bool = False,
    ) -> list[dict[str, Any]]:
        if not isinstance(payload, Iterable):
            return []
        resources: list[dict[str, Any]] = []
        for item in payload:
            if isinstance(item, dict) and "resource" in item and not preserve_wrapper:
                resources.append(item["resource"])
            else:
                resources.append(item)
        return resources


def _truncate_json(payload: Any, limit: int = 512) -> str:
    try:
        serialized = json.dumps(payload)
    except Exception:
        serialized = str(payload)
    if len(serialized) <= limit:
        return serialized
    return f"{serialized[:limit]}…"

