# Private Storage for Custom Agent Owners

Private storage enables self-service agent owners to connect their agents to personal knowledge sources without requiring administrator assistance or sharing data with other agents.

## Overview

When creating a self-service agent, owners can configure a private storage location that:

- Is accessible only to their specific agent
- Contains documents relevant to the agent's purpose
- Is automatically vectorized and indexed
- Remains isolated from other users and agents

## Use Cases

| Scenario | Benefit |
|----------|---------|
| Department-specific knowledge | Team can manage their own documents |
| Project documentation | Isolated context for project agents |
| Personal reference materials | Individual productivity agents |
| Confidential content | Restricted access knowledge bases |

## How Private Storage Works

```
Agent Owner                 FoundationaLLM
    |                            |
    | Upload documents           |
    |--------------------------->| Private Storage
    |                            |      |
    |                            |      v
    |                            | Data Pipeline
    |                            |      |
    |                            |      v
    |                            | Agent-Specific Index
    |                            |
    | Query via Agent            |
    |<-------------------------->|
```

### Key Characteristics

1. **Isolation**: Documents are not visible to other agents or users
2. **Automatic Processing**: Files are automatically vectorized upon upload
3. **Owner Control**: Only the agent owner can add or remove documents
4. **Agent Binding**: Storage is tied to a specific agent

## Prerequisites

Before using private storage:

- Self-service agent creation must be enabled
- You must be the owner or collaborator on the agent
- Sufficient storage quota must be available

<!-- [TODO: Document storage quota configuration] -->

## Configuring Private Storage

### During Agent Creation

1. In the agent creation wizard, navigate to **Tools**
2. Enable **Private Storage Knowledge Source**
3. Configure storage settings:

| Setting | Description |
|---------|-------------|
| **Storage Name** | Display name for this storage |
| **Description** | Optional description |

<!-- [TODO: Add screenshot of private storage configuration UI] -->

### After Agent Creation

1. Edit your agent in **My Agents**
2. Navigate to the **Tools** section
3. Toggle on **Private Storage**
4. Save changes

## Uploading Documents

### Via User Portal

1. Select your agent in the chat interface
2. Navigate to **Agent Settings** > **Knowledge**
3. Click **Upload Files**
4. Select documents from your computer
5. Wait for processing to complete

<!-- [TODO: Confirm upload UX location] -->

### Supported File Types

| Type | Extensions | Max Size |
|------|------------|----------|
| Documents | .pdf, .docx, .txt | <!-- [TODO: Document limits] --> |
| Spreadsheets | .csv, .xlsx | <!-- [TODO: Document limits] --> |
| Presentations | .pptx | <!-- [TODO: Document limits] --> |
| Data | .json, .md | <!-- [TODO: Document limits] --> |

### Upload Limits

<!-- [TODO: Document specific limits] -->

| Limit | Value |
|-------|-------|
| Max file size | TBD |
| Max total storage | TBD |
| Max files per agent | TBD |

## Managing Documents

### Viewing Uploaded Documents

1. Navigate to **My Agents** > [Your Agent]
2. Open **Knowledge** or **Private Storage** section
3. View the list of uploaded documents

### Removing Documents

1. Find the document in your list
2. Click **Delete** or the trash icon
3. Confirm removal
4. Document is removed from the index

> [!NOTE]
> Removing a document also removes its vectors from the search index.

## Processing Pipeline

When you upload a document to private storage:

1. **Upload**: Document stored in agent-specific storage
2. **Extract**: Text extracted from the document
3. **Partition**: Text split into chunks
4. **Embed**: Vectors generated for each chunk
5. **Index**: Vectors stored in agent-specific index partition

Processing typically completes within a few minutes for small documents.

<!-- [TODO: Provide processing time estimates] -->

## Access Control

### Who Can Access Private Storage

| Role | View Documents | Add Documents | Remove Documents |
|------|----------------|---------------|------------------|
| Owner | ✅ | ✅ | ✅ |
| Collaborator | ✅ | ✅ | ✅ |
| User | ❌ | ❌ | ❌ |

### Data Isolation

Private storage ensures:

- Documents are not visible in Management Portal to non-owners
- Vectors are stored in isolated index partitions
- Other agents cannot query your private storage
- Administrators can see storage usage but not content

<!-- [TODO: Confirm admin visibility and audit capabilities] -->

## Storage Location

Private storage documents are stored in:

<!-- [TODO: Document actual storage location (Azure Blob, ADLS, etc.)] -->

| Component | Location |
|-----------|----------|
| Documents | TBD |
| Vectors | TBD |
| Metadata | TBD |

## Quotas and Limits

Each user has private storage quotas:

<!-- [TODO: Document quota system] -->

| Quota | Default | Configurable |
|-------|---------|--------------|
| Storage per agent | TBD | TBD |
| Total storage per user | TBD | TBD |
| File count limit | TBD | TBD |

### Checking Your Usage

1. Navigate to **My Agents** > [Your Agent]
2. View storage usage in the **Knowledge** section

### Requesting More Storage

Contact your administrator to request quota increases.

## Troubleshooting

### Upload Failures

| Issue | Cause | Solution |
|-------|-------|----------|
| File too large | Exceeds size limit | Split into smaller files |
| Unsupported type | File extension not supported | Convert to supported format |
| Quota exceeded | Storage limit reached | Remove unused documents |
| Processing timeout | Very large file | Try smaller file |

### Documents Not Searchable

If uploaded documents aren't appearing in agent responses:

1. Verify upload completed successfully
2. Wait a few minutes for indexing
3. Check the document list to confirm it's indexed
4. Test with a direct query about the document content

### Performance Issues

If agent responses are slow after adding documents:

1. Reduce the number of documents if excessive
2. Ensure documents are well-structured
3. Contact support if issues persist

## Best Practices

1. **Organize before upload**: Clean and organize documents
2. **Use clear filenames**: Helps with search and management
3. **Regular maintenance**: Remove outdated documents
4. **Monitor quotas**: Stay within allocated limits
5. **Test thoroughly**: Verify agent can retrieve uploaded content

## Security Considerations

1. **Sensitive data**: Be cautious about uploading confidential information
2. **Sharing implications**: Collaborators can access your private storage
3. **Backup**: Private storage is not a backup solution
4. **Compliance**: Ensure uploads comply with your organization's policies

## Related Topics

- [Self-Service Agent Creation](../user-portal/self-service-agent-creation.md)
- [Agent Sharing Model](../user-portal/agent-sharing-model.md)
- [Data Pipeline Management](../management-ui/data-pipeline-management.md)
- [Azure Data Lake Guide](../vectorization/azure-data-lake-guide.md)
