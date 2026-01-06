# Resource Management in FoundationaLLM

With the introduction of the Management API, you can now manage resources in FoundationaLLM programmatically or through the Management API User Interface Portal. This includes creating, updating, and deleting resources in the system.

## Resource Providers

The main concept of the Management API is the resource provider. A resource provider is a service that provides resources to the FoundationaLLM system. For example, the agents, prompts and datasources are provided by a resource provider. The Management API provides a way to manage these resources without the need to manually work with JSON files in storage containers and mimics the same concept and functionality of resources in the Azure Portal.

## Resource Provider Structure

The **resource-provider** container in the main storage account that was deployed on your behalf in your subscription contains the following structure:

![](../../../media/RS-Provider-1.png)

## Agent References

This first folder **FoundationaLLM.Agent** contains the Agent References.

![](../../../media/RS-Provider-2.png)

The content of the **_agent-references** references all the locations of the JSON files that contain the agent information. The **_agent-references** folder contains the following structure:

```json
{
	"AgentReferences": [
		{
			"Name": "sotu-2023",
			"Filename": "/FoundationaLLM.Agent/sotu-2023.json",
			"Type": "knowledge-management"
		},
		{
			"Name": "sotu2",
			"Filename": "/FoundationaLLM.Agent/sotu2.json",
			"Type": "knowledge-management"
		},
		{
			"Name": "sotu3",
			"Filename": "/FoundationaLLM.Agent/sotu3.json",
			"Type": "knowledge-management"
		},
		{
			"Name": "sotu",
			"Filename": "/FoundationaLLM.Agent/sotu.json",
			"Type": "knowledge-management"
		}
	]
}
```

From that starting point for the agent references, we get to point to JSON file that describes each agent available to the system.  Let's start by taking a look at one odf the agents from above called **sotu-2023.json**

```json
{
  "name": "sotu-2023",
  "type": "knowledge-management",
  "object_id": "/instances/1bc45134-6985-48b9-9466-c5f70ddaaa65/providers/FoundationaLLM.Agent/agents/sotu-2023",
  "description": "Knowledge Management Agent that queries the State of the Union speech transcript",
  "indexing_profile": "/instances/1bc45134-6985-48b9-9466-c5f70ddaaa65/providers/FoundationaLLM.Vectorization/indexingprofiles/sotu-index",
  "embedding_profile": "/instances/1bc45134-6985-48b9-9466-c5f70ddaaa65/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
  "language_model": {
    "type": "openai",
    "provider": "microsoft",
    "temperature": 0.0,
    "use_chat": true,
    "api_endpoint": "FoundationaLLM:AzureOpenAI:API:Endpoint",
    "api_key": "FoundationaLLM:AzureOpenAI:API:Key",
    "api_version": "FoundationaLLM:AzureOpenAI:API:Version",
    "version": "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion",
    "deployment": "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName"
  },
  "sessions_enabled": true,
  "conversation_history": {
    "enabled": true,
    "max_history": 5
  },
  "gatekeeper": {
    "use_system_setting": false,
    "options": [
      "ContentSafety",
      "Presidio"
    ]
  },
  "orchestrator": "LangChain",
  "prompt": "/instances/1bc45134-6985-48b9-9466-c5f70ddaaa65/providers/FoundationaLLM.Prompt/prompts/sotu"
}
```

Notice all the different keys and values that are present to identify the agent. This JSON file is usually created or modifed through the Management API UI Portal or via POST or PUT requests to the Management API using a product like POSTMAN.

The **type** could be "knowledge-management" or "analytical"
The **language-model** section is to identify the provider, its accuracy and endpoints to retrieve from the app configuration resource.
**sessions_enabled** is a boolean to enable or disable the ability to start a session Vs just a one time query using an API tool like Postman.

**conversation_history** is to enable or disable the ability to store the conversation history and the maximum number of conversations to store in case the previous **session_enabled** is set to true.

The **gatekeeper** section is to enable or disable the use of the system settings for content safety and presidio. If set to false, then the options array will be used to identify the specific gatekeepers to use.

The **orchestrator** is the name of the orchestrator to use for the agent. The orchestrator is the component that is responsible for managing the flow of the conversation and the execution of the agent's logic. It could be **LangChain** or **Semantic Kernel** and more options could be used in the future with the growth of the platform and the industry for orchestrators.

The **prompt** is the reference to the prompt that the agent will use to start the conversation. The prompt is a resource that is used to start the conversation with the agent. It is a JSON file that contains the prompt text and the prompt settings.

## Prompt References

The second folder **FoundationaLLM.Prompt** contains the Prompt References.
Within that folder, we have the **_prompt-references** JSON file that contains the following structure:

```json
{
	"PromptReferences": [
		{
			"Name": "sotu5",
			"Filename": "/FoundationaLLM.Prompt/sotu5.json"
		},
		{
			"Name": "sotu-test",
			"Filename": "/FoundationaLLM.Prompt/sotu-test.json"
		},
		{
			"Name": "sotu",
			"Filename": "/FoundationaLLM.Prompt/sotu.json"
		}
	]
}
```
These references point to the JSON files that contain the prompt information. Let's take a look at one of the prompts from above called **sotu5.json** for an example:

```json
{
  "name": "sotu5",
  "type": "prompt",
  "object_id": "/instances/1bc45134-6985-48b9-9466-c5f70ddaaa65/providers/FoundationaLLM.Prompt/prompts/sotu5",
  "description": "Prompt for the Knowledge Management Agent that queries the State of the Union speech transcript",
  "prefix": "You are a political science professional named Baldwin. You are responsible for answering questions regarding the February 2023 State of the Union Address.\nAnswer only questions about the February 2023 State of the Union address. Do not make anything up. Check your answers before replying.\nProvide concise answers that are polite and professional.",
  "suffix": ""
}
```

It contains the name, type of **prompt**, the object_id reference, description and of course most importantly the **prefix** and **suffix** of the prompt. The prefix and suffix are the text that will be used to start and end the conversation with the agent.

## Data Source References

A Data Source refers to the location of data that is to be leveraged by an agent. The data source could be a storage account, database, website, etc. The data source references are stored in the **FoundationaLLM.DataSource** folder.

The references are stored in the **_datasource-references** JSON file that contains the following structure:

```json
{
    "DataSourceReferences": [
        {
            "Name": "datalake01",
            "Filename": "/FoundationaLLM.DataSource/datalake01.json",
            "Type": "azure-data-lake",
            "Deleted": false
        },
        {
            "Name": "sharepointsite01",
            "Filename": "/FoundationaLLM.DataSource/sharepointsite01.json",
            "Type": "sharepoint-online-site",
            "Deleted": false
        }
    ]
}
```

These references point to the JSON files that contain the prompt information. Let's take a look at one of the prompts from above called **datalake01.json** for an example:

```json
{
  "name": "datalake01",
  "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.DataSource/dataSources/datalake01",
  "display_name": null,
  "description": "Azure Data Lake data source.",
  "folders": [
    "/vectorization-input/journals/2024"
  ],
  "configuration_references": {
    "AuthenticationType": "FoundationaLLM:DataSources:datalake01:AuthenticationType",
    "ConnectionString": "FoundationaLLM:DataSources:datalake01:ConnectionString",
    "APIKey": "FoundationaLLM:DataSources:datalake01:APIKey",
    "Endpoint": "FoundationaLLM:DataSources:datalake01:Endpoint"
  },
  "created_on": "0001-01-01T00:00:00+00:00",
  "updated_on": "0001-01-01T00:00:00+00:00",
  "created_by": null,
  "updated_by": null,
  "deleted": false
}
```

In this example, the data source is an Azure Data Lake data source. The **folders** array contains the paths to the folders in the data lake that contain the data to be used by the agent. The **configuration_references** section contains the references to the configuration settings that are used to connect to the data source. The **created_on**, **updated_on**, **created_by**, **updated_by** are the timestamps and the user that created and updated the data source. The **deleted** flag is used to mark the data source as deleted.

## Vectorization Profile References

Finally the third folder **FoundationaLLM.Vectorization** contains the Vectorization References.

![](../../../media/RS-Provider-3.png)

Where you will find important JSON files:

- **vectorization-indexing-profiles.json**
```json
{
    "DefaultResourceName": "AzureAISearch_Default_002",
    "Resources": [        
        {
            "type": "indexing-profile",
            "name": "AzureAISearch_Default_002",
            "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/indexingprofiles/AzureAISearch_Default_002",
            "description": null,
            "deleted": false,
            "indexer": "AzureAISearchIndexer",
            "settings": {
                "IndexName": "fllm-default-002",
                "TopN": "3",
                "Filters": "",
                "EmbeddingFieldName": "Embedding",
                "TextFieldName": "Text"
            },
            "configuration_references": {               
                "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
                "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
            }
        }
    ]
}
```

This is where we identify the name and the Indexer to use for the indexing of the content. And within the **configuration_references** section, we identify the AuthenticationType and Endpoint to use for the indexing. It could be indexing against the Azure AI Search or any other indexer that is available in the system and more will be supported in the future. The **DefaultResourceName** is the name of the default indexing profile to use in the system if none is specified.

- **vectorization-text-embedding-profiles.json**
```json
{
    "DefaultResourceName": "AzureOpenAI_Embedding",
    "Resources": [
     {
            "type": "text-embedding-profile",
            "name": "AzureOpenAI_Embedding",
            "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            "description": null,
            "deleted": false,
            "text_embedding": "SemanticKernelTextEmbedding",
            "settings": {},
            "configuration_references": {
                "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
                "APIVersion": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIVersion",
                "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
                "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
                "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
            }
        }
    ]
}
```
This is where we identify the name and the Text Embedding to use for the vectorization of the text. And within the **configuration_references** section, we identify the APIKey, APIVersion, AuthenticationType, DeploymentName and Endpoint to use for the text embedding.

- **vectorization-text-partitioning-profiles.json**
```json
{
    "DefaultResourceName": "DefaultTokenTextPartition_Small",
    "Resources": [
        {
            "type": "text-partitioning-profile",
            "name": "DefaultTokenTextPartition_Small",
            "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/textpartitioningprofiles/DefaultTokenTextPartition_Small",
            "display_name": null,
            "description": null,
            "text_splitter": "TokenTextSplitter",
            "settings": {
                "Tokenizer": "MicrosoftBPETokenizer",
                "TokenizerEncoder": "cl100k_base",
                "ChunkSizeTokens": "500",
                "OverlapSizeTokens": "50"
            },
            "configuration_references": {},
            "created_on": "0001-01-01T00:00:00+00:00",
            "updated_on": "0001-01-01T00:00:00+00:00",
            "created_by": null,
            "updated_by": null,
            "deleted": false
        }
    ]
}
```
This is where we identify the name and the Text Splitter to use for the chunking and overlapping of the text.
In the settings section, we identify the tokenizer and the encoder to use for the text partitioning and the chunk size and overlap size in tokens.

- **vectorization-pipelines.json**

A vectorization pipeline provides a definition for a reusable and triggerable profile that includes identifying a data source that is the source for vectorization, vectorization profiles, as well as the trigger type.

```json
{
    "DefaultResourceName": "sdzwa",
    "Resources": [
        {
            "type": "vectorization-pipeline",
            "name": "sdzwa",
            "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/vectorizationPipelines/sdzwa",
            "display_name": null,
            "description": "Vectorization data pipeline dedicated to the sdzwa january 2024 pdf.",
            "active": false,
            "data_source_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.DataSource/dataSources/datalake01",
            "text_partitioning_profile_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/Streamline",
            "text_embedding_profile_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding_v2",
            "indexing_profile_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/indexingprofiles/AzureAISearch_Default_002",
            "trigger_type": "Event",
            "trigger_cron_schedule": null,
            "created_on": "0001-01-01T00:00:00+00:00",
            "updated_on": "0001-01-01T00:00:00+00:00",
            "created_by": null,
            "updated_by": null,
            "deleted": false
        }
    ]
}
```

## Vectorization Request Resources

The storage of vectorization request resources are located in the `vectorization-state` container following the standard organization of `/requests/yyyyMMdd/yyyyMMdd-vectorizationrequestid.json` where `yyyyMMdd` (UTC) is the date of the request and `vectorizationrequestid` is the unique identifier of the request.

When a vectorization request is received, the request gets created and is updated once the request has been processed. An example of a completed vectorization request is:

```json
{
    "id": "f8d940a2-77c0-4b3e-8709-e26445f9743e",
    "object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/vectorizationRequests/f8d940a2-77c0-4b3e-8709-e26445f9743e",
    "Expired": false,
    "resource_filepath": "requests/20240419/20240419-f8d940a2-77c0-4b3e-8709-e26445f9743e.json",
    "content_identifier": {
        "data_source_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.DataSource/dataSources/datalake01",
        "multipart_id": [
            "fllmaks14sa.dfs.core.windows.net",
            "vectorization-input",
            "sdzwa/journals/2024/SDZWA-Journal-January-2024.pdf"
        ],
        "canonical_id": "sdzwa/journals/2024/SDZWA-Journal-January-2024",
        "metadata": null
    },
    "processing_type": "Asynchronous",
    "pipeline_object_id": "/instances/1e22cd2a-7b81-4160-b79f-f6443e3a6ac2/providers/FoundationaLLM.Vectorization/vectorizationPipelines/sdzwa",
    "pipeline_execution_id": "541ae81c-08ea-4ba0-b58c-9d904260a5a2",
    "processing_state": "Completed",
    "execution_start": "2024-04-19T03:57:33.4571856Z",
    "execution_end": "2024-04-19T04:00:41.8572236Z",
    "error_messages": [],
    "steps": [
        {
            "id": "extract",
            "parameters": {}
        },
        {
            "id": "partition",
            "parameters": {
                "text_partitioning_profile_name": "Streamline"
            }
        },
        {
            "id": "embed",
            "parameters": {
                "text_embedding_profile_name": "AzureOpenAI_Embedding_v2"
            }
        },
        {
            "id": "index",
            "parameters": {
                "indexing_profile_name": "AzureAISearch_Default_002"
            }
        }
    ],
    "completed_steps": [
        "extract",
        "partition",
        "embed",
        "index"
    ],
    "remaining_steps": [],
    "current_step": null,
    "error_count": 0,
    "running_operations": {},
    "last_successful_step_time": "2024-04-19T04:00:41.8554435Z"
}
```

Valid states for the `processing_state` property are `New`, `InProgress`, `Completed`, and `Failed`. Any errors encountered during the processing of a request are stored in the `error_messages` array.

>**Note**: Triggering the vectorization process is done through the Management API by issuing a `process` action on the resource. See the [Triggering Vectorization](../../vectorization/vectorization-triggering.md) section for more information.

### Synchronous Versus Asynchronous Vectorization

The vectorization process can be done in a synchronized or asynchronized manner.  The synchronized manner is when the vectorization process is done in real time **in memory** and the results are returned immediately.  The asynchronized manner is when the vectorization process is done in the background and the results are returned at a later time.  The asynchronized manner is useful when the vectorization process is expected to take a long time to complete and the user does not want to wait for the results.  The asynchronized manner is also useful when the vectorization process is expected to be done in batches and the user does not want to wait for the results of each batch.

For example, you would use syncronized vactorization when you have one or few files that you want to vectorize and you want the results immediately.  You would use asynchronized vectorization when you have hundred or thousands of files that you want to vectorize and you want the results at a later time.
