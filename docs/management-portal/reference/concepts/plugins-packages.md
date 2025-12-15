# Plugins & Plugin Packages

This document provides reference information for plugins and plugin packages in FoundationaLLM.

## Overview

Plugins extend the functionality of the FoundationaLLM platform. Plugins can be used to add new features, integrate with external systems, or perform other tasks. Plugins can be managed using the FoundationaLLM Management Portal (interactively) or the FoundationaLLM Management API (programmatically).

## Plugin Packages

A FoundationaLLM plugin package is a unit of versioning and deployment of one or more plugins.

### Supported Package Types

| Platform | Package Type | Description |
|----------|--------------|-------------|
| Python | ZIP package | Contains agent workflow and agent tool plugins. Scheduled to be replaced by Wheel packages. |
| .NET | NuGet package | Contains data source, data pipeline stage, content text extraction, and content text partitioning plugins. Fully supported by Management API. |

### Package Naming Convention

Plugin package names must follow a strict naming convention: `{platform}-{name}`, where:
- `{platform}` can be `Dotnet` or `Python`
- `{name}` can only contain alphanumerical characters, underlines, or hyphens

Example: `Dotnet-FoundationaLLMDataPipelinePlugins`

### Managing Plugin Packages via API

#### Create or Update a Plugin Package

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Plugin/pluginPackages/{packageName}
```

Request body:
- Must be of type `form-data`
- Must contain a file with the key `file` (the NuGet package)
- Must contain a text with key `resource`:

```json
{
    "type": "plugin-package",
    "name": "{packageName}"
}
```

#### List Plugin Packages

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Plugin/pluginPackages
```

---

## Plugins

### Plugin Types

| Platform | Category | Description |
|----------|----------|-------------|
| Python | Agent Workflow | Implements agent workflows running in LangChain API |
| Python | Agent Tool | Implements agent tools for LangChain workflow host |
| .NET | Data Source | Implements data sources for data pipelines |
| .NET | Data Pipeline Stage | Implements stages for data pipeline execution |
| .NET | Content Text Extraction | Implements text extraction from binary content |
| .NET | Content Text Partitioning | Implements text partitioning strategies |

### Plugin Naming Convention

Plugin names must follow the format `{platform}-{packageName}-{pluginName}`, where:
- `{platform}` can be `Dotnet` or `Python`
- `{packageName}` is the plugin package name (alphanumerical, underlines, hyphens only)
- `{pluginName}` is the plugin name (alphanumerical, underlines, hyphens only)

Example: `Dotnet-FoundationaLLMDataPipelinePlugins-AzureAISearchIndexingDataPipelineStage`

### Plugin Definition Structure

```json
{
    "type": "plugin",
    "name": "Dotnet-FoundationaLLMDataPipelinePlugins-AzureAISearchIndexingDataPipelineStage",
    "object_id": "instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/...",
    "display_name": "Azure AI Search Indexing Data Pipeline Stage (FoundationaLLM)",
    "description": "Provides the FoundationaLLM standard implementation for indexing data pipeline stages.",
    "category": "Data Pipeline Stage",
    "parameters": [...],
    "parameter_selection_hints": {...},
    "dependencies": [...]
}
```

### Plugin Parameters

Each plugin has zero or more parameters. Supported parameter types:

| Type | Description |
|------|-------------|
| `string` | A single string value |
| `int` | A single integer value |
| `float` | A single floating-point value |
| `bool` | A single boolean value |
| `datetime` | A single date and time value |
| `array` | An array of values |
| `resource-object-id` | A FoundationaLLM resource identifier |

#### Parameter Selection Hints

For `resource-object-id` parameters, the `parameter_selection_hints` property provides UI guidance:

```json
"parameter_selection_hints": {
    "APIEndpointConfigurationObjectId": {
        "resourcePath": "providers/FoundationaLLM.Configuration/apiEndpointConfigurations",
        "filterActionPayload": {
            "Category": "General",
            "Subcategory": "Indexing"
        }
    }
}
```

### Plugin Dependencies

Plugins can have dependencies on other plugins. Dependency types:
- **Single**: Exactly one dependency plugin must be selected
- **Multiple**: One or more dependency plugins must be selected

```json
"dependencies": [
    {
        "selection_type": "Single",
        "dependency_plugin_names": [
            "Dotnet-FoundationaLLMDataPipelinePlugins-TokenContentTextPartitioning",
            "Dotnet-FoundationaLLMDataPipelinePlugins-SemanticContentTextPartitioning"
        ]
    }
]
```

### Managing Plugins via API

#### List All Plugins

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins
```

#### Filter Plugins by Category

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/filter
```

Request body:
```json
{
    "categories": [
        "Data Source",
        "Data Pipeline Stage"
    ]
}
```

Supported categories: `Data Source`, `Data Pipeline Stage`, `Context Text Extraction`, `Content Text Partitioning`

## Related Topics

- [Managing Plugins](../../how-to-guides/managing-plugins.md)
- [Data Pipelines](data-pipelines.md)
- [Agents & Workflows](agents-workflows.md)
