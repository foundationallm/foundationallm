# Management API & Portal Post-Deployment Configuration

Complete these steps after running `azd up` to finalize Management API and Portal authentication.

## Prerequisites

- Deployment completed successfully
- App Configuration access configured (see [Prerequisites](../pre-requisites.md))
- App registrations created (see [Pre-Deployment Setup](../pre-deployment/management-authentication-setup.md))
- Core API configuration completed (see [Core API Post-Deployment](core-authentication-post.md))

## Update App Configuration Settings

### Step 1: Access App Configuration

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Navigate to your deployment resource group
3. Select the **App Configuration** resource
4. Select **Configuration explorer**

### Step 2: Filter Settings

1. Enter `entra` in the search box
2. Select all management-related settings

### Step 3: Update Values

Update the following settings with values from your app registrations:

| Key | Value |
|-----|-------|
| `FoundationaLLM:Management:Entra:ClientId` | Application (client) ID of `FoundationaLLM-Management-Portal` |
| `FoundationaLLM:Management:Entra:TenantId` | Directory (tenant) ID |
| `FoundationaLLM:Management:Entra:Scopes` | `api://FoundationaLLM-Management/Data.Manage` |
| `FoundationaLLM:ManagementAPI:Entra:ClientId` | Application (client) ID of `FoundationaLLM-Management-API` |
| `FoundationaLLM:ManagementAPI:Entra:TenantId` | Directory (tenant) ID |

### Step 4: Verify Default Values

Confirm these values are correct:

| Key | Expected Value |
|-----|----------------|
| `FoundationaLLM:Management:Entra:CallbackPath` | `/signin-oidc` |
| `FoundationaLLM:Management:Entra:Instance` | `https://login.microsoftonline.com/` |
| `FoundationaLLM:ManagementAPI:Entra:Instance` | `https://login.microsoftonline.com/` |
| `FoundationaLLM:ManagementAPI:Entra:Scopes` | `Data.Manage` |

### Step 5: Save Changes

Click **Apply** to save all configuration changes.

## Restart Services

After updating configuration, restart the services to apply changes.

### Azure Container Apps (Quick Start)

1. Navigate to your resource group
2. Select the **Management API** Container App (ends with `managementapica`)
3. Select **Revisions** in left menu
4. Select the active revision
5. Click **Restart** in the Revision details panel
6. Repeat for **Management UI** Container App (ends with `managementuica`)

### Azure Kubernetes Service (Standard)

**Via kubectl:**
```bash
kubectl rollout restart deployment/management-api -n fllm
kubectl rollout restart deployment/management-ui -n fllm
```

## Verify Authentication

### Test Sign-In

1. Navigate to your Management Portal URL
2. You should be redirected to Microsoft sign-in
3. Enter your Entra ID credentials
4. Verify successful authentication
5. Confirm you can access the Management Portal dashboard

### Troubleshoot Issues

| Issue | Solution |
|-------|----------|
| Redirect loop | Verify redirect URI matches deployment URL |
| Invalid client | Check `FoundationaLLM:Management:Entra:ClientId` value |
| Access denied | Verify user has appropriate RBAC roles |
| Invalid scope | Verify scope: `api://FoundationaLLM-Management/Data.Manage` |

### Check Logs

**AKS:**
```bash
kubectl logs deployment/management-api -n fllm --tail=100
kubectl logs deployment/management-ui -n fllm --tail=100
```

## Next Steps

1. Complete [Authorization Post-Deployment](authorization-post.md)
2. Configure [Role-Based Access Control](../../role-based-access-control/index.md)
3. Test [Management Portal Features](../../../../management-portal/index.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [Core API Post-Deployment](core-authentication-post.md)
- [App Configuration Values](../../../deployment/app-configuration-values.md)
