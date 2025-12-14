# Invoking Data Pipelines

Learn how to trigger and run data pipelines.

## Overview

Data pipelines can be invoked manually, on schedule, or automatically.

## Manual Invocation

### Via Management Portal
1. Navigate to **Data Pipelines**
2. Select your pipeline
3. Click **Run Pipeline**
4. Confirm the execution

### Via Management API
```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataPipeline/dataPipelines/{{pipelineName}}/process
```

## Scheduled Execution

Configure a cron schedule during pipeline creation:
- Pipeline runs automatically at specified times
- View scheduled runs in the Data Pipeline Runs section

## Event-Triggered Execution

When configured for event triggers:
- Pipeline runs when source data changes
- Monitors data source for new or modified files

## Execution Options

- **Full Run**: Process all data
- **Incremental**: Process only changes since last run

## Related Topics

- [Creating Data Pipelines](creating-data-pipelines.md)
- [Monitoring Data Pipelines](monitoring-data-pipelines.md)
