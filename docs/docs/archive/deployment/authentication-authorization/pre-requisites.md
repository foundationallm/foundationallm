### Pre-requisites for post-deployment configuration

> [!IMPORTANT]
> Be aware that after completing this registration and the other app registrations in Entra ID as instructed in the [docs](docs/deployment/authentication-authorization/index.md) you will complete the deployment steps outlined at [deploy the solution](../../deployment/deployment-starter.md) then you will be revisiting your app registrations to complete some of the settings that require the solution to be deployed before the entire app registration is completed successfully.

#### Setup App Configuration access

1. Sign in to the [Azure portal](https://portal.azure.com/) as at least a Contributor.
2. Navigate to the Resource Group that was created as part of the deployment.
> [!NOTE]
> If you performed an Azure Container Apps (ACA) or Azure Kubernetes Service (AKS) deployment, you will see an extra Resource Group that starts with `ME_` or `MC_` in addition to the Resource Group defined during the deployment. You will need to navigate to the Resource Group that **does not start with** `ME_` or `MC_` to access the App Configuration resource.
3. Select the **App Configuration** resource and select **Configuration explorer** to view the values. If you cannot access the configurations, add your user account as an **App Configuration Data Owner** through Access Control (IAM). You need this role in order to update the configurations as a required part of the authentication setup. To add your user account to the appropriate role, follow the instructions in the [Configure access control for services](../../deployment/configure-access-control-for-services.md#azure-app-configuration-service) document.

#### Obtain the URL for the chat UI application

You need this URL to assign the redirect URI for the client application.

If you performed an **Azure Container Apps (ACA)** deployment, follow these steps to obtain the URL for the chat UI application:

1. Within the Resource Group that was created as part of the deployment, select the **Container App** resource whose name ends with `chatuica`.

    ![The Chat UI container app is selected in the deployed resource group.](media/resource-group-aca.png)

2. Within the Overview pane, copy the **Application Url** value. This is the URL for the chat application.

    ![The container app's Application Url is highlighted.](media/aca-application-url.png)

If you performed an **Azure Kubernetes Service (AKS)** deployment, follow these steps to obtain the URL for the chat UI application:

1. Within the Resource Group that was created as part of the deployment, select the **Kubernetes Service** resource.

    ![The Kubernetes service is selected in the deployed resource group.](media/resource-group-aks.png)

2. Select **Properties** in the left-hand menu and copy the **HTTP application routing domain** value. This is the URL for the chat application.

    ![The HTTP application routing domain property is highlighted.](media/aks-http-app-routing-domain.png)