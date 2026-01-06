# Monitoring and troubleshooting vectorization

The typical steps you have to perform when monitoring and troubleshooting vectorization in FoundationaLLM (FLLM) are:

1. Check the configuration of the Vectorization API and Vectorization Worker. For more details, see [Configuring vectorization](vectorization-configuration.md).
2. Check the working condition of the Management API, Vectorization API and Vectorization Worker(s). Ensure the services have started and initialized successfully.
3. Check the status endpoints for the Core API, Vectorization API and the Management API. You can do this by submitting a HTTP GET request to the `/status` endpoint of these APIs and validate that you get a HTTP 200 OK response with body like `<api_name> - ready`.
4. Check the logs of the Management Vectorization API and Vectorization Worker(s) for errors. By default, the logs are written to the Azure App Insights Log Analytics Workspace deployed by FLLM.
5. Check the definitions of the vectorization profiles used in the vectorization requests. For more details, see [Managing vectorization profiles](vectorization-profiles.md). Ensure all the required app configuration elements are present and have the correct values.
6. Check the state of the vectorization requests. By default, the vectorization requests are stored in the `vectorization-state` container of the Azure Storage account deployed by FLLM.

## State and logging of vectorization requests

All state and logging of vectorization requests are stored in the `vectorization-state` container of the Azure Storage account deployed by FLLM.

### Vectorization request resource files

Each vectorization request resource is stored in the `vectorization-state/requests` folder. The request resources are created and managed through the Management API. The naming convention is: `vectorization-state/requests/<request_id>-<yyyMMdd>.json`.

The resource file is updated as the vectorization request progresses through the processing. The resource file contains the following fields that can assist in troubleshooting:

| Field | Description |
| --- | --- |
| `id` | The unique identifier of the vectorization request. When looking up the subsequent execution state, this is the identifier that is used in the file name. |
| `content-identifier.canonical_id` | The canonical id of the vectorization request. This is the path within the `execution-state` folder where additional logs and associated vectorization artifacts are stored. |
| `processing-state` | The current state of the vectorization request, values can be `New`, `InProgress`, `Completed`, `Failed` |
| `error_messages` | A high level list of error messages encountered during processing. |
| `current_step` | The step currently being executed, or the step in which a failure occurred. |
| `pipeline_object_id` | When created through a vectorization pipeline, this field contains the object id of the pipeline. |
| `pipeline_execution_id` | When `process` is initiated through a vectorization pipeline, this field contains the unique identifier of the pipeline execution. |

### Vectorization execution state files

The execution state of a vectorization request is stored in the `vectorization-state/execution-state` folder. The naming convention is: `vectorization-state/execution-state/<canonical_id>/<file_name>_state_<request_id>.json`. The execution state file provide verbose details about the request that is updated as the vectorization request is processed. This file records generated assets and logs. Error messages can be found in the `log` field.

### Vectorization pipeline state files

The state of the vectorization pipeline is stored in the `vectorization-state/pipeline-state` folder. The naming convention is: `vectorization-state/pipeline-state/<pipeline_name>/<pipeline_name>-<pipeline_execution_id>.json`. The pipeline state records associated vectorization requests that are processed together in a single pipeline in the `vectorization_requests` field. The overall pipeline state is calculated based on the states of the collection of vectorization requests, this state is calculated by the following table in order:

| Condition | Pipeline state |
| --- | --- |
| At least one request is `InProgress` | `InProgress` |
| All requests are `Completed` | `Completed` |
| At least one request is `Failed` | `Failed` |
| All requests are `New` or there are no requests being tracked. | `New` |

You can use the Management API with the object id of the request to retrieve the vectorization request resource that contains a high level overview of any errors that have occurred. If more detailed information is required, then reviewing the execution state file is recommended.
