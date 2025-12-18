# FoundationaLLM Copilot Instructions

## Project Overview

FoundationaLLM is an enterprise platform for deploying, scaling, securing, and governing generative AI agents. It's a multi-language monorepo with .NET 8 backend APIs, Python orchestration services, and Vue.js (Nuxt 3) frontends.

## Architecture

### Request Flow
User Portal → **CoreAPI** → **GatekeeperAPI** (security/filtering) → **OrchestrationAPI** → Orchestrator (**LangChainAPI**/SemanticKernelAPI) → LLM

### Key Components
- **src/dotnet/**: .NET 8 APIs and core services
  - `CoreAPI`: Main user-facing API, handles completions and conversations
  - `OrchestrationAPI`: Routes requests to appropriate orchestrators
  - `GatekeeperAPI`: Security layer with content filtering
  - `ManagementAPI`: Admin operations for agents, prompts, data sources
  - `Common/`: Shared interfaces, models, constants (referenced by all projects)
  - `Core/Services/CoreService.cs`: Central service coordinating downstream APIs
- **src/python/**: Python SDK and orchestration
  - `LangChainAPI`: FastAPI service for LangChain-based orchestration
  - `PythonSDK/foundationallm/`: Shared Python library (config, models, plugins)
  - `plugins/agent_*`: Agent plugin packages (core, langchain, azure_ai, experimental)
- **src/ui/**: Vue.js Nuxt 3 portals (UserPortal, ManagementPortal)

### Resource Provider Pattern
All domain entities are managed through resource providers (`IResourceProviderService`). Each provider handles CRUD for specific resource types:
- `FoundationaLLM.Agent`, `FoundationaLLM.Prompt`, `FoundationaLLM.Configuration`
- `FoundationaLLM.AIModel`, `FoundationaLLM.DataSource`, `FoundationaLLM.Plugin`
- See `Common/Constants/ResourceProviders/ResourceProviderNames.cs` for the full list

## Development Workflow

### Prerequisites
```powershell
# Required environment variables (system level)
$env:FoundationaLLM_AppConfig_ConnectionString = "<App Configuration connection string>"  # .NET
$env:FOUNDATIONALLM_APP_CONFIGURATION_URI = "<App Configuration URI>"                      # Python
$env:FOUNDATIONALLM_VERSION = "0.x.x"                                                      # Version validation
```

### Building & Running
```powershell
# .NET - Open src/FoundationaLLM.sln in Visual Studio 2022 17.8+
cd src && dotnet build

# Python SDK - install in development mode
cd src/python/PythonSDK && pip install -e .

# Python APIs - FastAPI with uvicorn
cd src/python/LangChainAPI && pip install -r requirements.txt && python run.py

# UI Portals - Nuxt 3
cd src/ui/UserPortal && npm install && npm run dev  # http://localhost:3000
```

### Testing
```powershell
# .NET tests
cd tests/dotnet && dotnet test

# Python tests
cd tests/python && pytest
```

## Coding Conventions

### .NET
- Namespace pattern: `FoundationaLLM.{Component}.{Area}` (e.g., `FoundationaLLM.Core.Services`)
- Use dependency injection via constructor; services registered in `DependencyInjection.cs` per project
- Interfaces in `Interfaces/` folders; implementations in `Services/`
- API controllers use `[Route("instances/{instanceId}")]` prefix for multi-tenant support
- Async methods throughout; use `CompletionRequest`/`CompletionResponse` for orchestration

### Python
- Package: `foundationallm` for SDK, `foundationallm_agent_plugins` for agent plugins
- Use `Configuration` class for app config access, `Telemetry` for logging/tracing
- FastAPI routers in `app/routers/`; use `@router.post` with OpenAPI metadata
- Models in `foundationallm/models/` mirror .NET types for API compatibility

### Plugins
- Naming: `{Platform}-{PackageName}-{PluginName}` (e.g., `Python-AgentCore-FunctionCalling`)
- Plugin metadata in `_metadata/foundationallm_manifest.json`
- See `docs/concepts/plugin/plugin.md` for parameter types and dependency patterns

## Key Files to Reference

| Purpose | Path |
|---------|------|
| Resource provider names | `src/dotnet/Common/Constants/ResourceProviders/ResourceProviderNames.cs` |
| Core service logic | `src/dotnet/Core/Services/CoreService.cs` |
| Orchestration flow | `src/dotnet/Orchestration/Orchestration/OrchestrationBuilder.cs` |
| Python completions endpoint | `src/python/LangChainAPI/app/routers/completions.py` |
| Plugin manager | `src/python/PythonSDK/foundationallm/plugins/plugin_manager.py` |
| Shared constants | `src/dotnet/Common/Constants/` |

## Trunk-Based Development

- Branch from `main` for features/fixes; merge via PR within same day when possible
- Release branches: `release/n.n.n` - created at sprint end
- No direct commits to `main`; all changes through PRs with code review
- Cherry-pick critical fixes to active release branch after merging to `main`
