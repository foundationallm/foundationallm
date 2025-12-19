# Permissions & Roles Reference

Reference documentation for FoundationaLLM permissions and role definitions.

## Overview

FoundationaLLM implements a Role-Based Access Control (RBAC) system that mirrors Azure RBAC patterns. Access is controlled through:

- **Role Definitions**: Named sets of permissions
- **Role Assignments**: Bindings between principals, roles, and scopes
- **Authorizable Actions**: Granular permission strings

## Role Definitions

### Core Roles

#### Owner

| Property | Value |
|----------|-------|
| ID | `1301f8d4-3bea-4880-945f-315dbd2ddb46` |
| Description | Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC. |
| Permissions | `*` (all actions) |

**Use Cases:**
- Instance administrators
- Full platform management
- Managing other users' access

#### Contributor

| Property | Value |
|----------|-------|
| ID | `a9f0020f-6e3a-49bf-8d1d-35fd53058edf` |
| Description | Full access to manage all resources without the ability to assign roles in FoundationaLLM RBAC. |
| Permissions | `*` (all actions) |
| Excluded | `FoundationaLLM.Authorization/*/write`, `FoundationaLLM.Authorization/*/delete` |

**Use Cases:**
- Creating and managing agents, data sources, pipelines
- Platform configuration (non-security)

#### Reader

| Property | Value |
|----------|-------|
| ID | `00a53e72-f66e-4c03-8f81-7e885fd2eb35` |
| Description | View all resources without the ability to make any changes. |
| Permissions | `*/read` |

**Use Cases:**
- Auditors and compliance reviewers
- Read-only access for reporting

#### User Access Administrator

| Property | Value |
|----------|-------|
| ID | `fb8e0fd0-f7e2-4957-89d6-19f44f7d6618` |
| Description | Manage access to FoundationaLLM resources. |
| Permissions | `*/read`, `FoundationaLLM.Authorization/*` |

**Use Cases:**
- Delegated access management
- User onboarding/offboarding

#### Role Based Access Control Administrator

| Property | Value |
|----------|-------|
| ID | `17ca4b59-3aee-497d-b43b-95dd7d916f99` |
| Description | Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC. |
| Permissions | Role assignment read/write/delete, role definition read |

**Use Cases:**
- Focused access management (no resource modification)

### Specialized Contributor Roles

#### Agents Contributor

| Property | Value |
|----------|-------|
| ID | `3f28aa77-a854-4aa7-ae11-ffda238275c9` |
| Description | Create new agents. |

#### Attachments Contributor

| Property | Value |
|----------|-------|
| ID | `8e77fb6a-7a78-43e1-b628-d9e2285fe25a` |
| Description | Upload attachments including uploading to Azure OpenAI file store. |
| Permissions | Attachment read/write, Azure OpenAI conversation/file mappings, API endpoint configs, AI models |

**Use Cases:**
- Users who need to upload files to agents

#### Conversations Contributor

| Property | Value |
|----------|-------|
| ID | `d0d21b90-5317-499a-9208-3a6cb71b84f9` |
| Description | Create and update conversations, including Azure OpenAI Assistants threads. |
| Permissions | Conversation read/write, Azure OpenAI conversation mappings, API endpoint configs, AI models |

**Use Cases:**
- Chat user portal users

#### Data Pipelines Contributor

| Property | Value |
|----------|-------|
| ID | `2da16a58-ed63-431a-b90e-9df32c2cae4a` |
| Description | Create new data pipelines. |
| Permissions | Read access to data pipelines, vectorization pipelines, data sources, profiles, plugins |

**Use Cases:**
- Data engineers creating pipelines

#### Data Pipelines Execution Manager

| Property | Value |
|----------|-------|
| ID | `e959eecb-8edf-4442-b532-4990f9a1df2b` |
| Description | Manage all aspects related to data pipeline runs. |
| Permissions | Data pipeline read/write, data sources, API endpoints, AI models, plugins, vector databases |

**Use Cases:**
- Operators running and monitoring pipelines

#### Prompts Contributor

| Property | Value |
|----------|-------|
| ID | `479e7b36-5965-4a7f-baf7-84e57be854aa` |
| Description | Create new prompts. |

#### Vector Databases Contributor

| Property | Value |
|----------|-------|
| ID | `c026f070-abc2-4419-aed9-ec0676f81519` |
| Description | Create new vector databases. |
| Permissions | Vector database read, API endpoint configuration read |

#### Agent Access Tokens Contributor

| Property | Value |
|----------|-------|
| ID | `8c5ea0d3-f5a1-4be5-90a7-a12921c45542` |
| Description | Create new agent access tokens. |

#### Resource Providers Administrator

| Property | Value |
|----------|-------|
| ID | `63b6cc4d-9e1c-4891-8201-cf58286ebfe6` |
| Description | Execute management actions on resource providers. |
| Permissions | `*/management/write` |

---

## Authorizable Actions

Actions follow the pattern: `{ResourceProvider}/{ResourceType}/{Operation}`

### Authorization Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Authorization/roleAssignments/read` | Read role assignments |
| `FoundationaLLM.Authorization/roleAssignments/write` | Create or update role assignments |
| `FoundationaLLM.Authorization/roleAssignments/delete` | Delete role assignments |
| `FoundationaLLM.Authorization/roleDefinitions/read` | Read role definitions |
| `FoundationaLLM.Authorization/securityPrincipals/read` | Read security principals (users, groups, service principals) |
| `FoundationaLLM.Authorization/management/write` | Execute management actions |

### Agent Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Agent/agents/read` | Read agents |
| `FoundationaLLM.Agent/agents/write` | Create or update agents |
| `FoundationaLLM.Agent/agents/delete` | Delete agents |
| `FoundationaLLM.Agent/workflows/read` | Read workflows |
| `FoundationaLLM.Agent/workflows/write` | Create or update workflows |
| `FoundationaLLM.Agent/workflows/delete` | Delete workflows |
| `FoundationaLLM.Agent/tools/read` | Read tools |
| `FoundationaLLM.Agent/tools/write` | Create or update tools |
| `FoundationaLLM.Agent/tools/delete` | Delete tools |
| `FoundationaLLM.Agent/agentTemplates/read` | Read agent templates |
| `FoundationaLLM.Agent/agentTemplates/write` | Create or update agent templates |
| `FoundationaLLM.Agent/agentTemplates/delete` | Delete agent templates |
| `FoundationaLLM.Agent/management/write` | Execute management actions |

### AI Model Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.AIModel/aiModels/read` | Read AI models |
| `FoundationaLLM.AIModel/aiModels/write` | Create or update AI models |
| `FoundationaLLM.AIModel/aiModels/delete` | Delete AI models |
| `FoundationaLLM.AIModel/management/write` | Execute management actions |

### Attachment Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Attachment/attachments/read` | Read attachments |
| `FoundationaLLM.Attachment/attachments/write` | Create or update attachments |
| `FoundationaLLM.Attachment/attachments/delete` | Delete attachments |

### Azure AI Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.AzureAI/agentConversationMappings/read` | Read Azure AI Agent Service conversation mappings |
| `FoundationaLLM.AzureAI/agentConversationMappings/write` | Create or update Azure AI Agent Service conversation mappings |
| `FoundationaLLM.AzureAI/agentConversationMappings/delete` | Delete Azure AI Agent Service conversation mappings |
| `FoundationaLLM.AzureAI/agentFileMappings/read` | Read Azure AI Agent Service file mappings |
| `FoundationaLLM.AzureAI/agentFileMappings/write` | Create or update Azure AI Agent Service file mappings |
| `FoundationaLLM.AzureAI/agentFileMappings/delete` | Delete Azure AI Agent Service file mappings |
| `FoundationaLLM.AzureAI/projects/read` | Read Azure AI project resources |
| `FoundationaLLM.AzureAI/projects/write` | Create or update Azure AI project resources |
| `FoundationaLLM.AzureAI/projects/delete` | Delete Azure AI project resources |
| `FoundationaLLM.AzureAI/management/write` | Execute management actions |

### Azure OpenAI Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.AzureOpenAI/conversationMappings/read` | Read Azure OpenAI conversation mappings |
| `FoundationaLLM.AzureOpenAI/conversationMappings/write` | Create or update Azure OpenAI conversation mappings |
| `FoundationaLLM.AzureOpenAI/conversationMappings/delete` | Delete Azure OpenAI conversation mappings |
| `FoundationaLLM.AzureOpenAI/fileMappings/read` | Read Azure OpenAI file mappings |
| `FoundationaLLM.AzureOpenAI/fileMappings/write` | Create or update Azure OpenAI file mappings |
| `FoundationaLLM.AzureOpenAI/fileMappings/delete` | Delete Azure OpenAI file mappings |
| `FoundationaLLM.AzureOpenAI/management/write` | Execute management actions |

### Configuration Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Configuration/appConfigurations/read` | Read app configurations |
| `FoundationaLLM.Configuration/appConfigurations/write` | Create or update app configurations |
| `FoundationaLLM.Configuration/appConfigurations/delete` | Delete app configurations |
| `FoundationaLLM.Configuration/appConfigurationSets/read` | Read app configuration sets |
| `FoundationaLLM.Configuration/keyVaultSecrets/read` | Read Key Vault secrets |
| `FoundationaLLM.Configuration/keyVaultSecrets/write` | Create or update Key Vault secrets |
| `FoundationaLLM.Configuration/keyVaultSecrets/delete` | Delete Key Vault secrets |
| `FoundationaLLM.Configuration/apiEndpointConfigurations/read` | Read API endpoint configurations |
| `FoundationaLLM.Configuration/apiEndpointConfigurations/write` | Create or update API endpoint configurations |
| `FoundationaLLM.Configuration/apiEndpointConfigurations/delete` | Delete API endpoint configurations |
| `FoundationaLLM.Configuration/management/write` | Execute management actions |

### Context Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Context/knowledgeSources/read` | Read context knowledge sources |
| `FoundationaLLM.Context/knowledgeSources/write` | Create or update context knowledge sources |
| `FoundationaLLM.Context/knowledgeSources/delete` | Delete context knowledge sources |
| `FoundationaLLM.Context/knowledgeUnits/read` | Read context knowledge units |
| `FoundationaLLM.Context/knowledgeUnits/write` | Create or update context knowledge units |
| `FoundationaLLM.Context/knowledgeUnits/delete` | Delete context knowledge units |
| `FoundationaLLM.Context/management/write` | Execute management actions |

### Conversation Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Conversation/conversations/read` | Read conversations |
| `FoundationaLLM.Conversation/conversations/write` | Create or update conversations |
| `FoundationaLLM.Conversation/conversations/delete` | Delete conversations |
| `FoundationaLLM.Conversation/management/write` | Execute management actions |

### Data Pipeline Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.DataPipeline/dataPipelines/read` | Read data pipelines |
| `FoundationaLLM.DataPipeline/dataPipelines/write` | Create or update data pipelines |
| `FoundationaLLM.DataPipeline/dataPipelines/delete` | Delete data pipelines |
| `FoundationaLLM.DataPipeline/management/write` | Execute management actions |

### Data Source Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.DataSource/dataSources/read` | Read data sources |
| `FoundationaLLM.DataSource/dataSources/write` | Create or update data sources |
| `FoundationaLLM.DataSource/dataSources/delete` | Delete data sources |
| `FoundationaLLM.DataSource/management/write` | Execute management actions |

### Plugin Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Plugin/plugins/read` | Read plugins |
| `FoundationaLLM.Plugin/plugins/write` | Create or update plugins |
| `FoundationaLLM.Plugin/plugins/delete` | Delete plugins |
| `FoundationaLLM.Plugin/pluginPackages/read` | Read plugin packages |
| `FoundationaLLM.Plugin/pluginPackages/write` | Create or update plugin packages |
| `FoundationaLLM.Plugin/pluginPackages/delete` | Delete plugin packages |
| `FoundationaLLM.Plugin/management/write` | Execute management actions |

### Prompt Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Prompt/prompts/read` | Read prompts |
| `FoundationaLLM.Prompt/prompts/write` | Create or update prompts |
| `FoundationaLLM.Prompt/prompts/delete` | Delete prompts |
| `FoundationaLLM.Prompt/management/write` | Execute management actions |

### Vector Actions

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Vector/vectorDatabases/read` | Read vector databases |
| `FoundationaLLM.Vector/vectorDatabases/write` | Create or update vector databases |
| `FoundationaLLM.Vector/vectorDatabases/delete` | Delete vector databases |
| `FoundationaLLM.Vector/management/write` | Execute management actions |

### Vectorization Actions (Legacy)

| Action | Description |
|--------|-------------|
| `FoundationaLLM.Vectorization/vectorizationPipelines/read` | Read vectorization pipelines |
| `FoundationaLLM.Vectorization/vectorizationPipelines/write` | Create or update vectorization pipelines |
| `FoundationaLLM.Vectorization/vectorizationPipelines/delete` | Delete vectorization pipelines |
| `FoundationaLLM.Vectorization/vectorizationRequests/read` | Read vectorization requests |
| `FoundationaLLM.Vectorization/vectorizationRequests/write` | Create or update vectorization requests |
| `FoundationaLLM.Vectorization/vectorizationRequests/delete` | Delete vectorization requests |
| `FoundationaLLM.Vectorization/contentSourceProfiles/read` | Read content source profiles |
| `FoundationaLLM.Vectorization/contentSourceProfiles/write` | Create or update content source profiles |
| `FoundationaLLM.Vectorization/contentSourceProfiles/delete` | Delete content source profiles |
| `FoundationaLLM.Vectorization/textPartitioningProfiles/read` | Read text partitioning profiles |
| `FoundationaLLM.Vectorization/textPartitioningProfiles/write` | Create or update text partitioning profiles |
| `FoundationaLLM.Vectorization/textPartitioningProfiles/delete` | Delete text partitioning profiles |
| `FoundationaLLM.Vectorization/textEmbeddingProfiles/read` | Read text embedding profiles |
| `FoundationaLLM.Vectorization/textEmbeddingProfiles/write` | Create or update text embedding profiles |
| `FoundationaLLM.Vectorization/textEmbeddingProfiles/delete` | Delete text embedding profiles |
| `FoundationaLLM.Vectorization/indexingProfiles/read` | Read indexing profiles |
| `FoundationaLLM.Vectorization/indexingProfiles/write` | Create or update indexing profiles |
| `FoundationaLLM.Vectorization/indexingProfiles/delete` | Delete indexing profiles |

---

## Scope Hierarchy

Permissions are evaluated against a hierarchical scope structure:

```
/instances/{instanceId}
├── /providers/FoundationaLLM.Agent
│   ├── /agents/{agentName}
│   ├── /workflows/{workflowName}
│   └── /tools/{toolName}
├── /providers/FoundationaLLM.AIModel
│   └── /aiModels/{modelName}
├── /providers/FoundationaLLM.Authorization
│   ├── /roleAssignments/{assignmentId}
│   └── /roleDefinitions/{definitionId}
├── /providers/FoundationaLLM.Configuration
│   ├── /appConfigurations/{configName}
│   └── /apiEndpointConfigurations/{endpointName}
├── /providers/FoundationaLLM.DataPipeline
│   └── /dataPipelines/{pipelineName}
├── /providers/FoundationaLLM.DataSource
│   └── /dataSources/{dataSourceName}
├── /providers/FoundationaLLM.Plugin
│   ├── /plugins/{pluginName}
│   └── /pluginPackages/{packageName}
├── /providers/FoundationaLLM.Prompt
│   └── /prompts/{promptName}
└── /providers/FoundationaLLM.Vector
    └── /vectorDatabases/{databaseName}
```

### Scope Inheritance

- Permissions assigned at a parent scope are inherited by child scopes
- Instance-level assignments (`/instances/{instanceId}`) apply to all resources
- Provider-level assignments apply to all resources of that type
- Resource-level assignments apply only to the specific resource

## Wildcards

Actions support wildcards:

| Pattern | Meaning |
|---------|---------|
| `*` | All actions on all resources |
| `*/read` | All read actions |
| `*/write` | All write actions |
| `*/delete` | All delete actions |
| `*/management/write` | All management write actions |
| `FoundationaLLM.Authorization/*` | All authorization actions |

## Related Topics

- [Instance Access Control](../how-to-guides/security/instance-access-control.md)
- [Resource Management Concepts](concepts/resource-management.md)
