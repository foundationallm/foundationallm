# Data Sources

Learn how to configure and manage data sources in the Management Portal.

## Overview

Data sources define the locations from which agents can retrieve information.

## Supported Data Source Types

- **Azure Data Lake Storage**: ADLS Gen2 storage accounts
- **SharePoint Online**: SharePoint sites and document libraries
- **Azure Blob Storage**: Blob containers
- **Web URLs**: Public web content

## Creating a Data Source

1. Navigate to **Data Sources** in the sidebar
2. Click **Create Data Source**
3. Select the data source type
4. Configure connection settings:
   - Authentication type
   - Connection string or endpoint
   - Folder paths
5. Save the data source

## Data Source Configuration

### Authentication Types
- Managed Identity
- Connection String
- API Key

### Folder Configuration
Specify which folders/paths contain your data.

## Managing Data Sources

- Edit connection settings
- Update folder paths
- Test connectivity
- Delete unused sources

## Related Topics

- [Azure Data Lake as a Knowledge Source](knowledge-sources/azure-data-lake.md)
- [SharePoint Online](knowledge-sources/sharepoint-online.md)
- [Data Pipelines](data-pipelines/creating-data-pipelines.md)
