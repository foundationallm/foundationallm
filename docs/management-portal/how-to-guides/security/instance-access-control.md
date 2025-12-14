# Instance Access Control

Learn how to manage access control and role assignments for your FoundationaLLM instance.

## Overview

Instance Access Control allows you to manage who can access your FoundationaLLM deployment and what actions they can perform. The system uses Role-Based Access Control (RBAC) to define permissions.

## Accessing Instance Access Control

1. In the Management Portal sidebar, click **Instance Access Control** under the **Security** section
2. The role assignments page loads, showing all current assignments

## Role Assignments Table

The table displays role assignments grouped by role:

| Column | Description |
|--------|-------------|
| **Name** | Principal name and email (if available) |
| **Type** | Principal type (User, Group, Service Principal) |
| **Scope** | Resource scope for the assignment |
| **Delete** | Remove the assignment |

### Principal Type Icons

| Icon | Type |
|------|------|
| 👤 | User |
| 👥 | Group |
| ✓ | Service Principal |

### Role Groups

Assignments are grouped by role, with an expandable header showing:
- Role display name
- Role description (hover over info icon)

## Available Roles

| Role | Description |
|------|-------------|
| **Owner** | Full access to all resources |
| **Contributor** | Create and manage resources (cannot manage access) |
| **Reader** | View resources only |
| **User Access Administrator** | Manage role assignments |

## Creating a Role Assignment

1. Click **Create Role Assignment** at the top right of the page
2. Navigate to the role assignment creation form

### Role Assignment Configuration

> **TODO**: Document the specific fields in the role assignment creation form, which may include:

| Field | Description |
|-------|-------------|
| **Principal Type** | User, Group, or Service Principal |
| **Principal ID** | Azure AD Object ID or email |
| **Role** | Role to assign |
| **Scope** | Resource scope for the assignment |

### Specifying Principals

#### Users

1. Select **User** as the principal type
2. Enter the user's email address or Object ID
3. The system validates the principal exists

#### Groups

1. Select **Group** as the principal type
2. Enter the security group's Object ID
3. All group members inherit the role

#### Service Principals

1. Select **Service Principal** as the principal type
2. Enter the service principal's Object ID
3. Used for application/service access

### Selecting Scope

Scope determines which resources the role applies to:

| Scope Level | Description |
|-------------|-------------|
| **Instance** | Entire FoundationaLLM instance |
| **Resource Provider** | All resources of a type (e.g., all agents) |
| **Resource** | Specific resource (e.g., one agent) |

**Scope Format Examples:**
- Instance: `/`
- All Agents: `providers/FoundationaLLM.Agent`
- Specific Agent: `providers/FoundationaLLM.Agent/agents/my-agent`

## Deleting a Role Assignment

1. Locate the assignment in the table
2. Click the **Trash** icon (🗑️)
3. Confirm deletion in the dialog: "Are you sure you want to delete the role assignment for [principal name]?"
4. Click **Yes** to confirm

> **Warning:** Removing access immediately prevents the principal from performing actions. Ensure this is intended before confirming.

## Refreshing the List

Click the **Refresh** button (🔄) at the top of the table to reload role assignments.

## Common Access Patterns

### Administrator Access

Grant full administrative access:
- **Role**: Owner
- **Scope**: Instance level (`/`)
- **Principal**: Admin user or admin group

### Developer Access

Grant agent development access:
- **Role**: Contributor
- **Scope**: Agent resource provider
- **Principal**: Developer group

### Read-Only Access

Grant view-only access:
- **Role**: Reader
- **Scope**: Appropriate level
- **Principal**: Viewer group

### Portal Access

For portal access specifically:
- See [Configuration](../fllm-platform/configuration.md) for portal access settings

## Best Practices

### Use Groups Over Individual Users

- Create Azure AD security groups for role-based teams
- Assign roles to groups instead of individual users
- Simplifies management as team membership changes

### Apply Least Privilege

- Grant the minimum permissions needed
- Use specific scopes rather than instance-wide
- Review and remove unnecessary assignments regularly

### Document Assignments

- Maintain records of who has what access and why
- Review assignments during security audits
- Update documentation when roles change

### Regular Review

- Periodically audit role assignments
- Remove access for departed team members
- Verify service principal access is still needed

## Troubleshooting

### User Cannot Access Expected Resources

1. Verify role assignment exists
2. Check the scope includes the resource
3. Ensure the user/group is correct
4. User may need to sign out and back in

### Role Assignment Not Working

1. Verify principal ID is correct
2. Check for typos in scope
3. Ensure Azure AD sync is current
4. Review for conflicting assignments

### Cannot Create Role Assignment

1. Verify you have User Access Administrator role
2. Check you have permission on the target scope
3. Ensure the principal exists in Azure AD

## Related Topics

- [Permissions and Roles Reference](../../reference/permissions-roles.md)
- [Configuration (Portal Access)](../fllm-platform/configuration.md)
- [Agent Access Tokens](../../reference/concepts/agent-access-tokens.md)
