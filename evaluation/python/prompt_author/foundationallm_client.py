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
from typing import Any, Dict, List, Optional

try:
    import requests
except ImportError:  # pragma: no cover - optional for unit tests
    requests = None

try:
    from openai import AzureOpenAI
except ImportError:  # pragma: no cover - optional for unit tests
    AzureOpenAI = None


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
        if requests is None:
            raise FoundationaLLMClientError(
                "The 'requests' package is required for management API access. Install 'requests' to continue."
            )
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


class AzureOpenAILLMClient:
    """
    Lightweight wrapper around Azure OpenAI chat completions for prompt refinement.
    """

    def __init__(
        self,
        *,
        endpoint: Optional[str] = None,
        api_key: Optional[str] = None,
        api_version: Optional[str] = None,
        deployment: Optional[str] = None,
        default_temperature: float = 0.2,
    ) -> None:
        if AzureOpenAI is None:
            raise FoundationaLLMClientError(
                "The 'openai' package is required for Azure OpenAI interactions. Install 'openai' to continue."
            )
        self.endpoint = (endpoint or os.getenv("AZURE_OPENAI_ENDPOINT", "")).strip()
        self.api_key = (api_key or os.getenv("AZURE_OPENAI_API_KEY", "")).strip()
        self.api_version = (api_version or os.getenv("AZURE_OPENAI_API_VERSION", "2024-02-15-preview")).strip()
        self.deployment = (deployment or os.getenv("AZURE_OPENAI_DEPLOYMENT", "")).strip()
        self.default_temperature = default_temperature

        if not self.endpoint:
            raise FoundationaLLMAuthenticationError(
                "Missing Azure OpenAI endpoint. Set AZURE_OPENAI_ENDPOINT or provide endpoint explicitly."
            )
        if not self.api_key:
            raise FoundationaLLMAuthenticationError(
                "Missing Azure OpenAI API key. Set AZURE_OPENAI_API_KEY or provide api_key explicitly."
            )
        if not self.deployment:
            raise FoundationaLLMAuthenticationError(
                "Missing Azure OpenAI deployment name. Set AZURE_OPENAI_DEPLOYMENT or provide deployment explicitly."
            )

        try:
            self.client = AzureOpenAI(
                azure_endpoint=self.endpoint,
                api_key=self.api_key,
                api_version=self.api_version,
            )
        except Exception as exc:  # pragma: no cover - Azure client init errors
            raise FoundationaLLMClientError(f"Failed to initialize Azure OpenAI client: {exc}") from exc

    def chat_completion(
        self,
        messages: List[Dict[str, str]],
        *,
        temperature: Optional[float] = None,
        max_tokens: int = 2000,
    ) -> str:
        """
        Execute a chat completion and return the assistant's textual response.
        """
        try:
            response = self.client.chat.completions.create(
                model=self.deployment,
                messages=messages,
                temperature=self._clamp_temperature(temperature),
                max_tokens=max_tokens,
            )
        except Exception as exc:  # pragma: no cover - Azure API failures
            raise FoundationaLLMClientError(f"Azure OpenAI chat completion failed: {exc}") from exc

        try:
            return response.choices[0].message.content.strip()
        except (AttributeError, IndexError, KeyError) as exc:
            raise FoundationaLLMClientError("Azure OpenAI response did not contain message content.") from exc

    def _clamp_temperature(self, temperature: Optional[float]) -> float:
        value = self.default_temperature if temperature is None else temperature
        return max(0.0, min(1.0, value))
