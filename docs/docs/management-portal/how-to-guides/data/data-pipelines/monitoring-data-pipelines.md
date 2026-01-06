# Monitoring Data Pipelines

Learn how to monitor data pipeline execution, track processing status, and understand performance characteristics.

## Overview

Monitoring data pipelines helps you:

- Track processing progress in real-time
- Identify and diagnose failures quickly
- Understand processing performance and latency
- Plan capacity and scheduling optimization
- Measure the effectiveness of performance improvements

## Performance and Latency

### Understanding Processing Latency

Data pipeline latency refers to the time between when data is submitted and when it becomes available for agent queries. FoundationaLLM optimizes this through:

| Optimization | Benefit |
|--------------|---------|
| **Parallel Stage Processing** | Multiple stages can run concurrently where dependencies allow |
| **Batch Processing** | Documents are processed in optimized batches |
| **Efficient Embedding** | Text embedding uses optimized batch sizes |
| **Incremental Indexing** | Only changed content is reprocessed |

### Factors Affecting Latency

| Factor | Impact | Mitigation |
|--------|--------|------------|
| **Document Size** | Larger documents take longer to process | Split large documents |
| **Document Count** | More documents increase total time | Use parallel processing |
| **Embedding Model** | Model complexity affects speed | Balance quality vs. speed |
| **Index Size** | Large indexes may slow indexing | Use index partitions |
| **Network Latency** | Remote services add overhead | Use regional deployments |

### Performance Configuration

To optimize pipeline performance:

1. **Adjust Batch Sizes**: Larger batches can improve throughput but increase memory usage
2. **Configure Parallelism**: Set appropriate concurrent processing limits
3. **Tune Chunk Sizes**: Balance between context quality and processing speed
4. **Schedule Off-Peak**: Run large pipelines during low-usage periods

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
