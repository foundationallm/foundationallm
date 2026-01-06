# Upload Files from SharePoint Online

This guide explains how to configure SharePoint Online as a knowledge source and enable file uploads from SharePoint for agent consumption.

## Overview

FoundationaLLM supports SharePoint Online as a data source for vectorization, allowing you to leverage existing document libraries as knowledge sources for your agents.

## Prerequisites

- SharePoint Online site with documents to index
- Microsoft Entra ID app registration with SharePoint permissions
- X.509 certificate for authentication
- Azure Key Vault for certificate storage
- Administrator access to Management Portal

## Architecture

```
SharePoint Online          FoundationaLLM
     |                          |
     | Document Libraries       |
     |------------------------->|  Data Pipeline
     |  (via Graph API)         |       |
     |                          |       v
     |                          |  Vectorization
     |                          |       |
     |                          |       v
     |                          |  Azure AI Search
```

## Step 1: Configure Entra ID App Registration

Before configuring SharePoint in FoundationaLLM, you must set up authentication.

### Create App Registration

1. Navigate to **Azure Portal** > **Microsoft Entra ID** > **App registrations**
2. Click **New registration**
3. Enter a name (e.g., "FoundationaLLM-SharePoint-Connector")
4. Click **Register**

### Configure API Permissions

Add the following SharePoint application permissions:

| Permission | Type | Description |
|------------|------|-------------|
| `Sites.Read.All` | Application | Read all site collections |
| OR `Sites.Selected` | Application | Read selected site collections only |
| `Group.ReadWrite.All` | Application | (If using group sites) |
| `User.ReadWrite.All` | Application | (If querying user info) |

> [!NOTE]
> Use `Sites.Selected` for more granular control. This requires additional configuration in SharePoint to grant access to specific sites.

### Grant Admin Consent

1. In the app registration, go to **API permissions**
2. Click **Grant admin consent for [organization]**
3. Confirm the action

### Configure Certificate Authentication

SharePoint Online requires certificate-based authentication:

1. Create a self-signed X.509 certificate or obtain one from a certificate authority
2. Upload the public certificate (.cer) to the app registration under **Certificates & secrets**
3. Upload the full certificate (.pfx) to Azure Key Vault
4. Note the certificate name for configuration

For detailed certificate setup, see the [SharePoint app-only access documentation](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

> [!WARNING]
> Certificate-based authentication is the only supported method for SharePoint Online access. API keys and other authentication methods will result in "Access Denied" errors.

## Step 2: Configure App Configuration Settings

Add the following settings to your Azure App Configuration:

| Setting Key | Description | Example Value |
|-------------|-------------|---------------|
| `FoundationaLLM:DataSources:{name}:ClientId` | App registration client ID | `a1b2c3d4-...` |
| `FoundationaLLM:DataSources:{name}:TenantId` | SharePoint tenant ID | `e5f6g7h8-...` |
| `FoundationaLLM:DataSources:{name}:CertificateName` | Certificate name in Key Vault | `sharepoint-connector-cert` |
| `FoundationaLLM:DataSources:{name}:KeyVaultURL` | Key Vault URL | `https://myvault.vault.azure.net/` |

Replace `{name}` with a unique identifier for your SharePoint data source (e.g., `sharepointsite01`).

## Step 3: Create SharePoint Data Source

### Using the Management Portal

1. Navigate to **Management Portal** > **Data Sources**
2. Click **Create Data Source**
3. Select **SharePoint Online** as the type
4. Fill in the configuration:

| Field | Description |
|-------|-------------|
| **Name** | Unique identifier for this data source |
| **Site URL** | Full URL of the SharePoint site |
| **Document Libraries** | List of libraries to index |

<!-- [TODO: Add screenshot of SharePoint data source creation UI] -->

### Using the Management API

```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/sharepointsite01
Content-Type: application/json

{
  "type": "SharePointOnline",
  "name": "sharepointsite01",
  "description": "Corporate documentation site",
  "site_url": "https://contoso.sharepoint.com/sites/documentation",
  "document_libraries": [
    "/documents",
    "/shared-files"
  ],
  "configuration_references": {
    "ClientId": "FoundationaLLM:DataSources:sharepointsite01:ClientId",
    "TenantId": "FoundationaLLM:DataSources:sharepointsite01:TenantId",
    "CertificateName": "FoundationaLLM:DataSources:sharepointsite01:CertificateName",
    "KeyVaultURL": "FoundationaLLM:DataSources:sharepointsite01:KeyVaultURL"
  }
}
```

## Step 4: Create a Data Pipeline

Create a data pipeline to process documents from SharePoint:

1. Navigate to **Management Portal** > **Data Pipelines**
2. Click **Create Pipeline**
3. Configure the pipeline stages:
   - **Data Source**: Select your SharePoint data source
   - **Text Extraction**: Extract text from documents
   - **Text Partitioning**: Split text into chunks
   - **Text Embedding**: Generate embeddings
   - **Indexing**: Store in Azure AI Search

For detailed pipeline configuration, see [Data Pipeline Management](../management-ui/data-pipeline-management.md).

## Step 5: Configure Pipeline Trigger

Set up how the pipeline processes SharePoint documents:

| Trigger Type | Description | Use Case |
|--------------|-------------|----------|
| **Manual** | Run on-demand | Testing, one-time imports |
| **Schedule** | Run on a schedule | Regular content updates |
| **Event** | Run on content changes | Real-time sync (coming soon) |

<!-- [TODO: Document event-based triggering when available] -->

## Supported File Types

The following file types are supported from SharePoint:

| Category | File Types |
|----------|------------|
| Documents | PDF, DOCX, DOC, TXT, RTF |
| Spreadsheets | XLSX, XLS, CSV |
| Presentations | PPTX, PPT |
| Web | HTML, HTM |
| Data | JSON, XML |

<!-- [TODO: Confirm complete list of supported file types] -->
<!-- [TODO: Document file size limits] -->

## Sync and Refresh Behavior

### Initial Sync

When you first run the pipeline:
1. All documents in configured libraries are retrieved
2. Documents are processed through the pipeline stages
3. Vectors are stored in the configured index

### Incremental Updates

Subsequent pipeline runs:
- Detect new and modified documents
- Process only changed content
- Update existing vectors for modified documents

<!-- [TODO: Confirm incremental update behavior] -->

## Troubleshooting

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| Access Denied | Authentication failure | Verify certificate and permissions |
| Site Not Found | Incorrect site URL | Check site URL format |
| Library Not Found | Invalid library path | Verify library path starts with `/` |
| Certificate Error | Missing or expired cert | Update certificate in Key Vault |

### Verifying Connectivity

To test SharePoint connectivity:

1. Check the data source status in Management Portal
2. Run a test query via the Management API
3. Review logs for authentication errors

## Security Considerations

1. **Least Privilege**: Use `Sites.Selected` when possible to limit access
2. **Certificate Rotation**: Rotate certificates before expiration
3. **Audit Logs**: Enable SharePoint audit logging for compliance
4. **Data Classification**: Be aware of sensitive content being vectorized

## Related Topics

- [Vectorization Profiles](vectorization-profiles.md) - Full data source configuration reference
- [Data Pipeline Management](../management-ui/data-pipeline-management.md) - Pipeline configuration guide
- [Azure Data Lake Guide](azure-data-lake-guide.md) - Alternative data source option
