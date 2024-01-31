# Triggering vectorization

Vectorization pipelines are started when the Vectorization API receives a vectorization request. The following types of triggers are supported:

- None (no triggering of vectorization pipelines).
- Manual (vectorization pipelines are triggered manually by calling the Vectorization API). The typical use cases for on-demand vectorization (either synchronous or asynchronous) are testing, manual vectorization (or re-vectorization), and application integration (where another platform component triggers vectorization).
- Content-based (vectorization pipelines are triggered automatically when either new content is added to a content source or existing content is updated).
- Schedule-based (vectorization pipelines are triggered automatically based on a schedule).

> [!NOTE]
> Content-based and schedule-based triggering are currently in pre-release and are not yet available in public releases of FLLM.

## Vectorization requests

The typical structure of a vectorization request is the following

```json
{
    "id": "<unique_identifier>",
    "content_identifier": {
        "content_source_profile_name": "<name>",
        "multipart_id": [
            "https://xyz.blob.core.windows.net",
            "vectorization-input",
            "The-fabulous-life-of-Jack-the-Cat.pdf"
        ],
        "canonical_id": "friends/stories/The-fabulous-life-of-Jack-the-Cat"
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
                "indexing_profile_name": "AzureAISearch_Default_001"
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

The following table describes the properties of a vectorization request.

| Property | Description |
| --- | --- |
| `id` | A unique identifier for the vectorization request. The caller is responsible for the generation of this identifier. The recommended format is GUID. |
| `content_identifier` | The content identifier of the content to be vectorized. |
| `content_identifier.content_source_profile_name` | The name of the content source profile to be used for loading the content. |
| `content_identifier.multipart_id` | The multipart ID of the content to be vectorized. The multipart ID is a list of strings that uniquely identifies the content. The multipart ID is specific to the content source profile. |
| `content_identifier.canonical_id` | The canonical ID of the content to be vectorized. The canonical ID is a string that uniquely identifies the content in a logical namespace. The caller is responsible for the generation of this identifier. The identifier should have a path form (using the `/` separator). The last part of the path should always be equal to the file name (without its extension). |
| `processing_type` | The type of processing to be performed. The following values are supported: `Synchronous` and `Asynchronous`. See [Vectorization concepts](./vectorization-concepts.md) for more details. |
| `steps` | The vectorization steps to be executed. Most vectorization requests will contain the full set of standard steps: `extract`, `partition`, `embed`, and `index`. Each step (except for the `extract` one) will contain one parameter specifying the name of the associated vectorization profile name. |
| `completed_steps` | Must always be an empty array. |
| `remaining_steps` | Must always be an array with the step names to be executed (order sensitive). |

### Submitting vectorization requests

This section describes how to submit vectorization requests using the Vectorization API.
`{{baseUrl}}` is the base URL of the Vectorization API.

```
POST {{baseUrl}}/vectorizationrequest
Content-Type: application/json
X-API-KEY: <vectorization_api_key>

BODY
<vectorization_request>
```

where <vectorization_api_key> is the API key of the Vectorization API and `<vectorization_request>` is the vectorization request to be submitted.
