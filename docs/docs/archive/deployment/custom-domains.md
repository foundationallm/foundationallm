# Custom Domains

FoundationaLLM uses Azure Container Apps (ACA) or Azure Kubernetes Services (AKS) to deploy the various services needed to support the GenAI platform.  Both of these support the ability to add custom domains.

## Azure Container Apps

To add a custom domain to your ACA environments, perform the following:

1. Open the Azure Portal.
2. Browse to the subscription and resource group that contains the target FLLM instance.
3. Select the ACA instance that you want to add a custom domain too.
4. Select **Settings**, select **Custom domains**.
5. Select **Add custom domain**.
6. Type the domain you would like to add.
7. In the dialog, notice the DNS entries you will need to add/modify in order to validate your environment.
8. Once you have validated the domain, select **Add**.
9. If you selected a managed certificate, after a few moments, an Azure based SSL certificate will be bound to your custom domain.

For more information, reference [Custom domain names and bring your own certificates in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/custom-domains-certificates).

## Kubernetes

For more information, reference [Set up a custom domain name and SSL certificate with the application routing add-on](https://learn.microsoft.com/en-us/azure/aks/app-routing-dns-ssl).

## Application Registration Redirects URIs

Two of the ACA instances (`management` and `chat`) will require you to add redirect urls in order for the custom domain to function properly.

1. Open the Azure Portal.
2. Browse to **Microsoft Entra**.
3. Select **Application Registrations**.
4. Search for the chat UI application, then select it.
5. Under **Manage**, select **Authentication**.
6. In the **Single-page application Redirect URIs** section, add your custom domain appended with `signin-oidc`.
7. Select **Save**

For the management UI, perform the following:

1. Select **Application Registrations**.
2. Search for the chat UI application, then select it.
3. Under **Manage**, select **Authentication**.
4. In the **Single-page application Redirect URIs** section, add your custom domain appended with `/management/signin-oidc`.
5. Select **Save**.
