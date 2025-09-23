# Create Entra ID app registration for SharePoint Online Data Source

This step-by-step guide walks through configuring an Entra ID app registration as is required for access to SharePoint Online data source.

## Step-by-step instructions

Apps typically access SharePoint Online through certificates: Anyone having the certificate and its private key can use the app with the permissions granted to it.

### Create a new **App registration** in your **Microsoft Entra ID** tenant:

1. Open the [Azure portal](https://portal.azure.com) and select **Microsoft Entra ID** from the left navigation or the portal search bar.
2. Navigate to **App registrations** and click **New registration**.
3. Enter a descriptive **Name** so other administrators can quickly identify the app.
4. Choose the **Supported account types** that match your scenario (most tenants use **Accounts in this organizational directory only**).
5. Leave the **Redirect URI** empty unless your app requires one, then select **Register** to create the app.
6. After the app is created, record the values shown on the **Overview** page for **Application (client) ID** and **Directory (tenant) ID** for later configuration steps.

### Add Permissions to the App Registration

1. Navigate to the **API Permissions** blade and click on **Add a permission** button Here you choose the permissions that you will grant to this application. Select **SharePoint** from the **Microsoft APIs** tab, then select **Application permissions** as the type of permissions required, choose the desired permissions (i.e. **Sites.Read.All**) and click on **Add permissions**. Here are the required scopes:

    - `Group.ReadWrite.All`
    - `User.ReadWrite.All`
    - `Sites.Read.All` OR `Sites.Selected`
      - `Sites.Read.All` will allow the application to read documents and list items in all site collections.
      - `Sites.Selected` will allow the application to access only a subset of site collections. The specific site collections and the permissions granted will be configured separately, in SharePoint Online.

2. The application permission requires admin consent in a tenant before it can be used. In order to do this, click on **API permissions** in the left menu again. At the bottom you will see a section **Grant consent**. Click on the **Grant admin consent for {{organization}}** button and confirm the action by clicking on the **Yes** button that appears at the top.

### Create a self-signed certificate for authentication

1. To invoke SharePoint Online with an app-only access token, you have to create and configure a **self-signed X.509 certificate**, which will be used to authenticate your application against Microsoft Entra ID. You can find additional details on how to do this in [this document](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

2. Next step is to register the certificate you created to this application. Click on **Certificates & secrets** blade. Next, click on the **Upload certificate** button, select the .CER file you generated earlier and click on **Add** to upload it. 

    To confirm that the certificate was successfully registered, click on **Manifest** blade and search for the `keyCredentials` property, which contains your certificate details. It should look like this:
    ```json
    "keyCredentials": [
        {
            "customKeyIdentifier": "<$base64CertHash>",
            "endDate": "yyyy-MM-ddThh:mm:ssZ",
            "keyId": "<$guid>",
            "startDate": "yyyy-MM-ddThh:mm:ssZ",
            "type": "AsymmetricX509Cert",
            "usage": "Verify",
            "value": "<$base64Cert>",
            "displayName": "CN=<$name of your cert>"
        }
    ]
    ```

3. Upload and store the certificate in the **KeyVault** where the FoundationaLLM Vectorization API has permissions to read **Secrets**. You will need the **Certificate Name** for the App Configuration settings listed in the table above.

    > **NOTE**
    >
    > Can I use other means besides certificates for realizing app-only access for my Azure AD app?
    >
    > **NO**, all other options are blocked by SharePoint Online and will result in an `Access Denied` message.

## Next steps

- [Create a new SharePoint Data Source](create-data-source.md) Using the App Registration and Certificate values you collected in the previous steps. 