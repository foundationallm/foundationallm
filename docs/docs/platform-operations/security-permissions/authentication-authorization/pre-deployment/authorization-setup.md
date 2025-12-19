# Authorization API Setup

This guide covers creating the Microsoft Entra ID app registration for the Authorization API service.

## Overview

The Authorization API handles role-based access control (RBAC) for FoundationaLLM resources.

| App Registration | Purpose | Type |
|------------------|---------|------|
| FoundationaLLM-Authorization-API | Authorization service authentication | API |

## Create the API Application

### Step 1: Register the Application

1. Sign in to [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Navigate to **Identity** > **Applications** > **App registrations**
3. Click **+ New registration**
4. Configure:
   - **Name:** `FoundationaLLM-Authorization-API`
   - **Supported account types:** Accounts in this organizational directory only
5. Click **Register**
6. **Record** the **Application (client) ID** and **Directory (tenant) ID**

### Step 2: Expose an API

1. Under **Manage**, select **Expose an API**
2. Click **Add a scope**
3. Set Application ID URI: `api://FoundationaLLM-Authorization`
4. Click **Save and continue**
5. Configure scope:

   | Field | Value |
   |-------|-------|
   | Scope name | `Authorization.Manage` |
   | Who can consent? | Admins and users |
   | Admin consent display name | Manage Authorization |
   | Admin consent description | Allows the app to manage data on behalf of the signed-in user |
   | User consent display name | Manage data on behalf of the user |
   | User consent description | Allows the app to manage data on behalf of the signed-in user |
   | State | Enabled |

6. Click **Add scope**
7. **Record** the scope name: `api://FoundationaLLM-Authorization/Authorization.Manage`

### Step 3: Update Manifest

1. Under **Manage**, select **Manifest**
2. Find `accessTokenAcceptedVersion`
3. Change value to `2`
4. Click **Save**

### Step 4: Configure Authentication

1. Under **Manage**, select **Authentication**
2. Click **Add a platform** > **Web**
3. Enter Redirect URI: `http://localhost`
4. Under **Implicit grant and hybrid flows**:
   - Check **Access tokens**
   - Check **ID tokens**
5. Click **Configure**

## Client Secret (For Standard Deployment)

Standard deployments require a client secret:

### Step 1: Create Secret

1. Under **Manage**, select **Certificates & secrets**
2. Click **+ New client secret**
3. Configure:
   - **Description:** `FoundationaLLM-Authorization`
   - **Expires:** Select appropriate duration
4. Click **Add**
5. **Record** the secret **Value** immediately (it won't be shown again)

### Step 2: Store in Deployment Manifest

For Standard deployments, add to `Deployment-Manifest.json`:

```json
{
  "entraClientSecrets": {
    "authorization": "<secret-value>"
  }
}
```

## Values to Record

Save these values for configuration:

| Value | Configuration Location |
|-------|----------------------|
| Application (client) ID | Deployment Manifest: `entraClientIds.authorization` |
| Directory (tenant) ID | Used across all configurations |
| Scope | `api://FoundationaLLM-Authorization/Authorization.Manage` |
| Client Secret | Deployment Manifest: `entraClientSecrets.authorization` |

## App Configuration Keys

After deployment, verify these values:

| Key | Expected Value |
|-----|----------------|
| `FoundationaLLM:APIs:AuthorizationAPI:APIScope` | `api://FoundationaLLM-Authorization` |

## Next Steps

1. Run deployment (`azd up`)
2. Complete [Post-Deployment Configuration](../post-deployment/authorization-post.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [Core API & Portal Setup](core-authentication-setup.md)
- [Management API & Portal Setup](management-authentication-setup.md)
- [Role-Based Access Control](../../role-based-access-control/index.md)
