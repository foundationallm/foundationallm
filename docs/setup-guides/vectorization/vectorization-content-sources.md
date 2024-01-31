# Vectorization Content Sources

This document describes the types of vectorization content sources that are supported by the vectorization pipeline.

## Azure Data Lake

The following table describes the required configuration parameters for the Azure Data Lake vectorization content source.

App configuration key | Referenced Key Vault key | Description
--- | --- | ---
`FoundationaLLM:Vectorization:ContentSources:<NAME>:AuthenticationType` | N/A | The authentication type used to connect to the underlying storage. Can be one of `AzureIdentity`, `AccountKey`, or `ConnectionString`.
`FoundationaLLM:Vectorization:ContentSources:<NAME>:ConnectionString` | `foundationallm-vectorization-contentsources-<NAME>-connectionstring` | The connection string to the Azure Storage account used for the the Azure Data Lake vectorization content source.

>**NOTE**
>
> `<NAME>` is a placeholder that needs to be replaced by the actual name of the vectorization content source. It should follow the App Configuration key naming conventions. Do not use special characters like: `%`, `.`, `..`, `:`, or `/`.
> 
> Example: `FoundationaLLM:Vectorization:ContentSources:SDZWAJournals:ConnectionString`

## Azure SQL Database

The following table describes the required configuration parameters for the Azure SQL Database vectorization content source.

App configuration key | Referenced Key Vault key | Description
--- | --- | ---
`FoundationaLLM:Vectorization:ContentSources:<NAME>:ConnectionString` | `foundationallm-vectorization-contentsources-<NAME>-connectionstring` | The connection string to the Azure SQL database used for the Azure SQL Database vectorization content source.

## SharePoint Online

The following table describes the required configuration parameters for the SharePoint Online vectorization content source.

App configuration key | Referenced Key Vault key | Description
--- | --- | ---
`FoundationaLLM:Vectorization:ContentSources:<NAME>:ClientId` | N/A | The Application (client) Id of the Microsoft Entra ID App Registration. For more details, see the [Entra ID App Registration](#entra-id-app-registration) section.
`FoundationaLLM:Vectorization:ContentSources:<NAME>:TenantId` | N/A | The unique identifier of the SharePoint Online tenant.
`FoundationaLLM:Vectorization:ContentSources:<NAME>:KeyVaultURL` | N/A | The URL of the KeyVault where the X.509 Certificate is stored.
`FoundationaLLM:Vectorization:ContentSources:<NAME>:CertificateName` | N/A | The name of the X.509 Certificate.

### Entra ID App Registration

Apps typically access SharePoint Online through certificates: Anyone having the certificate and its private key can use the app with the permissions granted to it.

1. Create a new **App registration** in your **Microsoft Entra ID** tenant. Next, provide a **Name** for your application and click on **Register** at the bottom of the blade.

2. Navigate to the **API Permissions** blade and click on **Add a permission** button Here you choose the permissions that you will grant to this application. Select **SharePoint** from the **Microsoft APIs** tab, then select **Application permissions** as the type of permissions required, choose the desired permissions (i.e. **Sites.Read.All**) and click on **Add permissions**. Here are the required scopes:

    - `Group.ReadWrite.All`
    - `User.ReadWrite.All`
    - `Sites.Read.All` OR `Sites.Selected`
      - `Sites.Read.All` will allow the application to read documents and list items in all site collections.
      - `Sites.Selected` will allow the application to access only a subset of site collections. The specific site collections and the permissions granted will be configured separately, in SharePoint Online.

3. The application permission requires admin consent in a tenant before it can be used. In order to do this, click on **API permissions** in the left menu again. At the bottom you will see a section **Grant consent**. Click on the **Grant admin consent for {{organization}}** button and confirm the action by clicking on the **Yes** button that appears at the top.

4. To invoke SharePoint Online with an app-only access token, you have to create and configure a **self-signed X.509 certificate**, which will be used to authenticate your application against Microsoft Entra ID. You can find additional details on how to do this in [this document](https://learn.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#setting-up-an-azure-ad-app-for-app-only-access).

5. Next step is to register the certificate you created to this application. Click on **Certificates & secrets** blade. Next, click on the **Upload certificate** button, select the .CER file you generated earlier and click on **Add** to upload it. 

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

6. Upload and store the certificate in the **KeyVault** where the FoundationaLLM Vectorization API has permissions to read **Secrets**. You will need the **Certificate Name** for the App Configuration settings listed in the table above.

    > **NOTE**
    >
    > Can I use other means besides certificates for realizing app-only access for my Azure AD app?
    >
    > **NO**, all other options are blocked by SharePoint Online and will result in an `Access Denied` message.

7. Navigate to the deployment storage account and edit the `resource-provider/FoundationaLLM.Vectorization/vectorization-content-source-profiles.json` file. Append the following snippet to the `ContentSourceProfiles` array.

    ```json
    {
        "Name": "[CONTENT SOURCE NAME]",
        "Type": "SharePointOnline"
    }
    ```