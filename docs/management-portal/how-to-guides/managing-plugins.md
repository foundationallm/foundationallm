# Managing Plugins

Learn how to manage plugins and plugin packages in FoundationaLLM.

## Overview

Plugins extend the functionality of FoundationaLLM with custom capabilities.

## Plugin Types

### Python Plugins
- **Agent Workflow**: Custom workflow implementations
- **Agent Tool**: Custom tools for agents

### .NET Plugins
- **Data Source**: Custom data source connectors
- **Data Pipeline Stage**: Custom pipeline processing
- **Content Text Extraction**: Custom text extractors
- **Content Text Partitioning**: Custom text splitters

## Plugin Packages

### Package Types
- **Python**: ZIP packages (Wheel coming soon)
- **.NET**: NuGet packages

### Managing Packages

1. Navigate to plugin management
2. Upload a new package
3. Configure package settings
4. Enable the package

## Using Plugins

### In Agents
Reference plugins in agent configuration:
- Workflow package name
- Workflow class name
- Tool package name
- Tool class name

### In Data Pipelines
Configure plugins for:
- Data source connections
- Pipeline stages
- Text extraction
- Text partitioning

## Plugin Development

For custom plugin development, see the developer documentation.

## Related Topics

- [Plugins & Packages Reference](reference/concepts/plugins-packages.md)
- [Agents & Workflows](reference/concepts/agents-workflows.md)
