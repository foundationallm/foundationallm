# Monitoring Data Pipelines

Learn how to monitor data pipeline execution and track processing status.

## Overview

Monitoring data pipelines helps you:

- Track processing progress
- Identify and diagnose failures
- Understand processing performance
- Plan capacity and scheduling

## Monitoring Locations

Pipeline monitoring is available in two places:

1. **Data Pipelines** - View pipeline configurations and status
2. **Data Pipeline Runs** - View detailed execution history

## Pipeline Status Indicators

### In the Pipelines List

| Column | Description |
|--------|-------------|
| **Active** | Whether the pipeline is enabled |
| **Last Run** | Most recent execution status (if shown) |

### In Pipeline Runs

| Status | Description |
|--------|-------------|
| **Running** | Currently processing data |
| **Completed** | Finished successfully |
| **Failed** | Encountered an error |
| **Cancelled** | Manually stopped |

## Real-Time Monitoring

### Watching Active Runs

1. Navigate to **Data Pipeline Runs**
2. Filter by **Status: Running**
3. Use the refresh button to update status
4. Watch for completion or failures

### Progress Tracking

> **TODO**: Document real-time progress indicators if available, such as:
> - Items processed count
> - Current stage indicator
> - Estimated time remaining
> - Processing rate metrics

## Run Details

Click on a specific run to view detailed information:

### Execution Log

> **TODO**: Document the detailed execution log view, including:
> - Stage-by-stage progress
> - Timestamps for each step
> - Items processed per stage
> - Error messages and stack traces

### Performance Metrics

> **TODO**: Document available performance metrics:
> - Total duration
> - Time per stage
> - Items per second
> - Resource utilization

## Alerting and Notifications

> **TODO**: Document alerting capabilities if available:
> - Failure notifications
> - Completion notifications
> - Integration with Azure Monitor or other systems

## Historical Analysis

### Viewing Trends

Use the Pipeline Runs page filters to analyze patterns:

1. Filter to a specific pipeline
2. Set a time range (e.g., Last 30 Days)
3. Review success rates and durations
4. Identify recurring issues

### Common Patterns

| Pattern | Possible Cause |
|---------|----------------|
| Intermittent failures | Network issues, resource contention |
| Increasing duration | Growing data volume, performance degradation |
| Consistent failures | Configuration error, permission issue |
| Success after retry | Transient errors, timeout issues |

## Troubleshooting from Monitoring

### Identifying Issues

1. **Failed Status**: Check error messages in run details
2. **Long Duration**: Review stage timing for bottlenecks
3. **Repeated Failures**: Look for patterns in failure timing/type

### Common Issues

| Issue | Investigation Steps |
|-------|---------------------|
| Connection failures | Check data source configuration, network |
| Timeout errors | Increase timeout settings, reduce batch size |
| Resource errors | Check storage capacity, API quotas |
| Data errors | Review source data quality, parsing settings |

## Best Practices

### Regular Monitoring

- Check pipeline runs daily during initial setup
- Set up alerts for critical pipelines
- Review trends weekly or monthly

### Proactive Management

- Address warnings before they become failures
- Plan maintenance during low-activity periods
- Monitor storage and quota utilization

### Documentation

- Record common failure patterns and solutions
- Document expected processing times
- Track changes that affect performance

## Related Topics

- [Data Pipeline Runs](../data-pipeline-runs.md)
- [Creating Data Pipelines](creating-data-pipelines.md)
- [Invoking Data Pipelines](invoking-data-pipelines.md)
