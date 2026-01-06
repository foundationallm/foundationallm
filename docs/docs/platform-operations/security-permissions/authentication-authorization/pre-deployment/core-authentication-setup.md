# Core API & User Portal Authentication Setup

This guide covers creating the Microsoft Entra ID app registrations for the Core API and Chat (User) Portal.

## Overview

You will create two app registrations:

| App Registration | Purpose | Type |
|------------------|---------|------|
| FoundationaLLM-User-Portal | Chat portal authentication | Client (SPA) |
| FoundationaLLM-Core-API | Core API authentication | API |

## Create the Client Application (User Portal)

### Step 1: Register the Application

1. Sign in to [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Navigate to **Identity** > **Applications** > **App registrations**
3. Click **+ New registration**
4. Configure:
   - **Name:** `FoundationaLLM-User-Portal`
   - **Supported account types:** Accounts in this organizational directory only
5. Click **Register**
6. **Record** the **Application (client) ID** and **Directory (tenant) ID**

### Step 2: Configure Authentication

1. Under **Manage**, select **Authentication**
2. Click **Add a platform** > **Single-page application**
3. Add Redirect URIs:

   | Environment | URI |
   |-------------|-----|
   | Production | `<CHAT_PORTAL_URL>/signin-oidc` |
   | Local Dev | `http://localhost:3000/signin-oidc` |

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

## Create the API Application (Core API)

### Step 1: Register the Application

1. Navigate to **App registrations** > **+ New registration**
2. Configure:
   - **Name:** `FoundationaLLM-Core-API`
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
3. Set Application ID URI: `api://FoundationaLLM-Core`
4. Click **Save and continue**
5. Configure scope:

   | Field | Value |
   |-------|-------|
   | Scope name | `Data.Read` |
   | Who can consent? | Admins and users |
   | Admin consent display name | Read data on behalf of users |
   | Admin consent description | Allows the app to read data on behalf of the signed-in user |
   | User consent display name | Read data on behalf of the user |
   | User consent description | Allows the app to read data on behalf of the signed-in user |
   | State | Enabled |

6. Click **Add scope**
7. **Record** the scope name: `api://FoundationaLLM-Core/Data.Read`

### Step 5: Add Authorized Client Application

1. Still in **Expose an API**, click **+ Add a client application**
2. Paste the **Application (client) ID** of `FoundationaLLM-User-Portal`
3. Check the `Data.Read` scope
4. Click **Add application**

### Step 6: Update Manifest

1. Under **Manage**, select **Manifest**
2. Find `accessTokenAcceptedVersion`
3. Change value to `2`
4. Click **Save**

## Configure API Permissions (Client App)

### Add Permissions to User Portal

1. Navigate to **App registrations**
2. Select `FoundationaLLM-User-Portal`
3. Under **Manage**, select **API permissions**
4. Click **+ Add a permission**
5. Select **My APIs** tab
6. Select `FoundationaLLM-Core-API`
7. Check `Data.Read`
8. Click **Add permissions**

### Grant Admin Consent (Optional)

If required by your organization:

1. Click **Grant admin consent for [tenant]**
2. Confirm

## Values to Record

Save these values for App Configuration:

| Value | App Configuration Key |
|-------|----------------------|
| User Portal Client ID | `FoundationaLLM:Chat:Entra:ClientId` |
| Core API Client ID | `FoundationaLLM:CoreAPI:Entra:ClientId` |
| Tenant ID | `FoundationaLLM:Chat:Entra:TenantId`, `FoundationaLLM:CoreAPI:Entra:TenantId` |
| Scope | `FoundationaLLM:Chat:Entra:Scopes` = `api://FoundationaLLM-Core/Data.Read` |

## Next Steps

1. Complete [Management API & Portal Setup](management-authentication-setup.md)
2. Complete [Authorization API Setup](authorization-setup.md)
3. Run deployment (`azd up`)
4. Complete [Post-Deployment Configuration](../post-deployment/core-authentication-post.md)

## Related Topics

- [Authentication Setup Overview](../index.md)
- [App Configuration Values](../../../deployment/app-configuration-values.md)
