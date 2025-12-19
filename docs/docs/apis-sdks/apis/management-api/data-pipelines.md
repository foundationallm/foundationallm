# Data Pipelines API

API reference for managing data pipelines programmatically through the Management API.

## Overview

The Data Pipelines API enables:

- Creating and configuring data pipelines
- Executing pipeline runs
- Monitoring pipeline execution history
- Managing pipeline lifecycle

## Resource Provider

**Provider:** `FoundationaLLM.DataPipeline`

**Resource Types:**
- `dataPipelines` - Pipeline definitions
- `dataPipelineRuns` - Execution history

## Pipeline Endpoints

### List Data Pipelines

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
[
  {
    "resource": {
      "type": "data-pipeline",
      "name": "documents-pipeline",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/documents-pipeline",
      "display_name": "Documents Pipeline",
      "description": "Processes documents from Azure Storage",
      "active": true
    },
    "roles": ["Owner"],
    "actions": ["read", "write", "delete", "process"]
  }
]
```

---

### Get Data Pipeline

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
{
  "resource": {
    "type": "data-pipeline",
    "name": "documents-pipeline",
    "object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/documents-pipeline",
    "display_name": "Documents Pipeline",
    "description": "Processes documents from Azure Storage",
    "active": true,
    "data_source_object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources/azure-storage",
    "stages": [
      {
        "name": "text-extraction",
        "type": "TextExtraction",
        "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/text-extractor",
        "parameters": {},
        "next_stages": ["text-partitioning"]
      },
      {
        "name": "text-partitioning",
        "type": "TextPartitioning",
        "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/text-partitioner",
        "parameters": {
          "chunk_size": 1000,
          "chunk_overlap": 200
        },
        "next_stages": ["embedding"]
      },
      {
        "name": "embedding",
        "type": "Embedding",
        "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/embedder",
        "parameters": {},
        "next_stages": ["indexing"]
      },
      {
        "name": "indexing",
        "type": "Indexing",
        "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/indexer",
        "parameters": {
          "index_name": "documents-index"
        },
        "next_stages": []
      }
    ],
    "trigger": {
      "type": "Manual"
    }
  },
  "roles": ["Owner"],
  "actions": ["read", "write", "delete", "process"]
}
```

---

### Create Data Pipeline

```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body:**

```json
{
  "type": "data-pipeline",
  "name": "my-pipeline",
  "display_name": "My Pipeline",
  "description": "Pipeline description",
  "active": true,
  "data_source_object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources/azure-storage",
  "stages": [
    {
      "name": "text-extraction",
      "type": "TextExtraction",
      "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/text-extractor",
      "parameters": {},
      "next_stages": ["text-partitioning"]
    },
    {
      "name": "text-partitioning",
      "type": "TextPartitioning",
      "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/text-partitioner",
      "parameters": {
        "chunk_size": 1000,
        "chunk_overlap": 200
      },
      "next_stages": ["embedding"]
    },
    {
      "name": "embedding",
      "type": "Embedding",
      "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/embedder",
      "parameters": {},
      "next_stages": ["indexing"]
    },
    {
      "name": "indexing",
      "type": "Indexing",
      "plugin_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/indexer",
      "parameters": {
        "index_name": "my-index"
      },
      "next_stages": []
    }
  ],
  "trigger": {
    "type": "Manual"
  }
}
```

**Response (200 OK):**

```json
{
  "object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/my-pipeline"
}
```

---

### Update Data Pipeline

Same endpoint as create - use `POST` with the updated pipeline definition.

---

### Delete Data Pipeline

```http
DELETE /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
Authorization: Bearer <token>
```

---

### Purge Data Pipeline

```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}/purge
Authorization: Bearer <token>
```

---

### Execute Data Pipeline

Trigger a pipeline run:

```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}/process
Content-Type: application/json
Authorization: Bearer <token>
```

**Request Body (Optional):**

```json
{
  "run_type": "full",
  "parameters": {}
}
```

**Response (202 Accepted):**

```json
{
  "run_id": "run-guid",
  "status": "Pending",
  "message": "Pipeline execution started"
}
```

---

## Pipeline Runs Endpoints

### List Pipeline Runs

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns
Authorization: Bearer <token>
```

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `pipelineName` | string | Filter by pipeline name |
| `status` | string | Filter by status (Pending, Running, Completed, Failed) |
| `startTime` | datetime | Filter by start time |

**Response (200 OK):**

```json
[
  {
    "resource": {
      "type": "data-pipeline-run",
      "name": "run-guid",
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns/run-guid",
      "pipeline_object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/documents-pipeline",
      "pipeline_name": "documents-pipeline",
      "status": "Completed",
      "start_time": "2024-01-15T10:30:00Z",
      "end_time": "2024-01-15T10:45:00Z",
      "documents_processed": 150,
      "documents_failed": 2
    }
  }
]
```

---

### Get Pipeline Run

```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns/{runId}
Authorization: Bearer <token>
```

**Response (200 OK):**

```json
{
  "resource": {
    "type": "data-pipeline-run",
    "name": "run-guid",
    "object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns/run-guid",
    "pipeline_object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/documents-pipeline",
    "pipeline_name": "documents-pipeline",
    "status": "Completed",
    "start_time": "2024-01-15T10:30:00Z",
    "end_time": "2024-01-15T10:45:00Z",
    "documents_processed": 150,
    "documents_failed": 2,
    "stage_results": [
      {
        "stage_name": "text-extraction",
        "status": "Completed",
        "items_processed": 150,
        "items_failed": 0,
        "duration_seconds": 120
      },
      {
        "stage_name": "text-partitioning",
        "status": "Completed",
        "items_processed": 150,
        "items_failed": 0,
        "duration_seconds": 60
      }
    ]
  }
}
```

---

## Pipeline Structure

### Stage Types

| Type | Description |
|------|-------------|
| `TextExtraction` | Extract text from documents |
| `TextPartitioning` | Split text into chunks |
| `Embedding` | Generate vector embeddings |
| `Indexing` | Store in vector database |
| `ImageDescription` | Generate descriptions for images |

### Trigger Types

| Type | Description |
|------|-------------|
| `Manual` | Triggered via API or UI |
| `Scheduled` | Runs on a schedule |
| `Event` | Triggered by events (e.g., new files) |

### Run Status Values

| Status | Description |
|--------|-------------|
| `Pending` | Queued for execution |
| `Running` | Currently executing |
| `Completed` | Finished successfully |
| `Failed` | Execution failed |
| `Cancelled` | Cancelled by user |

---

## Code Examples

### Python

```python
import requests

base_url = "https://management-api.example.com"
instance_id = "your-instance-id"
token = "your-bearer-token"

headers = {
    "Authorization": f"Bearer {token}",
    "Content-Type": "application/json"
}

# List pipelines
response = requests.get(
    f"{base_url}/instances/{instance_id}/providers/FoundationaLLM.DataPipeline/dataPipelines",
    headers=headers
)
pipelines = response.json()

# Execute pipeline
response = requests.post(
    f"{base_url}/instances/{instance_id}/providers/FoundationaLLM.DataPipeline/dataPipelines/my-pipeline/process",
    headers=headers,
    json={"run_type": "full"}
)
run = response.json()
print(f"Started run: {run['run_id']}")
```

### PowerShell

```powershell
$baseUrl = "https://management-api.example.com"
$instanceId = "your-instance-id"
$token = "your-bearer-token"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# List pipelines
$pipelines = Invoke-RestMethod `
    -Uri "$baseUrl/instances/$instanceId/providers/FoundationaLLM.DataPipeline/dataPipelines" `
    -Headers $headers

# Execute pipeline
$run = Invoke-RestMethod `
    -Uri "$baseUrl/instances/$instanceId/providers/FoundationaLLM.DataPipeline/dataPipelines/my-pipeline/process" `
    -Method Post `
    -Headers $headers `
    -Body '{"run_type": "full"}'

Write-Host "Started run: $($run.run_id)"
```

---

## Related Topics

- [Management API Overview](index.md)
- [Creating Data Pipelines](../../../management-portal/how-to-guides/data/data-pipelines/creating-data-pipelines.md)
- [Monitoring Data Pipelines](../../../management-portal/how-to-guides/data/data-pipelines/monitoring-data-pipelines.md)
- [Data Pipelines Concepts](../../../management-portal/reference/concepts/data-pipelines.md)
