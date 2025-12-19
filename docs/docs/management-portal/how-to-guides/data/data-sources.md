# Data Sources

Learn how to configure and manage data sources in the Management Portal.

## Overview

Data sources define the storage locations from which agents and data pipelines can retrieve information. They provide the connection configuration needed to access your organizational data.

## Accessing Data Sources

1. In the Management Portal sidebar, click **Data Sources** under the **Data** section
2. The data sources list loads, showing all configured sources

## Data Source List

The table displays:

| Column | Description |
|--------|-------------|
| **Name** | Unique identifier for the data source |
| **Source Type** | Type of storage (Azure Data Lake, SharePoint, etc.) |
| **Edit** | Settings icon to modify configuration |
| **Delete** | Trash icon to remove the data source |

### Searching and Managing

- Use the search box to filter by name
- Click the refresh button to reload the list
- Click column headers to sort

## Supported Data Source Types

| Type | Description |
|------|-------------|
| **Azure Data Lake** | Azure Data Lake Storage Gen2 containers |
| **OneLake** | Microsoft Fabric OneLake storage |
| **SharePoint Online** | SharePoint document libraries |
| **Azure SQL Database** | Azure SQL databases |
| **Web** | Public web content via URLs |

## Creating a Data Source

1. Click **Create Data Source** at the top right of the page
2. Configure the data source settings

### Basic Information

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Data Source Name** | Unique identifier | Letters, numbers, dashes, underscores only |
| **Description** | Purpose and contents | Optional but recommended |

### Select Data Source Type

Choose the appropriate type from the dropdown:
- Azure Data Lake
- OneLake
- SharePoint Online
- Azure SQL Database
- Web

### Configure Connection Details

Connection settings vary by data source type:

#### Azure Data Lake

| Field | Description |
|-------|-------------|
| **Authentication Type** | How to authenticate (Connection String, Account Key, Azure Identity) |
| **Connection String** | Full connection string (if using Connection String auth) |
| **API Key** | Storage account key (if using Account Key auth) |
| **Endpoint** | Storage account endpoint URL |
| **Account Name** | Storage account name (if using Azure Identity) |
| **Folders** | Container/folder paths to access (press Enter after each) |

**Authentication Options:**

| Type | Description | Best For |
|------|-------------|----------|
| **Connection String** | Full connection string with SAS or key | Development, testing |
| **Account Key** | Storage account access key | Simple deployments |
| **Azure Identity** | Managed Identity authentication | Production, secure access |

#### OneLake

Similar to Azure Data Lake, with authentication options for Fabric workspaces.

#### SharePoint Online

| Field | Description |
|-------|-------------|
| **Site URL** | SharePoint site URL |
| **Document Library** | Library to access |
| **Authentication** | Microsoft Graph authentication settings |

#### Azure SQL Database

| Field | Description |
|-------|-------------|
| **Server** | SQL Server hostname |
| **Database** | Database name |
| **Authentication** | SQL or Azure AD authentication |
| **Tables/Views** | Specific tables or views to access |

#### Web

| Field | Description |
|-------|-------------|
| **URL(s)** | Web pages to crawl |
| **Crawl Depth** | How deep to follow links |

### Specifying Folders

For storage-based sources, specify folders using chips:
1. Type the folder path (e.g., `container/folder`)
2. Press **Enter** or **,** to add the chip
3. Repeat for additional folders
4. Click the X on a chip to remove it

## Editing Data Sources

1. Locate the data source in the list
2. Click the **Settings** icon (⚙️)
3. Modify settings as needed
4. Click **Save Changes**

> **Note:** The data source name cannot be changed after creation.

## Deleting Data Sources

1. Click the **Trash** icon (🗑️) for the data source
2. Confirm deletion in the dialog

> **Warning:** Deleting a data source affects any data pipelines or agents using it. Verify dependencies before deleting.

## Access Control

Configure who can access and manage the data source:

1. Open the data source for editing
2. Click **Access Control** at the top right
3. Add or remove role assignments

| Permission | Description |
|------------|-------------|
| `FoundationaLLM.DataSource/dataSources/read` | View the data source |
| `FoundationaLLM.DataSource/dataSources/write` | Edit configuration |
| `FoundationaLLM.DataSource/dataSources/delete` | Delete the data source |

## Best Practices

### Security

- Use Azure Identity (Managed Identity) for production environments
- Avoid embedding secrets in connection strings when possible
- Apply principle of least privilege to folder access

### Organization

- Use descriptive names that indicate the data content
- Document the purpose in the description field
- Group related data in logical folder structures

### Performance

- Limit folder scope to only necessary paths
- Consider separate data sources for different data types

## Troubleshooting

### Connection Test Fails

- Verify authentication credentials are correct
- Check network connectivity to the storage endpoint
- Ensure firewall rules allow access from FoundationaLLM services

### Data Not Accessible

- Verify folder paths are correct
- Check that the service principal or managed identity has appropriate permissions
- Review Azure Storage access policies

## Related Topics

- [Azure Data Lake Knowledge Source](knowledge-sources/azure-data-lake.md)
- [SharePoint Online](knowledge-sources/sharepoint-online.md)
- [Creating Data Pipelines](data-pipelines/creating-data-pipelines.md)
