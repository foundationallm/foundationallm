# Configuration

Learn how to manage platform configuration and portal access settings.

## Overview

The Configuration page allows administrators to manage portal access permissions and instance-level settings for the FoundationaLLM deployment.

## Accessing Configuration

1. In the Management Portal sidebar, click **Configuration** under the **FLLM Platform** section
2. The configuration page loads with access control settings

## Portal Access Configuration

The Configuration page provides access control for both portals:

### User Portal Access

Control who can access the Chat User Portal:

| Setting | Description |
|---------|-------------|
| **Scope** | `providers/FoundationaLLM.Configuration/appConfigurationSets/UserPortal` |
| **Role** | Reader (required for portal access) |

### Management Portal Access

Control who can access the Management Portal:

| Setting | Description |
|---------|-------------|
| **Scope** | `providers/FoundationaLLM.Configuration/appConfigurationSets/ManagementPortal` |
| **Role** | Reader (required for portal access) |

## Managing Portal Access

### Granting Access

1. Click **Access Control** for the desired portal section
2. In the dialog, click **Add Role Assignment**
3. Configure the assignment:
   - **Principal Type**: User, Group, or Service Principal
   - **Principal ID/Email**: The Azure AD identifier
   - **Role**: Reader (for portal access)
4. Click **Save**

### Revoking Access

1. Click **Access Control** for the portal section
2. Find the role assignment in the list
3. Click the delete icon
4. Confirm removal

## Role Requirements

| Portal | Required Role | Result |
|--------|---------------|--------|
| **User Portal** | Reader on UserPortal config | Can access Chat User Portal |
| **Management Portal** | Reader on ManagementPortal config | Can access Management Portal |

> **Note:** Users without at least Reader role on the appropriate configuration set will not be able to access the portal.

## Bulk Access Management

For managing access for multiple users:

1. Create an Azure AD security group
2. Add users to the group
3. Grant the Reader role to the group
4. All group members gain portal access

## Access Patterns

### Self-Service User Portal

Grant Reader role to `All Users` or a broad group for self-service agent access.

### Restricted Management Portal

Limit Management Portal access to:
- IT Administrators
- Platform operators
- Development teams

### Department-Level Access

Create groups per department:
- `FLLM-Users-HR`
- `FLLM-Users-Finance`
- `FLLM-Admins`

## Troubleshooting

### User Cannot Access Portal

1. Verify the user's email/principal ID
2. Check role assignments in Access Control
3. Ensure the user is in an assigned group
4. Verify Azure AD sync is current

### Access Control Changes Not Applying

1. User may need to sign out and sign back in
2. Token cache may need to clear (wait 5-10 minutes)
3. Check for conflicting role assignments

### Groups Not Appearing

1. Verify the group exists in Azure AD
2. Check that group sync is enabled
3. Enter the group's Object ID directly

## Related Topics

- [Instance Access Control](../security/instance-access-control.md)
- [Permissions and Roles Reference](../../reference/permissions-roles.md)
- [Deployment Information](deployment-information.md)
