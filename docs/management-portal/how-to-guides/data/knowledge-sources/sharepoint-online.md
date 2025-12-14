# Uploading Files from SharePoint Online

Learn how to configure SharePoint Online as a knowledge source.

## Overview

FoundationaLLM can connect to SharePoint Online to retrieve documents for agent knowledge bases.

## Prerequisites

- SharePoint Online site access
- Appropriate permissions configured
- Azure AD app registration (if required)

## Configuring SharePoint as a Data Source

1. Navigate to **Data Sources**
2. Click **Create Data Source**
3. Select **SharePoint Online Site**
4. Configure connection settings:
   - Site URL
   - Authentication type
   - Document library path
5. Save the data source

## Authentication Options

- **Managed Identity**: Recommended for Azure deployments
- **App Registration**: Using client credentials

## Folder Selection

Specify which document libraries or folders to include:
- Root site documents
- Specific libraries
- Subfolder paths

## Syncing Content

After configuration:
1. Create a data pipeline using the SharePoint source
2. Run the pipeline to process documents
3. Documents become available to agents

## Related Topics

- [Data Sources](../data-sources.md)
- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
