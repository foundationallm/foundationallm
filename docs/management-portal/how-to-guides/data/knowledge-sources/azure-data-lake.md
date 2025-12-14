# Azure Data Lake as a Knowledge Source

Learn how to configure Azure Data Lake Storage Gen2 as a knowledge source for FoundationaLLM agents.

## Overview

Azure Data Lake Storage Gen2 (ADLS) provides scalable, secure storage for large volumes of documents. It's ideal for:

- Large document repositories
- Structured data files (CSV, JSON, Parquet)
- Enterprise content management
- Multi-team data sharing

## Prerequisites

Before configuring ADLS as a knowledge source:

- **Azure Data Lake Storage Gen2 account** with hierarchical namespace enabled
- **Container** created for your documents
- **Access permissions** configured (see Authentication Options)
- **Network connectivity** from FoundationaLLM services to storage account
- **Firewall rules** allowing access (if enabled)

## Configuring ADLS as a Data Source

### Step 1: Navigate to Data Sources

1. In the Management Portal sidebar, click **Data Sources**
2. Click **Create Data Source**
3. Select **Azure Data Lake** from the type dropdown

### Step 2: Enter Basic Information

| Field | Description |
|-------|-------------|
| **Data Source Name** | Unique identifier (e.g., `corporate-docs-adls`) |
| **Description** | Purpose and contents (e.g., "Corporate policy documents") |

### Step 3: Select Authentication Type

Choose the authentication method:

| Type | Description | Best For |
|------|-------------|----------|
| **Azure Identity** | Managed Identity authentication | Production deployments |
| **Account Key** | Storage account access key | Development, testing |
| **Connection String** | Full connection string | Legacy configurations |

### Step 4: Configure Connection Details

#### For Azure Identity (Managed Identity)

| Field | Value |
|-------|-------|
| **Authentication Type** | Azure Identity |
| **Account Name** | Your storage account name (e.g., `mystorageaccount`) |

#### For Account Key

| Field | Value |
|-------|-------|
| **Authentication Type** | Account Key |
| **API Key** | Storage account access key |
| **Endpoint** | Storage endpoint URL (e.g., `https://mystorageaccount.dfs.core.windows.net`) |

#### For Connection String

| Field | Value |
|-------|-------|
| **Authentication Type** | Connection String |
| **Connection String** | Full storage connection string |

### Step 5: Specify Folders

Enter the container and folder paths containing your documents:

1. Type the path (e.g., `mycontainer/documents/policies`)
2. Press **Enter** or **,** to add it
3. Repeat for additional paths

**Path Format Examples:**
- Container root: `mycontainer`
- Single folder: `mycontainer/documents`
- Nested folder: `mycontainer/departments/hr/policies`

### Step 6: Save the Data Source

Click **Create Data Source** to save the configuration.

## Authentication Configuration

### Using Managed Identity (Recommended)

Managed Identity provides secure, credential-free access:

1. Ensure the FoundationaLLM deployment has a managed identity
2. Grant the managed identity the **Storage Blob Data Reader** role on the storage account
3. Select **Azure Identity** authentication in the data source configuration

**Azure CLI Example:**
```bash
az role assignment create \
  --assignee <managed-identity-object-id> \
  --role "Storage Blob Data Reader" \
  --scope /subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.Storage/storageAccounts/<storage-account>
```

### Using Account Keys

For development or when managed identity isn't available:

1. Navigate to your storage account in Azure Portal
2. Go to **Access Keys**
3. Copy a key value
4. Enter it in the **API Key** field

> **Warning:** Account keys provide full access. Rotate them regularly and use managed identity in production.

### Using Connection Strings

Connection strings are useful for quick configuration:

1. Navigate to your storage account in Azure Portal
2. Go to **Access Keys**
3. Copy the connection string
4. Enter it in the **Connection String** field

## Folder Structure Best Practices

### Organizing Documents

```
mycontainer/
├── departments/
│   ├── hr/
│   │   ├── policies/
│   │   └── procedures/
│   └── finance/
│       ├── reports/
│       └── guidelines/
└── shared/
    └── templates/
```

### Path Selection Tips

- **Specific paths** reduce processing time and improve relevance
- **Multiple paths** can be specified for different content types
- **Avoid very deep nesting** which complicates management

## Using in Data Pipelines

After creating the data source:

1. Navigate to **Data Pipelines**
2. Create a new pipeline
3. Select your ADLS data source
4. Configure processing stages
5. Run the pipeline to index documents

## Supported File Types

ADLS data sources typically support:

| Category | Extensions |
|----------|------------|
| **Documents** | PDF, DOCX, DOC, PPTX, PPT, XLSX, XLS |
| **Text** | TXT, MD, HTML, XML, JSON |
| **Data** | CSV, TSV, Parquet |

Specific support depends on the data pipeline configuration.

## Troubleshooting

### Authentication Failures

- Verify managed identity has correct role assignments
- Check account key hasn't been rotated
- Ensure connection string is complete and correct

### Access Denied Errors

- Verify folder paths are correct
- Check container exists and is accessible
- Review storage account firewall settings

### Network Connectivity Issues

- Ensure FoundationaLLM services can reach the storage endpoint
- Check VNet configurations if using private endpoints
- Review firewall allow lists

## Related Topics

- [Data Sources](../data-sources.md)
- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Private Storage](private-storage.md)
