# Invoking Data Pipelines

Learn how to manually trigger data pipeline execution.

## Overview

Data pipelines can be invoked manually to process new or updated data on demand. This is useful for:

- Initial data loading
- Processing specific datasets
- Testing pipeline configurations
- Ad-hoc data updates

## Accessing Pipeline Invocation

1. Navigate to **Data Pipelines** in the sidebar
2. Locate the pipeline you want to run
3. Use the run action to invoke it

## Manual Invocation

### From the Pipelines List

1. Find the pipeline in the list
2. Click the **Run** icon (▶️) in the actions column
3. Confirm the invocation if prompted
4. The pipeline begins execution

### From Pipeline Edit Page

1. Open the pipeline for editing
2. Click **Run Pipeline** (if available)
3. The pipeline starts processing

## Invocation Options

> **TODO**: Document specific invocation options available in the UI, such as:
> - Full vs. incremental processing
> - Folder/file selection for partial runs
> - Override parameters for this run

## Monitoring the Run

After invoking a pipeline:

1. Navigate to **Data Pipeline Runs**
2. Find your pipeline in the list (sorted by most recent)
3. Monitor the status as it progresses
4. Review results when complete

## Run Scheduling

> **TODO**: Document scheduled/automatic pipeline invocation if supported, including:
> - Cron-based scheduling
> - Event-triggered runs
> - Continuous processing modes

## Best Practices

### Before Running

1. Verify the data source is accessible
2. Check that target storage/index has capacity
3. Review pipeline configuration for correctness

### During Execution

1. Monitor the run status in Pipeline Runs
2. Check for early errors
3. Be prepared to cancel if issues arise

### After Completion

1. Verify data was processed correctly
2. Check the target index/storage for new content
3. Test agent queries against the updated data

## Troubleshooting

### Pipeline Won't Start

- Verify you have permission to run the pipeline
- Check if another run is already in progress
- Ensure the pipeline is in an active state

### Run Fails Immediately

- Check data source connectivity
- Verify authentication credentials
- Review error messages in run details

### Processing Slower Than Expected

- Large datasets take longer to process
- Check embedding model throughput
- Review stage configurations

## Related Topics

- [Creating Data Pipelines](creating-data-pipelines.md)
- [Monitoring Data Pipelines](monitoring-data-pipelines.md)
- [Data Pipeline Runs](../data-pipeline-runs.md)
