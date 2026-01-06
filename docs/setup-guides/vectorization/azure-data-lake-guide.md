# Azure Data Lake as a Knowledge Source

This guide explains how to configure Azure Data Lake Storage (ADLS) Gen2 as a knowledge source for FoundationaLLM agents.

## Overview

Azure Data Lake Storage Gen2 is a scalable and cost-effective data lake solution that integrates seamlessly with FoundationaLLM. Use ADLS Gen2 when you need to:

- Process large volumes of documents
- Organize content in a hierarchical folder structure
- Leverage existing data lake investments
- Scale vectorization for enterprise workloads

## Prerequisites

- Azure Data Lake Storage Gen2 account
- Containers and folders with documents to index
- Appropriate authentication credentials
- Administrator access to Management Portal

## Architecture

```
Azure Data Lake Storage Gen2        FoundationaLLM
         |                               |
         | Containers/Folders            |
         |------------------------------>|  Data Pipeline
         |  (via Azure SDK)              |       |
         |                               |       v
         |                               |  Vectorization
         |                               |       |
         |                               |       v
         |                               |  Azure AI Search
```

## Authentication Options

ADLS Gen2 supports multiple authentication methods:

| Method | Description | Use Case |
|--------|-------------|----------|
| **Azure Identity** | Managed Identity or Service Principal | Production (recommended) |
| **Account Key** | Storage account access key | Development/testing |
| **Connection String** | Full connection string | Legacy compatibility |

### Recommended: Azure Identity

For production deployments, use Azure Identity (Managed Identity):

1. Enable managed identity on FoundationaLLM services
2. Grant **Storage Blob Data Reader** role on the storage account
3. No secrets to manage or rotate

## Step 1: Prepare Your Data Lake

### Organize Your Content

FoundationaLLM indexes content from specified folders. Recommended structure:

```
storage-account/
├── container-name/
│   ├── knowledge-base-1/
│   │   ├── category-a/
│   │   │   ├── document1.pdf
│   │   │   └── document2.docx
│   │   └── category-b/
│   │       └── document3.txt
│   └── knowledge-base-2/
│       └── ...
```

### Best Practices for Organization

| Practice | Benefit |
|----------|---------|
| Logical folder hierarchy | Easier to manage and update |
| Separate containers per use case | Better access control |
| Consistent naming conventions | Simpler automation |
| Avoid deeply nested structures | Better performance |

<!-- [TODO: Document maximum folder depth recommendations] -->

## Step 2: Configure Authentication

### Option A: Azure Identity (Recommended)

1. In Azure Portal, navigate to your storage account
2. Go to **Access Control (IAM)**
3. Add role assignment:
   - Role: **Storage Blob Data Reader**
   - Assign to: FoundationaLLM service managed identity

### Option B: Account Key

Add the following App Configuration setting:

```
FoundationaLLM:DataSources:{name}:APIKey -> (Key Vault reference to storage account key)
```

### Option C: Connection String

Add the connection string to App Configuration:

```
FoundationaLLM:DataSources:{name}:ConnectionString -> (Key Vault reference to connection string)
```

## Step 3: Configure App Configuration Settings

Add these settings to Azure App Configuration:

| Setting Key | Description |
|-------------|-------------|
| `FoundationaLLM:DataSources:{name}:AuthenticationType` | `AzureIdentity`, `AccountKey`, or `ConnectionString` |
| `FoundationaLLM:DataSources:{name}:ConnectionString` | Connection string (if using ConnectionString auth) |
| `FoundationaLLM:DataSources:{name}:APIKey` | Account key (if using AccountKey auth) |
| `FoundationaLLM:DataSources:{name}:Endpoint` | Storage account endpoint |

Replace `{name}` with a unique identifier (e.g., `datalake01`).

## Step 4: Create ADLS Data Source

### Using the Management Portal

1. Navigate to **Management Portal** > **Data Sources**
2. Click **Create Data Source**
3. Select **Azure Data Lake** as the type
4. Configure:

| Field | Description | Example |
|-------|-------------|---------|
| **Name** | Unique identifier | `datalake01` |
| **Folders** | List of folder paths | `/container/knowledge-base-1` |
| **Authentication Type** | Authentication method | `AzureIdentity` |

<!-- [TODO: Add screenshot of ADLS data source creation UI] -->

### Using the Management API

```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/datalake01
Content-Type: application/json

{
  "type": "AzureDataLake",
  "name": "datalake01",
  "description": "Corporate knowledge base storage",
  "folders": [
    "/vectorization-input/knowledge-base/documents"
  ],
  "configuration_references": {
    "AuthenticationType": "FoundationaLLM:DataSources:datalake01:AuthenticationType",
    "ConnectionString": "FoundationaLLM:DataSources:datalake01:ConnectionString"
  }
}
```

## Step 5: Create a Data Pipeline

Create a pipeline to process ADLS documents:

1. Navigate to **Management Portal** > **Data Pipelines**
2. Click **Create Pipeline**
3. Select your ADLS data source
4. Configure pipeline stages:
   - Text extraction
   - Text partitioning
   - Embedding
   - Indexing

See [Data Pipeline Management](../management-ui/data-pipeline-management.md) for detailed configuration.

## Pipeline Trigger Configuration

### Manual Trigger

For on-demand processing:

```json
{
  "triggers": []
}
```

### Scheduled Trigger

For regular updates:

```json
{
  "triggers": [
    {
      "name": "Daily update",
      "trigger_type": "Schedule",
      "trigger_cron_schedule": "0 6 * * *",
      "parameter_values": {
        "DataSource.datalake01.Folders": [
          "/vectorization-input/documents"
        ]
      }
    }
  ]
}
```

## Supported File Types

| Category | Extensions | Notes |
|----------|------------|-------|
| Documents | .pdf, .docx, .doc, .txt, .rtf | Most common formats |
| Spreadsheets | .xlsx, .xls, .csv | Tabular data |
| Presentations | .pptx, .ppt | Slide content |
| Web | .html, .htm, .md | Markup formats |
| Data | .json, .xml, .yaml | Structured data |
| Code | .py, .js, .cs, .java | Source code |

## Performance Optimization

### Folder Structure Impact

| Structure | Processing Behavior |
|-----------|---------------------|
| Flat (all files in one folder) | Parallel processing, good for small sets |
| Hierarchical | Sequential folder traversal, better for large sets |
| Multiple folders in config | Parallel folder processing |

### Batch Size Recommendations

| Document Count | Recommendation |
|----------------|----------------|
| < 1,000 | Synchronous processing |
| 1,000 - 10,000 | Asynchronous with defaults |
| > 10,000 | Asynchronous with tuned workers |

<!-- [TODO: Document specific batch size configuration options] -->

## Monitoring and Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Authentication failed | Invalid credentials or permissions | Verify IAM roles or keys |
| Folder not found | Incorrect path | Check path includes container name |
| No files processed | Empty folder or wrong file types | Verify files exist and are supported |
| Slow processing | Large files or many documents | Use async processing, increase workers |

### Viewing Processing Status

1. Check pipeline run status in Management Portal
2. Review Application Insights logs
3. Check Azure Storage metrics for access patterns

## Security Best Practices

1. **Use Managed Identity**: Avoid storing credentials where possible
2. **Principle of Least Privilege**: Grant only Storage Blob Data Reader
3. **Network Security**: Use private endpoints for production
4. **Encryption**: Enable storage account encryption at rest
5. **Monitoring**: Enable storage account diagnostic logging

## ADLS Gen1 vs Gen2

FoundationaLLM supports **Azure Data Lake Storage Gen2** only.

| Feature | ADLS Gen2 | ADLS Gen1 |
|---------|-----------|-----------|
| Supported | ✅ Yes | ❌ No |
| Hierarchical namespace | ✅ | N/A |
| Azure Blob API compatibility | ✅ | N/A |

<!-- [TODO: Confirm Gen1 support status] -->

## Related Topics

- [Vectorization Profiles](vectorization-profiles.md) - Full data source configuration reference
- [Data Pipeline Management](../management-ui/data-pipeline-management.md) - Pipeline configuration guide
- [SharePoint Online Guide](sharepoint-upload-guide.md) - Alternative data source option
- [Vectorization Configuration](vectorization-configuration.md) - Worker and API settings
