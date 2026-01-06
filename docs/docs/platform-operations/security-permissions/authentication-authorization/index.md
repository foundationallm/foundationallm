# Authentication & Authorization Setup

FoundationaLLM uses Microsoft Entra ID for user authentication and authorization across all platform components.

## Overview

| Component | Authentication App | Authorization |
|-----------|-------------------|---------------|
| Chat Portal | FoundationaLLM-User-Portal | Core API access |
| Core API | FoundationaLLM-Core-API | User operations |
| Management Portal | FoundationaLLM-Management-Portal | Management API access |
| Management API | FoundationaLLM-Management-API | Admin operations |
| Authorization API | FoundationaLLM-Authorization-API | RBAC management |

## Required App Registrations

You must create **6 app registrations** in Microsoft Entra ID:

| App Registration | Purpose |
|------------------|---------|
| FoundationaLLM-User-Portal | Chat portal authentication |
| FoundationaLLM-Core-API | Core API authentication |
| FoundationaLLM-Management-Portal | Management portal authentication |
| FoundationaLLM-Management-API | Management API authentication |
| FoundationaLLM-Authorization-API | Authorization service |
| FoundationaLLM-Reader | Read-only access (optional) |

## Setup Options

### Option 1: Automated Script (Recommended)

Run the automated script to create all app registrations:

```powershell
cd deploy/common/scripts
./Create-FllmEntraIdApps.ps1
```

The script:
- Creates all 6 app registrations
- Configures scopes and permissions
- Sets up proper token configurations

After script completion, verify in **Azure Portal** > **Microsoft Entra ID** > **App registrations**.

### Option 2: Manual Setup

Complete the following guides in order:

#### Pre-Deployment (Before running `azd up`)

1. [Core API & User Portal Setup](pre-deployment/core-authentication-setup.md)
2. [Management API & Portal Setup](pre-deployment/management-authentication-setup.md)
3. [Authorization API Setup](pre-deployment/authorization-setup.md)

#### Post-Deployment (After running `azd up`)

1. [Prerequisites](pre-requisites.md)
2. [Core API Post-Deployment](post-deployment/core-authentication-post.md)
3. [Management API Post-Deployment](post-deployment/management-authentication-post.md)
4. [Authorization Post-Deployment](post-deployment/authorization-post.md)

## Configuration Summary

### App Configuration Keys

After setup, verify these App Configuration values:

| Key | Value |
|-----|-------|
| `FoundationaLLM:Chat:Entra:ClientId` | User Portal client ID |
| `FoundationaLLM:Chat:Entra:TenantId` | Your tenant ID |
| `FoundationaLLM:Chat:Entra:Scopes` | `api://FoundationaLLM-Core/Data.Read` |
| `FoundationaLLM:CoreAPI:Entra:ClientId` | Core API client ID |
| `FoundationaLLM:CoreAPI:Entra:TenantId` | Your tenant ID |
| `FoundationaLLM:Management:Entra:ClientId` | Management Portal client ID |
| `FoundationaLLM:Management:Entra:Scopes` | `api://FoundationaLLM-Management/Data.Manage` |
| `FoundationaLLM:ManagementAPI:Entra:ClientId` | Management API client ID |

### Required Permissions

| Role | Scope | Purpose |
|------|-------|---------|
| Cloud Application Administrator | Entra ID | Create app registrations |
| Global Administrator OR Privileged Role Administrator | Entra ID | Assign MS Graph permissions |
| Contributor | Azure Subscription | Access App Configuration |

## Post-Deployment Scripts

### Configure MS Graph Permissions

After deployment, run:

```powershell
cd deploy/quick-start  # or deploy/standard
../common/scripts/Set-FllmGraphRoles.ps1 -resourceGroupName <resource-group>
```

This grants managed identities the required MS Graph permissions.

### Update OAuth Callback URIs

Update redirect URIs with deployment URLs:

```powershell
../common/scripts/Update-OAuthCallbackUris.ps1
```

## Verifying Setup

### Test Authentication

1. Navigate to Chat Portal URL
2. Sign in with Entra ID account
3. Verify successful login

### Troubleshoot Authentication Issues

| Issue | Solution |
|-------|----------|
| Redirect loop | Check redirect URIs in app registration |
| Invalid token | Verify client IDs in App Configuration |
| Access denied | Check API permissions and scopes |
| 401 Unauthorized | Verify tenant ID configuration |

See [Troubleshooting](../../monitoring-troubleshooting/troubleshooting.md) for detailed diagnostics.

## Security Considerations

| Practice | Recommendation |
|----------|----------------|
| **Token Lifetime** | Use default settings |
| **Conditional Access** | Configure based on security requirements |
| **MFA** | Enable for all users |
| **Secret Rotation** | Rotate client secrets before expiration |

## Related Topics

- [Role-Based Access Control](../role-based-access-control/index.md)
- [Platform Security](../platform-security.md)
- [App Configuration Values](../../deployment/app-configuration-values.md)
