"""FoundationaLLM MCP client tool implementation."""

from __future__ import annotations

import asyncio
import json
from contextlib import asynccontextmanager
from dataclasses import dataclass
from datetime import timedelta
from enum import Enum
from typing import Any, AsyncGenerator, Dict, Optional, Tuple, Type

from langchain_core.callbacks import (
    AsyncCallbackManagerForToolRun,
    CallbackManagerForToolRun,
)
from langchain_core.messages import SystemMessage, HumanMessage, AIMessage
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from opentelemetry.trace import SpanKind
from pydantic import BaseModel, Field, HttpUrl, TypeAdapter, ValidationError

from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult,
)
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    ContentArtifactTypeNames,
    RunnableConfigKeys,
)
from foundationallm.models.orchestration import ContentArtifact

from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from foundationallm.config import Configuration, UserIdentity

from mcp import ClientSession, Implementation, StdioServerParameters, stdio_client
from mcp.client.streamable_http import streamablehttp_client
from mcp.client.sse import sse_client

from .foundationallm_mcp_client_tool_input import (
    FoundationaLLMMCPClientToolInput,
    MCPTransportOverrides,
)


class MCPTransport(str, Enum):
    """Supported MCP transport modes."""

    STDIO = "stdio"
    STREAMABLE_HTTP = "streamable_http"
    HTTP_SSE = "http_sse"


class MCPStdIOConfiguration(BaseModel):
    """Configuration for STDIO transport."""

    command: str = Field(..., description="Command that launches the MCP server executable.")
    args: list[str] = Field(default_factory=list, description="Arguments supplied to the command.")
    env: Dict[str, str] | None = Field(
        default=None, description="Environment variables appended to the process environment."
    )
    cwd: str | None = Field(
        default=None, description="Working directory for the launched process."
    )
    encoding: str = Field(default="utf-8", description="Encoding used for STDIO streams.")
    encoding_error_handler: str = Field(
        default="strict",
        description="Encoding error handler applied to STDIO streams.",
    )


class MCPHTTPConfiguration(BaseModel):
    """Configuration shared by HTTP-based transports."""

    url: HttpUrl
    headers: Dict[str, str] = Field(default_factory=dict)
    timeout_seconds: float = Field(default=30.0)
    sse_read_timeout_seconds: float = Field(default=300.0)


class MCPStreamableHTTPConfiguration(MCPHTTPConfiguration):
    """Additional settings for streamable HTTP transport."""

    terminate_on_close: bool = Field(default=True)


class MCPClientToolConfiguration(BaseModel):
    """Root configuration for the MCP client tool."""

    transport: MCPTransport = Field(
        default=MCPTransport.STREAMABLE_HTTP,
        description="Transport used to connect to the target MCP server.",
    )
    stdio: MCPStdIOConfiguration | None = None
    streamable_http: MCPStreamableHTTPConfiguration | None = None
    http_sse: MCPHTTPConfiguration | None = None
    client_name: str = Field(
        default="foundationallm-mcp-client",
        description="Client identifier reported during MCP initialization.",
    )
    client_version: str = Field(
        default="1.0.0",
        description="Version string reported during MCP initialization.",
    )
    default_operation_timeout_seconds: float | None = Field(
        default=None,
        description=(
            "Optional default timeout (seconds) applied to operations that accept a "
            "read timeout parameter."
        ),
    )

    @property
    def http_configuration(self) -> MCPHTTPConfiguration | None:
        if self.transport == MCPTransport.STREAMABLE_HTTP:
            return self.streamable_http
        if self.transport == MCPTransport.HTTP_SSE:
            return self.http_sse
        return None


@dataclass
class MCPConnectionDetails:
    """Information about an established MCP session."""

    transport: MCPTransport
    target: str
    session_id: str | None
    initialize_result: Any
    transport_metadata: Dict[str, Any]


class FoundationaLLMMCPClientTool(FoundationaLLMToolBase):
    """Tool that proxies requests to arbitrary MCP servers."""

    args_schema: Type[BaseModel] = FoundationaLLMMCPClientToolInput

    _ALLOWED_OPERATIONS = {
        "call_tool",
        "list_tools",
        "list_resources",
        "read_resource",
        "list_prompts",
        "get_prompt",
        "complete",
        "ping",
        "intelligent_execute",
    }

    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: 'UserIdentity',
        config: 'Configuration',
    ):
        super().__init__(tool_config, objects, user_identity, config)
        properties = tool_config.properties or {}
        try:
            self._mcp_config = MCPClientToolConfiguration.model_validate(properties)
        except ValidationError as exc:
            raise ToolException(
                f"Invalid MCP client tool configuration: {exc}"
            ) from exc

        match self._mcp_config.transport:
            case MCPTransport.STDIO if self._mcp_config.stdio is None:
                raise ToolException(
                    "STDIO transport selected but no STDIO configuration provided."
                )
            case MCPTransport.STREAMABLE_HTTP if self._mcp_config.streamable_http is None:
                raise ToolException(
                    "Streamable HTTP transport selected but configuration is missing."
                )
            case MCPTransport.HTTP_SSE if self._mcp_config.http_sse is None:
                raise ToolException(
                    "HTTP+SSE transport selected but configuration is missing."
                )

        self._client_info = Implementation(
            name=self._mcp_config.client_name,
            version=self._mcp_config.client_version,
            title="FoundationaLLM MCP Client",
        )

        # Value adapters for argument normalization.
        self._any_url_adapter = TypeAdapter(HttpUrl)

        # Initialize LLM and orchestration prompt for intelligent_execute
        self.main_llm = self.get_main_language_model()
        self.orchestration_prompt = self.get_main_prompt()

    def _run(
        self,
        operation: str,
        arguments: Optional[Dict[str, Any]] = None,
        transport_overrides: Optional[MCPTransportOverrides] = None,
        response_format: str = "json",
        run_manager: Optional[CallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig | None = None,
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """Synchronous execution entrypoint."""

        loop = asyncio.new_event_loop()
        try:
            return loop.run_until_complete(
                self._arun(
                    operation=operation,
                    arguments=arguments or {},
                    transport_overrides=transport_overrides,
                    response_format=response_format,
                    runnable_config=runnable_config,
                )
            )
        finally:
            loop.run_until_complete(loop.shutdown_asyncgens())
            loop.close()

    async def _arun(
        self,
        operation: str,
        arguments: Optional[Dict[str, Any]] = None,
        transport_overrides: Optional[MCPTransportOverrides] = None,
        response_format: str = "json",
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig | None = None,
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """Asynchronous execution entrypoint."""

        arguments = arguments or {}
        transport_overrides = transport_overrides or MCPTransportOverrides()

        original_prompt = self._extract_original_prompt(runnable_config)
        invocation_summary = json.dumps(
            {
                "operation": operation,
                "arguments": arguments,
                "transport_overrides": transport_overrides.model_dump(exclude_none=True),
            },
            indent=2,
            default=str,
        )
        if original_prompt is None:
            original_prompt = invocation_summary

        if operation not in self._ALLOWED_OPERATIONS:
            raise ToolException(
                f"Operation '{operation}' is not supported by the MCP client tool."
            )

        # Route to intelligent_execute for LLM-powered orchestration
        if operation == "intelligent_execute":
            return await self._intelligent_execute(
                arguments, transport_overrides, response_format, runnable_config, original_prompt
            )

        with self.tracer.start_as_current_span(
            f"{self.name}.session", kind=SpanKind.CLIENT
        ) as connect_span:
            connect_span.set_attribute("mcp.operation", operation)
            connect_span.set_attribute("mcp.transport", self._mcp_config.transport.value)

            try:
                async with self._open_session(transport_overrides) as (
                    session,
                    connection_details,
                ):
                    connect_span.set_attribute(
                        "mcp.target", connection_details.target
                    )
                    if connection_details.session_id:
                        connect_span.set_attribute(
                            "mcp.session_id", connection_details.session_id
                        )

                    with self.tracer.start_as_current_span(
                        f"{self.name}.request", kind=SpanKind.CLIENT
                    ) as request_span:
                        request_span.set_attribute(
                            "mcp.operation", operation
                        )
                        response_model = await self._invoke_operation(
                            session,
                            operation,
                            arguments,
                        )
                        result_payload = self._serialize_result(response_model)
            except Exception as exc:
                self.logger.exception(
                    "MCP client tool invocation failed for operation %s", operation
                )
                error_artifact = self.create_error_content_artifact(
                    original_prompt,
                    exc,
                )
                return (
                    f"MCP client invocation failed: {exc}",
                    FoundationaLLMToolResult(
                        content=f"MCP client invocation failed: {exc}",
                        content_artifacts=[error_artifact],
                        input_tokens=0,
                        output_tokens=0,
                    ),
                )

        content, artifacts = self._format_response(
            response_format,
            result_payload,
            connection_details,
            original_prompt,
            invocation_summary,
        )

        return (
            content,
            FoundationaLLMToolResult(
                content=content,
                content_artifacts=artifacts,
                input_tokens=0,
                output_tokens=0,
            ),
        )

    async def _execute_direct_mcp_operation(
        self, operation: str, arguments: Dict[str, Any], transport_overrides: MCPTransportOverrides
    ) -> str:
        """Execute a direct MCP operation and return the content string result."""
        
        with self.tracer.start_as_current_span(
            f"{self.name}.session", kind=SpanKind.CLIENT
        ) as connect_span:
            connect_span.set_attribute("mcp.operation", operation)
            connect_span.set_attribute("mcp.transport", self._mcp_config.transport.value)

            try:
                async with self._open_session(transport_overrides) as (
                    session,
                    connection_details,
                ):
                    connect_span.set_attribute(
                        "mcp.target", connection_details.target
                    )
                    if connection_details.session_id:
                        connect_span.set_attribute(
                            "mcp.session_id", connection_details.session_id
                        )

                    with self.tracer.start_as_current_span(
                        f"{self.name}.request", kind=SpanKind.CLIENT
                    ) as request_span:
                        request_span.set_attribute(
                            "mcp.operation", operation
                        )
                        response_model = await self._invoke_operation(
                            session,
                            operation,
                            arguments,
                        )
                        result_payload = self._serialize_result(response_model)
            except Exception as exc:
                self.logger.exception(
                    "MCP client tool invocation failed for operation %s", operation
                )
                raise ToolException(f"MCP client invocation failed: {exc}") from exc

        content, _ = self._format_response(
            "json",  # Always use JSON format for internal operations
            result_payload,
            connection_details,
            f"MCP operation: {operation}",
            json.dumps({"operation": operation, "arguments": arguments}, indent=2),
        )
        
        return content

    async def _intelligent_execute(
        self,
        arguments: Dict[str, Any],
        transport_overrides: MCPTransportOverrides,
        response_format: str,
        runnable_config: RunnableConfig | None,
        original_prompt: str
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """LLM-powered orchestration of MCP operations."""
        
        # Extract prompt from arguments
        user_prompt = arguments.get("prompt")
        if not user_prompt:
            raise ToolException("Missing required 'prompt' argument for intelligent_execute operation")
        
        # Check if LLM is configured
        if not self.main_llm:
            raise ToolException("LLM not configured. Please configure main_prompt resource object ID for intelligent_execute operation")
        
        if not self.orchestration_prompt:
            raise ToolException("Orchestration prompt not configured. Please configure main_prompt resource object ID for intelligent_execute operation")
        
        # Track token usage
        input_tokens = 0
        output_tokens = 0
        
        try:
            # Discovery Phase: Get available tools
            with self.tracer.start_as_current_span(f'{self.name}_discovery', kind=SpanKind.INTERNAL):
                tools_response = await self._execute_direct_mcp_operation("list_tools", {}, transport_overrides)
                available_tools = json.loads(tools_response)
            
            # Planning Phase: Use LLM to create execution plan
            with self.tracer.start_as_current_span(f'{self.name}_orchestration_llm_call', kind=SpanKind.INTERNAL):
                orchestration_messages = [
                    SystemMessage(content=self.orchestration_prompt),
                    HumanMessage(content=f"""
User Query: {user_prompt}

Available MCP Tools:
{json.dumps(available_tools, indent=2)}

Please create an execution plan for this request.
You must return ONLY a valid JSON object with no surrounding text or Markdown code fences.
Do not include ```json fences or any prose.
""")
                ]
                
                llm_response = await self.main_llm.ainvoke(orchestration_messages)
                input_tokens += llm_response.usage_metadata.get('input_tokens', 0)
                output_tokens += llm_response.usage_metadata.get('output_tokens', 0)
                
                # Parse execution plan with fail-fast validation
                try:
                    execution_plan = json.loads(llm_response.content)
                except json.JSONDecodeError as e:
                    raise ToolException(f"LLM returned invalid JSON execution plan: {e}. Response: {llm_response.content}")
            
            # Execution Phase: Execute the planned operations
            results = []
            for i, step in enumerate(execution_plan.get("tools_to_execute", [])):
                with self.tracer.start_as_current_span(f'{self.name}_execution_step_{i}', kind=SpanKind.INTERNAL):
                    step_operation = step.get("operation")
                    step_arguments = step.get("arguments", {})
                    
                    if not step_operation:
                        raise ToolException(f"Execution step {i} missing 'operation' field")
                    
                    step_result = await self._execute_direct_mcp_operation(
                        step_operation, step_arguments, transport_overrides
                    )
                    results.append(step_result)
            
            # Synthesis Phase: Use LLM to synthesize final response
            with self.tracer.start_as_current_span(f'{self.name}_synthesis_llm_call', kind=SpanKind.INTERNAL):
                synthesis_messages = [
                    SystemMessage(content="Synthesize the MCP tool results into a coherent response that addresses the user's query."),
                    HumanMessage(content=f"""
Original Query: {user_prompt}

Tool Results:
{json.dumps(results, indent=2)}

Provide a comprehensive response that addresses the user's query.
""")
                ]
                
                synthesis_response = await self.main_llm.ainvoke(synthesis_messages)
                input_tokens += synthesis_response.usage_metadata.get('input_tokens', 0)
                output_tokens += synthesis_response.usage_metadata.get('output_tokens', 0)
                
                final_response = synthesis_response.content
            
            # Create content artifact with token tracking
            content_artifact = self.create_content_artifact(
                original_prompt,
                title="Intelligent MCP Execution",
                tool_input=f"Prompt: {user_prompt}",
                prompt_tokens=input_tokens,
                completion_tokens=output_tokens
            )
            
            return (
                final_response,
                FoundationaLLMToolResult(
                    content=final_response,
                    content_artifacts=[content_artifact],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens,
                ),
            )
            
        except Exception as exc:
            self.logger.exception("Intelligent execute failed: %s", exc)
            error_artifact = self.create_error_content_artifact(original_prompt, exc)
            return (
                f"Intelligent execute failed: {exc}",
                FoundationaLLMToolResult(
                    content=f"Intelligent execute failed: {exc}",
                    content_artifacts=[error_artifact],
                    input_tokens=input_tokens,
                    output_tokens=output_tokens,
                ),
            )

    async def _invoke_operation(
        self,
        session: ClientSession,
        operation: str,
        arguments: Dict[str, Any],
    ) -> Any:
        """Dispatch the requested operation to the MCP session."""

        normalized_arguments = self._normalize_arguments(operation, arguments)
        target_fn = getattr(session, operation, None)
        if target_fn is None or not callable(target_fn):
            raise ToolException(f"Unsupported MCP operation: {operation}")

        return await target_fn(**normalized_arguments)

    def _normalize_arguments(
        self, operation: str, arguments: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Normalize user supplied arguments for the requested operation."""

        normalized = dict(arguments)

        if operation == "call_tool":
            timeout = normalized.get("read_timeout_seconds")
            if timeout is None and self._mcp_config.default_operation_timeout_seconds:
                timeout = self._mcp_config.default_operation_timeout_seconds
            if timeout is not None:
                normalized["read_timeout_seconds"] = timedelta(seconds=float(timeout))

        if operation == "read_resource" and "uri" in normalized:
            normalized["uri"] = self._any_url_adapter.validate_python(
                normalized["uri"]
            )

        if operation == "complete" and "ref" in normalized:
            # Ensure the reference is a dict suitable for validation by pydantic.
            ref = normalized["ref"]
            if not isinstance(ref, dict):
                raise ToolException(
                    "The 'ref' argument for 'complete' must be a dictionary conforming to the MCP specification."
                )

        return normalized

    @asynccontextmanager
    async def _open_session(
        self, overrides: MCPTransportOverrides
    ) -> AsyncGenerator[Tuple[ClientSession, MCPConnectionDetails], None]:
        """Open an MCP client session using the configured transport."""

        transport = self._mcp_config.transport
        if transport == MCPTransport.STDIO:
            stdio_settings = self._mcp_config.stdio
            assert stdio_settings is not None
            server = StdioServerParameters(
                command=stdio_settings.command,
                args=stdio_settings.args,
                env=self._resolve_env(stdio_settings.env),
                cwd=stdio_settings.cwd,
                encoding=stdio_settings.encoding,
                encoding_error_handler=stdio_settings.encoding_error_handler,
            )
            async with stdio_client(server) as (read_stream, write_stream):
                async with ClientSession(
                    read_stream,
                    write_stream,
                    client_info=self._client_info,
                ) as session:
                    initialize_result = await session.initialize()
                    details = MCPConnectionDetails(
                        transport=transport,
                        target=server.command,
                        session_id=None,
                        initialize_result=initialize_result,
                        transport_metadata={"args": server.args},
                    )
                    yield session, details
        else:
            http_config = self._mcp_config.http_configuration
            assert http_config is not None
            merged_headers = self._resolve_headers(http_config.headers)
            if overrides.headers:
                merged_headers.update(self._resolve_headers(overrides.headers))

            url = overrides.url or str(http_config.url)
            timeout = overrides.timeout_seconds or http_config.timeout_seconds
            sse_timeout = (
                overrides.sse_read_timeout_seconds
                or http_config.sse_read_timeout_seconds
            )

            if transport == MCPTransport.STREAMABLE_HTTP:
                stream_config = self._mcp_config.streamable_http
                assert stream_config is not None
                terminate_on_close = (
                    overrides.terminate_on_close
                    if overrides.terminate_on_close is not None
                    else stream_config.terminate_on_close
                )
                async with streamablehttp_client(
                    url=url,
                    headers=merged_headers,
                    timeout=timeout,
                    sse_read_timeout=sse_timeout,
                    terminate_on_close=terminate_on_close,
                ) as (read_stream, write_stream, get_session_id):
                    async with ClientSession(
                        read_stream,
                        write_stream,
                        client_info=self._client_info,
                    ) as session:
                        initialize_result = await session.initialize()
                        details = MCPConnectionDetails(
                            transport=transport,
                            target=url,
                            session_id=get_session_id(),
                            initialize_result=initialize_result,
                            transport_metadata={"headers": merged_headers},
                        )
                        yield session, details
            else:
                async with sse_client(
                    url=url,
                    headers=merged_headers,
                    timeout=timeout,
                    sse_read_timeout=sse_timeout,
                ) as (read_stream, write_stream):
                    async with ClientSession(
                        read_stream,
                        write_stream,
                        client_info=self._client_info,
                    ) as session:
                        initialize_result = await session.initialize()
                        details = MCPConnectionDetails(
                            transport=transport,
                            target=url,
                            session_id=None,
                            initialize_result=initialize_result,
                            transport_metadata={"headers": merged_headers},
                        )
                        yield session, details

    def _resolve_env(self, env: Dict[str, str] | None) -> Dict[str, str] | None:
        if env is None:
            return None
        return {key: self._resolve_value(value) for key, value in env.items()}

    def _resolve_headers(self, headers: Dict[str, str]) -> Dict[str, str]:
        return {key: self._resolve_value(value) for key, value in headers.items()}

    def _resolve_value(self, value: str) -> str:
        if value is None:
            return value
        if value.startswith("config://"):
            config_key = value.split("config://", 1)[1]
            return self.config.get_value(config_key)
        if value.startswith("env://"):
            env_key = value.split("env://", 1)[1]
            return self.config.get_value(env_key)
        return value

    def _serialize_result(self, response: Any) -> Dict[str, Any]:
        if hasattr(response, "model_dump"):
            return response.model_dump()
        if isinstance(response, dict):
            return response
        return json.loads(json.dumps(response, default=str))

    def _format_response(
        self,
        response_format: str,
        payload: Dict[str, Any],
        connection_details: MCPConnectionDetails,
        original_prompt: str,
        invocation_summary: str,
    ) -> Tuple[str, list[ContentArtifact]]:
        formatted_content = (
            json.dumps(payload, indent=2, default=str)
            if response_format.lower() == "json"
            else str(payload)
        )

        metadata = {
            "transport": connection_details.transport.value,
            "target": connection_details.target,
            "session_id": connection_details.session_id,
            "transport_metadata": connection_details.transport_metadata,
            "invocation": json.loads(invocation_summary),
        }
        if hasattr(connection_details.initialize_result, "model_dump"):
            metadata["initialize_result"] = connection_details.initialize_result.model_dump()

        artifact = ContentArtifact(
            id=self.name,
            title=f"{self.name} - {metadata['transport']} {metadata['target']}",
            source=self.name,
            type=ContentArtifactTypeNames.TOOL_EXECUTION,
            content=formatted_content,
            metadata=metadata,
        )
        return formatted_content, [artifact]

    def _extract_original_prompt(
        self, runnable_config: RunnableConfig | None
    ) -> Optional[str]:
        if not runnable_config:
            return None
        configurable = runnable_config.get("configurable", {})
        user_prompt_rewrite = configurable.get(
            RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE
        )
        if user_prompt_rewrite:
            return user_prompt_rewrite
        return configurable.get(RunnableConfigKeys.ORIGINAL_USER_PROMPT)

