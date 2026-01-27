# Role-Based Access Control

FoundationaLLM RBAC provides fine-grained access control to platform resources.

## Overview

FoundationaLLM RBAC enables:
- Controlling who can access resources
- Defining what actions users can perform
- Scoping access to specific resources or the entire instance
- Auditing access changes

## Key Concepts

| Concept | Description |
|---------|-------------|
| **Role Definition** | A collection of permissions (actions) |
| **Role Assignment** | Grants a role to a principal at a scope |
| **Principal** | User, group, service principal, or managed identity |
| **Scope** | Where the access applies (instance or resource) |

## Built-in Roles

| Role | Description |
|------|-------------|
| **Owner** | Full access including role assignment management |
| **Contributor** | Full access except role assignment management |
| **Reader** | Read-only access to resources |
| **User Access Administrator** | Manage role assignments only |

See [Role Definitions](role-definitions.md) for complete details.

## How RBAC Works

```mermaid
graph LR
    A[Principal] -->|assigned| B[Role]
    B -->|defines| C[Permissions]
    C -->|applied at| D[Scope]
    D -->|grants access to| E[Resources]
```

1. **Principal** requests access
2. System checks **role assignments**
3. **Permissions** are evaluated at the **scope**
4. Access is granted or denied

## Management Options

### Management Portal

The Management Portal provides a UI for:
- Viewing role assignments
- Creating new assignments
- Deleting assignments
- Managing access at instance and resource levels

### Management API

Programmatic access via REST API:
- List role definitions
- Create/delete role assignments
- Query assignments by scope
- Retrieve identity information

See [Role Management](role-management.md) for API details.

## Quick Start

### Grant Instance Access

1. Navigate to Management Portal
2. Select **Security** > **Instance Access Control**
3. Click **Add Role Assignment**
4. Select principal (user/group)
5. Select role
6. Click **Save**

### Grant Resource Access

1. Navigate to the resource (e.g., Agent)
2. Click **Access Control** button
3. Click **Add Role Assignment**
4. Select principal and role
5. Click **Save**

## Inheritance

| Scope | Inherits From |
|-------|---------------|
| Resource | Instance |
| Instance | None (top level) |

- Roles assigned at instance level apply to all resources
- Resource-level assignments add specific permissions
- Cannot remove inherited permissions at lower levels

## Documentation

| Topic | Description |
|-------|-------------|
| [RBAC & Permissioning Capabilities](../rbac-permissioning-capabilities.md) | Comprehensive summary of RBAC and permissioning system capabilities |
| [Role Definitions](role-definitions.md) | Understanding role structure and permissions |
| [Role Assignments](role-assignments.md) | How assignments work |
| [Scope](scope.md) | Understanding scope levels |
| [Role Management](role-management.md) | Managing assignments via Portal and API |
| [Agent Role Assignments](agent-role-assignments.md) | Automating agent access control |

## Related Topics

- [Authentication Setup](../authentication-authorization/index.md)
- [Platform Security](../platform-security.md)
- [Permissions Reference](../../../management-portal/reference/permissions-roles.md)
