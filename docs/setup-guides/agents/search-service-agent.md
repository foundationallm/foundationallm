# Search Service Agent

This agent is used to perform a hybrid search against an index in Azure AI Search.

## Backend configuration

Backend configuration includes adding Azure AI Search key value into Azure Key Vault and Azure App Configuration.
Access the Azure AI Search service in the Azure portal to retrieve its access key and endpoint values.

### Add Key Vault secret for Azure AI Search

The key can be named anything relevant to the Azure AI Search Service resource and the role it plays in the organization. Use the following as an example of adding a secret to the Key Vault:

- **Name**: `foundationallm-cognitivesearch-key`
- **Value**: The value of the Azure AI Search Service access key.

### Add setting to App Configuration

A new key vault reference app configuration value must be added to Azure App Configuration. This key vault reference can be named anything relevant to the role of the Azure AI Search resource in your organization, the following is an example:

- **Key**: `FoundationaLLM:CognitiveSearch:Key`
- **Secret**: The key vault reference should should point to the key vault secret created above.

## Blob storage files

The following metadata files should be added to blob storage, and will be used to assemble the agent and underlying data store.

## Data source JSON

To configure the connection to the underlying Azure AI Search Index, a JSON file should be added into the `data-sources` container in blob storage with the following structure. The file should be saved with a name of `{data-source-name}.json`, where `{data-source-name}` reflects the name of the index being queried or a shortened version of it. As an example, for an index named `sotu`, the data source name could be `sotu-ds`.

```json
{
    "name": "{data-source-name}",
    "underlying_implementation": "search-service",    
    "authentication": {
        "endpoint": "{search-service-endpoint}",
        "key_secret": "{access-key-secret-name}"
    },   
    "index_name": "{index-name}",
    "top_n" : {number-of-vector-results},
    "embedding_field_name": "{embedding-field-name}",
    "text_field_name": "{text-field-name}",
    "description": "{description-of-index}"
}
```

| Field | Description |
| --- | --- |
| {data-source-name} | The name of the data source. |
| {search-service-endpoint} | The endpoint of the Azure AI Search service. |
| {access-key-secret-name} | The name of the access key configuration in App Configuration. |
| {index-name} | The name of the index to query. |
| {number-of-vector-results} | Limits the number of results to return from the Azure AI Search index. |
| {embedding-field-name} | The name of the field in the Azure AI Search index that contains the vector embeddings. |
| {text-field-name} | The name of the field in the Azure AI Search index that contains the text to be searched. |
| {description-of-index} | A general description of the index. |

## Agent JSON

A JSON file should be added into the `agents` container in blob storage. The name of the file should be the `{agent-name}.json`. For Azure AI Search Service agents, the file should look like the following:

```json
{
    "name": "{agent-name}",
    "type": "search-service",
    "description": "{agent-description}",
    "allowed_data_source_names": [ {data-source-name} ],
    "language_model": {
        "type": "openai",
        "provider": "microsoft",
        "temperature": 0,
        "use_chat": true
    },
    "embedding_model": {
        "type": "openai",
        "provider": "microsoft",
        "deployment": "embeddings",
        "model": "text-embedding-ada-002",
        "chunk_size": 10
    },
    "orchestrator": "LangChain"
}
```

The names added to the `allowed_data_source_names` list should be valid data source files in the `data_sources` container, minus the `.json`. For example, if there is a data source file named `sotu-ds.json`, the string entered into the `allowed_data_source_names` list would be `"sotu-ds"`.

The `temperature` value can be a float between 0.0 and 1.0.

The language model and embedding model can be replaced depending on the environment. These entries are reflective of the default installation of FoundationaLLM.

## Prompt text file

The prompt for the agent should be added as a file named `default.txt` into a folder that matches the `{agent-name}` within the `prompts` container.

In the prompt, describe the function of the agent in detail, providing any guidelines and instructions in a clear and concise way. The following is an example of a prompt for an agent that searches the State of the Union addresses:

```text
You are a political science professional named Baldwin. You are responsible for answering questions regarding the February 2023 State of the Union Address.
Answer only questions about the February 2023 State of the Union address. Use only the information provided in the context.
Provide concise answers that are polite and professional.
```
