# Vectorization Worker

## Configuration

The following table describes the Azure artifacts required for the vectorization pipelines.

| Artifact Name | Description |
| --- | --- |
| `embed` | Azure storage queue used for the embed vectorization pipeline. Can be created on the storage account used for the other queues. |
| `extract` | Azure storage queue used for the extract vectorization pipeline. Can be created on the storage account used for the other queues. |
| `index` | Azure storage queue used for the index vectorization pipeline. Can be created on the storage account used for the other queues. |
| `partition` | Azure storage queue used for the partition vectorization pipeline. Can be created on the storage account used for the other queues. |
| `vectorization-state` | Azure storage container used for the vectorization state service. Can be created on the storage account used for the other queues. |
| `resource-provider`| Azure storage container used for the internal states of the FoundationaLLM resource providers. |
| `resource-provider/FoundationaLLM.Vectorization/vectorization-content-source-profiles.json` | Azure storage blob used for the content sources managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization content source profiles](#default-vectorization-content-source-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-partition-profiles.json` | Azure storage blob used for the text partitioning profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization text partitioning profiles](#default-vectorization-text-partitioning-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-text-embedding-profiles.json` | Azure storage blob used for the text embedding profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization text embedding profiles](#default-vectorization-text-embedding-profiles).
| `resource-provider/FoundationaLLM.Vectorization/vectorization-indexing-profiles.json` | Azure storage blob used for the indexing profiles managed by the `FoundationaLLM.Vectorization` resource provider. For more details, see [default vectorization indexing profiles](#default-vectorization-indexing-profiles).

The following table describes the environment variables required for the vectorization pipelines.

| Environment variable | Description |
| --- | --- |
| `FoundationaLLM:AppConfig:ConnectionString` | Connection string to the Azure App Configuration instance. |

The following table describes the required App Configuration parameters for the vectorization pipelines.

| App Configuration Key | Default Value | Description |
| --- | --- | --- |
| `FoundationaLLM:APIs:VectorizationWorker:APIUrl` | | The URL of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:APIKey` | Key Vault secret name: `foundationallm-apis-vectorizationworker-apikey` | The API key of the vectorization worker API. |
| `FoundationaLLM:APIs:VectorizationWorker:AppInsightsConnectionString` | Key Vault secret name: `foundationallm-app-insights-connection-string` | The connection string to the Application Insights instance used by the vectorization worker API. |
| `FoundationaLLM:Vectorization:VectorizationWorker` | | The settings used by each instance of the vectorization worker service. For more details, see [default vectorization worker settings](#default-vectorization-worker-settings). |
| `FoundationaLLM:Vectorization:Queues:Embed:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the embed vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Extract:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the extract vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Index:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the index vectorization queue. |
| `FoundationaLLM:Vectorization:Queues:Partition:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-queues-connectionstring` | The connection string to the Azure Storage account used for the partition vectorization queue. |
| `FoundationaLLM:Vectorization:StateService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:StateService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-state-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:AuthenticationType` | | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `FoundationaLLM:Vectorization:ResourceProviderService:Storage:ConnectionString` | Key Vault secret name: `foundationallm-vectorization-resourceprovider-storage-connectionstring` | The connection string to the Azure Storage account used for the vectorization state service. |
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey` | Key Vault secret name: `foundationallm-vectorization-semantickerneltextembedding-openai-apikey` | The API key used to connect to the Azure OpenAI service.
| `FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType` | | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`.
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
| `https://openaipublic.blob.core.windows.net` | The public Azure Blob Storage account used to download the OpenAI BPE ranking files. |

> [!NOTE]
> The vectorization worker must be able to open HTTPS connections to the external content listed above.

### Default vectorization worker settings

Default settings for the vectorization worker:

```json
{
    "RequestManagers": [
        {
            "RequestSourceName": "extract",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "partition",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "embed",
            "MaxHandlerInstances": 1
        },
        {
            "RequestSourceName": "index",
            "MaxHandlerInstances": 1
        }
    ],
    "RequestSources": [
        {
            "Name": "extract",
            "ConnectionConfigurationName": "Extract:ConnectionString",
            "VisibilityTimeoutSeconds": 120
        },
        {
            "Name": "partition",
            "ConnectionConfigurationName": "Partition:ConnectionString",
            "VisibilityTimeoutSeconds": 120
        },
        {
            "Name": "embed",
            "ConnectionConfigurationName": "Embed:ConnectionString",
            "VisibilityTimeoutSeconds": 120
        },
        {
            "Name": "index",
            "ConnectionConfigurationName": "Index:ConnectionString",
            "VisibilityTimeoutSeconds": 120
        }
    ],
    "QueuingEngine": "AzureStorageQueue"
}
```

### Default vectorization content source profiles

Default structure for the `vectorization-content-source-profiles.json` file:

```json
{
    "ContentSourceProfiles": [
        {
            "Name": "SDZWAJournals",
            "Type": "AzureDataLake",
            "Settings": {},
            "ConfigurationReferences": {
                "AuthenticationType": "FoundationaLLM:Vectorization:ContentSources:SDZWAJournals:AuthenticationType",
                "ConnectionString": "FoundationaLLM:Vectorization:ContentSources:SDZWAJournals:ConnectionString"
            }
        }
    ]
}
```

### Default vectorization text partitioning profiles

Default structure for the `vectorization-text-partition-profiles.json` file:

```json
{
    "TextPartitioningProfiles": [
        {
            "Name": "DefaultTokenTextPartition",
            "TextSplitter": "TokenTextSplitter",
            "TextSplitterSettings": {
                "Tokenizer": "MicrosoftBPETokenizer",
                "TokenizerEncoder": "cl100k_base",
                "ChunkSizeTokens": "2000",
                "OverlapSizeTokens": "200"
            }
        }
    ]
}
```

### Default vectorization text embedding profiles

Default structure for the `vectorization-text-embedding-profiles.json` file:

```json
{
    "TextEmbeddingProfiles": [
        {
            "Name": "AzureOpenAI_Embedding",
            "TextEmbedding": "SemanticKernelTextEmbedding",
            "Settings": {},
            "ConfigurationReferences": {
                "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
                "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
                "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
                "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
            }
        }
    ]
}
```

### Default vectorization indexing profiles

Default structure for the `vectorization-indexing-profiles.json` file:

```json
{
    "IndexingProfiles": [
        {
            "Name": "AzureAISearch_Test_001",
            "Indexer": "AzureAISearchIndexer",
            "Settings": {
                "IndexName": "fllm-test-001"
            },
            "ConfigurationReferences": {
                "APIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey",
                "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
                "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
            }
        }
    ]
}
```

## Vectorization request

Sample structure of a vectorization request:

```json
{
    "id": "d4669c9c-e330-450a-a41c-a4d6649abdef",
    "content_identifier": {
        "content_source_profile_name": "SDZWAJournals",
        "multipart_id": [
            "https://fllmaks14sa.blob.core.windows.net",
            "vectorization-input",
            "SDZWA-Journal-January-2024.pdf"
        ],
        "canonical_id": "sdzwa/journals/SDZWA-Journal-January-2024"
    },
    "processing_type": "Asynchronous",
    "steps": [
        {
            "id": "extract",
            "parameters": {}
        },
        {
            "id": "partition",
            "parameters": {
                "text_partition_profile_name": "DefaultTokenTextPartition"
            }
        },
        {
            "id": "embed",
            "parameters": {
                "text_embedding_profile_name": "AzureOpenAI_Embedding"
            }
        },
        {
            "id": "index",
            "parameters": {
                "indexing_profile_name": "AzureAISearch_Test_001"
            }
        }
    ],
    "completed_steps": [],
    "remaining_steps": [
        "extract",
        "partition",
        "embed",
        "index"
    ]
}
```

The `processing_type` property can be one of `Asynchronous` or `Synchronous`. The `Asynchronous` value indicates that the vectorization request is processed asynchronously via the Vectorization workers. The `Synchronous` value indicates that the vectorization request is processed synchronously via the Vectorization API.
