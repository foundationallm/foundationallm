# Data Pipeline Management

This guide explains how to create, manage, and monitor data pipelines using the Management Portal.

## Overview

Data pipelines in FoundationaLLM process documents from various sources (SharePoint, Azure Data Lake, etc.) through a series of stages to extract, transform, and load content into searchable indexes.

## Pipeline Components

A data pipeline consists of:

| Component | Description |
|-----------|-------------|
| **Data Source** | Where content comes from (ADLS, SharePoint, etc.) |
| **Stages** | Processing steps (extract, partition, embed, index) |
| **Triggers** | When the pipeline runs (manual, scheduled, event) |
| **Parameters** | Configurable values for each stage |

## Creating a Data Pipeline

### Step 1: Navigate to Data Pipelines

1. Open the **Management Portal**
2. In the side navigation, click **Data Pipelines**
3. Click **Create Pipeline**

<!-- [TODO: Add screenshot of Data Pipelines page] -->

### Step 2: Configure Basic Information

| Field | Description |
|-------|-------------|
| **Name** | Unique identifier (alphanumeric, underscores, hyphens) |
| **Display Name** | User-friendly name |
| **Description** | Purpose of this pipeline |
| **Active** | Enable/disable the pipeline |

<!-- [TODO: Add screenshot of basic information form] -->

### Step 3: Select Data Source

1. Click **Add Data Source**
2. Choose from available data sources:
   - Azure Data Lake
   - SharePoint Online
   - Website
   - Azure SQL Database

3. Configure data source parameters:

| Parameter | Description |
|-----------|-------------|
| **Plugin** | The plugin that processes this data source |
| **Data Source Object** | Reference to the configured data source |
| **Folders/Libraries** | Specific paths to process |

<!-- [TODO: Add screenshot of data source selection] -->

### Step 4: Configure Pipeline Stages

Add stages to define how content is processed:

#### Text Extraction Stage

Extracts text from binary documents (PDF, DOCX, etc.):

| Parameter | Description |
|-----------|-------------|
| **Plugin** | Text extraction plugin to use |

#### Text Partitioning Stage

Splits extracted text into chunks:

| Parameter | Description | Default |
|-----------|-------------|---------|
| **Partitioning Strategy** | `Token` or `Semantic` | Token |
| **Partition Size (tokens)** | Chunk size | 400 |
| **Partition Overlap (tokens)** | Overlap between chunks | 100 |

#### Text Embedding Stage

Generates vector embeddings for text chunks:

| Parameter | Description | Default |
|-----------|-------------|---------|
| **Embedding Model** | Model for embeddings | text-embedding-3-large |
| **Embedding Dimensions** | Vector dimensions | 2048 |

#### Indexing Stage

Stores embeddings in a vector index:

| Parameter | Description |
|-----------|-------------|
| **API Endpoint Configuration** | Azure AI Search endpoint |
| **Index Name** | Name of the target index |
| **Index Partition Name** | Logical partition within index |

<!-- [TODO: Add screenshots of each stage configuration] -->

### Step 5: Configure Triggers

Set how the pipeline is invoked:

#### Manual Trigger

Pipeline runs only when explicitly triggered:

```json
{
  "triggers": []
}
```

#### Scheduled Trigger

Pipeline runs on a schedule (cron format):

| Schedule | Cron Expression |
|----------|-----------------|
| Daily at 6 AM | `0 6 * * *` |
| Every hour | `0 * * * *` |
| Weekly Sunday | `0 0 * * 0` |

#### Event Trigger

Pipeline runs when content changes (coming soon).

<!-- [TODO: Document event trigger when available] -->

### Step 6: Save and Activate

1. Review all configuration
2. Click **Save Pipeline**
3. Toggle **Active** to enable
4. Pipeline is ready to run

## Running a Data Pipeline

### Manual Execution

1. Navigate to **Data Pipelines**
2. Find your pipeline in the list
3. Click **Run** or the play button
4. Monitor progress in the status panel

### Providing Parameter Values

For manual runs, you may need to provide parameter values:

| Parameter | Description |
|-----------|-------------|
| Source folders | Specific paths to process |
| Partition settings | Override default chunk sizes |
| Index settings | Target index configuration |

## Monitoring Pipeline Status

### Pipeline Run States

| State | Description |
|-------|-------------|
| **Pending** | Pipeline queued for execution |
| **Running** | Pipeline actively processing |
| **Completed** | Pipeline finished successfully |
| **Failed** | Pipeline encountered an error |
| **Cancelled** | Pipeline was manually stopped |

<!-- [TODO: Add screenshot of pipeline status panel] -->

### Viewing Run History

1. Click on a pipeline name
2. Navigate to **Run History** tab
3. View past executions with:
   - Start/end times
   - Duration
   - Status
   - Error messages (if failed)

### Stage-Level Status

For running pipelines, view individual stage progress:

| Information | Description |
|-------------|-------------|
| **Stage Name** | Current processing stage |
| **Progress** | Items processed / total items |
| **Duration** | Time spent on stage |
| **Errors** | Any stage-level errors |

<!-- [TODO: Add screenshot of stage-level status] -->

## Troubleshooting Failed Pipelines

### Common Failure Causes

| Issue | Cause | Solution |
|-------|-------|----------|
| Data source error | Connectivity/permissions | Verify data source configuration |
| Extraction failure | Unsupported file format | Check file compatibility |
| Embedding timeout | Large batch size | Reduce batch size |
| Indexing error | Index schema mismatch | Verify index configuration |

### Viewing Error Details

1. Navigate to the failed pipeline run
2. Click on the failed stage
3. View detailed error message
4. Check logs for additional context

### Retrying Failed Pipelines

1. Fix the underlying issue
2. Click **Retry** on the failed run, or
3. Start a new run

## Pipeline Best Practices

### Design Considerations

1. **Start small**: Test with a subset of documents first
2. **Monitor performance**: Track processing times
3. **Handle failures**: Design for retry scenarios
4. **Document configuration**: Keep notes on parameter choices

### Performance Optimization

| Optimization | Description |
|--------------|-------------|
| Batch sizing | Adjust for your document sizes |
| Parallel workers | Scale for large volumes |
| Partition strategy | Choose based on content type |
| Index partitions | Organize for efficient queries |

For more on performance, see [Reduced Vectorization Latency](#performance-configuration).

## Performance Configuration

### Reducing Vectorization Latency

Several configuration options affect pipeline performance:

| Setting | Impact | Recommendation |
|---------|--------|----------------|
| **Worker instances** | Parallelization | Increase for large volumes |
| **Batch size** | Memory vs. throughput | Balance based on document sizes |
| **Embedding dimensions** | Index size vs. quality | 2048 for most use cases |

### Scaling for Large Volumes

For enterprise-scale vectorization:

1. Deploy multiple Vectorization Workers
2. Use asynchronous processing
3. Configure appropriate resource limits
4. Monitor Azure service quotas

See [Vectorization Configuration](../vectorization/vectorization-configuration.md) for detailed settings.

## Managing Existing Pipelines

### Editing a Pipeline

1. Click on the pipeline name
2. Click **Edit**
3. Modify configuration
4. Save changes

> [!NOTE]
> Changes don't affect currently running executions.

### Deactivating a Pipeline

1. Toggle the **Active** switch to off
2. Scheduled triggers are suspended
3. Manual runs still possible

### Deleting a Pipeline

1. Select the pipeline
2. Click **Delete**
3. Confirm deletion

> [!WARNING]
> Deleting a pipeline doesn't remove indexed content.

## API Reference

### List Pipelines

```http
GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataPipeline/dataPipelines
```

### Get Pipeline Status

```http
GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataPipeline/dataPipelines/{name}/status
```

### Run Pipeline

```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataPipeline/dataPipelines/{name}/run
Content-Type: application/json

{
  "parameter_values": {
    "DataSource.{sourceName}.Folders": ["/path/to/folder"]
  }
}
```

## Related Topics

- [Data Pipeline Concepts](../../concepts/data-pipeline/data-pipeline.md)
- [SharePoint Upload Guide](../vectorization/sharepoint-upload-guide.md)
- [Azure Data Lake Guide](../vectorization/azure-data-lake-guide.md)
- [Vectorization Configuration](../vectorization/vectorization-configuration.md)
