# Configuring vectorization

This section provides details on how to configure the vectorization API and workers in FoundationaLLM.

> [!NOTE]
> These configurations should already be in place if you deployed FoundationaLLM (FLLM) using the recommended deployment scripts.
> The details presented here are provided for cases in which you need to troubleshoot or customize the configuration.

## Configuration for Vectorization API

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact name | Description |
| --- | --- |
| `vectorization-input` | Azure storage container used by default to store documents to be picked up by the vectorization pipeline. Must be created on a Data Lake storage account (with the hierarchical namespace enabled). |

The following table describes the environment variables required for the vectorization pipelines.

Environment variable | Description
--- | ---
`FoundationaLLM_AppConfig_ConnectionString` | Connection string to the Azure App Configuration instance.

The following table describes the required configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationAPI:APIUrl` | | The URL of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationapi-apikey` | The API key of the vectorization API. |
| `FoundationaLLM:APIs:VectorizationAPI:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization API. |

> [!NOTE]
> Refer to the [App Configuration values](../../deployment/app-configuration-values.md) page for more information on how to set these and other configuration values.

## Configuration for Vectorization workers

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact Name | Description |
| --- | --- |
| `embed` | Azure storage queue used for the embed vectorization pipeline. Can be created on the storage account used for the other queues. |
| `extract` | Azure storage queue used for the extract vectorization pipeline. Can be created on the storage account used for the other queues. |
| `index` | Azure storage queue used for the index vectorization pipeline. Can be created on the storage account used for the other queues. |
| `partition` | Azure storage queue used for the partition vectorization pipeline. Can be created on the storage account used for the other queues. |
| `vectorization-state` | Azure storage container used for the vectorization state service. Can be created on the storage account used for the other queues. |
| `resource-provider`| Azure storage container used for the internal states of the FoundationaLLM resource providers. |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-pipelines.json` | Azure storage blob used for the vectorization pipeline resources managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization pipelines](vectorization-concepts.md#vectorization-pipelines). |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-partitioning-profiles.json` | Azure storage blob used for the text partitioning profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization text partitioning profiles](vectorization-profiles.md#text-partitioning-profiles). |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-embedding-profiles.json` | Azure storage blob used for the text embedding profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization text embedding profiles](vectorization-profiles.md#text-embedding-profiles). |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-indexing-profiles.json` | Azure storage blob used for the indexing profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [vectorization indexing profiles](vectorization-profiles.md#indexing-profiles). |
| `resource-provider/FoundationaLLM.DataSources` | Azure storage directory where the data sources managed by the `FoundationaLLM.DataSources` resource provider are stored. |
| `vectorization-state/requests/{requestid-yyyyMMdd}.json` | Azure storage directory where vectorization requests managed by the vectorization state service are stored. |
| `vectorization-state/execution-state/{canonical_id}` | Azure storage directory where the execution state of the vectorization requests and their resulting artifacts are stored. The canonical_id is defined in the vectorization request. |
| `vectorization-state/pipeline-state/{pipeline_name}/{pipeline_name}-{execution_id}.json` | Azure storage directory where the state of the vectorization pipeline execution is stored. The pipeline_name is the name of the vectorization pipeline and the execution_id is the unique identifier of the execution. |

The following table describes the environment variables required for the vectorization pipelines.

| Environment variable | Description |
| --- | --- |
| `FoundationaLLM_AppConfig_ConnectionString` | Connection string to the Azure App Configuration instance. |

The following table describes the required App Configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationWorker:APIUrl` | | The URL of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationworker-apikey` | The API key of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization worker API. |
| `FoundationaLLM:Vectorization:VectorizationWorker` | | The settings used by each instance of the vectorization worker service. For more details, see [default vectorization worker settings](#default-vectorization-worker-settings). |
| `FoundationaLLM:Vectorization:Queues:Embed:AccountName` | | The account name of the Azure Storage account used for the embed vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Extract:AccountName` | | The account name of the Azure Storage account used for the extract vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Index:AccountName` | | The account name of the Azure Storage account used for the index vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Partition:AccountName` | | The account name of the Azure Storage account used for the partition vectorization queue. |
| `FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-semantickerneltextembedding-openai-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`. |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName` | | The name of the Azure OpenAI model deployment. The default value is `embeddings`.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint` | | The endpoint of the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-azureaisearch-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
| `FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint` | | The endpoint of the Azure OpenAI service.

> [!NOTE]
> Refer to the [App Configuration values](../../app-configuration-values.md) page for more information on how to set these and other configuration values.

The following table describes the external content used by the vectorization worker to initialize:

| Uri | Description |
| --- | --- |
| `https://openaipublic.blob.core.windows.net/encodings/cl100k_base.tiktoken` | The public Azure Blob Storage account used to download the OpenAI BPE ranking files. |

> [!NOTE]
> The vectorization worker must be able to open HTTPS connections to the external content listed above.

### Default vectorization worker settings

The default settings for the vectorization worker are stored in the `FoundationaLLM:Vectorization:VectorizationWorker` App Configuration key. The default structure for this key is:

```json
{
    "RequestManagers": [
        {
            "RequestSourceName": "extract",
            "MaxHandlerInstances": 1,
            "QueueProcessingPace": 5,
            "QueuePollingInterval": 60,
            "QueueMaxNumberOfRetries": 5
        },
        {
            "RequestSourceName": "partition",
            "MaxHandlerInstances": 1,
            "QueueProcessingPace": 5,
            "QueuePollingInterval": 60,
            "QueueMaxNumberOfRetries": 5
        },
        {
            "RequestSourceName": "embed",
            "MaxHandlerInstances": 1,
            "QueueProcessingPace": 5,
            "QueuePollingInterval": 60,
            "QueueMaxNumberOfRetries": 5
        },
        {
            "RequestSourceName": "index",
            "MaxHandlerInstances": 1,
            "QueueProcessingPace": 5,
            "QueuePollingInterval": 60,
            "QueueMaxNumberOfRetries": 5
        }
    ],
    "RequestSources": [
        {
            "Name": "extract",
            "AccountName": "{{accountName}}",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "partition",
            "AccountName": "{{accountName}}",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "embed",
            "AccountName": "{{accountName}}",
            "VisibilityTimeoutSeconds": 600
        },
        {
            "Name": "index",
            "AccountName": "{{accountName}}",
            "VisibilityTimeoutSeconds": 600
        }
    ],
    "QueuingEngine": "AzureStorageQueue"
}
```

The following table provides details about the configuration parameters:

| Parameter | Description |
| --- | --- |
| `RequestManagers` | The list of request managers used by the vectorization worker. Each request manager is responsible for managing the execution of vectorization pipelines for a specific vectorization step. The configuration must include all request managers. |
| `RequestManagers.MaxHandlerInstances` | The maximum number of request handlers that process requests for the specified request source. By default, the value is 1. You can change the value to increase the processing capacity of each vectorization worker instance. The value applies to all istances of the vectorization worker. NOTE: It is important to align the value of this setting with the level of compute and memory resources allocated to the individual vectorization worker instances. |
| `RequestManagers.QueueProcessingPace` | **Optional** The delay in seconds to wait between requests after a request has been processed. The default value is 5. |
| `RequestManagers.QueuePollingInterval` | **Optional** The polling interval in seconds, this is the amount of time to wait if the previous check on the queue had no items. The default value is 60. |
| `RequestManagers.QueueMaxNumberOfRetries` | **Optional** The maximum number of retries to attempt to process a request before being removed from the queue. The default value is 5. |
| `RequestSources` | The list of request sources used by the vectorization worker. Each request source is responsible for managing the requests for a specific vectorization step. The configuration must include all request sources. |
| `RequestSources.Name` | The name of the request source. The name must match the name of the request manager. |
| `RequestSources.AccountName` | The name of the configuration key for the Azure Storage account used for the queue (include the tokens after **FoundationaLLM:Vectorization:Queues:**). |
| `RequestSources.VisibilityTimeoutSeconds` | In the case of queue-based request sources (the default for the vectorization worker), specifies the time in seconds until a dequeued vectorization step request must be executed. During this timeout, the message will not be visible to other handler instances within the same worker or from other worker instances. If the handler fails to process the vectorization step request successfully and remove it from the queue within the specified timeout, the message will become visibile again. The default value is 600 seconds and should not be changed.|
