# Managing Plugins

Learn how to manage plugins and plugin packages to extend FoundationaLLM functionality.

## Overview

Plugins extend FoundationaLLM with custom capabilities:

- **Custom Workflows**: Agent orchestration logic
- **Custom Tools**: Specialized agent capabilities
- **Data Processing**: Custom pipeline stages and extractors

## Plugin Architecture

### Plugin Types

| Type | Language | Purpose |
|------|----------|---------|
| **Agent Workflow** | Python | Custom orchestration workflows |
| **Agent Tool** | Python | Custom tools for agents |
| **Data Source** | .NET | Custom data source connectors |
| **Pipeline Stage** | .NET | Custom data pipeline processing |
| **Text Extraction** | .NET | Custom content extraction |
| **Text Partitioning** | .NET | Custom text chunking |

### Plugin Packages

Plugins are distributed as packages:

| Package Type | Format | Platform |
|--------------|--------|----------|
| **Python** | ZIP (Wheel planned) | Agent workflows and tools |
| **.NET** | NuGet | Data pipeline components |

## Managing Plugin Packages

> **TODO**: Document the specific UI for plugin management in the Management Portal when available.

### Viewing Available Plugins

1. Navigate to plugin management (if available in UI)
2. View list of installed packages
3. Check plugin status and versions

### Installing Plugin Packages

1. Obtain the plugin package file
2. Upload to the plugin storage location
3. Configure package settings
4. Enable the plugin

### Updating Plugins

1. Upload the new version
2. Update configuration if needed
3. Test with affected agents/pipelines
4. Enable the updated version

### Removing Plugins

1. Verify no active resources use the plugin
2. Disable the plugin
3. Remove from storage

## Using Plugins in Agents

### Workflow Plugins

Reference in agent workflow configuration:

| Field | Description |
|-------|-------------|
| **Workflow Package Name** | Python package name |
| **Workflow Class Name** | Class implementing the workflow |

**Example:**
- Package: `foundationallm_agent_plugins`
- Class: `FoundationaLLMFunctionCallingWorkflow`

### Tool Plugins

Reference in agent tool configuration:

| Field | Description |
|-------|-------------|
| **Tool Package Name** | Python package name |
| **Tool Class Name** | Class implementing the tool |

**Examples:**

| Tool | Package | Class |
|------|---------|-------|
| Code Interpreter | `foundationallm_agent_plugins` | `FoundationaLLMCodeInterpreterTool` |
| Knowledge Tool | `foundationallm_agent_plugins` | `FoundationaLLMKnowledgeTool` |

## Using Plugins in Data Pipelines

### Data Source Plugins

Configure when creating data sources:

1. Select the data source type
2. Choose the appropriate plugin
3. Configure plugin parameters

### Pipeline Stage Plugins

Configure in pipeline stage settings:

1. Add a pipeline stage
2. Select the stage plugin
3. Configure stage parameters

### Text Processing Plugins

For content extraction and partitioning:

| Plugin Type | Use Case |
|-------------|----------|
| **Text Extraction** | Extract text from documents |
| **Text Partitioning** | Split text into chunks |
| **Image Description** | Generate text from images |

## Built-in Plugins

FoundationaLLM includes several built-in plugins:

### Agent Plugins (Python)

| Plugin | Description |
|--------|-------------|
| `FoundationaLLMFunctionCallingWorkflow` | Model agnostic function calling |
| `FoundationaLLMCodeInterpreterTool` | Python code execution |
| `FoundationaLLMKnowledgeTool` | Knowledge base search |

### Data Pipeline Plugins (.NET)

> **TODO**: Document specific built-in .NET plugins for data pipelines.

## Custom Plugin Development

### Python Plugins

For agent workflows and tools:

1. Implement the required interface
2. Package as a Python wheel or ZIP
3. Deploy to the plugin storage
4. Reference in agent configuration

### .NET Plugins

For data pipeline components:

1. Implement the required interface
2. Package as a NuGet package
3. Deploy to the plugin storage
4. Configure in pipeline settings

> **Note:** See developer documentation for detailed plugin development guides.

## Plugin Configuration

### Environment Variables

Some plugins use environment variables:

| Variable | Description |
|----------|-------------|
| `PLUGIN_STORAGE_PATH` | Location of plugin packages |
| `PLUGIN_CONFIG_PATH` | Plugin configuration files |

### Plugin Settings

Configure plugin-specific settings in:

- Agent configuration (workflow/tool parameters)
- Pipeline configuration (stage parameters)
- App Configuration (global settings)

## Troubleshooting

### Plugin Not Loading

- Verify package is in correct location
- Check package format is valid
- Review logs for loading errors

### Plugin Errors in Execution

- Check plugin dependencies are installed
- Verify configuration parameters
- Review agent/pipeline logs

### Version Conflicts

- Check for dependency version conflicts
- Ensure compatible package versions
- Consider isolated environments

## Best Practices

### Plugin Management

- Version plugins for traceability
- Test plugins in development before production
- Document plugin requirements and configuration

### Security

- Review plugin code before deployment
- Limit plugin permissions appropriately
- Monitor plugin activity

### Updates

- Test updates in non-production first
- Have rollback plans
- Document changes

## Related Topics

- [Plugins & Packages Reference](../reference/concepts/plugins-packages.md)
- [Agents & Workflows](../reference/concepts/agents-workflows.md)
- [Creating Data Pipelines](data/data-pipelines/creating-data-pipelines.md)
