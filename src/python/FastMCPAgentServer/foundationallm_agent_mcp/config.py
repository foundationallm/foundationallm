"""Configuration helpers for the FoundationaLLM MCP server."""

from __future__ import annotations

from typing import Literal

from pydantic import AnyHttpUrl, Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Centralized server configuration loaded from environment variables."""

    management_endpoint: AnyHttpUrl = Field(
        ...,
        alias="FOUNDATIONALLM_MANAGEMENT_ENDPOINT",
        description="Base URL for the FoundationaLLM management API (e.g. https://host/management).",
    )
    management_scope: str = Field(
        ...,
        alias="FOUNDATIONALLM_MANAGEMENT_SCOPE",
        description="AAD scope used when requesting tokens for the management API.",
    )
    instance_id: str = Field(
        ...,
        alias="FOUNDATIONALLM_INSTANCE_ID",
        description="FoundationaLLM instance identifier (GUID).",
    )
    api_version: str = Field(
        default="2024-02-16",
        alias="FOUNDATIONALLM_MANAGEMENT_API_VERSION",
        description="Management API version query string.",
    )
    verify_ssl: bool = Field(
        default=True,
        alias="FOUNDATIONALLM_VERIFY_SSL",
        description="Disable only for dev/self-signed environments.",
    )
    request_timeout_seconds: float = Field(
        default=90.0,
        alias="FOUNDATIONALLM_REQUEST_TIMEOUT_SECONDS",
        description="HTTP timeout applied to management API calls.",
    )
    auth_mode: Literal["default", "cli", "managed_identity"] = Field(
        default="default",
        alias="FOUNDATIONALLM_AUTH_MODE",
        description="Selects which Azure Identity credential chain to use.",
    )
    tenant_id: str | None = Field(
        default=None,
        alias="FOUNDATIONALLM_TENANT_ID",
        description="Optional tenant hint passed to Azure Identity credentials.",
    )
    log_http: bool = Field(
        default=False,
        alias="FOUNDATIONALLM_LOG_HTTP",
        description="Emit concise HTTP request/response summaries for debugging.",
    )

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
        case_sensitive=False,
    )

