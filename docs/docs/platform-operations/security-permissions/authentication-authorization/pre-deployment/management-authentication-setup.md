# Management API & Portal Authentication Setup

This guide covers creating the Microsoft Entra ID app registrations for the Management API and Management Portal.

## Overview

You will create two app registrations:

| App Registration | Purpose | Type |
|------------------|---------|------|
| FoundationaLLM-Management-Portal | Management portal authentication | Client (SPA) |
| FoundationaLLM-Management-API | Management API authentication | API |

## Create the Client Application (Management Portal)

### Step 1: Register the Application

1. Sign in to [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Navigate to **Identity** > **Applications** > **App registrations**
3. Click **+ New registration**
4. Configure:
   - **Name:** `FoundationaLLM-Management-Portal`
   - **Supported account types:** Accounts in this organizational directory only
5. Click **Register**
6. **Record** the **Application (client) ID** and **Directory (tenant) ID**

### Step 2: Configure Authentication

1. Under **Manage**, select **Authentication**
2. Click **Add a platform** > **Single-page application**
3. Add Redirect URIs:

   | Environment | URI |
   |-------------|-----|
   | Production | `<MANAGEMENT_PORTAL_URL>/signin-oidc` |
   | Local Dev | `http://localhost:3001/signin-oidc` |

4. Click **Configure**

### Step 3: Enable Implicit Grant

Under **Implicit grant and hybrid flows**:

1. Check **Access tokens**
2. Check **ID tokens**
3. Click **Save**

### Step 4: (Optional) Add Postman Redirect

For API testing with Postman:

1. Click **Add a platform** > **Mobile and desktop applications**
2. Add: `https://oauth.pstmn.io/v1/callback`
3. Click **Configure**

### Step 5: Update Manifest

1. Under **Manage**, select **Manifest**
2. Find `accessTokenAcceptedVersion`
3. Change value to `2`
4. Click **Save**

## Create the API Application (Management API)

### Step 1: Register the Application

1. Navigate to **App registrations** > **+ New registration**
2. Configure:
   - **Name:** `FoundationaLLM-Management-API`
   - **Supported account types:** Accounts in this organizational directory only
3. Click **Register**
4. **Record** the **Application (client) ID** and **Directory (tenant) ID**

### Step 2: Configure Authentication

1. Under **Manage**, select **Authentication**
2. Click **Add a platform** > **Web**
3. Enter Redirect URI: `http://localhost`
4. Click **Configure**

> **Note:** The localhost URI is required to enable token options but isn't used in production.

### Step 3: Enable Implicit Grant

1. Check **Access tokens**
2. Check **ID tokens**
3. Click **Save**

### Step 4: Expose an API

1. Under **Manage**, select **Expose an API**
2. Click **Add a scope**
3. Set Application ID URI: `api://FoundationaLLM-Management` (or accept default)
4. Click **Save and continue**
5. Configure scope:

   | Field | Value |
   |-------|-------|
   | Scope name | `Data.Manage` |
   | Who can consent? | Admins and users |
   | Admin consent display name | Manage data on behalf of users |
   | Admin consent description | Allows the app to manage data on behalf of the signed-in user |
   | User consent display name | Manage data on behalf of the user |
   | User consent description | Allows the app to manage data on behalf of the signed-in user |
   | State | Enabled |

6. Click **Add scope**
7. **Record** the scope name: `api://FoundationaLLM-Management/Data.Manage`

### Step 5: Add Authorized Client Application

1. Still in **Expose an API**, click **+ Add a client application**
2. Paste the **Application (client) ID** of `FoundationaLLM-Management-Portal`
3. Check the `Data.Manage` scope
4. Click **Add application**

### Step 6: Update Manifest

1. Under **Manage**, select **Manifest**
2. Find `accessTokenAcceptedVersion`
3. Change value to `2`
4. Click **Save**

## Configure API Permissions (Client App)

### Add Permissions to Management Portal

1. Navigate to **App registrations**
2. Select `FoundationaLLM-Management-Portal`
3. Under **Manage**, select **API permissions**
4. Click **+ Add a permission**
5. Select **My APIs** tab
6. Select `FoundationaLLM-Management-API`
7. Check `Data.Manage`
8. Click **Add permissions**

### Grant Admin Consent (Optional)

If required by your organization:

1. Click **Grant admin consent for [tenant]**
2. Confirm

## Values to Record

Save these values for App Configuration:

| Value | App Configuration Key |
|-------|----------------------|
| Management Portal Client ID | `FoundationaLLM:Management:Entra:ClientId` |
| Management API Client ID | `FoundationaLLM:ManagementAPI:Entra:ClientId` |
| Tenant ID | `FoundationaLLM:Management:Entra:TenantId`, `FoundationaLLM:ManagementAPI:Entra:TenantId` |
| Scope | `FoundationaLLM:Management:Entra:Scopes` = `api://FoundationaLLM-Management/Data.Manage` |

## Next Steps

1. Complete [Authorization API Setup](authorization-setup.md)
2. Run deployment (`azd up`)
3. Complete [Post-Deployment Configuration](../post-deployment/management-authentication-post.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [Core API & Portal Setup](core-authentication-setup.md)
- [App Configuration Values](../../../deployment/app-configuration-values.md)
