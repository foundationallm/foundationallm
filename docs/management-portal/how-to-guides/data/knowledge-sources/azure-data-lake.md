# Azure Data Lake as a Knowledge Source

Learn how to configure Azure Data Lake Storage as a knowledge source.

## Overview

Azure Data Lake Storage Gen2 (ADLS) provides scalable storage for large volumes of documents and data.

## Prerequisites

- Azure Data Lake Storage Gen2 account
- Appropriate access permissions
- Network connectivity to the storage account

## Configuring ADLS as a Data Source

1. Navigate to **Data Sources**
2. Click **Create Data Source**
3. Select **Azure Data Lake**
4. Configure connection settings:
   - Storage account name or endpoint
   - Container name
   - Authentication type
   - Folder paths
5. Save the data source

## Authentication Options

### Managed Identity (Recommended)
Use Azure Managed Identity for secure, credential-free access.

### Connection String
Use a storage account connection string.

### API Key
Use storage account access keys.

## Folder Configuration

Specify which folders contain your documents:
```json
{
  "folders": [
    "/documents/policies",
    "/documents/procedures"
  ]
}
```

## Related Topics

- [Data Sources](../data-sources.md)
- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
