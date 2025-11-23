# FoundationaLLM 0.9.7 Release Notes

## What's New

This release introduces several major features that enhance the FoundationaLLM experience for end users:

* **End User Agent Selection**: An enhanced Settings dialog allows you to pick and choose which agents are displayed in your agent dropdown, giving you control over your agent list and enabling you to customize your workspace to show only the agents you use most frequently.

* **Featured Agents in Chat Portal**: A new "Featured agents" section in the agent dropdown enables highlighting new agents that users should try, making it easier to discover and explore new capabilities.

* **Model-Agnostic Agents with Agent Parity**: The new Model Agnostic Agents enables agents and tools to use different models (GPT, Claude, Gemini) as best suited to each task. Three new agents have been built with agent parity in mind, ensuring consistent behavior across models. Capabilities not natively supported by one model are now enabled consistently for all.

* **Data Pipelines**: Data pipelines provide powerful capabilities for processing and indexing files across FoundationaLLM. In the front end, data pipelines automatically process end user file uploads, extracting content, generating embeddings, and making files searchable for your agents. On the backend, data pipelines handle files private to specific agents, ensuring secure and isolated processing. Additionally, data pipelines enable the ingestion and indexing of large document collections, making it possible to build comprehensive knowledge bases from thousands or millions of documents efficiently and at scale.

* **Prompt Injection Detection for Knowledge Sources**: If this shield is enabled for the data pipeline used by your agent, whenever you or a user upload a file, it will be automatically scanned for malicious prompt injections. If a prompt injection is detected, you will receive an error message and the file will not be uploaded, indexed, or available for subsequent conversations. This acts like an "antivirus" for knowledge sources, protecting your agents from malicious content embedded in files.

* **Enhanced Code Interpreter Tool**: The code interpreter has been significantly improved with structured data handling (automatic previews of pandas DataFrames), a protocol that guides LLM code generation to produce results of interest, and improved error handling that provides clear feedback to users (e.g., when images exceed available memory in the sandbox).

* **Multiple File Uploads**: You can now upload up to 10 documents at a time with a single request, making it faster and more convenient to share multiple files with your agent. Later, you can ask the agent to tell you what files are available in your conversation.

* **Artifacts improvements**: Improved artifact display and organization for better visibility into tool execution details make it easier to understand how the agent processed your request. For example, the code interpreter tool's Code artifact now makes it easier to read both the code that was generated and input to the tool, as well as the code that was actually executed by the tool

## Enhancements and Features

### What's New in the User Portal

* **End user agent selection**: A new Settings dialog allows you to pick and choose which agents are displayed in your agent dropdown. This gives you control over your agent list, enabling you to customize your workspace to show only the agents you use most frequently and hide those you don't need.

* **Featured agents section**: New "Featured agents" section in the agent dropdown that enables highlighting new agents that users should try. This helps users discover and try new agents more easily, improving agent adoption and exploration.
* **Better agent discovery & navigation**: 
  * Filters by name for quick agent search
  * Enabled-only view to focus on active agents
  * Featured agents prominently displayed
  * Persist selection across sessions
  * Correct returns to agent lists after navigation
* **Redesigned prompt area**: Consistent branding and improved editors in the User Portal for a more polished experience.
* **Multimodal content support**: 
  * Files embedded into completion requests: Files from conversation history can be embedded directly into completion requests with proper content type detection and handling
  * Direct image sending: Image files are automatically detected and sent with proper base64 encoding and MIME type handling
  * Audio content blocks support: Support for WAV and MP3 audio files in completion requests with dedicated audio analysis service
* **Multiple file uploads**: Support for uploading up to 10 documents at a time with a single request, making it faster and more convenient to share multiple files with your agent. You can later ask the agent to tell you what files are available in your conversation.
* **File handling improvements**:
  * File names are enclosed in `/` delimiters to prevent misinterpretation (e.g., files starting with numbers like `1.`, `2.` won't be treated as numbered lists)
  * Context file embedding: Files from conversation history can be embedded directly into completion requests with proper content type detection
  * Multimodal file support: Automatic detection and handling of image files (base64 encoding), audio files (WAV, MP3 with proper MIME types), and text files in context messages
  * File path normalization: Improved handling of file paths in annotations and message content

* **Artifacts improvements**: 
  * Improved artifact display and organization for better visibility into tool execution details make it easier to understand how the agent processed your request. For example, the code interpreter tool's Code artifact now makes it easier to read both the code that was generated and input to the tool, as well as the code that was actually executed by the tool
  * 

* **Accessibility & reliability**: Numerous accessibility improvements including keyboard-only actions, Safari button states, consistent pop-up timeouts, visible scrollbars.

### What's New in Terms of Agents

* **Model-Agnostic Agents**:
  * **FoundationaLLMFunctionCallingWorkflow**: New workflow enabling agents and tools to use different models as best suited to the problem. This model-agnostic approach allows optimal model selection per task.
  * **Agent Parity**: Three new agents have been built with agent parity in mind, ensuring that the behavior of an agent using GPT is similar to an agent built with Claude, which is similar to Gemini. This consistency provides a uniform experience regardless of the underlying model.
  * **Tool-Based Capability Enablement**: Capabilities that might not be supported natively by one model are now enabled consistently for all through the agentic use of tools. Examples include file processing and generation, which work uniformly across all model types.
* **Code Interpreter Tool Enhancements**:
  * **Structured data handling with previews**: Automatic detection and handling of pandas DataFrames and Series. DataFrames are auto-saved to CSV with preview generation (first 10 rows) returned as structured metadata. Series are converted to dictionaries for serialization, making it easy to work with structured data in code execution results.
  * **Protocol for guiding LLM code generation**: AST-based variable extraction from the last line of code (`_extract_last_variable_names` function) guides the LLM to structure code that returns results as variables in the last line. This protocol supports single variables, tuples, and assignments, enabling the LLM to generate code that produces results of interest more reliably.
  * **Enhanced error handling and resilience**: Comprehensive try-except blocks catch container crashes, timeouts, and memory limit errors. Error details are captured and provided to the agent for user communication (e.g., "image exceeds available memory in sandbox"). Errors within the code interpreter are conveyed to the agent for summary to the user, providing clear feedback on execution issues.
  * **File management improvements**: Upload/download capabilities, automatic detection of newly generated files by comparing before/after file lists, comprehensive file metadata tracking including file object IDs, original file names, file paths, sizes, content types, and conversation IDs.
  * **Code preparation**: Automatic markdown code block stripping via `__prepare_code` method ensures clean code execution.
  * **Session management**: Persistent code sessions with dynamic session IDs from runnable_config, enabling stateful code execution across multiple tool invocations.
  * **Comprehensive artifact generation**: Detailed metadata including original prompts, generated code, tool output, errors, and results, enabling full traceability of code execution.
  * **Token usage tracking**: Input/output token tracking for monitoring and cost analysis across code generation and execution.

### What's New in Backend Improvements

* **Data Pipelines**:
  * **Filtering, re-run/testing, state persistence**, duration metrics, and performance optimization 
  * Better **artifact creation**, content item listing, retrieval, and metadata handling (extraction, serialization, configurable property names, array filters)
  * Indexing reliability (fix repeated runs, payload limits, refresh & failed stage handling) and Azure AI Search handling/logging
  * **Prompt Injection Detection (Antivirus for Knowledge Sources)**: 
    * Azure AI Content Safety Shielding Data Pipeline Stage Plugin automatically scans files during pipeline processing to detect file-based indirect prompt injection attacks
    * Acts as an antivirus for knowledge sources, protecting against malicious content embedded in uploaded files
    * Automatic scanning of content parts during pipeline processing
    * Unsafe content detection with detailed logging and reporting
    * Notification to data source plugins when prompt injection is detected, allowing them to handle unsafe content appropriately
    * Content part shielding marks safe content parts for downstream processing
    * Prevents malicious prompts from being injected into the knowledge base through file uploads

* **Code Sessions & Execution**:
  * **Standardized JSON response schema**: Consistent response format across all code execution endpoints
  * **Enhanced error reporting**: Better error messages for timeout scenarios, gateway timeouts, and container crashes
  * **Container crash detection**: Improved handling and reporting when custom containers crash or become unresponsive
  * **Extended HTTP timeouts**: Longer timeout values for complex code execution scenarios
  * **Pandas DataFrame/Series auto-serialization**: Automatic CSV export and metadata generation for pandas objects in code results
* **Gateway API**: Optimized embeddings, limited retries, better timeouts, richer logging, race condition fixes. Execute completions via Gateway API with model parameters added; support for new **tokenizers** and sparse text chunk positions.
* **Context API & Orchestration**: 
  * Deserialization fixes, dependency injection corrections, improved logging
  * JSON handling improvements
  * Encoding detection/standardization (force UTF-8 when needed)
  * Conversation/file history correctness
  * Message history conversion: Automatic conversion between FoundationaLLM message history format and LangChain message types
  * Context file message construction: Intelligent construction of multimodal messages with proper content type handling for images, audio, and text
* **Azure AI Search**: Safer payload sizes, filter expression logging, vector store filter fixes, query corrections, index update handling.
* **Developer Tooling & Extensibility**:
  * **Python/.NET plugins** support (wheel loading, plugin base classes published), **PowerShell module** broadened (agents, prompts, conversations, vector DBs), and **Azure OpenAI & Cosmos DB CLIs**
  * **Databricks** added as a model provider and a dedicated **Databricks agent tool**
  * **Agent Evaluations**:
    * Comprehensive test framework: Automated test execution, validation, and result management for FoundationaLLM agents
    * Multiple validation modes: Rule-based validation (pattern matching, numeric validation, artifact checks), LLM-based validation (semantic similarity), and hybrid approaches
    * Test generation capabilities: Automated test expansion with multiple strategies (variations, edge cases, negative tests, combinations)
    * Interactive test suite creation: Interactive mode for creating and managing test suites with multiline input support
    * Rich reporting: HTML dashboards with detailed analysis, CSV/JSON outputs, and performance metrics
    * Cross-agent comparison: Support for running the same test suite across multiple agents for comparison
    * Reliability testing: Built-in support for repeating tests multiple times to assess consistency
    * Encoding/content type fixes, dynamic loading fixes, standardized schemas for code session results
* **Management Portal improvements**: Redesigned prompt area with consistent branding and improved editors. Management endpoints for **restricted operations**, **role definitions**, and **resource role assignments**; instance identifier required in resource paths. Extensive **UI/UX** work in Settings and creation/edit flows (progress patterns, disabled states, read-only handling, enabled filters, featured agents, spinners, tooltip/accessibility fixes).

## Improvements

### Security & Authorization

* Stronger **authorization checks** across resource providers, clearer exception messages with resource paths, ignore optional role when parent is used, improved identity management (on‑premises names, UPN casing), and role assignments loaded & filtered for current user.

### Code Sessions & Execution

* **Standardized JSON response schema**: Consistent response format across all code execution endpoints
* **Enhanced error reporting**: Better error messages for timeout scenarios, gateway timeouts, and container crashes
* **Container crash detection**: Improved handling and reporting when custom containers crash or become unresponsive
* **Extended HTTP timeouts**: Longer timeout values for complex code execution scenarios
* **Pandas DataFrame/Series auto-serialization**: Automatic CSV export and metadata generation for pandas objects in code results
* Longer HTTP timeouts, improved error reporting for custom containers, and JSON error handling improvements

### Developer Experience & CI/CD

* Release workflow updates (RC tags, versioning fixes), removal of deprecated references, improved OpenTelemetry source alignment, and simplified internal deployment flows.
* SDK/clients updated to **allow empty response collections**, correct headers, enforce filename resolution, and expose missing properties.

### Workflow Execution & Content Artifacts

* **Execution metrics**: Workflow execution time tracking and token usage aggregation across router and final LLM calls
* **Content artifact generation**: Automatic creation of workflow execution artifacts with timing and token information
* **Intermediate response tracking**: Captures intermediate tool responses for final response generation
* **Tool not found handling**: Graceful error handling when a tool specified in tool_calls is not found in the tools list
* **Empty response handling**: SDK/clients updated to allow empty response collections without errors

### User Interface & Usability Fixes

* Numerous fixes across portals: avatar rendering/clipping, welcome message HTML, inconsistent branding at login, disabled states, tooltips association, "no agents" messaging, delete confirmations, edit icon behavior, and keyboard‑only deletion.
* Consistent progress indicators for lists/create/edit, stronger pencil icon visibility, filtered dropdowns, empty states, and correct handling when no agent is selected or after refresh.

## Contact Information

For support and further inquiries regarding this release, please reach out to us:

* **Support Contact:** https://foundationallm.ai/contact
* **Website:** FoundationalLLM

## Conclusion

We hope you enjoy the new features and improvements in FoundationalLLM version 0.9.7. Your feedback continues to be instrumental in driving our product forward. Thank you for your continued support.

