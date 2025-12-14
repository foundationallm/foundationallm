# Creating Data Pipelines

Learn how to create and configure data pipelines for processing and indexing your data.

## Overview

Data pipelines define the processing workflow for transforming raw data into indexed, searchable content that agents can use. A pipeline consists of:

- **Data Source**: Where to read data from
- **Stages**: Processing steps (text extraction, splitting, embedding, indexing)
- **Configuration**: Parameters for each stage

## Accessing Data Pipelines

1. In the Management Portal sidebar, click **Data Pipelines** under the **Data** section
2. The pipelines list shows all configured pipelines

## Pipeline List

The table displays:

| Column | Description |
|--------|-------------|
| **Name** | Pipeline identifier |
| **Description** | Purpose of the pipeline |
| **Active** | Whether the pipeline is enabled |
| **Edit** | Settings icon to modify configuration |
| **Run** | Execute the pipeline manually |
| **Delete** | Remove the pipeline |

## Creating a Pipeline

1. Click **Create Pipeline** at the top right
2. Configure the pipeline settings

### Basic Information

| Field | Description |
|-------|-------------|
| **Pipeline Name** | Unique identifier (letters, numbers, dashes, underscores) |
| **Display Name** | User-friendly name |
| **Description** | Purpose and what data it processes |

### Select Data Source

Choose an existing data source from the dropdown. This determines where the pipeline reads input data.

After selecting a data source:

| Field | Description |
|-------|-------------|
| **Data Source Name** | Override name for this pipeline's data reference |
| **Data Source Description** | Additional description |
| **Data Source Plugin** | Plugin for processing this data source type |

### Configure Data Source Plugin

The plugin determines how data is read. Configure default values for plugin parameters:

| Parameter Type | Input Method |
|----------------|--------------|
| **String/Int/Float/DateTime** | Text input |
| **Boolean** | Toggle switch |
| **Array** | Chips (comma-separated values) |
| **Resource Object ID** | Dropdown selection |

## Pipeline Stages

Stages define the processing workflow. Common stages include:

### Stage Types

| Stage | Purpose |
|-------|---------|
| **Text Extraction** | Extract text content from documents |
| **Text Partitioning** | Split text into chunks |
| **Text Embedding** | Generate vector embeddings |
| **Indexing** | Store in vector database |

### Configuring Stages

For each stage:

1. **Select Plugin**: Choose the processing plugin
2. **Configure Parameters**: Set stage-specific settings
3. **Define Outputs**: Specify where results go

### Stage Parameters

Common parameters by stage type:

**Text Partitioning:**

| Parameter | Description |
|-----------|-------------|
| **Chunk Size** | Maximum size of each text chunk |
| **Overlap Size** | Characters overlapping between chunks |
| **Tokenizer** | How to count tokens |

**Text Embedding:**

| Parameter | Description |
|-----------|-------------|
| **Model** | Embedding model to use |
| **Dimensions** | Vector size |
| **Batch Size** | Documents per batch |

**Indexing:**

| Parameter | Description |
|-----------|-------------|
| **Vector Store** | Target vector database |
| **Index Name** | Name for the index |

### Stage Ordering

Stages can be reordered:
- Drag and drop stages to change order
- Use the visual connectors to see flow
- Ensure logical progression (extraction → partitioning → embedding → indexing)

## Advanced Configuration

### Trigger Settings

Configure when the pipeline runs:

| Trigger Type | Description |
|--------------|-------------|
| **Manual** | Run on demand only |
| **Scheduled** | Run on a cron schedule |
| **Event-driven** | Run when data changes |

> **TODO**: Document specific trigger configuration options when available in the UI.

### Prompt Configuration

Some stages use prompts for processing:

1. Select a prompt from the available options
2. Configure prompt parameters
3. Set the prompt role (e.g., `main_prompt`)

## Saving the Pipeline

1. Review all configuration sections
2. Click **Create Pipeline** or **Save Changes**
3. Wait for validation and saving

## Best Practices

### Design

- Start with simple pipelines and add complexity as needed
- Use descriptive names for stages
- Document expected input and output formats

### Performance

- Tune chunk sizes based on your content type
- Use appropriate batch sizes for embedding
- Consider parallel processing where supported

### Maintenance

- Test with sample data before production
- Monitor pipeline runs for errors
- Keep data sources and plugins updated

## Related Topics

- [Data Sources](../data-sources.md)
- [Invoking Data Pipelines](invoking-data-pipelines.md)
- [Monitoring Data Pipelines](monitoring-data-pipelines.md)
- [Data Pipelines Reference](../../../reference/concepts/data-pipelines.md)
