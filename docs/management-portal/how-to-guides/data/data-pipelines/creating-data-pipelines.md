# Creating Data Pipelines

Learn how to create data pipelines in the Management Portal.

## Overview

Data pipelines process data from sources to make it suitable for AI workloads.

## Pipeline Components

### Data Source
The origin of your data.

### Text Extraction
Extracts text from various file formats.

### Text Partitioning
Splits text into chunks for processing.

### Text Embedding
Converts text to vector representations.

### Indexing
Stores processed data in searchable indexes.

## Creating a Pipeline

1. Navigate to **Data Pipelines**
2. Click **Create Pipeline**
3. Configure pipeline settings:
   - **Name**: Unique identifier
   - **Description**: Purpose of the pipeline
   - **Data Source**: Select source
   - **Partitioning Profile**: Configure chunking
   - **Embedding Profile**: Select embedding model
   - **Indexing Profile**: Configure index target
4. Configure trigger settings
5. Save the pipeline

## Trigger Types

- **Manual**: Run on demand
- **Schedule**: Run on a cron schedule
- **Event**: Run when data changes

## Related Topics

- [Invoking Data Pipelines](invoking-data-pipelines.md)
- [Monitoring Data Pipelines](monitoring-data-pipelines.md)
- [Data Pipelines Reference](../../reference/concepts/data-pipelines.md)
