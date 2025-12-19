# Data Pipeline Runs

Learn how to view, filter, and analyze data pipeline execution history.

## Overview

The Data Pipeline Runs page provides visibility into pipeline execution history, allowing you to monitor processing status, investigate failures, and track data ingestion progress.

## Accessing Pipeline Runs

1. In the Management Portal sidebar, click **Data Pipeline Runs** under the **Data** section
2. The runs list loads, showing execution history sorted by most recent first

## Pipeline Runs Table

The table displays:

| Column | Description |
|--------|-------------|
| **Item** | Pipeline that was executed |
| **Status** | Current state (Running, Completed, Failed) |
| **Success** | Whether the run succeeded |
| **Start Time** | When execution began |
| **End Time** | When execution completed (if finished) |
| **Duration** | Total execution time |

## Filtering Runs

Use the filters at the top of the table to narrow results:

### Available Filters

| Filter | Description |
|--------|-------------|
| **Item** | Select a specific pipeline |
| **Status** | Filter by execution status |
| **Success** | Filter by success/failure |
| **Start Time** | Time range presets |
| **Start Time From/To** | Custom date range (when "Custom" is selected) |

### Time Range Options

| Option | Description |
|--------|-------------|
| **All** | Show all runs |
| **Last Hour** | Runs started in the last hour |
| **Last 24 Hours** | Runs started in the last day |
| **Last 7 Days** | Runs started in the last week |
| **Last 30 Days** | Runs started in the last month |
| **Custom** | Specify start/end dates manually |

### Using Custom Date Range

1. Select **Custom** from the Start Time dropdown
2. Use the **Start Time From** calendar to set the begin date
3. Use the **Start Time To** calendar to set the end date
4. The list automatically filters

### Clearing Filters

Click **Clear Filters** to reset all filters to their default values.

## Refreshing the List

Click the **Refresh** button (🔄) to reload the pipeline runs list with the latest data.

## Run Status Indicators

| Status | Description |
|--------|-------------|
| **Running** | Pipeline is currently executing |
| **Completed** | Pipeline finished execution |
| **Failed** | Pipeline encountered an error |
| **Cancelled** | Pipeline was manually stopped |

## Viewing Run Details

> **TODO**: Document how to view detailed run information when clicking on a specific run. This may include step-by-step execution logs, items processed, errors, and performance metrics.

## Common Scenarios

### Finding Failed Runs

1. Set the **Success** filter to **Failed**
2. Review the list of failed runs
3. Click on a run to investigate the cause

### Monitoring Active Runs

1. Set the **Status** filter to **Running**
2. Monitor progress of active pipelines
3. Use refresh to see updated status

### Reviewing Recent Activity

1. Set **Start Time** to **Last 24 Hours**
2. Review all pipeline activity from the last day

## Troubleshooting

### Pipeline Takes Too Long

- Check if the data source has more data than expected
- Review stage configurations for optimization opportunities
- Consider breaking large datasets into smaller batches

### Frequent Failures

- Check data source connectivity
- Review error messages in run details
- Verify authentication credentials haven't expired

### Runs Not Appearing

- Click refresh to update the list
- Verify your filters aren't excluding the runs
- Check that you have permission to view the pipeline

## Related Topics

- [Invoking Data Pipelines](data-pipelines/invoking-data-pipelines.md)
- [Monitoring Data Pipelines](data-pipelines/monitoring-data-pipelines.md)
- [Creating Data Pipelines](data-pipelines/creating-data-pipelines.md)
