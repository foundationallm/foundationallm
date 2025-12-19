# Private Storage for Agents

Learn how to configure private storage for agent-specific data in FoundationaLLM.

## Overview

Private storage provides dedicated, isolated storage containers for individual agents. This enables:

- **Data Isolation**: Keep agent-specific data separate from shared resources
- **Access Control**: Granular permissions for who can access agent data
- **Governance**: Meet compliance requirements for data segregation

## Use Cases

| Scenario | Description |
|----------|-------------|
| **Department-specific agents** | HR, Finance, Legal agents with sensitive data |
| **Project isolation** | Separate data for different projects or clients |
| **Compliance requirements** | Data that must be isolated for regulatory reasons |
| **Development/Testing** | Isolated environments for agent development |

## Accessing Private Storage Configuration

Private storage is configured at the agent level:

1. Navigate to **Agents** in the sidebar
2. Edit an existing agent or create a new one
3. Look for the **Private Storage** button in the agent configuration header

## Configuring Private Storage

### Prerequisites

- An agent using a workflow that supports private storage
  - OpenAI Assistants workflow
  - FoundationaLLM Function Calling workflow
- Appropriate permissions to configure the agent

### Setup Steps

1. **Access Private Storage Settings**
   - Open the agent for editing
   - Click the **Private Storage** button
   - The Private Storage configuration panel appears

2. **Configure Storage Location**
   > **TODO**: Document specific storage configuration options available in the Private Storage dialog, including:
   > - Storage account selection
   > - Container naming
   > - Folder structure

3. **Set Access Permissions**
   - Define who can read/write to the private storage
   - Configure role-based access as needed

4. **Save Configuration**
   - Apply the changes
   - Storage is provisioned for the agent

## Tool Association

Private storage is typically associated with specific tools:

1. When configuring tools (e.g., Knowledge Tool), you can specify the private storage
2. The **Tools** parameter in the Private Storage dialog shows which tools use this storage

## Data Management

### Uploading Data

Data in private storage can be populated through:

1. **File Uploads**: Users upload files through the Chat User Portal
2. **Data Pipelines**: Process and index data into the private storage
3. **API Integration**: Programmatic data upload via APIs

### Data Organization

Best practices for organizing private storage:

```
agent-private-storage/
├── uploads/           # User-uploaded files
├── processed/         # Pipeline-processed content
└── indexes/           # Vector indexes and embeddings
```

## Access Control

### Agent-Level Permissions

Private storage inherits the agent's access control settings:

- Users with agent access can interact with its private storage
- Owners can manage storage configuration
- Contributors can upload and access data

### Resource-Level Permissions

> **TODO**: Document specific resource-level permissions for private storage if available.

## Security Considerations

| Practice | Description |
|----------|-------------|
| **Least Privilege** | Grant only necessary access to private storage |
| **Regular Review** | Periodically audit who has access |
| **Data Classification** | Understand what data sensitivity level is stored |
| **Encryption** | Ensure data is encrypted at rest and in transit |

## Monitoring and Maintenance

### Storage Usage

> **TODO**: Document how to monitor private storage usage and capacity.

### Cleanup

- Remove unused files periodically
- Archive old data as needed
- Follow retention policies

## Troubleshooting

### Storage Not Available

- Verify the agent workflow supports private storage
- Check that storage was properly provisioned
- Review permissions configuration

### Upload Failures

- Check storage capacity
- Verify user has upload permissions
- Review file size limits

### Data Not Searchable

- Ensure data pipeline processed the content
- Check indexing completed successfully
- Verify tool configuration points to correct storage

## Related Topics

- [Azure Data Lake as a Knowledge Source](azure-data-lake.md)
- [Data Sources](../data-sources.md)
- [Instance Access Control](../../security/instance-access-control.md)
- [Create New Agent](../../agents/create-new-agent.md)
