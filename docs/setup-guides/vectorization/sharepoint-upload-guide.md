# Using SharePoint Online with FoundationaLLM

This guide explains the two ways to use SharePoint Online with FoundationaLLM: end-user file selection from OneDrive and administrator-configured backend knowledge sources.

## Overview

FoundationaLLM integrates with SharePoint Online and OneDrive in two distinct ways:

| Approach | User | Purpose |
|----------|------|---------|
| **OneDrive File Selection** | End Users | Upload individual files from OneDrive during conversations |
| **Backend Knowledge Source** | Administrators | Configure SharePoint document libraries as knowledge sources for agents |

## Approach 1: End User OneDrive File Selection

### Overview

End users can upload files directly from their OneDrive during conversations with agents. This provides a seamless way to share documents without downloading and re-uploading.

### How It Works

1. While chatting with an agent in the User Portal, click the **attachment** icon
2. Select **"Select file from OneDrive"** button
3. Browse and select files from your OneDrive
4. Files are uploaded to the conversation for the agent to analyze

<!-- [TODO: Add screenshot of OneDrive file selection button] -->

### User Experience

```
User Portal Chat
        |
        v
   Attachment Menu
        |
        ├── Upload from Computer
        └── Select file from OneDrive  <-- Opens OneDrive picker
                    |
                    v
              OneDrive Picker
                    |
                    v
              File Selected
                    |
                    v
              Agent Analysis
```

### Supported File Types

Files selected from OneDrive follow the same file type support as regular uploads:

| Category | Extensions |
|----------|------------|
| Documents | PDF, DOCX, DOC, TXT, RTF |
| Spreadsheets | XLSX, XLS, CSV |
| Presentations | PPTX, PPT |
| Images | PNG, JPG, GIF (for vision-capable agents) |

### Authentication

- Uses the user's existing Microsoft 365 authentication
- No additional configuration required for end users
- Files are accessed with the user's permissions

### Use Cases

- Share a specific document for analysis
- Get insights from a spreadsheet
- Ask questions about a presentation
- Provide context for a conversation

---

## Approach 2: Backend Knowledge Source Configuration

### Overview

Administrators can configure SharePoint Online document libraries as backend knowledge sources. This enables agents to access organizational knowledge bases stored in SharePoint.

### Key Differences from OneDrive Selection

| Aspect | OneDrive Selection | Backend Knowledge Source |
|--------|-------------------|-------------------------|
| Configured by | End user (per conversation) | Administrator |
| Scope | Single file, single conversation | Entire document library, all conversations |
| Processing | On-demand | Pre-processed via data pipeline |
| Search | Direct file analysis | Vector-based semantic search |
| Persistence | Conversation duration | Permanent until removed |

### Architecture

```
SharePoint Online          FoundationaLLM
     |                          |
     | Document Libraries       |
     |------------------------->|  Data Pipeline
     |  (via Graph API)         |       |
     |                          |       v
     |                          |  Context Engineering
     |                          |  (embedding, indexing)
     |                          |       |
     |                          |       v
     |                          |  Azure AI Search
     |                          |       |
Agent queries <-----------------+-------+
```

### Prerequisites

- SharePoint Online site with documents to index
- Microsoft Entra ID app registration with SharePoint permissions
- X.509 certificate for authentication
- Azure Key Vault for certificate storage
- Administrator access to Management Portal

### Step 1: Configure Entra ID App Registration

Before configuring SharePoint in FoundationaLLM, you must set up authentication.

#### Create App Registration

1. Navigate to **Azure Portal** > **Microsoft Entra ID** > **App registrations**
2. Click **New registration**
3. Enter a name (e.g., "FoundationaLLM-SharePoint-Connector")
4. Click **Register**

#### Configure API Permissions

Add the following SharePoint application permissions:

| Permission | Type | Description |
|------------|------|-------------|
| `Sites.Read.All` | Application | Read all site collections |
| OR `Sites.Selected` | Application | Read selected site collections only |
| `Group.ReadWrite.All` | Application | (If using group sites) |
| `User.ReadWrite.All` | Application | (If querying user info) |

> [!NOTE]
> Use `Sites.Selected` for more granular control. This requires additional configuration in SharePoint to grant access to specific sites.

#### Grant Admin Consent

1. In the app registration, go to **API permissions**
2. Click **Grant admin consent for [organization]**
3. Confirm the action

#### Configure Certificate Authentication

SharePoint Online requires certificate-based authentication:

1. Create a self-signed X.509 certificate or obtain one from a certificate authority
2. Upload the public certificate (.cer) to the app registration under **Certificates & secrets**
3. Upload the full certificate (.pfx) to Azure Key Vault
4. Note the certificate name for configuration

For detailed certificate setup, see the [SharePoint app-only access documentation](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

> [!WARNING]
> Certificate-based authentication is the only supported method for SharePoint Online access. API keys and other authentication methods will result in "Access Denied" errors.

### Step 2: Configure App Configuration Settings

Add the following settings to your Azure App Configuration:

| Setting Key | Description | Example Value |
|-------------|-------------|---------------|
| `FoundationaLLM:DataSources:{name}:ClientId` | App registration client ID | `a1b2c3d4-...` |
| `FoundationaLLM:DataSources:{name}:TenantId` | SharePoint tenant ID | `e5f6g7h8-...` |
| `FoundationaLLM:DataSources:{name}:CertificateName` | Certificate name in Key Vault | `sharepoint-connector-cert` |
| `FoundationaLLM:DataSources:{name}:KeyVaultURL` | Key Vault URL | `https://myvault.vault.azure.net/` |

Replace `{name}` with a unique identifier for your SharePoint data source (e.g., `sharepointsite01`).

### Step 3: Create SharePoint Data Source

#### Using the Management Portal

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

#### Using the Management API

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

### Step 4: Create a Data Pipeline

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

### Step 5: Configure Pipeline Trigger

Set up how the pipeline processes SharePoint documents:

| Trigger Type | Description | Use Case |
|--------------|-------------|----------|
| **Manual** | Run on-demand | Testing, one-time imports |
| **Schedule** | Run on a schedule | Regular content updates |
| **Event** | Run on content changes | Real-time sync (coming soon) |

<!-- [TODO: Document event-based triggering when available] -->

### Supported File Types

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

### Sync and Refresh Behavior

#### Initial Processing

When you first run the pipeline:
1. All documents in configured libraries are retrieved
2. Documents are processed through the pipeline stages
3. Vectors are stored in the configured index

#### Incremental Updates

Subsequent pipeline runs:
- Detect new and modified documents
- Process only changed content
- Update existing vectors for modified documents

<!-- [TODO: Confirm incremental update behavior] -->

---

## Choosing the Right Approach

| Scenario | Recommended Approach |
|----------|---------------------|
| User wants to analyze their own document | OneDrive File Selection |
| Organization-wide knowledge base | Backend Knowledge Source |
| Ad-hoc document queries | OneDrive File Selection |
| Frequently referenced documentation | Backend Knowledge Source |
| User-specific files | OneDrive File Selection |
| Shared team resources | Backend Knowledge Source |

## Troubleshooting

### OneDrive Selection Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| OneDrive button not visible | Feature disabled | Contact administrator |
| Can't access files | Permission issue | Verify OneDrive permissions |
| File upload fails | Size limit | Check file size limits |

### Backend Knowledge Source Issues

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
4. **Data Classification**: Be aware of sensitive content being processed
5. **User Permissions**: OneDrive selection respects user's SharePoint permissions

## Related Topics

- [Knowledge Source Profiles](vectorization-profiles.md) - Full data source configuration reference
- [Data Pipeline Management](../management-ui/data-pipeline-management.md) - Pipeline configuration guide
- [Azure Data Lake Guide](azure-data-lake-guide.md) - Alternative data source option
