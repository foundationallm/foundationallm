"""Tests for the FoundationaLLM MCP client tool."""

from __future__ import annotations

import asyncio
import json
import os
import sys
import types
import unittest
from contextlib import asynccontextmanager
from dataclasses import dataclass
from importlib import util
from pathlib import Path
from typing import Any, Dict

sys.path.append('src/python/PythonSDK')

from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import ContentArtifactTypeNames

ROOT = Path(__file__).resolve().parents[5]

if 'foundationallm.config' not in sys.modules:
    fake_config_module = types.ModuleType('foundationallm.config')

    class _StubConfiguration:
        def get_value(self, key: str) -> str:
            raise KeyError(key)

        def get_feature_flag(self, key: str) -> bool:
            return False

    class _StubUserIdentity:
        pass

    fake_config_module.Configuration = _StubConfiguration
    fake_config_module.UserIdentity = _StubUserIdentity
    sys.modules['foundationallm.config'] = fake_config_module

if 'foundationallm.langchain.language_models' not in sys.modules:
    fake_language_models = types.ModuleType('foundationallm.langchain.language_models')

    class _StubLanguageModelFactory:
        def __init__(self, objects: Dict[str, Any], config: Any) -> None:
            self.objects = objects
            self.config = config

        def get_language_model(self, *args, **kwargs):
            raise NotImplementedError('Language model access is not available in tests.')

    fake_language_models.LanguageModelFactory = _StubLanguageModelFactory
    sys.modules['foundationallm.langchain.language_models'] = fake_language_models

if 'foundationallm.telemetry' not in sys.modules:
    fake_telemetry = types.ModuleType('foundationallm.telemetry')

    class _StubTelemetry:
        @staticmethod
        def get_logger(name: str):
            class _Logger:
                def info(self, *args, **kwargs):
                    pass

                def warning(self, *args, **kwargs):
                    pass

                def error(self, *args, **kwargs):
                    pass

                def exception(self, *args, **kwargs):
                    pass

            return _Logger()

        @staticmethod
        def get_tracer(name: str):
            class _Span:
                def __enter__(self):
                    return self

                def __exit__(self, exc_type, exc_value, traceback):
                    return False

                def set_attribute(self, *args, **kwargs):
                    pass

            class _Tracer:
                def start_as_current_span(self, *args, **kwargs):
                    return _Span()

            return _Tracer()

    fake_telemetry.Telemetry = _StubTelemetry
    sys.modules['foundationallm.telemetry'] = fake_telemetry

if 'foundationallm.operations' not in sys.modules:
    fake_operations = types.ModuleType('foundationallm.operations')

    class _StubOperationsManager:
        pass

    fake_operations.OperationsManager = _StubOperationsManager
    sys.modules['foundationallm.operations'] = fake_operations

package_root = ROOT / "src/python/FoundationaLLMAgentPlugins/src/foundationallm_agent_plugins"
if 'foundationallm_agent_plugins' not in sys.modules:
    package_module = types.ModuleType('foundationallm_agent_plugins')
    package_module.__path__ = [str(package_root)]
    sys.modules['foundationallm_agent_plugins'] = package_module

tools_package_path = package_root / 'tools'
if 'foundationallm_agent_plugins.tools' not in sys.modules:
    tools_package_module = types.ModuleType('foundationallm_agent_plugins.tools')
    tools_package_module.__path__ = [str(tools_package_path)]
    sys.modules['foundationallm_agent_plugins.tools'] = tools_package_module

tool_module_path = ROOT / "src/python/FoundationaLLMAgentPlugins/src/foundationallm_agent_plugins/tools/foundationallm_mcp_client_tool.py"
tool_module_spec = util.spec_from_file_location(
    "foundationallm_agent_plugins.tools.foundationallm_mcp_client_tool",
    tool_module_path,
)
assert tool_module_spec and tool_module_spec.loader
tool_module = util.module_from_spec(tool_module_spec)
sys.modules[tool_module_spec.name] = tool_module
tool_module_spec.loader.exec_module(tool_module)

input_module_path = ROOT / "src/python/FoundationaLLMAgentPlugins/src/foundationallm_agent_plugins/tools/foundationallm_mcp_client_tool_input.py"
input_module_spec = util.spec_from_file_location(
    "foundationallm_agent_plugins.tools.foundationallm_mcp_client_tool_input",
    input_module_path,
)
assert input_module_spec and input_module_spec.loader
input_module = util.module_from_spec(input_module_spec)
sys.modules[input_module_spec.name] = input_module
input_module_spec.loader.exec_module(input_module)

FoundationaLLMMCPClientTool = tool_module.FoundationaLLMMCPClientTool
MCPConnectionDetails = tool_module.MCPConnectionDetails
MCPTransport = tool_module.MCPTransport
MCPTransportOverrides = input_module.MCPTransportOverrides


def _is_verbose() -> bool:
    return any(arg.startswith('-v') for arg in sys.argv)


class _FakeConfig:
    def __init__(self, values: Dict[str, str] | None = None) -> None:
        self._values = values or {}

    def get_value(self, key: str) -> str:
        if key not in self._values:
            raise KeyError(key)
        return self._values[key]


class _FakeResponse:
    def __init__(self, payload: Dict[str, Any]) -> None:
        self._payload = payload

    def model_dump(self) -> Dict[str, Any]:
        return self._payload


class _FakeInitialize:
    def model_dump(self) -> Dict[str, Any]:
        return {"protocolVersion": "test"}


class _FakeSession:
    async def list_tools(self, cursor: str | None = None) -> _FakeResponse:
        return _FakeResponse({"tools": [{"name": "demo"}]})

    async def call_tool(self, name: str, arguments: Dict[str, Any] | None = None, read_timeout_seconds=None):
        return _FakeResponse({"name": name, "arguments": arguments or {}})


@dataclass
class _ToolHarness:
    tool: FoundationaLLMMCPClientTool
    session: _FakeSession


def _create_tool(transport: str = "streamable_http") -> _ToolHarness:
    properties: Dict[str, Any]
    if transport == "streamable_http":
        properties = {
            "transport": "streamable_http",
            "default_operation_timeout_seconds": 1,
            "streamable_http": {
                "url": "https://example.org/mcp",
                "headers": {"X-Test": "value"},
                "timeout_seconds": 10,
                "sse_read_timeout_seconds": 60,
                "terminate_on_close": True,
            },
        }
    elif transport == "stdio":
        properties = {
            "transport": "stdio",
            "stdio": {"command": "echo", "args": ["hello"]},
        }
    else:
        properties = {
            "transport": "http_sse",
            "http_sse": {
                "url": "https://example.org/mcp",
                "headers": {},
                "timeout_seconds": 10,
                "sse_read_timeout_seconds": 60,
            },
        }

    agent_tool = AgentTool(
        name="mcp_test",
        description="Test MCP tool",
        package_name="FoundationaLLM",
        class_name="FoundationaLLMMCPClientTool",
        properties=properties,
    )

    config = _FakeConfig({"my-secret": "resolved"})
    tool = FoundationaLLMMCPClientTool(agent_tool, {}, None, config)  # type: ignore[arg-type]

    fake_session = _FakeSession()

    @asynccontextmanager
    async def fake_open_session(self, overrides: MCPTransportOverrides):
        merged_headers = {
            **properties.get("streamable_http", {}).get("headers", {}),
        }
        if overrides.headers:
            merged_headers.update(overrides.headers)
        details = MCPConnectionDetails(
            transport=MCPTransport(self._mcp_config.transport.value),
            target="https://example.org/mcp",
            session_id="session-123",
            initialize_result=_FakeInitialize(),
            transport_metadata={"headers": merged_headers},
        )
        yield fake_session, details

    tool._open_session = types.MethodType(fake_open_session, tool)
    return _ToolHarness(tool=tool, session=fake_session)


class FoundationaLLMMCPClientToolTests(unittest.IsolatedAsyncioTestCase):
    async def test_async_invocation_returns_artifact(self) -> None:
        harness = _create_tool()
        content, result = await harness.tool._arun(operation="list_tools")
        self.assertIn("demo", content)
        self.assertEqual(result.input_tokens, 0)
        self.assertEqual(len(result.content_artifacts), 1)
        artifact = result.content_artifacts[0]
        self.assertEqual(artifact.type, ContentArtifactTypeNames.TOOL_EXECUTION)
        self.assertEqual(artifact.metadata["transport"], "streamable_http")
        self.assertEqual(artifact.metadata["session_id"], "session-123")

    async def test_transport_overrides_merge_headers(self) -> None:
        harness = _create_tool()
        overrides = MCPTransportOverrides(headers={"X-Trace": "abc"})
        content, result = await harness.tool._arun(
            operation="list_tools",
            transport_overrides=overrides,
        )
        artifact = result.content_artifacts[0]
        metadata_headers = artifact.metadata["transport_metadata"]["headers"]
        self.assertIn("X-Test", metadata_headers)
        self.assertIn("X-Trace", metadata_headers)
        self.assertIn("demo", content)

    def test_sync_invocation_uses_event_loop(self) -> None:
        harness = _create_tool()
        content, result = harness.tool._run(operation="list_tools")
        self.assertIn("demo", content)
        self.assertEqual(len(result.content_artifacts), 1)

    async def test_call_tool_normalizes_timeout(self) -> None:
        harness = _create_tool()

        async def fake_call_tool(name: str, arguments=None, read_timeout_seconds=None):
            self.assertEqual(name, "ping_tool")
            self.assertIsNotNone(read_timeout_seconds)
            return _FakeResponse({"called": True})

        harness.session.call_tool = fake_call_tool  # type: ignore[assignment]
        content, _ = await harness.tool._arun(
            operation="call_tool",
            arguments={"name": "ping_tool"},
        )
        self.assertIn("called", content)


class FoundationaLLMMCPClientIntegrationTests(unittest.IsolatedAsyncioTestCase):
    @unittest.skipUnless(
        os.getenv("MSLEARN_MCP_ENDPOINT"),
        "MSLEARN_MCP_ENDPOINT environment variable is not defined.",
    )
    async def test_mslearn_list_tools(self) -> None:
        endpoint = os.environ["MSLEARN_MCP_ENDPOINT"]
        agent_tool = AgentTool(
            name="mslearn",
            description="Microsoft Learn MCP",
            package_name="FoundationaLLM",
            class_name="FoundationaLLMMCPClientTool",
            properties={
                "transport": "streamable_http",
                "streamable_http": {
                    "url": endpoint,
                    "headers": {},
                    "timeout_seconds": 30,
                    "sse_read_timeout_seconds": 300,
                    "terminate_on_close": True,
                },
            },
        )

        config = _FakeConfig()
        tool = FoundationaLLMMCPClientTool(agent_tool, {}, None, config)  # type: ignore[arg-type]
        if _is_verbose():
            print(json.dumps({
                "operation": "list_tools",
                "arguments": {}
            }, indent=2))

        content, result = await tool._arun(operation="list_tools")

        if _is_verbose():
            print(json.dumps({
                "response": json.loads(content)
            }, indent=2))
        self.assertTrue(result.content)
        payload = json.loads(content)
        self.assertIn("tools", payload)

    @unittest.skipUnless(
        os.getenv("MSLEARN_MCP_ENDPOINT"),
        "MSLEARN_MCP_ENDPOINT environment variable is not defined.",
    )
    async def test_mslearn_docs_search(self) -> None:
        endpoint = os.environ["MSLEARN_MCP_ENDPOINT"]
        agent_tool = AgentTool(
            name="mslearn",
            description="Microsoft Learn MCP",
            package_name="FoundationaLLM",
            class_name="FoundationaLLMMCPClientTool",
            properties={
                "transport": "streamable_http",
                "streamable_http": {
                    "url": endpoint,
                    "headers": {},
                    "timeout_seconds": 30,
                    "sse_read_timeout_seconds": 300,
                    "terminate_on_close": True,
                },
                "default_operation_timeout_seconds": 30,
            },
        )

        config = _FakeConfig()
        tool = FoundationaLLMMCPClientTool(agent_tool, {}, None, config)  # type: ignore[arg-type]

        # Discover available tools and assert microsoft_docs_search is present
        if _is_verbose():
            print(json.dumps({
                "operation": "list_tools",
                "arguments": {}
            }, indent=2))

        content, _ = await tool._arun(operation="list_tools")

        if _is_verbose():
            print(json.dumps({
                "response": json.loads(content)
            }, indent=2))
        tools_payload = json.loads(content)
        tool_names = {t.get("name") for t in tools_payload.get("tools", [])}
        self.assertIn("microsoft_docs_search", tool_names)

        # Invoke microsoft_docs_search with the specified query
        query = "How does Azure use Model Context Protocol (MCP)?"
        if _is_verbose():
            print(json.dumps({
                "operation": "call_tool",
                "arguments": {
                    "name": "microsoft_docs_search",
                    "arguments": {"query": query},
                    "read_timeout_seconds": 30,
                }
            }, indent=2))

        content, _ = await tool._arun(
            operation="call_tool",
            arguments={
                "name": "microsoft_docs_search",
                "arguments": {"query": query},
                "read_timeout_seconds": 30,
            },
        )

        if _is_verbose():
            print(json.dumps({
                "response": json.loads(content)
            }, indent=2))

        payload = json.loads(content)
        self.assertTrue(isinstance(payload, dict))
        has_content_list = isinstance(payload.get("content"), list) and len(payload["content"]) > 0
        has_results_like = any(k in payload for k in ("results", "items", "documents"))
        self.assertTrue(has_content_list or has_results_like)

