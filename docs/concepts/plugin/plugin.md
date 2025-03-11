# Plugins

## Overview

### Plugin naming convention

All plugins must follow a strict naming convention. The name must be in the format `{platform}-{packageName}-{pluginName}`, where:
- `{platform}` can be one of `Dotnet` or `Python`.
- `{packageName}` is the name of the plugin package that provides the plugin and can only contain alphanumerical characters, underlines, or hyphens.
- `{pluginName}` is the name of the plugin and can only contain alphanumerical characters, underlines, or hyphens.

For example, the plugin name of the FoundationaLLM Azure AI Search data pipeline stage plugin  is `Dotnet-FoundationaLLMDataPipelinePlugins-AzureAISearchIndexingDataPipelineStage`.

### Plugin properties

Here is an example of a plugin definition:

```json
{
        "type": "plugin",
        "name": "Dotnet-FoundationaLLMDataPipelinePlugins-AzureAISearchIndexingDataPipelineStage",
        "object_id": "instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/Dotnet-FoundationaLLMDataPipelinePlugins-AzureAISearchIndexingDataPipelineStage",
        "display_name": "Azure AI Search Indexing Data Pipeline Stage (FoundationaLLM)",
        "description": "Provides the FoundationaLLM standard implementation for indexing data pipeline stages that use Azure AI Search.",
        "cost_center": null,
        "category": "Data Pipeline Stage",
        "parameters": [
            {
                "name": "APIEndpointConfigurationObjectId",
                "type": "resource-object-id",
                "description": "The FoundationaLLM resource identifier of the API Endpoint Configuration resource that identifies the Azure AI Search instance."
            },
            {
                "name": "IndexName",
                "type": "string",
                "description": "The name of the Azure AI Search index."
            },
            {
                "name": "IndexPartitionName",
                "type": "string",
                "description": "The name of the Azure AI Search index partition (to be added as a metadata entry and used for logical separation within a given physical index)."
            }
        ],
        "parameter_selection_hints": {
            "APIEndpointConfigurationObjectId": {
                "resourcePath": "providers/FoundationaLLM.Configuration/apiEndpointConfigurations",
                "filterActionPayload": {
                    "Category": "General",
                    "Subcategory": "Indexing"
                }
            }
        },
        "dependencies": [],
        "properties": null,
        "created_on": "2025-03-09T19:59:41.110155+00:00",
        "updated_on": "0001-01-01T00:00:00+00:00",
        "created_by": "ciprian@foundationaLLM.ai",
        "updated_by": null,
        "deleted": false,
        "expiration_date": null
    }
```

**Parameters**

Each plugin has zero, one, or more parameters that are required for its execution. Parameters can be of the following types:

Type | Description
--- | ---
`string` | A single string value.
`int` | A single integer value.
`float` | A single floating-point value.
`bool` | A single boolean value.
`datetime` | A single date and time value.
`array` | An array of values (individual items in the array can be of any type).
`resource-object-id` | A FoundationaLLM resource identifier that references another resource in the system.

>[!NOTE]
> When working with `resource-object-id` parameters, the `parameter_selection_hints` property can be used to provide additional information about the resource that the parameter references. This information is used by the Management Portal to provide a user-friendly selection experience when configuring the plugin. The `parameter_selection_hints` property contains a dictionary where the keys are parameter names of type `resource-object-id` and the values are objects with the following properties:
> - `resourcePath`: The path to the resource type that the parameter references.
> - `filterActionPayload`: A JSON object that can be used to filter the resources of the specified type. The object can contain any number of properties that are used to filter the resources and it depends on the specific resource type.
>
> User interface developers can use the `resourcePath` and `filterActionPayload` properties to build a resource filtering request for the Management API. Using the exemple above, the following filtering request can be built: `POST /instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/filter`. The request body must be the JSON object specified in the `filterActionPayload` property.

**Dependencies**

Each plugin can have zero, one, or more dependencies. A dependency is a reference to another plugin that must be installed in the system in order for the plugin to work correctly.

When configuring plugins in the Management Portal, the dependency plugins must also be selected and configured. The `dependencies ` section provides the information needed to determine which plugins are required. The section contains a list of plugin selections that are required for the plugin to work correctly. Plugin selections can be either `Single` (meaning exactly one dependency plugin must be selected) or `Multiple` (meaning one or more dependency plugins must be selected).

Here is an example of a plugin with single selection dependencies:

```json
 {
        "type": "plugin",
        "name": "Dotnet-FoundationaLLMDataPipelinePlugins-TextPartitioningDataPipelineStage",
        "object_id": "instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/Dotnet-FoundationaLLMDataPipelinePlugins-TextPartitioningDataPipelineStage",
        "display_name": "Text Partitioning Data Pipeline Stage (FoundationaLLM)",
        "description": "Provides the FoundationaLLM standard implementation for text partitioning data pipeline stages.",
        "cost_center": null,
        "category": "Data Pipeline Stage",
        "parameters": [
            {
                "name": "PartitioningStrategy",
                "type": "string",
                "description": "Strategy used to partition text (can be Token or Semantic)."
            }
        ],
        "parameter_selection_hints": {},
        "dependencies": [
            {
                "selection_type": "Single",
                "dependency_plugin_names": [
                    "Dotnet-FoundationaLLMDataPipelinePlugins-TokenContentTextPartitioning",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-SemanticContentTextPartitioning"
                ]
            }
        ],
        "properties": null,
        "created_on": "2025-03-09T19:59:36.9258434+00:00",
        "updated_on": "0001-01-01T00:00:00+00:00",
        "created_by": "ciprian@foundationaLLM.ai",
        "updated_by": null,
        "deleted": false,
        "expiration_date": null
    }
```

Here is an example of a plugin with multiple selection dependencies:

```json
{
        "type": "plugin",
        "name": "Dotnet-FoundationaLLMDataPipelinePlugins-AzureDataLakeDataSource",
        "object_id": "instances/8ac6074c-bdde-43cb-a140-ec0002d96d2b/providers/FoundationaLLM.Plugin/plugins/Dotnet-FoundationaLLMDataPipelinePlugins-AzureDataLakeDataSource",
        "display_name": "Azure Data Lake Data Source (FoundationaLLM)",
        "description": "Provides the FoundationaLLM standard implementation for Azure Data Lake data sources.",
        "cost_center": null,
        "category": "Data Source",
        "parameters": [
            {
                "name": "Folders",
                "type": "array",
                "description": "List of strings defining data lake folders (the first part identifies the container name)."
            }
        ],
        "parameter_selection_hints": {},
        "dependencies": [
            {
                "selection_type": "Multiple",
                "dependency_plugin_names": [
                    "Dotnet-FoundationaLLMDataPipelinePlugins-PDFContentTextExtraction",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-DOCXContentTextExtraction",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-PPTXContentTextExtraction",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-XSLXContentTextExtraction",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-ImageContentTextExtraction",
                    "Dotnet-FoundationaLLMDataPipelinePlugins-ImageMetadataTextExtraction"
                ]
            }
        ],
        "properties": null,
        "created_on": "2025-03-09T19:59:32.3378998+00:00",
        "updated_on": "0001-01-01T00:00:00+00:00",
        "created_by": "ciprian@foundationaLLM.ai",
        "updated_by": null,
        "deleted": false,
        "expiration_date": null
    }
```


## Managing plugins using the FoundationaLLM Management API

### List all plugins

Management API endpoint: `GET /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins`

### Filter plugins by category

Management API endpoint: `POST /instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/filter`.

The request body must contain a JSON object with the following content:

```json
{
    "categories": [
        "Data Source",
        "Data Pipeline Stage"
    ]
}
```
>[!NOTE]
> The following plugin categories are supported for use in the filter: `Data Source`, `Data Pipeline Stage`, `Context Text Extraction`, `Content Text Partitioning`.