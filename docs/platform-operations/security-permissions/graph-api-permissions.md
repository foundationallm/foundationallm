# Microsoft Graph API Permissions

FoundationaLLM requires Microsoft Graph API permissions for its role-based access control (RBAC) implementation.

## Overview

Graph API permissions enable:
- User and group enumeration
- Security principal lookups
- Group membership resolution
- Display name mapping

## Why Graph API Permissions Are Required

### RBAC Implementation

A fully functional RBAC system requires:

| Capability | Graph API Requirement |
|------------|----------------------|
| Resolve group membership | `Group.Read.All` |
| Display user names | `User.Read.All` |
| Display service principals | `Application.Read.All` |
| Assign roles to groups | `Group.Read.All` |

### Enterprise Scenarios

Standard approaches (like group claims in tokens) have limitations:
- Token size limits restrict group count
- Large organizations exceed claim limits
- Graph API reference is needed anyway

FoundationaLLM directly calls Graph API for robust handling of enterprise scenarios.

## Required Permissions

| Permission | Type | Purpose |
|------------|------|---------|
| `Group.Read.All` | Application | Read all groups, resolve memberships |
| `User.Read.All` | Application | Read all users, display names |
| `Application.Read.All` | Application | Read service principals |

### Permission Justification

**`Group.Read.All`:**
- Get group membership for authenticated users
- Evaluate role membership
- List groups for role assignment

**`User.Read.All`:**
- Retrieve user display names
- List users for role assignment
- Resolve user object IDs

**`Application.Read.All`:**
- Read service principal information
- Support for non-user principals in groups
- Required when groups contain service principals

## Assigned Services

Permissions are granted to managed identities for:

| Service | Purpose |
|---------|---------|
| **Core API** | User authentication, RBAC evaluation |
| **Management API** | RBAC management, user/group lookup |

## Granting Permissions

### Using the Deployment Script

After deployment, run:

```powershell
cd deploy/quick-start  # or deploy/standard
../common/scripts/Set-FllmGraphRoles.ps1 -resourceGroupName <resource-group>
```

### Requirements

| Requirement | Description |
|-------------|-------------|
| **Role** | Global Administrator OR Privileged Role Administrator |
| **Scope** | Entra ID tenant |

### Manual Assignment

If the script cannot be used:

1. Navigate to **Azure Portal** > **Microsoft Entra ID**
2. Select **Enterprise applications**
3. Find the Core API managed identity
4. Select **Permissions**
5. Click **Grant admin consent**
6. Repeat for Management API

## Transparency & Auditing

### Source Code Review

All Graph API interactions are implemented in:
```
src/dotnet/Common/Services/Security/MicrosoftGraphIdentityManagementService.cs
```

The code:
- Performs only the operations described above
- Uses minimal property sets
- Is fully auditable in the public GitHub repository

### Audit Graph API Calls

Monitor Graph API usage:

```kql
AuditLogs
| where OperationName contains "Microsoft Graph"
| project TimeGenerated, OperationName, InitiatedBy, TargetResources
| order by TimeGenerated desc
```

## Security Considerations

### Principle of Least Privilege

- Only `Read` permissions are requested
- No write or modify capabilities
- Application permissions (not delegated)

### Managed Identity Benefits

| Benefit | Description |
|---------|-------------|
| No credentials | No secrets to manage |
| Auto-rotation | Azure handles key management |
| Audit trail | All access logged |
| Scoped access | Limited to assigned permissions |

### Alternative Approaches

| Approach | Limitation | Why Not Used |
|----------|------------|--------------|
| Token claims | 200 group limit | Enterprise scenarios exceed limits |
| Delegated permissions | User context only | Service-to-service needs app permissions |
| Client secrets | Credential management | Managed identities are more secure |

## Troubleshooting

### Permission Errors

| Error | Solution |
|-------|----------|
| "Insufficient privileges" | Run `Set-FllmGraphRoles.ps1` |
| "Access denied" | Verify admin consent granted |
| "Graph API error" | Check managed identity permissions |

### Verify Permissions

```powershell
# List permissions for managed identity
$servicePrincipal = Get-AzADServicePrincipal -DisplayName "<managed-identity-name>"
Get-AzADAppPermission -ObjectId $servicePrincipal.Id
```

## Related Topics

- [Platform Security](platform-security.md)
- [Role-Based Access Control](role-based-access-control/index.md)
- [Authentication Setup](authentication-authorization/index.md)
