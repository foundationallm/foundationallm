# Core API & User Portal Post-Deployment Configuration

Complete these steps after running `azd up` to finalize Core API and Chat Portal authentication.

## Prerequisites

- Deployment completed successfully
- App Configuration access configured (see [Prerequisites](../pre-requisites.md))
- App registrations created (see [Pre-Deployment Setup](../pre-deployment/core-authentication-setup.md))

## Update App Configuration Settings

### Step 1: Access App Configuration

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Navigate to your deployment resource group
3. Select the **App Configuration** resource
4. Select **Configuration explorer**

### Step 2: Filter Settings

1. Enter `entra` in the search box
2. Check the box next to **Key** to select all Entra-related settings
3. Click **Edit** to open the JSON editor

### Step 3: Update Values

Update the following settings with values from your app registrations:

| Key | Value |
|-----|-------|
| `FoundationaLLM:Chat:Entra:ClientId` | Application (client) ID of `FoundationaLLM-User-Portal` |
| `FoundationaLLM:Chat:Entra:TenantId` | Directory (tenant) ID |
| `FoundationaLLM:Chat:Entra:Scopes` | `api://FoundationaLLM-Core/Data.Read` |
| `FoundationaLLM:CoreAPI:Entra:ClientId` | Application (client) ID of `FoundationaLLM-Core-API` |
| `FoundationaLLM:CoreAPI:Entra:TenantId` | Directory (tenant) ID |

### Step 4: Verify Default Values

Confirm these values are correct:

| Key | Expected Value |
|-----|----------------|
| `FoundationaLLM:Chat:Entra:CallbackPath` | `/signin-oidc` |
| `FoundationaLLM:Chat:Entra:Instance` | `https://login.microsoftonline.com/` |
| `FoundationaLLM:CoreAPI:Entra:CallbackPath` | `/signin-oidc` |
| `FoundationaLLM:CoreAPI:Entra:Instance` | `https://login.microsoftonline.com/` |
| `FoundationaLLM:CoreAPI:Entra:Scopes` | `Data.Read` |

### Step 5: Save Changes

Click **Apply** to save all configuration changes.

## Restart Services

After updating configuration, restart the services to apply changes.

### Azure Container Apps (Quick Start)

1. Navigate to your resource group
2. Select the **Core API** Container App (ends with `coreapica`)
3. Select **Revisions** in left menu
4. Select the active revision
5. Click **Restart** in the Revision details panel
6. Repeat for **Chat UI** Container App (ends with `chatuica`)

### Azure Kubernetes Service (Standard)

**Via Azure Portal:**
1. Navigate to the AKS resource
2. Select **Workloads** > **Pods** tab
3. Filter by namespace: `fllm`
4. Select `core-api` and `chat-ui` pods
5. Click **Delete** (new pods auto-create)

**Via kubectl:**
```bash
kubectl rollout restart deployment/core-api -n fllm
kubectl rollout restart deployment/chat-ui -n fllm
```

## Verify Authentication

### Test Sign-In

1. Navigate to your Chat Portal URL
2. You should be redirected to Microsoft sign-in
3. Enter your Entra ID credentials
4. Verify successful authentication

### Troubleshoot Issues

| Issue | Solution |
|-------|----------|
| Redirect loop | Verify redirect URI in app registration matches deployment URL |
| Invalid client | Check `FoundationaLLM:Chat:Entra:ClientId` value |
| Invalid scope | Verify scope format: `api://FoundationaLLM-Core/Data.Read` |
| AADSTS50011 | Add correct redirect URI to app registration |
| AADSTS700016 | Verify tenant ID is correct |

### Check Logs

**ACA:**
```bash
az containerapp logs show -n <app-name> -g <resource-group>
```

**AKS:**
```bash
kubectl logs deployment/core-api -n fllm --tail=100
kubectl logs deployment/chat-ui -n fllm --tail=100
```

## Next Steps

1. Complete [Management API Post-Deployment](management-authentication-post.md)
2. Complete [Authorization Post-Deployment](authorization-post.md)
3. Configure [Role-Based Access Control](../../role-based-access-control/index.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [App Configuration Values](../../../deployment/app-configuration-values.md)
- [Troubleshooting](../../../monitoring-troubleshooting/troubleshooting.md)
