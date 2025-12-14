# Monitoring Data Pipelines

Learn how to monitor pipeline execution and troubleshoot issues.

## Overview

Monitor your data pipelines to ensure data is processed correctly.

## Pipeline Status

View pipeline run status:
- **New**: Just created, not started
- **In Progress**: Currently executing
- **Completed**: Successfully finished
- **Failed**: Encountered errors

## Viewing Pipeline Runs

1. Navigate to **Data Pipeline Runs**
2. View the list of executions
3. Click on a run for details

## Run Details

Each run shows:
- Start and end times
- Processing steps completed
- Error messages (if any)
- Items processed

## Troubleshooting

### Common Issues
- Connection failures to data sources
- File format issues
- Memory constraints for large files
- Rate limiting from external services

### Error Messages
Review the error_messages array in run details for specific issues.

## Related Topics

- [Creating Data Pipelines](creating-data-pipelines.md)
- [Invoking Data Pipelines](invoking-data-pipelines.md)
- [Troubleshooting](../../../platform-operations/monitoring-troubleshooting/troubleshooting.md)
