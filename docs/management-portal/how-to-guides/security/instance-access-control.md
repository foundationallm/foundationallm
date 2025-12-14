# Instance Access Control

Learn how to manage access control for your FoundationaLLM instance.

## Overview

Instance Access Control allows you to manage who can access your FoundationaLLM deployment and what they can do.

## Accessing Access Control

1. Navigate to **Security** in the sidebar
2. Click **Instance Access Control**

## Role-Based Access Control

FoundationaLLM uses RBAC to manage permissions:
- Assign roles to users and groups
- Define scope of access
- Manage agent-specific permissions

## Key Concepts

### Principals
- Users (Entra ID users)
- Groups (Security groups)
- Service Principals

### Roles
- Owner: Full access
- Contributor: Create and manage resources
- Reader: View-only access

### Scope
- Instance level
- Resource provider level
- Individual resource level

## Managing Assignments

### Add Role Assignment
1. Click **Add Role Assignment**
2. Select the scope
3. Choose the principal type
4. Enter the principal ID
5. Select the role
6. Save

### Remove Role Assignment
1. Find the assignment in the list
2. Click Delete
3. Confirm removal

## Related Topics

- [Role-Based Access Control](../../../platform-operations/security-permissions/role-based-access-control/index.md)
- [Agent Role Assignments](../../../platform-operations/security-permissions/role-based-access-control/agent-role-assignments.md)
