# Data Pipelines API

API reference for managing data pipelines.

## Overview

The Data Pipelines API allows you to create, manage, and execute data pipelines programmatically.

## Endpoints

### List Data Pipelines
```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines
```

### Get Data Pipeline
```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
```

### Create Data Pipeline
```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
Content-Type: application/json

{
  "name": "my-pipeline",
  "description": "Pipeline description",
  "data_source_object_id": "/instances/{instanceId}/providers/FoundationaLLM.DataSource/dataSources/{sourceName}",
  "text_partitioning_profile_object_id": "...",
  "text_embedding_profile_object_id": "...",
  "indexing_profile_object_id": "...",
  "trigger_type": "Manual"
}
```

### Execute Data Pipeline
```http
POST /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}/process
Content-Type: application/json

{}
```

### Delete Data Pipeline
```http
DELETE /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelines/{pipelineName}
```

## Pipeline Runs

### List Pipeline Runs
```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns
```

### Get Pipeline Run
```http
GET /instances/{instanceId}/providers/FoundationaLLM.DataPipeline/dataPipelineRuns/{runId}
```

## Related Topics

- [Creating Data Pipelines](../../../management-portal/how-to-guides/data/data-pipelines/creating-data-pipelines.md)
- [Data Pipelines Reference](../../../management-portal/reference/concepts/data-pipelines.md)
