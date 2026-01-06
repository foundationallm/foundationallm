# Authorization API Post-Deployment Configuration

Complete these steps after running `azd up` to finalize Authorization API configuration.

## Prerequisites

- Deployment completed successfully
- App Configuration access configured (see [Prerequisites](../pre-requisites.md))
- Authorization API app registration created (see [Pre-Deployment Setup](../pre-deployment/authorization-setup.md))

## Update App Configuration Settings

### Step 1: Access App Configuration

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Navigate to your deployment resource group
3. Select the **App Configuration** resource
4. Select **Configuration explorer**

### Step 2: Update Authorization Scope

1. Search for `authorization` in the filter
2. Find `FoundationaLLM:APIs:AuthorizationAPI:APIScope`
3. Click **Edit**
4. Set value to: `api://FoundationaLLM-Authorization`
5. Click **Apply**

## Verify Configuration

### Check App Configuration Values

Verify these authorization-related settings:

| Key | Expected Value |
|-----|----------------|
| `FoundationaLLM:APIs:AuthorizationAPI:APIScope` | `api://FoundationaLLM-Authorization` |
| `FoundationaLLM:APIs:AuthorizationAPI:APIUrl` | Your Authorization API URL |

## Configure Initial Role Assignments

After authorization is configured, set up initial RBAC:

### Assign Admin Role

The deployment administrator needs the Owner role:

1. Navigate to Management Portal
2. Go to **Security** > **Role Assignments**
3. Create assignment:
   - **Principal:** Your admin user/group
   - **Role:** Owner
   - **Scope:** `/instances/{instanceId}`

Or via Management API:

```http
POST /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments
Content-Type: application/json
Authorization: Bearer <token>

{
  "name": "admin-assignment",
  "principal_id": "<admin-object-id>",
  "principal_type": "User",
  "role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
  "scope": "/instances/{instanceId}"
}
```

### Verify Authorization

Test that authorization is working:

1. Sign in to Management Portal
2. Navigate to any resource
3. Verify you can perform expected operations

## Run MS Graph Roles Script

The Authorization API requires MS Graph permissions for user lookups:

```powershell
cd deploy/quick-start  # or deploy/standard
../common/scripts/Set-FllmGraphRoles.ps1 -resourceGroupName <resource-group>
```

> **Requirement:** User running script must be **Global Administrator** or have **Privileged Role Administrator** role.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Authorization denied | Verify role assignments exist |
| User lookup fails | Run MS Graph roles script |
| Invalid scope | Check `APIScope` value in App Configuration |
| 403 Forbidden | Verify user has appropriate role for action |

### Check Authorization API Logs

**AKS:**
```bash
kubectl logs deployment/authorization-api -n fllm --tail=100
```

### Verify Role Assignments

Query existing assignments:

```http
GET /instances/{instanceId}/providers/FoundationaLLM.Authorization/roleAssignments
Authorization: Bearer <token>
```

## Next Steps

1. Review [Role Definitions](../../role-based-access-control/role-definitions.md)
2. Configure [Role Assignments](../../role-based-access-control/role-assignments.md)
3. Set up [Agent-Level Permissions](../../role-based-access-control/agent-role-assignments.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [Role-Based Access Control](../../role-based-access-control/index.md)
- [Permissions Reference](../../../../management-portal/reference/permissions-roles.md)
