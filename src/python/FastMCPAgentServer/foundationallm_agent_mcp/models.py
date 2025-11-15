"""Pydantic models shared by the MCP tools."""

from __future__ import annotations

from datetime import date, datetime
from pathlib import Path
from typing import Dict, List, Optional

from pydantic import BaseModel, Field, validator


class PrivateStoreFileInput(BaseModel):
    """Describes a local file that should be uploaded to an agent's private store."""

    path: Path = Field(..., description="Absolute or relative path to the file to upload.")
    description: Optional[str] = Field(
        default=None, description="Optional friendly description (stored in metadata)."
    )
    overwrite: bool = Field(
        default=False,
        description="Delete an existing file with the same name before uploading.",
    )
    tool_visibility: Optional[Dict[str, bool]] = Field(
        default=None,
        description="Map of tool names to booleans (e.g. {'OpenAIAssistantsCodeInterpreter': true}).",
    )


class BasicAgentTemplateInput(BaseModel):
    """Input payload for creating an agent from the BasicAgentTemplate."""

    display_name: str = Field(..., description="Friendly name shown to users.")
    description: str = Field(
        default="",
        description="Optional description that appears in management experiences.",
    )
    welcome_message: Optional[str] = Field(
        default=None,
        description="Optional welcome message stored in properties.welcome_message.",
    )
    agent_name: Optional[str] = Field(
        default=None,
        description="Resource name. If omitted, a slug is generated from the display name.",
    )
    expiration_date: Optional[str | date | datetime] = Field(
        default=None,
        description="Optional expiration date. Accepts ISO string, date, or datetime.",
    )
    enable_file_uploads: Optional[bool] = Field(
        default=None,
        description="Toggle end-user file uploads (sets show_file_upload).",
    )
    private_store_files: Optional[List[PrivateStoreFileInput]] = Field(
        default=None,
        description="Files to upload immediately after creation.",
    )

    @validator("display_name")
    def _strip_display_name(cls, value: str) -> str:
        if not value.strip():
            raise ValueError("display_name cannot be empty.")
        return value.strip()


class DependencyFilters(BaseModel):
    """Flags for the dependency inventory tool."""

    include_prompts: bool = True
    include_ai_models: bool = True
    include_workflows: bool = True
    include_vectorization_profiles: bool = True
    include_data_sources: bool = True
    include_agent_tools: bool = True


class AgentNameInput(BaseModel):
    """Simple helper model that only requires an agent name."""

    agent_name: str = Field(..., description="Agent resource name.")


class UploadPrivateFileInput(AgentNameInput):
    """Upload-or-update a specific private store file."""

    file: PrivateStoreFileInput


class DeletePrivateFileInput(AgentNameInput):
    """Removes a file from an agent's private store."""

    file_name: str = Field(..., description="Name of the file to delete.")


class FileToolToggleInput(AgentNameInput):
    """Updates tool associations for a private store file."""

    file_object_id: Optional[str] = Field(
        default=None,
        description="Full object id of the agent file. Provide either this or file_name.",
    )
    file_name: Optional[str] = Field(
        default=None,
        description="Fallback identifier when the object id is unknown.",
    )
    tool_visibility: Dict[str, bool] = Field(
        ...,
        description="Map of tool names to booleans (true = allow tool to access the file).",
    )


class FileUploadPolicyInput(AgentNameInput):
    """Enable or disable end-user file uploads for an agent."""

    enabled: bool = Field(..., description="Set show_file_upload to this value.")


class AccessTokenCreateInput(AgentNameInput):
    """Creates a new agent access token."""

    description: str = Field(..., description="Friendly description for the token.")
    display_name: Optional[str] = Field(
        default=None,
        description="Optional display name; defaults to the description.",
    )
    active: bool = Field(default=True, description="Whether the token is enabled on creation.")
    expiration_days: Optional[int] = Field(
        default=120,
        description="Convenience helper that adds N days to now to compute the expiration date.",
    )
    expiration_date: Optional[datetime | date | str] = Field(
        default=None,
        description="Explicit expiration date. Overrides expiration_days when provided.",
    )


class AccessTokenDeleteInput(AgentNameInput):
    """Deletes an agent access token."""

    token_name: str = Field(..., description="Token resource name or identifier.")


class ListAccessTokensInput(AgentNameInput):
    """Lists agent access tokens."""

    include_inactive: bool = Field(
        default=True, description="Return inactive tokens alongside active ones."
    )

