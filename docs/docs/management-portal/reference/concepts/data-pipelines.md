# Data Pipelines

## Overview

The following diagram illustrates the high level structure of a FoundationaLLM data pipeline:

![Data Pipeline Overview](../../media/concepts-data-pipeline.png)

Data pipelines have three main components:
- **Data Pipeline Data Source**: The source of the data that will be processed by the pipeline.
- **Data Pipeline Stages**: The stages that the data will go through in the pipeline. The stages are structure as a forest, with one or more starting stages.
- **Data Pipeline Trigger**: The trigger that will start the pipeline execution. Trigger can be a schedule, an event (e.g., new content added to the data source), or a manual action.

The data source and the stages define parameters that are required for the pipeline execution. The parameters metadata is provided by the plugin that implements that particular data source or stage. Parameters can have default values, which can be overridden when the pipeline is executed.

>[!NOTE]
> Triggers of type `Schedule` or `Event` must provide a complete set of parameter values. This is because the trigger will use these values to start the pipeline execution and there is no user interaction to provide missing values. In the case of a manual trigger, the user will be prompted to provide the missing values.

Here is an example of a simple data pipeline that reads data from an Azure Data Lake storage account, extracts text from files, partitions the text into chunks of a certain size, embeds the chunks and deposits them into an Azure AI Search index:

```json
{
    "type": "data-pipeline",
    "name": "DataPipeline01",
    "display_name": "Data Pipeline 01",
    "description": "Data Pipeline demo.",
    "active": false,
    "data_source": {
        "name": "VGDataLake",
        "description": "Victorious Ground data lake storage.",
        "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/AzureDataLakeDataSource",
        "plugin_parameters": [
            {
                "parameter_metadata": {
                    "name": "Folders",
                    "type": "array",
                    "description": "A list of strings defining data lake folders."
                },
                "default_value": null
            }
        ],
        "data_source_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/alchemy"
    },
    "starting_stages": [
        {
            "name": "Extract",
            "description": "Extract text from binary content items.",
            "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/TextExtractionDataPipelineStage",
            "plugin_parameters": null,
            "next_stages": [
                {
                    "name": "Partition",
                    "description": "Partition text into chunks.",
                    "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/TextPartitioningDataPipelineStage",
                    "plugin_parameters": [
                        {
                            "parameter_metadata": {
                                "name": "PartitioningStrategy",
                                "type": "string",
                                "description": "The partitioning strategy to be used (can be Token or Semantic)."
                            },
                            "default_value": "Token"
                        }
                    ],
                    "plugin_dependencies": [
                        {
                            "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/TokenContentTextPartitioning",
                            "plugin_parameters": [
                                {
                                    "parameter_metadata": {
                                        "name": "PartitionSizeTokens",
                                        "type": "int",
                                        "description": "The size in tokens of the text partitions."
                                    },
                                    "default_value": 400
                                },
                                {
                                    "parameter_metadata": {
                                        "name": "PartitionOverlapTokens",
                                        "type": "int",
                                        "description": "The size in tokens of text partitions overlap."
                                    },
                                    "default_value": 100
                                }
                            ]
                        }
                    ],
                    "next_stages": [
                        {
                            "name": "Embed",
                            "description": "Embed chunks of text.",
                            "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/GatewayTextEmbeddingDataPipelineStage",
                            "plugin_parameters": [
                                {
                                    "parameter_metadata": {
                                        "name": "EmbeddingModel",
                                        "type": "string",
                                        "description": "The embedding model used for embedding."
                                    },
                                    "default_value": "text-embedding-3-large"
                                },
                                {
                                    "parameter_metadata": {
                                        "name": "EmbeddingDimensions",
                                        "type": "int",
                                        "description": "The number of dimensions used for embedding."
                                    },
                                    "default_value": 2048
                                }
                            ],
                            "next_stages": [
                                {
                                    "name": "Index",
                                    "description": "Persist embeddings to a vector store.",
                                    "plugin_object_id": "instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/AzureAISearchIndexingDataPipelineStage",
                                    "plugin_parameters": [
                                        {
                                            "parameter_metadata": {
                                                "name": "APIEndpointConfigurationObjectId",
                                                "type": "resource-object-id",
                                                "description": "The FoundationaLLM resource object identifier of the API Endpoint Configuration resource that represents the target Azure AI Search instance."
                                            },
                                            "default_value": "instances/{{instanceId}}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/AzureAISearch"
                                        },
                                        {
                                            "parameter_metadata": {
                                                "name": "IndexName",
                                                "type": "string",
                                                "description": "The name of the Azure AI Search index."
                                            },
                                            "default_value": "demo-index"
                                        },
                                        {
                                            "parameter_metadata": {
                                                "name": "IndexPartitionName",
                                                "type": "string",
                                                "description": "The name of the index partition (to be added as a metadata entry and used for logical separation within a given physical index."
                                            },
                                            "default_value": "Dune"
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        }
    ],
    "triggers": [
        {
            "name": "Default pipeline schedule",
            "trigger_type": "Schedule",
            "trigger_cron_schedule": "0 6 * * *",
            "parameter_values": {
                "DataSource.VGDataLake.Folders": [
                    "vectorization-input/Dune"
                ],
                "Stage.Partition.PartitioningStrategy": "Token",
                "Stage.Partition.Dependency.TokenContentTextPartitioning.PartitionSizeTokens": 400,
                "Stage.Partition.Dependency.TokenContentTextPartitioning.PartitionOverlapTokens": 100,
                "Stage.Embed.EmbeddingModel": "text-embedding-3-large",
                "Stage.Embed.EmbeddingDimensions": 2048,
                "Stage.Index.APIEndpointConfigurationObjectId": "instances/{{instanceId}}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/AzureAISearch",
                "Stage.Index.IndexName": "demo-index",
                "Stage.Index.IndexPartitionName": "Dune"
            }
        }
    ]
}
```

The most important rules that govern the structure of data pipelines are:

- Both the data source and the stages must specify the configuration of the plugins that implement them. This is done by using the following properties:
  - `plugin_object_id`: The FoundationaLLM resource identifier of the plugin that implements the data source or stage.
  - `plugin_parameters`: The parameters that are required by the plugin. The parameters are defined by the plugin and can have default values. The values in the `parameter_metadata` are provided by the plugin definition. For more details about plugin parameters, see [Plugins](../plugin/plugin.md).
  - `plugin_dependencies`: The dependencies that the plugin has. Dependencies are other plugins that the plugin requires to function properly. Dependencies can have their own parameters. The metadata required to select dependency plugins is provided by the plugin definition. For more details about plugin dependencies, see [Plugins](../plugin/plugin.md).
- The `active` flag indicates whether the pipeline can be triggered or not. If the pipeline is not active, it will not be executed.
- The data source must specify the FoundationaLLM resource identifier of the data source that it represents. This is done by using the `data_source_object_id` property. The data source object must be compatible with the pluging specified by the `plugin_object_id` property.
- The data source name and the stage names must be unique within the data pipeline pipeline.
- The data source name and the stage names can only contain alphanumerical characters, underlines, or hyphens.
- The stages must specify the next stages that the data will go through. This is done by using the `next_stages` property. The stages can have multiple next stages, forming a forest of stages. The stages must specify the configuration of the plugins that implement them, as described above.
- The triggers must specify the parameter values that are required by the data source and the stages. The parameter values are provided as a dictionary where the key is the parameter name and the value is the parameter value. The parameter values must be provided for all the parameters that are required by the data source and the stages. The parameter values are used to start the pipeline execution.

### Trigger parameter values naming convention

The trigger parameter values are named using the following convention:

- For the data source, the naming structure is `DataSource.{DataSourceName}.{ParameterName}`. In the example above, the data source is named `VGDataLake`, so the parameter values for the data source are named `DataSource.VGDataLake.{ParameterName}`.
- For the stages, the naming structure is `Stage.{StageName}.{ParameterName}`. In the example above, the stages are named `Extract`, `Partition`, `Embed`, and `Index`, so the parameter values for the stages are named `Stage.{StageName}.{ParameterName}`.
- For stages that have dependency plugins, the naming structure is `Stage.{StageName}.Dependency.{DependencyPluginName}.{ParameterName}`. In the example above, the `Partition` stage has a dependency plugin named `TokenContentTextPartitioning`, so the parameter values for the dependency plugin are named `Stage.Partition.Dependency.TokenContentTextPartitioning.{ParameterName}`.