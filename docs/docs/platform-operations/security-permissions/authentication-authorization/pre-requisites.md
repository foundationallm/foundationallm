# Post-Deployment Prerequisites

Complete these prerequisites before configuring authentication settings after deployment.

## Overview

After running `azd up` or `azd deploy`, you need to:
1. Access App Configuration
2. Obtain application URLs
3. Update app registrations with deployment-specific values

## Setup App Configuration Access

### Step 1: Navigate to Resources

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Navigate to your deployment resource group

> **Note:** For ACA deployments, you'll see an additional `ME_*` resource group. For AKS, you'll see `MC_*`. Access the main resource group (without prefix) for App Configuration.

### Step 2: Verify Access

1. Select the **App Configuration** resource
2. Select **Configuration explorer**
3. Attempt to view values

### Step 3: Add Permissions (if needed)

If you cannot access configurations:

1. Select **Access control (IAM)** in App Configuration
2. Click **+ Add** > **Add role assignment**
3. Select role: **App Configuration Data Owner**
4. Assign to your user account
5. Click **Review + assign**

## Obtain Application URLs

### Azure Container Apps (Quick Start)

**Chat Portal URL:**
1. Navigate to resource group
2. Select Container App ending with `chatuica`
3. Copy **Application Url** from Overview

**Management Portal URL:**
1. Select Container App ending with `managementuica`
2. Copy **Application Url** from Overview

### Azure Kubernetes Service (Standard)

**Chat Portal URL:**
1. Navigate to resource group
2. Select the **Kubernetes service** resource
3. Select **Properties**
4. Note the **HTTP application routing domain**
5. Chat URL: `https://<domain>/`

**Management Portal URL:**
- Management URL: `https://<domain>/management/`

**Or from hosts file:**
Check `deploy/standard/config/hosts.ingress` generated during deployment.

## URLs for Redirect URIs

Record these URLs for app registration updates:

| Application | URL Pattern (ACA) | URL Pattern (AKS) |
|-------------|-------------------|-------------------|
| Chat Portal | `https://<name>chatuica.<region>.azurecontainerapps.io` | `https://chat.<domain>` |
| Management Portal | `https://<name>managementuica.<region>.azurecontainerapps.io` | `https://management.<domain>` |

## Update Redirect URIs

After obtaining URLs, update app registrations:

### Chat Portal (FoundationaLLM-User-Portal)

1. Navigate to **Microsoft Entra ID** > **App registrations**
2. Select **FoundationaLLM-User-Portal**
3. Select **Authentication**
4. Under **Single-page application**, add:
   ```
   <CHAT_PORTAL_URL>/signin-oidc
   ```
5. Click **Save**

### Management Portal (FoundationaLLM-Management-Portal)

1. Select **FoundationaLLM-Management-Portal**
2. Select **Authentication**
3. Under **Single-page application**, add:
   ```
   <MANAGEMENT_PORTAL_URL>/signin-oidc
   ```
4. Click **Save**

## Automation Script

For Quick Start deployments, use the provided script:

```powershell
cd deploy/quick-start
../common/scripts/Update-OAuthCallbackUris.ps1
```

This automatically updates redirect URIs based on deployed resources.

## Next Steps

After completing prerequisites:

1. [Complete Core API Post-Deployment](post-deployment/core-authentication-post.md)
2. [Complete Management API Post-Deployment](post-deployment/management-authentication-post.md)
3. [Complete Authorization Post-Deployment](post-deployment/authorization-post.md)

## Related Topics

- [Authentication Setup Overview](index.md)
- [App Configuration Values](../../deployment/app-configuration-values.md)
- [Troubleshooting](../../monitoring-troubleshooting/troubleshooting.md)
