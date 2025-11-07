"""
FoundationaLLM Management and Core API clients used by the prompt authoring CLI.

This module wraps the HTTP interactions required to
  * inspect agents and prompt resources via the Management API
  * persist prompt updates after optimization iterations
  * interact with the Core API completions endpoint for LLM-driven authoring
"""

from __future__ import annotations

import json
import os
import pathlib
from dataclasses import dataclass
from datetime import datetime
from typing import Any, Dict, Optional

import requests


class FoundationaLLMClientError(RuntimeError):
    """Base error for FoundationaLLM client failures."""


class FoundationaLLMAuthenticationError(FoundationaLLMClientError):
    """Raised when authentication configuration is missing or invalid."""


def _ensure_trailing_slash(url: str) -> str:
    return url if url.endswith("/") else f"{url}/"


def _headers_with_bearer(token: str) -> Dict[str, str]:
    return {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
    }


@dataclass
class AgentGetResult:
    """Represents the management API agent payload."""

    name: str
    object_id: str
    resource: Dict[str, Any]
    raw_response: Dict[str, Any]


class FoundationaLLMManagementClient:
    """
    Lightweight Client for FoundationaLLM Management API interactions.
    """

    def __init__(
        self,
        endpoint: Optional[str] = None,
        bearer_token: Optional[str] = None,
        timeout_seconds: int = 30,
    ) -> None:
        self.endpoint = _ensure_trailing_slash(
            endpoint or os.getenv("FLLM_MGMT_ENDPOINT", "").strip()
        )
        self.bearer_token = (bearer_token or os.getenv("FLLM_MGMT_BEARER_TOKEN", "")).strip()
        self.timeout_seconds = timeout_seconds

        if not self.endpoint:
            raise FoundationaLLMAuthenticationError(
                "Missing FoundationaLLM management endpoint. "
                "Set FLLM_MGMT_ENDPOINT or provide endpoint explicitly."
            )
        if not self.bearer_token:
            raise FoundationaLLMAuthenticationError(
                "Missing FoundationaLLM management bearer token. "
                "Set FLLM_MGMT_BEARER_TOKEN or provide bearer_token explicitly."
            )

    def _request(
        self,
        method: str,
        path: str,
        payload: Optional[Dict[str, Any]] = None,
    ) -> Any:
        url = f"{self.endpoint.rstrip('/')}/{path.lstrip('/')}"
        response = requests.request(
            method=method.upper(),
            url=url,
            headers=_headers_with_bearer(self.bearer_token),
            json=payload,
            timeout=self.timeout_seconds,
        )

        if response.status_code == 401:
            raise FoundationaLLMAuthenticationError(
                "Management API returned 401 Unauthorized. "
                "Confirm the bearer token has Data.Manage scope."
            )

        try:
            response.raise_for_status()
        except requests.HTTPError as exc:
            raise FoundationaLLMClientError(
                f"Management API request failed ({method.upper()} {url}): {response.status_code} {response.text}"
            ) from exc

        if not response.content:
            return None

        try:
            return response.json()
        except json.JSONDecodeError as exc:
            raise FoundationaLLMClientError(
                f"Unable to decode JSON response from {url}: {exc}"
            ) from exc

    def get_agent(self, agent_name: str) -> AgentGetResult:
        """
        Retrieve a single agent resource by name.

        The Management API returns an array of ResourceProviderGetResult payloads.
        """
        payload = self._request(
            "GET",
            f"providers/FoundationaLLM.Agent/agents/{agent_name}",
        )

        if isinstance(payload, list) and payload:
            result = payload[0]
        elif isinstance(payload, dict):
            result = payload
        else:
            raise FoundationaLLMClientError(
                f"Unexpected agent response payload type for '{agent_name}': {type(payload)}"
            )

        resource = result.get("resource")
        if resource is None:
            raise FoundationaLLMClientError(
                f"Agent '{agent_name}' response did not include 'resource'."
            )

        return AgentGetResult(
            name=resource.get("name") or agent_name,
            object_id=resource.get("object_id") or result.get("object_id") or "",
            resource=resource,
            raw_response=result,
        )

    def get_prompt(self, prompt_name: str) -> Dict[str, Any]:
        """Retrieve a prompt resource by name."""
        payload = self._request(
            "GET",
            f"providers/FoundationaLLM.Prompt/prompts/{prompt_name}",
        )

        if isinstance(payload, list) and payload:
            prompt_resource = payload[0].get("resource") or payload[0]
        elif isinstance(payload, dict):
            prompt_resource = payload.get("resource") or payload
        else:
            raise FoundationaLLMClientError(
                f"Unexpected prompt response payload for '{prompt_name}'."
            )

        return prompt_resource

    def upsert_prompt(self, prompt_payload: Dict[str, Any]) -> Dict[str, Any]:
        """
        Create or update a prompt via POST.
        """
        prompt_name = prompt_payload.get("name")
        if not prompt_name:
            raise ValueError("Prompt payload must include a 'name'.")

        return self._request(
            "POST",
            f"providers/FoundationaLLM.Prompt/prompts/{prompt_name}",
            payload=prompt_payload,
        )

    def backup_prompt(
        self,
        prompt_payload: Dict[str, Any],
        agent_name: str,
        output_dir: pathlib.Path,
    ) -> pathlib.Path:
        """
        Persist a JSON backup of the supplied prompt payload.
        """
        timestamp = datetime.utcnow().strftime("%Y%m%dT%H%M%SZ")
        prompt_name = prompt_payload.get("name", "unknown-prompt")
        backup_dir = output_dir / agent_name / prompt_name
        backup_dir.mkdir(parents=True, exist_ok=True)

        backup_path = backup_dir / f"{prompt_name}-{timestamp}.json"
        with backup_path.open("w", encoding="utf-8") as fh:
            json.dump(prompt_payload, fh, ensure_ascii=False, indent=2)

        return backup_path


class FoundationaLLMCompletionClient:
    """
    Client for interacting with the FoundationaLLM Core API completions surface.
    """

    def __init__(
        self,
        endpoint: Optional[str] = None,
        access_token: Optional[str] = None,
        default_agent: Optional[str] = None,
        timeout_seconds: int = 60,
    ) -> None:
        self.endpoint = _ensure_trailing_slash(
            endpoint or os.getenv("FLLM_ENDPOINT", "").strip()
        )
        self.access_token = (access_token or os.getenv("FLLM_ACCESS_TOKEN", "")).strip()
        self.default_agent = default_agent or os.getenv("FLLM_PROMPT_OPTIMIZER_AGENT", "").strip()
        self.timeout_seconds = timeout_seconds

        if not self.endpoint:
            raise FoundationaLLMAuthenticationError(
                "Missing FoundationaLLM Core API endpoint. Set FLLM_ENDPOINT or pass endpoint explicitly."
            )
        if not self.access_token:
            raise FoundationaLLMAuthenticationError(
                "Missing FoundationaLLM access token. Set FLLM_ACCESS_TOKEN or provide access_token explicitly."
            )

    def _core_headers(self) -> Dict[str, str]:
        return {
            "X-AGENT-ACCESS-TOKEN": self.access_token,
            "Content-Type": "application/json",
        }

    def _core_request(self, method: str, path: str, payload: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        url = f"{self.endpoint.rstrip('/')}/{path.lstrip('/')}"
        response = requests.request(
            method=method.upper(),
            url=url,
            headers=self._core_headers(),
            json=payload,
            timeout=self.timeout_seconds,
        )

        if response.status_code == 401:
            raise FoundationaLLMAuthenticationError(
                "Core API returned 401 Unauthorized. Confirm the access token is valid."
            )

        try:
            response.raise_for_status()
        except requests.HTTPError as exc:
            raise FoundationaLLMClientError(
                f"Core API request failed ({method.upper()} {url}): {response.status_code} {response.text}"
            ) from exc

        try:
            return response.json()
        except json.JSONDecodeError as exc:
            raise FoundationaLLMClientError(
                f"Unable to decode Core API JSON response from {url}: {exc}"
            ) from exc

    def create_session(self, session_name: Optional[str] = None) -> str:
        session_payload = {"name": session_name} if session_name else {}
        response = self._core_request("POST", "sessions", payload=session_payload)
        session_id = response.get("sessionId") or response.get("session_id")
        if not session_id:
            raise FoundationaLLMClientError("Session creation response did not include sessionId.")
        return session_id

    def complete(
        self,
        prompt: str,
        *,
        agent_name: Optional[str] = None,
        session_id: Optional[str] = None,
        attachments: Optional[list[str]] = None,
        conversation: Optional[list[Dict[str, Any]]] = None,
    ) -> Dict[str, Any]:
        """
        Submit a completion request using the provided prompt.
        """
        agent_to_use = agent_name or self.default_agent
        if not agent_to_use:
            raise ValueError(
                "No agent specified for completion request. Provide agent_name or configure FLLM_PROMPT_OPTIMIZER_AGENT."
            )

        session = session_id or self.create_session()
        payload: Dict[str, Any] = {
            "user_prompt": prompt,
            "agent_name": agent_to_use,
            "session_id": session,
        }
        if attachments:
            payload["attachments"] = attachments
        if conversation:
            payload["conversation"] = conversation

        return self._core_request("POST", "completions", payload=payload)
