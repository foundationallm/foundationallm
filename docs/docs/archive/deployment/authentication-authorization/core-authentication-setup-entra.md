# Core API and User Portal authentication setup: Microsoft Entra ID

FoundationaLLM comes with out-of-the-box support for Microsoft Entra ID authentication. This means that you can use your Microsoft Entra ID account to log in to the chat interface.

## Create the Microsoft Entra ID applications

To enable Microsoft Entra ID authentication for the Core API and user portal, you need to create two applications in the Microsoft Azure portal:

- A client application that will be used by the user portal chat interface to authenticate users.
- An API application that will be used by the Core API to authenticate users.

### Create the client application

#### Register the client application in the Microsoft Entra ID admin center

1. Sign in to the [Microsoft Entra ID admin center](https://entra.microsoft.com/) as at least a Cloud Application Administrator.
2. Browse to **Identity** > **Applications** > **App registrations**.

    ![The app registrations menu item in the left-hand menu is highlighted.](media/entra-app-registrations.png)

3. On the page that appears, select **+ New registration**.
4. When the **Register an application** page appears, enter the following name for your application *FoundationaLLM-User-Portal*. 
5. Under **Supported account types**, select *Accounts in this organizational directory only*.
6. Select **Register**.

    ![The new client app registration form is displayed.](media/entra-register-client-app.png)

7. The application's **Overview** pane displays upon successful registration. Record the **Application (client) ID** and **Directory (tenant) ID** to add to your App Configuration settings later.

    ![The Entra app client ID and Directory ID values are highlighted in the Overview blade.](media/entra-client-app-overview.png)

#### Add a redirect URI to the client application

1. Under **Manage**, select **Authentication**.
2. Under **Platform configurations**, select **Add a platform**. In the pane that opens, select **Single-page application**. This is for the Vue.js chat application.
3. Add a **Redirect URI** under Single-page application for your deployed Vue.js application. Enter `<YOUR_CHAT_APP_URL>/signin-oidc`, replacing `<YOUR_CHAT_APP_URL>` with the chat UI application URL obtained in the [Pre-requisites](#pre-requisites) section above. For example, it should look something like `https://d85a09ce067141d5807a.eastus.aksapp.io/signin-oidc` for an AKS deployment, or `https://fllmaca002chatuica.graybush-c554b849.eastus.azurecontainerapps.io/signin-oidc` for an ACA deployment.
4. Add a **Redirect URI** under Single-page application for local development of the Vue.js application: `http://localhost:3000/signin-oidc`.

    ![The Authentication left-hand menu item and redirect URIs are highlighted.](media/entra-app-client-authentication-uris.png)

<!-- 8. Under **Front-channel logout URL**, enter `<YOUR_CHAT_APP_URL>/signout-oidc`. -->

If you wish to [configure authentication in Postman](../../development/directly-calling-apis.md#postman-collection) for executing calls against the Core API, you will need to add a **Redirect URI** under **Mobile and desktop applications** for Postman. Enter `https://oauth.pstmn.io/v1/callback` for the URI. To do this, complete the following steps:

1. Under **Platform configurations**, select **Add a platform**. In the pane that opens, select **Mobile and desktop applications**.
2. Enter `https://oauth.pstmn.io/v1/callback` for the **Custom redirect URIs** value.

    ![The Authentication left-hand menu item and redirect URIs are highlighted.](media/entra-app-client-authentication-uris-postman.png)

3. Select **Configure** to apply the changes.

#### Implicit grant and hybrid flows for the client application

1. Check **Access tokens** and **ID tokens** under **Implicit grant**.
2. Select **Configure** to apply the changes (if the button is present).
3. Select **Save** at the bottom of the page to save the changes.

    ![Both the Access tokens and ID tokens checkboxes are checked and the Save button is highlighted.](media/entra-app-client-authentication-implicit-grant.png)

<!-- #### Client secret for the client application

1. Under **Manage**, select **Certificates & secrets**.
2. Under **Client secrets**, select **+ New client secret**.
3. For **Description**, enter a description for the secret. For example, enter *FoundationaLLM-Client*.
4. Select a desired expiration date.
5. Select **Add**.
6. **Record the secret value** to add to your App Configuration settings later. Do this by selecting the **Copy to clipboard** icon next to the secret value.

    ![The steps to create a client secret are highlighted.](media/entra-client-app-secret.png) -->

#### Update the client application manifest

1. Under **Manage**, select **Manifest**.
2. Locate the `accessTokenAcceptedVersion` property and set its value to `2`.

    ![The accessTokenAcceptedVersion property is highlighted.](media/entra-client-app-manifest.png)

3. Select **Save** at the top of the page to save the changes.

### Create the API application

#### Register the API application in the Microsoft Entra ID admin center

1. Return to the [Microsoft Entra ID admin center](https://entra.microsoft.com).
2. Browse to **Identity** > **Applications** > **App registrations** and select **+ New registration**.

    ![The app registrations menu item in the left-hand menu is highlighted.](media/entra-app-registrations.png)

3. For **Name**, enter the name *FoundationaLLM-Core-API* for the application. 
4. Under **Supported account types**, select *Accounts in this organizational directory only*.
5. Select **Register**.

    ![The new API app registration form is displayed.](media/entra-register-api-app.png)

6. The application's **Overview** pane displays upon successful registration. Record the **Application (client) ID** and **Directory (tenant) ID** to add to your App Configuration settings later.

    ![The Entra app client ID and Directory ID values are highlighted in the Overview blade.](media/entra-api-app-overview.png)

#### Implicit grant and hybrid flows for the API application

1. Select **Authentication** under **Manage** in the left-hand menu.
2. Select **+ Add a platform** under **Platform configurations**. In the pane that opens, select **Web**.
3. Under "Redirect URIs", enter `http://localhost` and select **Configure**. Please note that this value is not used in the FoundationaLLM solution, but is required in order to be able to select the access and ID tokens in the next step.

    ![The redirect URIs value is displayed as described.](media/entra-app-api-web-redirect-uri.png)

4. Check **Access tokens** and **ID tokens** under **Implicit grant**.
5. Select **Configure** to apply the changes.
6. Select **Save** at the bottom of the page to save the changes.

    ![Both the Access tokens and ID tokens checkboxes are checked and the Save button is highlighted.](media/entra-app-client-authentication-implicit-grant.png)

<!-- #### Client secret for the API application

1. Under **Manage**, select **Certificates & secrets**.
2. Under **Client secrets**, select **+ New client secret**.
3. For **Description**, enter a description for the secret. For example, enter *FoundationaLLM*.
4. Select a desired expiration date.
5. Select **Add**.
6. **Record the secret value** to add to your App Configuration settings later. Do this by selecting the **Copy to clipboard** icon next to the secret value.

    ![The steps to create a client secret are highlighted.](media/entra-api-app-secret.png) -->

#### Expose an API for the API application

1. Under **Manage**, select **Expose an API** > **Add a scope**. For **Application ID URI**, make sure to use `api://FoundationaLLM-Core`, then enter the following details:
   - **Scope name**: `Data.Read`
   - **Who can consent?**: **Admins and users**
   - **Admin consent display name**: `Read data on behalf of users`
   - **Admin consent description**: `Allows the app to read data on behalf of the signed-in user.`
   - **User consent display name**: `Read data on behalf of the user`
   - **User consent description**: `Allows the app to read data on behalf of the signed-in user.`
   - **State**: **Enabled**
2. Select **Add scope** to complete the scope addition.

   ![The Add a scope form is displayed as described in the bulleted list above.](media/entra-api-app-add-scope.png)

3. Copy the **Scope name** value to add to your App Configuration settings later. For example, it should look something like `api://FoundationaLLM-Core/Data.Read`.

   ![The new scope name is displayed with the Copy button highlighted.](media/entra-api-app-scope-copy-name.png)

#### Add authorized client application

1. While still in the **Expose an API** section, select **+ Add a client application**.
2. Paste the **Application (client) ID** of the client application that you [created earlier](#register-the-client-application-in-the-microsoft-entra-admin-center).
3. Check the `Data.Read` authorized scope that you created.
4. Select **Add application** to complete the client application addition.

    ![The add a client application form is displayed as described.](media/entra-api-app-add-client-app.png)

#### Update the API application manifest

1. Under **Manage**, select **Manifest**.
2. Locate the `accessTokenAcceptedVersion` property and set its value to `2`.

    ![The accessTokenAcceptedVersion property is highlighted.](media/entra-client-app-manifest.png)

3. Select **Save** at the top of the page to save the changes.

### Add API permissions for the client application

1. Browse to **Identity** > **Applications** > **App registrations**.

    ![The app registrations menu item in the left-hand menu is highlighted.](media/entra-app-registrations.png)

2. Select the `FoundationaLLM-Client` application that you [created earlier](#register-the-client-application-in-the-microsoft-entra-admin-center).
3. Select **API permissions**.
4. Select **+ Add a permission** under the "Configured permissions" section.
5. In the "Request API permissions" pan, select the **My APIs** tab, then select the `FoundationaLLM` API application.

    ![The FoundationaLLM API is selected under My APIs.](media/entra-app-add-api-permission.png)

6. Select the `Data.Read` scope that you created earlier, then select **Add permissions**.

    ![The Data.Read scope is selected.](media/entra-app-add-api-permission-scope.png)

The client application's configured permissions should now look like the following:

![The client application's configured permissions are displayed.](media/entra-app-configured-permissions.png)