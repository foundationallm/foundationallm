# FoundationaLLM Agent MCP Server

This package hosts a FastMCP server that exposes a curated set of tools for creating and configuring FoundationaLLM agents through the Management API. It wraps the same routes that the Management and User portals rely on so you can script agent creation, private store management, and access-token workflows directly from your MCP-enabled IDE.

## Features

- Create agents from the `BasicAgentTemplate`, including automatic name generation that mirrors the portal.
- Upload, list, and delete private store resources plus toggle which server-side tools (Code Interpreter, Knowledge Tool, etc.) can consume each file.
- Enable or disable end-user file uploads for an agent.
- Manage agent access tokens (list, create, revoke) and retrieve the generated key material in one step.
- Enumerate dependencies such as prompts, AI models, workflows, vectorization assets, and data sources to simplify downstream configuration.

## Getting Started

1. **Install dependencies**

   ```bash
   cd src/python/FastMCPAgentServer
   pip install -e .
   ```

2. **Configure environment variables**

   | Variable | Description |
   | --- | --- |
   | `FOUNDATIONALLM_MANAGEMENT_ENDPOINT` | Base URL to the FoundationaLLM Management API (e.g. `https://contoso-api.azurewebsites.net/management`). |
   | `FOUNDATIONALLM_MANAGEMENT_SCOPE` | The AAD scope used to request tokens for the management API. |
   | `FOUNDATIONALLM_INSTANCE_ID` | The target FoundationaLLM instance GUID. |
   | `FOUNDATIONALLM_AUTH_MODE` | Optional. `default`, `cli`, or `managed_identity`. Defaults to `default` (Azure `DefaultAzureCredential`). |
   | `FOUNDATIONALLM_TENANT_ID` | Optional tenant hint passed to Azure Identity credentials. |
   | `FOUNDATIONALLM_VERIFY_SSL` | Optional. Set to `false` for self-signed dev environments. |
   | `FOUNDATIONALLM_MANAGEMENT_API_VERSION` | Optional. Defaults to `2024-02-16`. |

   You can place these values in a local `.env` file that sits next to the `pyproject.toml`.

3. **Run the server**

   ```bash
   cd src/python/FastMCPAgentServer
   python -m foundationallm_agent_mcp
   ```

   When the server starts it will advertise the available MCP tools. Connect to it from Cursor, VS Code, or any MCP-capable client using the standard transport options (stdio, WebSocket, etc.) exposed by FastMCP.

## Repository Layout

```
src/python/FastMCPAgentServer/
├── foundationallm_agent_mcp/
│   ├── __init__.py
│   ├── auth.py
│   ├── config.py
│   ├── management_client.py
│   ├── models.py
│   └── server.py
├── pyproject.toml
└── README.md
```

`server.py` wires the tools into FastMCP. `management_client.py` contains the HTTP client that talks to the FoundationaLLM Management API, while `models.py` defines the strongly-typed request payloads each MCP tool expects.

## Development Notes

- The project targets Python 3.12 to stay aligned with the rest of the repository.
- `fastmcp>=2.13.1` is required because the server relies on the latest Context helpers and tool metadata features.
- If you need to debug HTTP traffic, set `FOUNDATIONALLM_LOG_HTTP=true` (see `config.py`), then re-run the server to emit request/response summaries through FastMCP’s logging pipeline.

## Next Steps

Future enhancements could include:

- Support for additional agent templates once they become self-service ready.
- Helper tools for updating orchestrator settings or LangChain workflows.
- Automated validation that cross-checks required vectorization resources before submitting a create/update call.

