# RBAC and Permissioning System Capabilities

This document provides a comprehensive summary of the Role-Based Access Control (RBAC) and permissioning system capabilities in FoundationaLLM.

## Overview

FoundationaLLM implements a comprehensive RBAC system that provides fine-grained access control to platform resources. The system mirrors Azure RBAC patterns and enables organizations to control who can access resources, what actions they can perform, and where those permissions apply.

## Core Capabilities

### 1. Role-Based Access Control (RBAC)

#### Key Concepts

| Concept | Description |
|---------|-------------|
| **Role Definition** | A collection of permissions (actions) that define what operations can be performed |
| **Role Assignment** | Grants a role to a principal (user, group, or service principal) at a specific scope |
| **Principal** | An identity that can be assigned roles (User, Group, Service Principal, or Managed Identity) |
| **Scope** | The level at which access applies (instance-level or resource-level) |
| **Action** | A granular permission string that defines a specific operation |

#### How RBAC Works

1. **Principal** requests access to a resource
2. System checks **role assignments** for the principal
3. **Permissions** from assigned roles are evaluated at the **scope**
4. Access is granted or denied based on the evaluation

### 2. Role Definitions

#### Built-in Core Roles

| Role | ID | Description | Permissions |
|------|-----|-------------|-------------|
| **Owner** | `1301f8d4-3bea-4880-945f-315dbd2ddb46` | Full access to manage all resources, including role assignment management | `*` (all actions) |
| **Contributor** | `a9f0020f-6e3a-49bf-8d1d-35fd53058edf` | Full access to manage all resources except role assignments | `*` (excluding `FoundationaLLM.Authorization/*/write` and `FoundationaLLM.Authorization/*/delete`) |
| **Reader** | `00a53e72-f66e-4c03-8f81-7e885fd2eb35` | Read-only access to all resources | `*/read` |
| **User Access Administrator** | `fb8e0fd0-f7e2-4957-89d6-19f44f7d6618` | Manage access to FoundationaLLM resources | `*/read`, `FoundationaLLM.Authorization/*` |
| **Role Based Access Control Administrator** | `17ca4b59-3aee-497d-b43b-95dd7d916f99` | Manage access by assigning roles using FoundationaLLM RBAC | Role assignment read/write/delete, role definition read |

#### Specialized Contributor Roles

| Role | ID | Description | Use Case |
|------|-----|-------------|----------|
| **Agents Contributor** | `3f28aa77-a854-4aa7-ae11-ffda238275c9` | Create new agents | Agent creation |
| **Attachments Contributor** | `8e77fb6a-7a78-43e1-b628-d9e2285fe25a` | Upload attachments including uploading to Azure OpenAI file store | File uploads |
| **Conversations Contributor** | `d0d21b90-5317-499a-9208-3a6cb71b84f9` | Create and update conversations, including Azure OpenAI Assistants threads | Chat user portal users |
| **Data Pipelines Contributor** | `2da16a58-ed63-431a-b90e-9df32c2cae4a` | Create new data pipelines | Data engineers |
| **Data Pipelines Execution Manager** | `e959eecb-8edf-4442-b532-4990f9a1df2b` | Manage all aspects related to data pipeline runs | Pipeline operators |
| **Prompts Contributor** | `479e7b36-5965-4a7f-baf7-84e57be854aa` | Create new prompts | Prompt authors |
| **Vector Databases Contributor** | `c026f070-abc2-4419-aed9-ec0676f81519` | Create new vector databases | Vector database management |
| **Agent Access Tokens Contributor** | `8c5ea0d3-f5a1-4be5-90a7-a12921c45542` | Create new agent access tokens | Token management |
| **Knowledge Sources Contributor** | `8eec6664-9abf-4beb-84f7-18d9c2917c7f` | Create new knowledge sources | Knowledge management |
| **Data Sources Contributor** | `78ee11d9-6e6a-4adc-8c16-3613e7445113` | Create new data sources | Data source management |
| **Knowledge Units Contributor** | `5f38b653-e3b7-47a8-8fde-e70ea9e4fa91` | Create new knowledge units | Knowledge unit management |
| **Resource Providers Administrator** | `63b6cc4d-9e1c-4891-8201-cf58286ebfe6` | Execute management actions on resource providers | System administration |

#### Role Definition Structure

Each role definition includes:
- **Name**: Display name of the role
- **Id**: Unique identifier (GUID)
- **Description**: Purpose of the role
- **Actions**: Control plane actions allowed
- **NotActions**: Actions excluded from Actions
- **DataActions**: Data plane actions allowed
- **NotDataActions**: Actions excluded from DataActions
- **AssignableScopes**: Where the role can be assigned

### 3. Permission System

#### Action Format

Actions follow this pattern:
```
FoundationaLLM.{ProviderName}/{resourceType}/{action}
```

#### Action Categories

The system supports actions across multiple resource providers:

- **Authorization**: Role assignments, role definitions, security principals
- **Agent**: Agents, workflows, tools, agent templates
- **AI Model**: AI model configurations
- **Attachment**: File attachments
- **Azure AI**: Azure AI Agent Service mappings and projects
- **Azure OpenAI**: Conversation and file mappings
- **Configuration**: App configurations, Key Vault secrets, API endpoint configurations
- **Context**: Knowledge sources and knowledge units
- **Conversation**: Conversation management
- **Data Pipeline**: Data pipeline operations
- **Data Source**: Data source management
- **Plugin**: Plugins and plugin packages
- **Prompt**: Prompt management
- **Vector**: Vector database operations
- **Vectorization** (Legacy): Vectorization pipelines, requests, and profiles

#### Wildcard Support

| Pattern | Meaning |
|---------|---------|
| `*` | All actions on all resources |
| `*/read` | All read actions |
| `*/write` | All write actions |
| `*/delete` | All delete actions |
| `*/management/write` | All management write actions |
| `FoundationaLLM.Authorization/*` | All authorization actions |
| `FoundationaLLM.Agent/agents/*` | All actions on agents |

#### Control Plane vs Data Plane

- **Control Plane Actions**: Specified in `Actions` and `NotActions`
  - Create/update/delete resources
  - Manage configurations
  - Manage role assignments

- **Data Plane Actions**: Specified in `DataActions` and `NotDataActions`
  - Read resource content
  - Execute operations
  - Access data

> **Important:** Control plane access is NOT inherited to data plane. Having `FoundationaLLM.Agent/agents/write` does not grant `FoundationaLLM.Agent/agents/read`.

### 4. Scope Hierarchy

#### Scope Levels

| Level | Description | Example |
|-------|-------------|---------|
| **Instance** | Entire FoundationaLLM deployment | All agents, prompts, data sources |
| **Resource** | Specific resource | Single agent, prompt, or data source |

#### Scope Format

Scopes are resource identifiers following this pattern:
```
/instances/{instanceId}/providers/{providerName}/{resourceType}/{resourceName}
```

#### Scope Hierarchy Structure

```
/instances/{instanceId}                           <- Instance scope
    /providers/{providerName}
        /{resourceType}
            /{resourceName}                       <- Resource scope
                /{subResourceType}
                    /{subResourceName}            <- Sub-resource scope
```

#### Scope Inheritance Rules

| Rule | Description |
|------|-------------|
| **Hierarchical** | Child scopes inherit parent permissions |
| **Additive** | Lower scopes can add permissions |
| **No Reduction** | Cannot remove inherited permissions at lower levels |

- Roles assigned at instance level apply to all resources
- Resource-level assignments add specific permissions
- Cannot remove inherited permissions at lower levels

### 5. Role Assignments

#### Assignment Components

| Component | Description |
|-----------|-------------|
| **Principal** | Who receives access (user, group, service principal) |
| **Role Definition** | What access is granted |
| **Scope** | Where access applies |
| **Name** | Unique identifier for the assignment (GUID) |
| **Description** | Optional explanation |

#### Principal Types

| Type | Description | Use Case |
|------|-------------|----------|
| **User** | Individual Entra ID user | Named user access |
| **Group** | Entra ID security group | Team/department access |
| **ServicePrincipal** | App registration or managed identity | Automation/integration |

#### Assignment Properties

- `RoleAssignmentName`: Unique GUID for the assignment
- `RoleAssignmentId`: Full resource path including name
- `Scope`: Resource identifier where assignment applies
- `RoleDefinitionName`: Human-readable role name
- `RoleDefinitionId`: Role definition GUID
- `ObjectId`: Principal's unique identifier
- `ObjectType`: `User`, `Group`, or `ServicePrincipal`
- `DisplayName`: Principal's display name
- `SignInName`: Principal's UPN
- `Description`: Optional description

### 6. Management Capabilities

#### Management Portal

The Management Portal provides a UI for:
- Viewing role assignments at instance and resource levels
- Creating new role assignments
- Deleting role assignments
- Browsing and searching for users, groups, and service principals
- Viewing assignment details and scope information
- Managing access control for individual resources

#### Management API

Programmatic access via REST API:

**Role Definition Endpoints:**
- `GET /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions` - List all role definitions

**Role Assignment Endpoints:**
- `POST /.../roleAssignments/filter` - Query assignments by scope
- `POST /.../roleAssignments/{name}` - Create assignment
- `DELETE /.../roleAssignments/{name}` - Delete assignment

**Identity Endpoints:**
- `POST /instances/{instanceId}/identity/users/retrieve` - Search users
- `POST /instances/{instanceId}/identity/groups/retrieve` - Search groups
- `POST /instances/{instanceId}/identity/objects/retrievebyids` - Get objects by ID

**Agent-Specific Endpoints:**
- `POST /instances/{instanceId}/providers/FoundationaLLM.Agent/agents/{agentName}/externalRoleAssignments` - Bulk manage agent role assignments

#### API Authentication

All API calls require:
- HTTPS
- Bearer token with `Data.Manage` scope
- User Access Administrator or Owner role

### 7. Advanced Features

#### Bulk Operations

The system supports bulk role assignment operations:
- Add multiple users/groups to a role in a single operation
- Remove multiple users/groups from a role in a single operation
- Combine add and remove operations in a single API call
- Support for expiration dates on assignments

#### External Role Assignments

Specialized endpoint for managing agent access:
- Accepts user principal names (UPNs) directly
- Supports bulk add/remove operations
- Optional expiration dates for temporary access
- Simplified identity management

#### Identity Integration

- **Microsoft Entra ID Integration**: Full integration with Entra ID for user and group management
- **Managed Identities**: Support for Azure managed identities for service-to-service authentication
- **Service Principals**: Support for app registrations and service principals
- **Group-Based Access**: Assign roles to security groups for easier management

#### Auditing

All role assignment changes are audited:
- Creation timestamp
- Modification history
- User who made changes
- Previous values
- Audit logs accessible via Management Portal and API

### 8. Data Storage

Role assignments are stored in a dedicated data store:
- Isolated from main platform data
- Uses Azure Cosmos DB or Data Lake Storage Gen2
- Enables independent scaling
- Supports compliance auditing

### 9. Security Features

#### Authentication

- Microsoft Entra ID for user authentication
- Service authentication via Managed Identities
- API authorization with bearer tokens
- Group-based access control

#### Authorization Evaluation

- Real-time permission evaluation
- Hierarchical scope evaluation
- Wildcard action matching
- Control plane and data plane separation

#### Best Practices

| Practice | Description |
|----------|-------------|
| **Use Groups** | Assign to groups, not individuals |
| **Least Privilege** | Grant minimum required access |
| **Document** | Add descriptions to assignments |
| **Review Regularly** | Audit assignments periodically |
| **Scope Appropriately** | Use resource scope when possible |
| **Start Narrow** | Grant at resource level when possible |
| **Use Instance Sparingly** | Only for true platform administrators |

### 10. Integration Capabilities

#### Automation Scenarios

- **HR System Integration**: Sync agent access with HR onboarding/offboarding
- **Access Review Automation**: Schedule periodic access reviews
- **Bulk User Provisioning**: Programmatically manage access for multiple users
- **Temporary Access Grants**: Support for time-limited access

#### PowerShell Support

- PowerShell scripts for role assignment management
- Azure CLI integration for token management
- Automation-friendly API design

## Use Cases

### Platform Administration
- **Owner** role at instance level for full platform management
- **User Access Administrator** for delegated access management

### Resource Management
- **Contributor** role for creating and managing resources
- **Reader** role for auditors and compliance reviewers

### Specialized Access
- **Agents Contributor** for agent creation
- **Conversations Contributor** for chat user portal users
- **Data Pipelines Contributor** for data engineers
- **Attachments Contributor** for file uploads

### Resource-Specific Access
- **Reader** role at agent scope for specific agent access
- **Contributor** role at resource scope for resource-specific management

## Related Documentation

- [Role-Based Access Control Overview](role-based-access-control/index.md)
- [Role Definitions](role-based-access-control/role-definitions.md)
- [Role Assignments](role-based-access-control/role-assignments.md)
- [Scope](role-based-access-control/scope.md)
- [Role Management](role-based-access-control/role-management.md)
- [Agent Role Assignments](role-based-access-control/agent-role-assignments.md)
- [Permissions & Roles Reference](../../management-portal/reference/permissions-roles.md)
- [Platform Security](platform-security.md)
- [Authentication & Authorization](authentication-authorization/index.md)
