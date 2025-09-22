# Creating custom tools

This guide walks through the end-to-end workflow for building a new FoundationaLLM tool: from understanding the shipped examples, to iterating locally with the test harness, debugging an invocation, and finally packaging the plugin so it can be uploaded to the `external-modules-python` storage container for use in the platform.

## Reference implementations

Use the built-in tools as a starting point while you design your own implementation. They all live under `src/python/FoundationaLLMAgentPlugins/src/foundationallm_agent_plugins/tools/`.

- `foundationallm_nop_tool.py` is the most compact example. It defines a minimal `FoundationaLLMToolBase` subclass and demonstrates how to log the original prompt via `RunnableConfig` metadata before returning a fixed response.
- `foundationallm_sql_tool.py` shows how to translate natural language instructions into SQL queries. Review how it validates arguments, builds the SQL request, and records any produced artifacts.
- `foundationallm_code_interpreter_tool.py` illustrates an advanced, async-only tool that orchestrates Azure Dynamic Sessions or custom containers. The module highlights how to require a Context API endpoint, create an HTTP client, manage session metadata, and emit `ContentArtifact` outputs that include generated files.

When you add a new tool module, register it in two places so the plugin manager can resolve it:

1. Export the class in `tools/__init__.py`.
2. Extend the `match` expression inside `foundationallm_agent_tool_plugin_manager.py` to return your tool when its `AgentTool.class_name` is requested.

## Prerequisites

- A FoundationaLLM deployment with access to the Chat Portal and the backing Azure Storage account.
- Visual Studio Code (recommended) with the repo cloned locally and the `FoundationaLLMAgentPlugins` virtual environment available.
- The App Configuration URI for your environment so the workflow can load secrets while you debug.

## Prepare the local test harness

The repository includes a workflow harness that replays captured conversations without needing to run the entire platform.

1. **Create a throwaway conversation.** In the Chat Portal, open the agent you want to target, start a new conversation, and send an initial message (for example, "Who are you?"). The URL will contain a `?chat=<conversation-id>` query string value—copy the ID.
2. **Update the unit test conversation ID.** Open `src/python/FoundationaLLMAgentPlugins/test/foundationallm_function_calling_workflow/test_workflow.py` and locate the `user_identity_json` definition. Replace the identity fields with your account information and keep the object handy—you will reuse it.
3. **Download the conversation payload.** In the Azure portal, navigate to the storage account that backs FoundationaLLM. Browse to **Containers** → `orchestration-completion-requests` → your user ID → the most recent folder. Open the file that ends with `completion-request-OUT.json`, format it, then copy the entire JSON body.
4. **Populate the harness input file.** Overwrite `src/python/FoundationaLLMAgentPlugins/test/full_request.json` with the JSON you copied. This file drives the test harness and contains the `session_id`, `user_prompt`, `objects`, and agent/tool definitions that will be replayed.
5. **(Optional) Update the prompt override.** If you have a bespoke prompt file you want to test, create or edit `src/python/FoundationaLLMAgentPlugins/test/main_prompt.txt`. When present, the harness will inject its contents into the replayed workflow.

The harness now mirrors a real conversation. Each time you capture a new run from the platform, replace `full_request.json` and adjust `user_identity_json` so the workflow executes with your credentials.

## Debug a built-in tool

Debugging an existing tool ensures your environment is configured correctly before you start authoring new code.

1. Open `test_workflow.py` and set `user_identity_json` to your details. You can also modify `full_request.json` so the `user_prompt` instructs the agent to use the tool you want to observe (for example, request a quick calculation to exercise the Code Interpreter).
2. In `.vscode/launch.json`, locate the `FoundationaLLMAgentPlugins - Debug Workflow` configuration. Add an `env` block if it is missing and specify your App Configuration endpoint:
   ```json
   "env": {
       "FOUNDATIONALLM_APP_CONFIGURATION_URI": "https://<your-app-configuration>.azconfig.io"
   }
   ```
3. Launch the configuration from the VS Code Run and Debug panel.
4. Set a breakpoint inside `src/python/FoundationaLLMAgentPlugins/src/foundationallm_agent_plugins/workflows/foundationallm_function_calling_workflow.py` on the line that checks `if llm_response.tool_calls:` within `FoundationaLLMFunctionCallingWorkflow.invoke_async`.
5. Trigger debugging. When the breakpoint hits, inspect `llm_response.tool_calls` to confirm which tool will run, and step through the invocation to observe argument preparation, content artifact creation, and any HTTP calls.

If debugging fails to start, verify that VS Code is using the virtual environment at `src/python/FoundationaLLMAgentPlugins/.venv/` and that the paths in the launch configuration match your operating system.

## Build your custom tool

1. **Create a new module.** Copy the NOP or SQL tool into a new file under `tools/` and rename the class. Update docstrings and logging to reflect your scenario.
2. **Define the input schema.** If your tool takes structured inputs, create a `pydantic.BaseModel` subclass (see `foundationallm_code_interpreter_tool_input.py` for an example) and assign it to `args_schema` so LangChain validates arguments automatically.
3. **Implement the async handler.** Override `_arun` to pull any metadata from `runnable_config['configurable']`. Use keys from `foundationallm.models.constants.RunnableConfigKeys` to access the original user prompt or uploaded files. Raise a `ToolException` from `langchain_core.tools` if required data is missing.
4. **Return results and artifacts.** Build and return a `FoundationaLLMToolResult` when you need to pass rich output upstream. To surface files or structured data back to the agent, append `ContentArtifact` objects (for example, the Code Interpreter tool emits both execution summaries and generated files).
5. **Register the tool.** Export your class in `tools/__init__.py`, add a constant to `FoundationaLLMAgentToolPluginManager`, and extend the `match` statement in `create_tool` so the manager instantiates your implementation.
6. **Cover it with tests.** Duplicate one of the existing tests under `src/python/FoundationaLLMAgentPlugins/test/` and adapt it to exercise your tool. Use the plugin manager to instantiate the class and assert on the returned content and artifacts.

## Package and publish the plugin

1. From `src/python/FoundationaLLMAgentPlugins/`, run the packaging script with a semantic version and configuration:
   ```bash
   python package/package.py -v <major.minor.patch> -c Debug
   ```
   Use `Debug` while iterating; switch to `Release` when you are ready to distribute optimized bytecode.
2. The script injects the version into `src/foundationallm_agent_plugins/foundationallm_manifest.json` and produces a ZIP archive in the same directory (`foundationallm_agent_plugins_debug-<version>.zip` or `foundationallm_agent_plugins-<version>.zip`).
3. Upload the ZIP to the `external-modules-python` container in your deployment’s storage account. If management tooling requires it, register the package so agents can reference the new class name.

## Next steps and additional resources

- Review `docs/development/development-local.md` for environment setup tips.
- Explore other tools (KQL, file analysis, knowledge search) to see how they query platform APIs and handle file metadata.
- Follow the guidance in `docs/development/contributing/style-guide.md` before submitting a pull request.

With these steps complete, you can confidently author, test, and ship custom FoundationaLLM tools that integrate seamlessly with the platform.
