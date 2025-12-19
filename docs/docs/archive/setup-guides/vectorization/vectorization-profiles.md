# Managing vectorization profiles

The FoundationaLLM (FLLM) vectorization pipelines require the following types of profiles:

- [Data Sources](#data-sources)
- [Text partitioning profiles](#text-partitioning-profiles)
- [Text embedding profiles](#text-embedding-profiles)
- [Indexing profiles](#indexing-profiles)

## Data Sources

Data sources are managed with the `FoundationaLLM.DataSource` resource provider through the Management API. The structure of a data source profile is the following:

```json
{
  "type": "<data_source_type>",
  "name": "<name>",
  "object_id": "/instances/<instance_id>/providers/FoundationaLLM.DataSource/dataSources/<name>",
  "display_name": null,
  "description": "<description>",
  "<settings>": [
    "<value>"
  ],
  "configuration_references": {
    "AuthenticationType": "FoundationaLLM:DataSources:datalake01:AuthenticationType",
    "ConnectionString": "FoundationaLLM:DataSources:datalake01:ConnectionString"
  },
  "created_on": "0001-01-01T00:00:00+00:00",
  "updated_on": "0001-01-01T00:00:00+00:00",
  "created_by": null,
  "updated_by": null,
  "deleted": false
}
```

where:

- `<data_source_type>` is the type of the data source. The supported types are `AzureDataLake`, `SharePointOnline`, `WebSite` and `AzureSQLDatabase`.
- `<name>` is the name of the data source.
- `<instance_id>` is the unique identifier of the FLLM instance.
- `<description>` is the description of the Data Source.
- `<settings>` is a JSON object containing the data source settings, the name of the property varies by data source type.
- `<configuration_references>` is a JSON object containing the profile configuration references. The content of this object also varies by data source type.

The reminder of this section describes the configuration parameters for each of the supported Data Source types.

### `AzureDataLake`

```json
  "folders": [
    "/vectorization-input/sdzwa/journals/2024"
  ],
  "configuration_references": {
    "AuthenticationType": "FoundationaLLM:DataSources:datalake01:AuthenticationType",
    "ConnectionString": "FoundationaLLM:DataSources:datalake01:ConnectionString",
    "APIKey": "FoundationaLLM:DataSources:datalake01:APIKey",
    "Endpoint": "FoundationaLLM:DataSources:datalake01:Endpoint"
  },
```

The configuration parameters for `AzureDataLake` are the following:

| Parameter | Description |
| --- | --- |
| `folders` | The list of folders in the Azure Data Lake storage account that contain the data to be vectorized. |
| `configuration_references.AuthenticationType` | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`. |
| `configuration_references.ConnectionString` | The connection string to the Azure Storage account used for the the Azure Data Lake vectorization Data Source. |

### `SharePointOnline`

```json
  "site_url": "https://solliance.sharepoint.com/sites/foundationallm01",
  "document_libraries": [
    "/documents01"
  ],
  "configuration_references": {
    "ClientId": "FoundationaLLM:DataSources:sharepointsite01:ClientId",
    "TenantId": "FoundationaLLM:DataSources:sharepointsite01:TenantId",
    "CertificateName": "FoundationaLLM:DataSources:sharepointsite01:CertificateName",
    "KeyVaultURL": "FoundationaLLM:DataSources:sharepointsite01:KeyVaultURL"
  },
```

The configuration parameters for `SharePointOnline` are the following:

| Parameter | Description |
| --- | --- |
| `site_url` | The URL of the SharePoint Online site collection. |
| `document_libraries` | The list of document libraries in the SharePoint Online site collection that contain the data to be vectorized. |
| `configuration_references.CertificateName` | The name of the X.509 Certificate. The certificate must be valid and be uploaded into an Azure Key Vault certificate store. |
| `configuration_references.KeyVaultURL` | The URL of the KeyVault where the X.509 Certificate is stored. |
| `configuration_references.ClientId` | The Application (client) Id of the Microsoft Entra ID App Registration. See [Entra ID app registration for SharePoint Online Data Source](#entra-id-app-registration-for-sharepoint-online-data-source). |
| `configuration_references.TenantId` | The unique identifier of the SharePoint Online tenant. |

### `AzureSQLDatabase`

```json
  "tables": [
    "Table1"
  ],
  "configuration_references": {
    "ConnectionString": "FoundationaLLM:DataSources:sqldatabase01:ConnectionString",
    "AuthenticationType": "FoundationaLLM:DataSources:sqldatabase01:AuthenticationType"
  },
```

The configuration parameters for `AzureSQLDatabase` are the following:

| Parameter | Description |
| --- | --- |
| `tables` | The list of tables in the Azure SQL database that contain the data to be vectorized. |
| `configuration_references.ConnectionString` | The connection string to the Azure SQL database used for the Azure SQL Database vectorization Data Source. |
| `configuration_references.AuthenticationType` | The authentication type used to connect to the Azure SQL database. Can be one of `AzureIdentity` or `ConnectionString`. |

### Managing Data Sources

This section describes how to manage Data Sources using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/<name>
Content-Type: application/json

BODY
<data source>
```
where `<data source>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/<name>
```

> [!NOTE]
> The delete operation is a *logical delete*. To purge a Data Source, call the `/purge` endpoint after deleting the Data Source.

**Purge**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/<name>/purge
Content-Type: application/json

BODY
{}
```

**Check Name**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/checkname
Content-Type: application/json

BODY
{
  "name": "<name>"
}
```

## Text partitioning profiles

The structure of a text partitioning profile is the following:

```json
{
    "type": "text-partitioning-profile",
    "name": "<name>",
    "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/<name>",
    "description": "<description>",
    "deleted": false,
    "text_splitter": "<text_splitter>",
    "settings": {<profile_settings>},
    "configuration_references": {<profile_configuration_references>}
}
```

where:

- `<name>` is the name of the text partitioning profile.
- `<description>` is the description of the text partitioning profile.
- `<text_splitter>` is the type of the text splitter. The supported types are `TextTokenSplitter`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported text splitters.

### `TextTokenSplitter`

```json
"settings" : {
    "Tokenizer": "MicrosoftBPETokenizer",
    "TokenizerEncoder": "cl100k_base",
    "ChunkSizeTokens": "2000",
    "OverlapSizeTokens": "200"
},
"configuration_references": {}
```

The configuration parameters for `TokenTextSplitter` are the following:

| Parameter | Description |
| --- | --- |
| `settings.Tokenizer` | The tokenizer used to split the text into tokens. Currently, the only supported tokenizer is `MicrosoftBPETokenizer`. Under the hood, it uses the .NET equivalent of OpenAI's [tiktoken](https://github.com/openai/tiktoken). |
| `settings.TokenizerEncoder` | The encoder used by the tokenizer. Currently, the only supported encoder is `cl100k_base`. This encoder is the one currently used by Azure OpenAI (and OpenAI) in `gpt-3.5-turbo` and `gpt-4`. |
| `settings.ChunkSizeTokens` | The maximum number of tokens in each text chunk. |
| `settings.OverlapSizeTokens` | The maximum number of tokens that overlap between two consecutive chunks. |

### Managing text partitioning profiles

This section describes how to manage text partitioning profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/<name>
Content-Type: application/json

BODY
<text partitioning profile>
```

where `<text partitioning profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/<name>
```

> [!NOTE]
> The delete operation is a *logical delete*. To purge a Text Partitioning Profile, call the `/purge` endpoint after deleting the Text Partitioning Profile.

**Purge**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/<name>/purge
Content-Type: application/json

BODY
{}
```

## Text embedding profiles

The structure of a text embedding profile is the following:

```json
{
    "type": "text-embedding-profile",
    "name": "<name>",
    "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/<name>",
    "display_name": null,
    "description": "<description>",
    "text_embedding": "<text_embedding>",
    "settings": {<profile_settings>},
    "configuration_references": {<profile_configuration_references>},
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false
}
```

where:

- `<name>` is the name of the text embedding profile.
- `<description>` is the description of the text embedding profile.
- `<text_embedding>` is the type of the text embedder. The supported types are `SemanticKernelTextEmbedding` and `GatewayTextEmbedding`. `SemanticKernelTextEmbedding` supports synchronous and asynchronous vectorization, while `GatewayTextEmbedding` only supports asynchronous vectorization.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported text embedders.

### `SemanticKernelTextEmbedding`

```json
"settings": {},
"configuration_references": {
    "APIKey": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey",
    "APIVersion": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIVersion",
    "AuthenticationType": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType",
    "DeploymentName": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName",
    "Endpoint": "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
}
```

The configuration parameters for `SemanticKernelTextEmbedding` are the following:

| Parameter | Description |
| --- | --- |
| `configuration_references.APIKey` | The API key used to connect to the Azure OpenAI service. By default, this maps to the Azure OpenAI service deployed by FLLM. |
| `configuration_references.APIVersion` | The API version used to connect to the Azure OpenAI service. By default, this value is `2023-05-15`. |
| `configuration_references.AuthenticationType` | The authentication type used to connect to the Azure OpenAI service. Can be one of `AzureIdentity` or `APIKey`. By default, it is set to `APIKey`.|
| `configuration_references.DeploymentName` | The name of the Azure OpenAI model deployment. The default value is `embeddings`.|
| `configuration_references.Endpoint` | The endpoint of the Azure OpenAI service. By default, this maps to the Azure OpenAI service deployed by FLLM. |

### `GatewayTextEmbedding`

```json
"settings": {
  "model_name": "embeddings"
},
"configuration_references": {}
```

The settings for `GatewayTextEmbedding` are the following:

| Parameter | Description |
| --- | --- |
| `settings.model_name` | The name of the embeddings model deployment in Azure OpenAI Service. |

### Managing text embedding profiles

This section describes how to manage text embedding profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/<name>
Content-Type: application/json

BODY
<text embedding profile>
```

where `<text embedding profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/<name>
```

> [!NOTE]
> The delete operation is a *logical delete*. To purge a Text Embedding Profile, call the `/purge` endpoint after deleting the Text Embedding Profile.

**Purge**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles/<name>/purge
Content-Type: application/json

BODY
{}
```

## Indexing profiles

The structure of an indexing profile is the following:

```json
{
    "type": "indexing-profile",
    "name": "<name>",
    "object_id": "/instances/[INSTANCE ID]/providers/FoundationaLLM.Vectorization/indexingProfiles/<name>",
    "display_name": null,
    "description": "<description>",
    "deleted": false,
    "indexer": "<indexer>",
    "settings": {<profile_settings>},
    "configuration_references": {<profile_configuration_references>},
    "created_on": "0001-01-01T00:00:00+00:00",
    "updated_on": "0001-01-01T00:00:00+00:00",
    "created_by": null,
    "updated_by": null,
    "deleted": false
}
```

where:

- `<name>` is the name of the indexing profile.
- `<description>` is the description of the indexing profile.
- `<indexer>` is the type of the indexer. The supported types are `AzureAISearchIndexer`.
- `<profile_settings>` is a JSON object containing the profile settings.
- `<profile_configuration_references>` is a JSON object containing the profile configuration references.

The reminder of this section describes the configuration parameters for each of the supported indexers.

### `AzureAISearchIndexer`

```json
"settings": {
    "IndexName": "fllm-default-001",
    "TopN": "3",
    "Filters": "",
    "EmbeddingFieldName": "Embedding",
    "TextFieldName": "Text"
},
"configuration_references": {
    "APIKey": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey",
    "AuthenticationType": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType",
    "Endpoint": "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
}
```

The configuration parameters for `AzureAISearchIndexer` are the following:

| Parameter | Description |
| --- | --- |
| `settings.IndexName` | The name of the Azure AI Search index. If the index does not exist, it will be created. |
| `settings.TopN` | The number of embeddings closest to the index query to return. |
| `settings.Filters` | Optional filters to further refine the index search. |
| `settings.EmbeddingFieldName` | Field name of the embedding vector in the JSON documents returned by Azure AI Search. |
| `settings.TextFieldName` | Field name of the text equivalent of the embedding vector in the JSON documents returned by Azure AI Search. |
| `configuration_references.APIKey` | The API key used to connect to the Azure AI Search service. By default, this maps to the Azure AI Search service deployed by FLLM. |
| `ConfigurationReference.AuthenticationType` | The authentication type used to connect to the Azure AI Search service. Can be one of `AzureIdentity` or `APIKey`. By default, it is set to `APIKey`. |
| `configuration_references.Endpoint` | The endpoint of the Azure AI Search service. By default, this maps to the Azure AI Search service deployed by FLLM. |

### Managing indexing profiles

This section describes how to manage indexing profiles using the Management API.
`{{baseUrl}}` is the base URL of the Management API. `{{instanceId}}` is the unique identifier of the FLLM instance.

**Retrieve**

```
HTTP GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingProfiles
```

**Create or update**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingProfiles/<name>
Content-Type: application/json

BODY
<indexing profile>
```

where `<indexing profile>` is a JSON object with the structure described above.

**Delete**

```
HTTP DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingProfiles/<name>
```

> [!NOTE]
> The delete operation is a *logical delete*. To purge an Indexing Profile, call the `/purge` endpoint after deleting the Indexing Profile.

**Purge**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingProfiles/<name>/purge
Content-Type: application/json

BODY
{}
```

**Check Name**

```
HTTP POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Vectorization/indexingProfiles/checkname
Content-Type: application/json

BODY
{
  "name": "<name>"
}
```

## Additional configuration steps

### Entra ID app registration for SharePoint Online Data Source

Apps typically access SharePoint Online through certificates: Anyone having the certificate and its private key can use the app with the permissions granted to it.

1. Create a new **App registration** in your **Microsoft Entra ID** tenant. Next, provide a **Name** for your application and click on **Register** at the bottom of the blade.

2. Navigate to the **API Permissions** blade and click on **Add a permission** button Here you choose the permissions that you will grant to this application. Select **SharePoint** from the **Microsoft APIs** tab, then select **Application permissions** as the type of permissions required, choose the desired permissions (i.e. **Sites.Read.All**) and click on **Add permissions**. Here are the required scopes:

    - `Group.ReadWrite.All`
    - `User.ReadWrite.All`
    - `Sites.Read.All` OR `Sites.Selected`
      - `Sites.Read.All` will allow the application to read documents and list items in all site collections.
      - `Sites.Selected` will allow the application to access only a subset of site collections. The specific site collections and the permissions granted will be configured separately, in SharePoint Online.

3. The application permission requires admin consent in a tenant before it can be used. In order to do this, click on **API permissions** in the left menu again. At the bottom you will see a section **Grant consent**. Click on the **Grant admin consent for {{organization}}** button and confirm the action by clicking on the **Yes** button that appears at the top.

4. To invoke SharePoint Online with an app-only access token, you have to create and configure a **self-signed X.509 certificate**, which will be used to authenticate your application against Microsoft Entra ID. You can find additional details on how to do this in [this document](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

5. Next step is to register the certificate you created to this application. Click on **Certificates & secrets** blade. Next, click on the **Upload certificate** button, select the .CER file you generated earlier and click on **Add** to upload it. 

    To confirm that the certificate was successfully registered, click on **Manifest** blade and search for the `keyCredentials` property, which contains your certificate details. It should look like this:
    ```json
    "keyCredentials": [
        {
            "customKeyIdentifier": "<$base64CertHash>",
            "endDate": "yyyy-MM-ddThh:mm:ssZ",
            "keyId": "<$guid>",
            "startDate": "yyyy-MM-ddThh:mm:ssZ",
            "type": "AsymmetricX509Cert",
            "usage": "Verify",
            "value": "<$base64Cert>",
            "displayName": "CN=<$name of your cert>"
        }
    ]
    ```

6. Upload and store the certificate in the **KeyVault** where the FoundationaLLM Vectorization API has permissions to read **Secrets**. You will need the **Certificate Name** for the App Configuration settings listed in the table above.

    > **NOTE**
    >
    > Can I use other means besides certificates for realizing app-only access for my Azure AD app?
    >
    > **NO**, all other options are blocked by SharePoint Online and will result in an `Access Denied` message.

7. Create a new [Data Source using the Management API.](#data-sources) Ensure that you set the necessary App Configuration settings appropriately.
