"""FastMCP server exposing FoundationaLLM agent management helpers."""

from __future__ import annotations

import logging
import re
import uuid
from datetime import UTC, date, datetime, timedelta
from pathlib import Path
from typing import Any

from dotenv import load_dotenv
from fastmcp import FastMCP
from fastmcp.context import Context

from .config import Settings
from .auth import TokenProvider
from .management_client import ManagementApiClient
from .models import (
    AccessTokenCreateInput,
    AccessTokenDeleteInput,
    AgentNameInput,
    BasicAgentTemplateInput,
    DeletePrivateFileInput,
    DependencyFilters,
    FileToolToggleInput,
    FileUploadPolicyInput,
    ListAccessTokensInput,
    PrivateStoreFileInput,
    UploadPrivateFileInput,
)

load_dotenv()

LOGGER = logging.getLogger(__name__)

settings = Settings()
token_provider = TokenProvider(settings)
client = ManagementApiClient(settings, token_provider)


async def lifespan(_app: FastMCP):
    """Ensures the underlying HTTP client is ready before the server starts."""
    await client.__aenter__()
    try:
        yield
    finally:
        await client.__aexit__(None, None, None)


server = FastMCP(
    name="FoundationaLLM Agent Manager",
    version="0.1.0",
    instructions=(
        "Use these tools to create and configure FoundationaLLM agents via the management API. "
        "All operations are scoped to the configured instance id."
    ),
    lifespan=lifespan,
)


# ---------------------------------------------------------------------------
# Helper utilities
# ---------------------------------------------------------------------------


def _slugify_agent_name(display_name: str) -> str:
    slug = re.sub(r"[^a-zA-Z0-9-]+", "-", display_name.strip().lower())
    slug = re.sub(r"-{2,}", "-", slug).strip("-")
    slug = slug[:63]
    return slug or f"agent-{uuid.uuid4().hex[:8]}"


def _ensure_datetime(value: str | date | datetime | None, fallback_days: int = 365) -> datetime:
    if value is None:
        return datetime.now(tz=UTC) + timedelta(days=fallback_days)
    if isinstance(value, datetime):
        if value.tzinfo is None:
            return value.replace(tzinfo=UTC)
        return value.astimezone(UTC)
    if isinstance(value, date):
        return datetime.combine(value, datetime.min.time(), tzinfo=UTC)
    if isinstance(value, str):
        try:
            parsed = datetime.fromisoformat(value)
        except ValueError as exc:
            raise ValueError(f"Unable to parse ISO8601 datetime: {value}") from exc
        if parsed.tzinfo is None:
            parsed = parsed.replace(tzinfo=UTC)
        return parsed
    raise TypeError(f"Unsupported datetime input type: {type(value)}")


def _format_datetime_iso(value: datetime | str | date | None, fallback_days: int = 365) -> str:
    return _ensure_datetime(value, fallback_days=fallback_days).isoformat()


async def _notify(ctx: Context | None, message: str) -> None:
    if ctx is None:
        return
    try:
        await ctx.console.log(message)
    except Exception:
        LOGGER.debug("Context logging failed", exc_info=True)


async def _resolve_file_record(agent_name: str, *, file_object_id: str | None = None, file_name: str | None = None) -> dict[str, Any]:
    files = await client.list_agent_files(agent_name)
    for entry in files:
        resource = entry.get("resource", entry)
        object_id = _get_object_id(resource)
        candidate_names = {
            resource.get("name"),
            resource.get("display_name"),
            resource.get("properties", {}).get("file_name"),
        }
        if entry.get("name"):
            candidate_names.add(entry["name"])
        if file_object_id and object_id == file_object_id:
            return resource
        if file_name:
            if any((name or "").lower() == file_name.lower() for name in candidate_names if name):
                return resource
    raise ValueError(f"File not found for agent '{agent_name}' with criteria object_id={file_object_id}, file_name={file_name}")


def _get_object_id(resource: dict[str, Any]) -> str | None:
    return resource.get("object_id") or resource.get("id")


async def _build_tool_map() -> dict[str, str]:
    tools = await client.list_agent_tools()
    mapping: dict[str, str] = {}
    for tool in tools:
        name = tool.get("name") or tool.get("display_name")
        object_id = tool.get("object_id") or tool.get("id")
        if name and object_id:
            mapping[name] = object_id
            mapping[object_id] = object_id
    if not mapping:
        raise RuntimeError("Agent tool catalog is empty; cannot manage tool associations.")
    return mapping


def _prepare_file_upload_payload(file: PrivateStoreFileInput) -> tuple[Path, str]:
    file_path = (Path(file.path)).expanduser()
    if not file_path.is_file():
        raise FileNotFoundError(f"Cannot find private store file at {file_path}")
    return file_path, file_path.name


# ---------------------------------------------------------------------------
# Tool implementations
# ---------------------------------------------------------------------------


@server.tool(name="describe_foundationallm_agent_schema")
async def describe_agent_schema(ctx: Context | None = None) -> dict[str, Any]:
    """Returns a quick reference to the knowledge-management agent schema."""
    doc_path = "docs/setup-guides/agents/knowledge-management-agent.md"
    summary = (
        "Knowledge management agents must declare type='knowledge-management' and include identity, "
        "security, orchestration, language model, vectorization, data sources, and UI preferences. "
        "The schema documented in docs/setup-guides/agents/knowledge-management-agent.md mirrors the "
        "Management API payloads used by this server."
    )
    await _notify(ctx, f"Referencing schema documentation at {doc_path}")
    return {
        "summary": summary,
        "reference": doc_path,
    }


@server.tool(name="list_foundationallm_dependencies")
async def list_dependencies(filters: DependencyFilters) -> dict[str, Any]:
    """Lists prompts, AI models, workflows, vectorization profiles, data sources, and tools."""
    results: dict[str, Any] = {}
    if filters.include_prompts:
        results["prompts"] = await client.list_prompts()
    if filters.include_ai_models:
        results["ai_models"] = await client.list_ai_models()
    if filters.include_workflows:
        results["workflows"] = await client.list_workflows()
    if filters.include_vectorization_profiles:
        results["vectorization_profiles"] = await client.list_vectorization_profiles()
    if filters.include_data_sources:
        results["data_sources"] = await client.list_data_sources()
    if filters.include_agent_tools:
        results["agent_tools"] = await client.list_agent_tools()
    return results


@server.tool(name="create_basic_agent")
async def create_basic_agent(request: BasicAgentTemplateInput, ctx: Context | None = None) -> dict[str, Any]:
    """Creates an agent using the BasicAgentTemplate and optionally uploads private files."""
    agent_name = request.agent_name or _slugify_agent_name(request.display_name)
    expiration_iso = _format_datetime_iso(request.expiration_date)
    template_payload = {
        "AGENT_NAME": agent_name,
        "AGENT_DISPLAY_NAME": request.display_name,
        "AGENT_EXPIRATION_DATE": expiration_iso,
        "AGENT_DESCRIPTION": request.description or "",
        "AGENT_WELCOME_MESSAGE": request.welcome_message or "",
    }
    await _notify(ctx, f"Creating agent {agent_name} via BasicAgentTemplate")
    creation_result = await client.create_basic_agent(template_payload)

    post_steps: list[dict[str, Any]] = []

    if request.enable_file_uploads is not None:
        await client.set_agent_file_upload_flag(agent_name, request.enable_file_uploads)
        post_steps.append({"action": "set_file_upload_policy", "enabled": request.enable_file_uploads})

    uploaded_files: list[dict[str, Any]] = []
    if request.private_store_files:
        tool_map = await _build_tool_map()
        for file_input in request.private_store_files:
            file_path, file_name = _prepare_file_upload_payload(file_input)
            if file_input.overwrite:
                await client.delete_agent_file(agent_name, file_name)
            await client.upload_agent_file(agent_name, file_path, file_name=file_name)
            file_record = await _resolve_file_record(agent_name, file_name=file_name)
            object_id = _get_object_id(file_record)
            if not object_id:
                raise RuntimeError("Uploaded file is missing an object_id; cannot continue.")
            file_summary: dict[str, Any] = {
                "file_name": file_name,
                "object_id": object_id,
            }
            if file_input.tool_visibility:
                missing_tools = [key for key in file_input.tool_visibility.keys() if key not in tool_map]
                if missing_tools:
                    raise ValueError(f"Unknown tool names: {', '.join(missing_tools)}")
                association_payload = {
                    object_id: {tool_map[key]: value for key, value in file_input.tool_visibility.items()}
                }
                await client.update_file_tool_associations(agent_name, association_payload)
                file_summary["tool_visibility"] = file_input.tool_visibility
            uploaded_files.append(file_summary)

    return {
        "agent_name": agent_name,
        "creation_result": creation_result,
        "post_creation_steps": post_steps,
        "uploaded_files": uploaded_files,
    }


@server.tool(name="list_private_store_files")
async def list_private_store_files(agent: AgentNameInput) -> dict[str, Any]:
    """Returns files stored in the agent's private store plus their tool associations."""
    files = await client.list_agent_files(agent.agent_name)
    associations = await client.list_file_tool_associations(agent.agent_name)
    association_map: dict[str, dict[str, Any]] = {}
    for item in associations:
        file_object_id = item.get("file_object_id")
        association_map[file_object_id] = item.get("associated_resource_object_ids", {})
    enriched_files = []
    for entry in files:
        resource = entry.get("resource", entry)
        object_id = _get_object_id(resource)
        enriched_files.append(
            {
                "name": resource.get("name"),
                "object_id": object_id,
                "properties": resource.get("properties", {}),
                "tool_associations": association_map.get(object_id, {}),
            }
        )
    return {"files": enriched_files}


@server.tool(name="upload_private_store_file")
async def upload_private_store_file(request: UploadPrivateFileInput) -> dict[str, Any]:
    """Uploads or replaces a private store file and optionally toggles tool access."""
    file_path, file_name = _prepare_file_upload_payload(request.file)
    if request.file.overwrite:
        await client.delete_agent_file(request.agent_name, file_name)
    upload_result = await client.upload_agent_file(request.agent_name, file_path, file_name=file_name)
    file_record = await _resolve_file_record(request.agent_name, file_name=file_name)
    object_id = _get_object_id(file_record)
    if not object_id:
        raise RuntimeError("Uploaded file is missing an object_id; cannot continue.")

    tool_visibility_result: dict[str, Any] | None = None
    if request.file.tool_visibility:
        tool_map = await _build_tool_map()
        unknown = [k for k in request.file.tool_visibility if k not in tool_map]
        if unknown:
            raise ValueError(f"Unknown tool names: {', '.join(unknown)}")
        associations = {object_id: {tool_map[name]: allowed for name, allowed in request.file.tool_visibility.items()}}
        tool_visibility_result = await client.update_file_tool_associations(request.agent_name, associations)

    return {
        "file_name": file_name,
        "object_id": object_id,
        "upload_result": upload_result,
        "tool_visibility_result": tool_visibility_result,
    }


@server.tool(name="delete_private_store_file")
async def delete_private_store_file(request: DeletePrivateFileInput) -> dict[str, Any]:
    """Deletes a file from the agent's private store."""
    result = await client.delete_agent_file(request.agent_name, request.file_name)
    return {"deleted_file": request.file_name, "result": result}


@server.tool(name="set_private_store_tool_access")
async def set_private_store_tool_access(request: FileToolToggleInput) -> dict[str, Any]:
    """Toggles which tools can access a private store file."""
    file_record = await _resolve_file_record(
        request.agent_name,
        file_object_id=request.file_object_id,
        file_name=request.file_name,
    )
    object_id = _get_object_id(file_record)
    if not object_id:
        raise RuntimeError("File is missing an object_id; cannot update associations.")
    tool_map = await _build_tool_map()
    unknown = [k for k in request.tool_visibility if k not in tool_map]
    if unknown:
        raise ValueError(f"Unknown tool names: {', '.join(unknown)}")
    associations = {object_id: {tool_map[name]: allowed for name, allowed in request.tool_visibility.items()}}
    update_result = await client.update_file_tool_associations(request.agent_name, associations)
    return {"file_object_id": object_id, "result": update_result}


@server.tool(name="set_agent_file_upload_policy")
async def set_agent_file_upload_policy(request: FileUploadPolicyInput) -> dict[str, Any]:
    """Enables or disables end-user file uploads for the specified agent."""
    result = await client.set_agent_file_upload_flag(request.agent_name, request.enabled)
    return {"agent_name": request.agent_name, "show_file_upload": request.enabled, "result": result}


@server.tool(name="list_agent_access_tokens")
async def list_agent_access_tokens(request: ListAccessTokensInput) -> dict[str, Any]:
    """Lists access tokens associated with an agent."""
    tokens = await client.list_agent_access_tokens(request.agent_name)
    if not request.include_inactive:
        tokens = [token for token in tokens if token.get("active")]
    return {"agent_name": request.agent_name, "tokens": tokens}


@server.tool(name="create_agent_access_token")
async def create_agent_access_token(request: AccessTokenCreateInput) -> dict[str, Any]:
    """Creates a new agent access token."""
    expiration = (
        _format_datetime_iso(request.expiration_date)
        if request.expiration_date is not None
        else _format_datetime_iso(datetime.now(tz=UTC) + timedelta(days=request.expiration_days or 120))
    )
    token_id = str(uuid.uuid4())
    token_payload = {
        "id": token_id,
        "name": token_id,
        "display_name": request.display_name or request.description,
        "description": request.description,
        "object_id": "",
        "type": "agentAccessToken",
        "cost_center": "",
        "expiration_date": expiration,
        "active": request.active,
    }
    result = await client.create_agent_access_token(request.agent_name, token_payload)
    return {"agent_name": request.agent_name, "token": token_payload, "upsert_result": result}


@server.tool(name="delete_agent_access_token")
async def delete_agent_access_token(request: AccessTokenDeleteInput) -> dict[str, Any]:
    """Deletes an agent access token."""
    result = await client.delete_agent_access_token(request.agent_name, request.token_name)
    return {"agent_name": request.agent_name, "deleted_token": request.token_name, "result": result}


__all__ = ["server"]

