# Deployment - Starter

Foundationa**LLM** deploys into your own Azure Subscription. By default it will deploy to Azure Container Apps (ACA) that make it fast to get started. When you want to deploy to production at scale, you can also deploy to Azure Kubernetes Service (AKS). Given that there are Azure Subscription quota limits to the number of Azure OpenAI Service resources you can deploy, you can choose to use an existing Azure OpenAI Service resource instead of a creating a new one with your deployment.

## Prerequisites

- Azure Subscription (Subscription needs to be whitelisted for Azure OpenAI).
- Subscription access to Azure OpenAI service. Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu).
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Docker Desktop.
- Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli)).
- Azure Developer CLI ([v1.6.1 or greater](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd))
- Powershell 7 ([7.4.1 or greater](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.4))
    > Note: Powershell 7 is required for `pwsh` to work in the AZD hooks as an alias to `powershell`
- Visual Studio 2022 (only needed if you plan to run/debug the solution locally).
- Minimum quota of 65 CPUs across all VM family types. Start here to [Manage VM Quotas](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests).
- Four App Registrations created in the Entra ID tenant (Azure Active Directory). Follow the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution. 
    - Chat Client App Registration
    - Core API App Registration
    - Management Client App Registration
    - Management API App Registration
- User with the following role assignments:
    - Owner on the target subscription;
    - Owner on the two app registrations.

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription. You will be prompted to log in to your Azure account during the deployment process.

1. Ensure all the prerequisites are met, and that Docker Desktop is running.  

1. From a PowerShell prompt, execute the following to clone the repository:

    ```cmd
    git clone https://github.com/solliancenet/foundationallm.git
    git checkout release/0.4.0
    ```

1. Open a PowerShell instance and run the following script to provision the infrastructure and deploy the API and frontend. This will provision all of the required infrastructure, deploy the API and web app services, and import data into Cosmos DB.

    Run the following command to set the appropriate application registration settings for OIDC authentication. Please refer to the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution and obtain the appropriate client Ids, scopes, and tenant Ids for the following steps.

    ```pwsh
    cd foundationallm
    cd deploy/starter

    az login            # Log into Azure CLI
    azd auth login      # Log into Azure Developer CLI

    azd env             # Set your target Subscription and Location

    azd env set ENTRA_CHAT_UI_CLIENT_ID <Chat UI Client Id>
    azd env set ENTRA_CHAT_UI_SCOPES <Chat UI Scope>
    azd env set ENTRA_CHAT_UI_TENANT_ID <Chat UI Tenant ID>

    azd env set ENTRA_CORE_API_CLIENT_ID <Core API Client Id>
    azd env set ENTRA_CORE_API_SCOPES <Core API Scope>
    azd env set ENTRA_CORE_API_TENANT_ID <Core API Tenant ID>

    azd env set ENTRA_MANAGEMENT_API_CLIENT_ID <Management API Client Id>
    azd env set ENTRA_MANAGEMENT_API_SCOPES <Management API Scope>
    azd env set ENTRA_MANAGEMENT_API_TENANT_ID <Management API Tenant ID>

    azd env set ENTRA_MANAGEMENT_UI_CLIENT_ID <Management UI Client Id>
    azd env set ENTRA_MANAGEMENT_UI_SCOPES <Management UI Scope>
    azd env set ENTRA_MANAGEMENT_UI_TENANT_ID <Management UI Tenant ID>

    azd env set ENTRA_VECTORIZATION_API_CLIENT_ID <Vectorization API Client Id>
    azd env set ENTRA_VECTORIZATION_API_SCOPES <Vectorization API Scope>
    azd env set ENTRA_VECTORIZATION_API_TENANT_ID <Vectorization API Tenant ID>
    ```

    After setting the OIDC specific settings in the AZD environment above, run `azd up` in the same folder location to build the docker images, provision the infrastructure, update the configuration, deploy the API and web app services into container app instances, and import files into the storage account.

    ```pwsh
    azd up
    ```

### Authentication setup

Follow the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution.

## Update APIs and portals from local code changes

To update APIs and portals from local code changes, run the following from the `./deploy/starter` folder in your locally cloned repository:

```pwsh
azd deploy
```

## Update individual APIs or portals

### Update individual APIs or portals with images from the public container repositorywhen using Microsoft Azure Container Apps (ACA)

To update an individual API or portal, you can use the following commands:

1. Login with your Entra ID account:
   
    ```pwsh
    az login
    ```
2. Set the target subscription:
   
    ```pwsh
    az account set --subscription <target_subscription_id>
    ```

3. Navigate to the FoundationaLLM GitHub Container Registry and obtain the SHA or image tag of the container you would like to update.

    ![Latest release of the image on the GitHub Container Registry.](./media/latest-image-release.png "Verifying Latest Image Release")

4. Use the following Azure CLI command to update the desired container. `--image` is a fully-qualified name (e.g., `ghcr.io/solliancenet/foundationallm/agent-factory-api:latest`).

    ```pwsh
    az containerapp update --name <aca_name> --resource-group <resource_group_name> --image <image_name>
    ```

    The following table indicates the mapping between each component of FLLM and the relevant Azure Container Apps instance (`--name`).

    | API | Container Name |
    | --- | -------------- |
    | Core API | `cacoreapi[SUFFIX]` |
    | Agent Factory API | `caagentfactoryapi[SUFFIX]` |
    | Agent Hub API | `caagenthubapi[SUFFIX]` |
    | Chat UI | `cachatui[SUFFIX]` |
    | Core Job API | `cacorejob[SUFFIX]` |
    | Data Source Hub API | `caadatasourcehubapi[SUFFIX]` |
    | Gatekeeper API | `cagatekeeperapi[SUFFIX]` |
    | Gatekeeper Integration API | `cagatekeeperintegrationapi[SUFFIX]` |
    | LangChain | `calangchainapi[SUFFIX]` |
    | Management API | `camanagementapi[SUFFIX]` |
    | Management UI | `camanagementui[SUFFIX]` |
    | Prompt Hub API | `caprompthubapi[SUFFIX]` |
    | Semantic Kernel API | `casemantickernelapi[SUFFIX]` |
    | Vectorization API | `camanagementapi[SUFFIX]` |
    | Vectorization Worker | `camanagementjob[SUFFIX]` |

