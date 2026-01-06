# SharePoint Online as a Knowledge Source

Learn how to use SharePoint Online and OneDrive with FoundationaLLM agents.

## Overview

FoundationaLLM provides **two ways** to integrate with SharePoint Online and OneDrive:

| Approach | Description | Best For |
|----------|-------------|----------|
| **End-User OneDrive Upload** | Users upload files directly from OneDrive in the Chat User Portal | Ad-hoc document analysis, individual user files |
| **Backend Knowledge Source** | Administrators configure SharePoint as a data source for agents | Organization-wide knowledge bases, departmental content |

This guide covers both approaches to help you choose the right integration method for your needs.

---

## Approach 1: End-User OneDrive Integration

End users can upload files directly from their OneDrive (Work or School) account through the Chat User Portal. This allows users to share personal documents with agents for analysis without requiring administrator configuration.

### How It Works

1. Users click the **paperclip icon** (📎) in the Chat User Portal
2. Click **Select file from OneDrive** (or **Connect to OneDrive** if not yet connected)
3. Authenticate with their work/school Microsoft account
4. Browse and select files from their OneDrive
5. Files are uploaded to the conversation for agent analysis

### User Experience

When using OneDrive upload:

- Files are uploaded to the **current conversation** only
- Documents are processed by the agent for that session
- This is ideal for **ad-hoc questions** about specific documents
- Files are **not** permanently indexed into a knowledge base

### Prerequisites for OneDrive Upload

For users to access the OneDrive upload feature:

1. **OneDrive for Business** must be enabled for your organization
2. **Microsoft Graph permissions** must be configured for the User Portal
3. Users must have **work or school accounts** (not personal Microsoft accounts)

### Enabling OneDrive Integration

OneDrive integration is configured at the platform level. Contact your administrator to:

1. Ensure the User Portal app registration has the required Graph permissions
2. Verify OneDrive for Business is available to users
3. Check that any conditional access policies allow the integration

### For More Information

See [Uploading Files to a Conversation](../../../../chat-user-portal/how-to-guides/using-agents/uploading-files.md) for detailed user instructions.

---

## Approach 2: Backend SharePoint Knowledge Source

Administrators can configure SharePoint Online as a **backend data source** to provide persistent knowledge for agents. This approach indexes SharePoint document libraries into a searchable knowledge base.

### When to Use Backend Integration

This approach is ideal for:

- Team collaboration documents
- Policy and procedure libraries
- Project documentation
- Departmental content that should be accessible to all users of an agent

### Prerequisites

Before configuring SharePoint as a backend knowledge source:

- **SharePoint Online site** with documents to index
- **Microsoft Entra ID (Azure AD) app registration** with appropriate permissions
- **Consent granted** for the app to access SharePoint
- **Network connectivity** to Microsoft 365 services
- **Management Portal access** with appropriate permissions

### Required Permissions

The app registration needs the following Microsoft Graph permissions:

| Permission | Type | Description |
|------------|------|-------------|
| `Sites.Read.All` | Application | Read items in all site collections |
| `Files.Read.All` | Application | Read all files user can access |

> **Note:** Permissions may vary based on your organization's requirements. Consult with your Microsoft 365 administrator.

### Configuring SharePoint as a Data Source

#### Step 1: Navigate to Data Sources

1. In the Management Portal sidebar, click **Data Sources**
2. Click **Create Data Source**
3. Select **SharePoint Online Site** from the type dropdown

#### Step 2: Enter Basic Information

| Field | Description |
|-------|-------------|
| **Data Source Name** | Unique identifier (e.g., `hr-sharepoint`) |
| **Description** | Purpose and contents (e.g., "HR department documents") |

#### Step 3: Configure Connection Settings

| Field | Description |
|-------|-------------|
| **Site URL** | Full URL to the SharePoint site (e.g., `https://contoso.sharepoint.com/sites/HR`) |
| **Authentication Type** | Authentication method (Managed Identity or App Registration) |

#### Step 4: Configure Authentication

**Using Managed Identity:**

1. Ensure your deployment has a managed identity
2. Grant the managed identity access to SharePoint via Microsoft Graph
3. Select **Azure Identity** or **Managed Identity** authentication

**Using App Registration:**

1. Create or use an existing Azure AD app registration
2. Configure the required permissions
3. Grant admin consent
4. Enter the client credentials in the data source configuration

> **TODO**: Document specific fields for app registration authentication (Client ID, Client Secret, Tenant ID) when visible in the UI.

#### Step 5: Specify Document Library

Indicate which document libraries or folders to access:

| Field | Description |
|-------|-------------|
| **Document Library** | Name of the library (e.g., `Documents`, `Shared Documents`) |
| **Folder Path** | Specific folder within the library (optional) |

#### Step 6: Save the Data Source

Click **Create Data Source** to save the configuration.

### Creating an App Registration

1. Navigate to Azure Portal > **Microsoft Entra ID** > **App registrations**
2. Click **New registration**
3. Enter a name (e.g., `FoundationaLLM-SharePoint-Reader`)
4. Select **Accounts in this organizational directory only**
5. Click **Register**

### Configuring Permissions

1. In the app registration, go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Choose **Application permissions**
5. Add required permissions (Sites.Read.All, Files.Read.All)
6. Click **Add permissions**
7. Click **Grant admin consent** (requires admin privileges)

### Creating Client Secret

1. In the app registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Enter a description and expiration
4. Copy the secret value immediately (it won't be shown again)

### Document Library Paths

#### Common Path Formats

| Library Type | Path Format |
|--------------|-------------|
| Default Documents | `Shared Documents` or `Documents` |
| Custom Library | Library display name |
| Subfolder | `Documents/Policies` |

#### Finding the Correct Path

1. Navigate to your SharePoint site
2. Open the document library
3. Note the library name from the URL or navigation

### Using in Data Pipelines

After creating the data source:

1. Navigate to **Data Pipelines**
2. Create a new pipeline
3. Select your SharePoint data source
4. Configure processing stages (text extraction, embedding, etc.)
5. Run the pipeline to index documents

### Supported File Types

SharePoint data sources typically support:

| Category | Extensions |
|----------|------------|
| **Office Documents** | DOCX, DOC, XLSX, XLS, PPTX, PPT |
| **PDF** | PDF |
| **Text** | TXT, MD, HTML |

### Sync and Updates

**Initial Indexing:** The first pipeline run indexes all documents in the specified libraries.

**Incremental Updates:**

> **TODO**: Document incremental sync capabilities and how to handle document updates/deletions.

---

## Comparison: Which Approach to Use

| Feature | OneDrive Upload | Backend Knowledge Source |
|---------|-----------------|--------------------------|
| **Setup Required** | Minimal (user-driven) | Administrator configuration |
| **Persistence** | Per-conversation only | Permanent knowledge base |
| **Scope** | Individual user files | Organization/team documents |
| **Use Case** | Ad-hoc analysis | Structured knowledge retrieval |
| **Data Pipeline** | Not required | Required for indexing |
| **File Access** | User's own OneDrive | Configured SharePoint sites |

### Best Practice Recommendations

- Use **OneDrive Upload** when users need to analyze their own documents on-demand
- Use **Backend Knowledge Source** when you need agents to have access to shared organizational content
- You can use **both approaches** together — agents can search backend knowledge while users also upload additional documents

---

## Troubleshooting

### OneDrive Upload Issues

**Can't connect to OneDrive:**
- Verify you're using a work or school account
- Check that OneDrive for Business is enabled
- Clear browser cache and try again

**Files not showing in picker:**
- Ensure files are in your OneDrive, not SharePoint
- Check file permissions
- Verify OneDrive sync is complete

### Backend Data Source Issues

**Authentication Failures:**
- Verify app registration credentials
- Check admin consent was granted
- Ensure client secret hasn't expired
- Confirm tenant ID is correct

**Access Denied:**
- Verify the site URL is correct
- Check the app has Sites.Read.All permission
- Ensure the document library name is correct

**Documents Not Found:**
- Verify folder paths are correct
- Check documents aren't in a subfolder not included
- Ensure documents aren't checked out or locked

**Connectivity Issues:**
- Verify network allows access to SharePoint Online
- Check for conditional access policies blocking access
- Review Microsoft 365 service health

---

## Security Considerations

- Use application permissions rather than delegated for background processing
- Apply principle of least privilege
- Regularly rotate client secrets
- Monitor access through Azure AD audit logs
- Review which SharePoint sites are exposed to agents

---

## Related Topics

- [Uploading Files to a Conversation](../../../../chat-user-portal/how-to-guides/using-agents/uploading-files.md)
- [Data Sources](../data-sources.md)
- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Azure Data Lake Knowledge Source](azure-data-lake.md)
