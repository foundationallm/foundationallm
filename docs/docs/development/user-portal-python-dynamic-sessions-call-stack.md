# User Portal to Python Dynamic Sessions call stack

## Purpose
Document the async completion path from the User Portal to the Python Dynamic Sessions invocation and list the services and configuration needed to debug every step locally.

## End-to-end call stack (async completions)
1. User Portal posts `/instances/{instanceId}/async-completions` and polls `/async-completions/{operationId}/status` for results.【F:src/ui/UserPortal/js/api.ts†L190-L214】【F:src/ui/UserPortal/js/api.ts†L330-L350】
2. Core API accepts `/async-completions` and calls `CoreService.StartCompletionOperation`.【F:src/dotnet/CoreAPI/Controllers/CompletionsController.cs†L116-L157】
3. Core Service routes the request to Gatekeeper API or Orchestration API based on gatekeeper settings (bypass or required).【F:src/dotnet/Core/Services/CoreService.cs†L227-L277】【F:src/dotnet/Core/Services/CoreService.cs†L1149-L1153】
4. Gatekeeper API (if enabled) or Orchestration API exposes `/async-completions` for downstream processing.【F:src/dotnet/GatekeeperAPI/Controllers/CompletionsController.cs†L31-L46】【F:src/dotnet/OrchestrationAPI/Controllers/CompletionsController.cs†L34-L48】
5. Orchestration selects the LangChain orchestrator for generic agents unless a workflow host overrides it.【F:src/dotnet/Orchestration/Orchestration/OrchestrationBuilder.cs†L97-L112】
6. During orchestration, tools marked `code_session_required` trigger a Context API call to create a code session, and the resulting `code_session_endpoint` and `code_session_id` are injected into tool parameters.【F:src/dotnet/Orchestration/Orchestration/OrchestrationBuilder.cs†L376-L427】【F:src/dotnet/Common/Constants/Agents/AgentToolPropertyNames.cs†L6-L25】
7. LangChain API accepts `/async-completions` and invokes its orchestration manager to run the workflow and tools.【F:src/python/LangChainAPI/app/routers/completions.py†L45-L83】【F:src/python/LangChainAPI/app/routers/completions.py†L142-L156】
8. The Code Interpreter tool calls Context API to upload files, execute code, and download generated files for the session.【F:src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_code_interpreter_tool.py†L96-L170】
9. Context API exposes `/codeSessions` endpoints and delegates to `CodeSessionService` for creation and execution operations.【F:src/dotnet/ContextAPI/Controllers/CodeSessionsController.cs†L28-L45】【F:src/dotnet/ContextAPI/Controllers/CodeSessionsController.cs†L93-L113】
10. `CodeSessionService` chooses the code session provider and persists the session record in Cosmos DB.【F:src/dotnet/ContextEngine/Services/CodeSessionService.cs†L35-L93】
11. For Python Dynamic Sessions, `AzureContainerAppsCodeInterpreterService` reads Python endpoints from configuration and calls the Dynamic Sessions execution endpoint (`/executions`).【F:src/dotnet/ContextEngine/Services/AzureContainerAppsCodeInterpreterService.cs†L30-L53】【F:src/dotnet/ContextEngine/Services/AzureContainerAppsCodeInterpreterService.cs†L141-L152】
12. Dynamic Sessions calls are authenticated with an Azure token for `https://dynamicsessions.io/.default`.【F:src/dotnet/ContextEngine/Services/AzureContainerAppsServiceBase.cs†L54-L64】

## Services to run locally for full-stack debugging
Run these services to step through the full call stack. Use the optional services when you need to debug those branches.

| Service | Repo path | Why it matters | Required |
| --- | --- | --- | --- |
| User Portal | `src/ui/UserPortal` | Sends async completion requests and polls results.【F:src/ui/UserPortal/js/api.ts†L190-L214】【F:src/ui/UserPortal/js/api.ts†L330-L350】 | Yes |
| Core API | `src/dotnet/CoreAPI` | Entry point for `/async-completions` and routing to downstream APIs.【F:src/dotnet/CoreAPI/Controllers/CompletionsController.cs†L116-L157】【F:src/dotnet/Core/Services/CoreService.cs†L227-L277】 | Yes |
| Gatekeeper API | `src/dotnet/GatekeeperAPI` | Optional gating layer for completions.【F:src/dotnet/GatekeeperAPI/Controllers/CompletionsController.cs†L31-L46】 | Optional |
| Orchestration API | `src/dotnet/OrchestrationAPI` | Dispatches to LangChain and builds tool orchestration.【F:src/dotnet/OrchestrationAPI/Controllers/CompletionsController.cs†L34-L48】【F:src/dotnet/Orchestration/Orchestration/OrchestrationBuilder.cs†L376-L427】 | Yes |
| LangChain API | `src/python/LangChainAPI` | Executes agent workflow and tool calls (including Code Interpreter).【F:src/python/LangChainAPI/app/routers/completions.py†L45-L83】【F:src/python/LangChainAPI/app/routers/completions.py†L142-L156】 | Yes |
| State API | `src/dotnet/StateAPI` | LangChain service polls operation status via State API.【F:src/dotnet/Orchestration/Services/LangChainService.cs†L145-L162】 | Yes for async |
| Context API | `src/dotnet/ContextAPI` | Creates code sessions and executes code via provider services.【F:src/dotnet/ContextAPI/Controllers/CodeSessionsController.cs†L28-L45】【F:src/dotnet/ContextEngine/Services/CodeSessionService.cs†L35-L93】 | Yes |
| Python Code Session API | `src/python/PythonCodeSessionAPI` | Only needed if you switch to the custom container provider for local sessions.【F:src/dotnet/ContextEngine/Constants/CodeSessionProviderNames.cs†L6-L16】【F:src/dotnet/Common/Templates/appconfig.template.json†L94-L109】 | Optional |
| Azure Container Apps Dynamic Sessions | External | Executes Python code when using the code interpreter provider.【F:src/dotnet/ContextEngine/Services/AzureContainerAppsCodeInterpreterService.cs†L141-L152】 | Required for dynamic sessions |

## Local configuration checklist

### Environment variables
- Set `FoundationaLLM_AppConfig_ConnectionString`, `FOUNDATIONALLM_APP_CONFIGURATION_URI`, and `FOUNDATIONALLM_VERSION` as described in the local dev prerequisites.【F:docs/docs/development/development-local.md†L3-L9】
- For Python services, set `FOUNDATIONALLM_ENV=dev` when running State API locally to allow non-SSL local calls.【F:docs/docs/development/development-local.md†L336-L348】

### User Portal
- Create `.env` from `.env.example` and set `APP_CONFIG_ENDPOINT` and (optionally) `LOCAL_API_URL` to the local Core API URL (`https://localhost:63279`).【F:docs/docs/development/development-local.md†L46-L51】

### Core API
- In `appsettings.Development.json`, point `GatekeeperAPI` and `OrchestrationAPI` to local URLs (defaults are `https://localhost:7180/` and `https://localhost:7324/`).【F:docs/docs/development/development-local.md†L88-L100】

### Gatekeeper API (if enabled)
- In `appsettings.Development.json`, set local `OrchestrationAPI` and `GatekeeperIntegrationAPI` URLs (defaults are `https://localhost:7324/` and `http://localhost:8042/`).【F:docs/docs/development/development-local.md†L172-L183】

### Orchestration API
- In `appsettings.Development.json`, set local API URLs for LangChain and related services (LangChain default: `http://localhost:8765/`).【F:docs/docs/development/development-local.md†L218-L238】

### Code session provider settings (Dynamic Sessions vs. local custom container)
- Set the App Configuration key `FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CodeInterpreter` to include Python endpoints for Dynamic Sessions (JSON format).【F:src/dotnet/Common/Templates/appconfig.template.json†L94-L100】
- If you want local execution instead of Azure Dynamic Sessions, set `FoundationaLLM:Code:CodeExecution:AzureContainerAppsDynamicSessions:CustomContainer` and point it to your local Python Code Session API endpoint, then use the custom container provider in agent tools.【F:src/dotnet/Common/Templates/appconfig.template.json†L103-L109】【F:src/dotnet/ContextEngine/Constants/CodeSessionProviderNames.cs†L6-L16】

### Agent tool configuration
- Tools that need Python Dynamic Sessions must specify:
  - `code_session_required: true`
  - `code_session_endpoint_provider: AzureContainerAppsCodeInterpreter`
  - `code_session_language: Python`
These keys and provider names are defined in the agent tool constants.【F:src/dotnet/Common/Constants/Agents/AgentToolPropertyNames.cs†L6-L21】【F:src/dotnet/ContextEngine/Constants/CodeSessionProviderNames.cs†L6-L16】

### Azure authentication for Dynamic Sessions
- Local Dynamic Sessions calls rely on Azure CLI auth (`AzureCliCredential`) and request tokens for `dynamicsessions.io`. Ensure you are logged in via Azure CLI and have access to the Dynamic Sessions resource.【F:src/dotnet/Common/Authentication/ServiceContext.cs†L25-L35】【F:src/dotnet/ContextEngine/Services/AzureContainerAppsServiceBase.cs†L54-L64】
