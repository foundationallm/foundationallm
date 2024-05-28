# Deployment - Quick Start

Foundationa**LLM** is designed for seamless deployment within your Azure Subscription. It initially utilizes Azure Container Apps (ACA) for rapid deployment and streamlined development. For scaling up to production environments, FoundationaLLM also supports deployment on Azure Kubernetes Service (AKS), offering robust scalability and management features.

Be mindful of the [Azure OpenAI regional quota limits](https://learn.microsoft.com/azure/ai-services/openai/quotas-limits) on the number of Azure OpenAI Service instances. To optimize resource usage, FoundationaLLM offers the flexibility to connect to an existing Azure OpenAI Service resource, thereby avoiding the creation of additional instances during deployment. This feature is particularly useful for managing resource allocation and ensuring efficient Azure OpenAI Service quota utilization.

## Prerequisites

You will need the following resources and access to deploy the solution:

- **Azure Subscription**: An Azure Subscription is a logical container in Microsoft Azure that links to an Azure account and is the basis for billing, resource management, and allocation. It allows users to create and manage Azure resources like virtual machines, databases, and more, providing a way to organize access and costs associated with these resources.
- **Subscription access to Azure OpenAI service**: Access to Azure OpenAI Service provides users with the ability to integrate OpenAI's advanced AI models and capabilities within Azure. This service combines OpenAI's powerful models with Azure's robust cloud infrastructure and security, offering scalable AI solutions for a variety of applications like natural language processing and generative tasks. **Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu)**
- **Minimum quota of 65 CPUs across all VM family types**: Azure CPU quotas refer to the limits set on the number and type of virtual CPUs that can be used in an Azure Subscription. These quotas are in place to manage resource allocation and ensure fair usage across different users and services. Users can request quota increases if their application or workload requires more CPU resources. **Start here to [Manage VM Quotas](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests)**
- **App Registrations created in the Entra ID tenant (formerly Azure Active Directory)**: Azure App Registrations is a feature in Entra ID that allows developers to register their applications for identity and access management. This registration process enables applications to authenticate users, request and receive tokens, and access Azure resources that are secured by Entra ID. **Follow the instructions in the [Authentication and Authorization setup document](authentication-authorization/index.md) to configure authentication for the solution.**
- **User with the proper role assignments**: Azure Role-Based Access Control (RBAC) roles are a set of permissions in Azure that control access to Azure resource management. These roles can be assigned to users, groups, and services in Azure, allowing granular control over who can perform what actions within a specific scope, such as a subscription, resource group, or individual resource.
    - Owner on the target subscription
    - Owner on the App Registrations described in the Authentication setup document

You will use the following tools during deployment:
- **Azure Developer CLI ([v1.6.1 or greater](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd))**
- **Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli)):**
- **Latest [Git](https://git-scm.com/downloads)**
- **PowerShell 7 ([7.4.1 or greater](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.4))**

**Optional** To run or debug the solution locally, you will need to install the following dependencies:

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
- **Visual Studio 2022**

**Optional** To build or test container images, you will need to install the following dependencies:

- **[Docker Desktop](https://www.docker.com/products/docker-desktop/)**

> [!IMPORTANT]
> The perception of the `main` branch in GitHub, or any version control system, can vary depending on the development workflow adopted by a particular team or organization. The Foundationa**LLM** team uses the `main` branch as the primary **development** branch. In this case, the `main` branch might indeed be considered a **work in progress**, with developers regularly pushing changes and updates directly to it. It is where ongoing development work happens. 
So for deployment purposes, it is recommended to use the latest release branch, which is considered stable and tested. The release branch is a snapshot of the `main` branch at a specific point in time, where the code is considered stable and ready for deployment. The release branch is tagged with a version number, such as `0.6.0`, and is the recommended branch for deployment.  Please find our latest releases [here](https://github.com/solliancenet/foundationallm/releases)


## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription.
If you are upgrading from a previous version, like `0.5.0`, please refer to the changes in the [release notes](changes-060.md).

> [!IMPORTANT]
> Follow the instructions in the [Authentication and Authorization setup document](authentication-authorization/index.md) to finalize authentication and authorization for the solution. Bear in mind that creating the app registrations in the Entra ID tenant is a **prerequisite** for the deployment, but you will have to revisit some of these settings after the deployment is complete later to fill in some missing values that are generated during the deployment.

1. Ensure all the prerequisites are met and you have installed the tools required to complete the deployment.

2. From a PowerShell prompt, execute the following to clone the repository:

    ```cmd
    git clone https://github.com/solliancenet/foundationallm.git
    cd foundationallm/deploy/quick-start
    git checkout release/0.6.0
    ```

3. **For release 0.7.0+:** Run the following script to install the deployment utilities, including `AzCopy`, locally.

    ```cmd
    ./scripts/bootstrap.ps1
    ```

4. Run the following commands to log into the Azure and Azure Developer CLIs:

    ```azurecli
    az login            # Log into Azure CLI
    azd auth login      # Log into Azure Developer CLI
    ```

5. Set up an `azd` environment targeting your Azure subscription and desired deployment region:

    ```azurecli
    # Set your target Subscription and Location
    azd env new --location <Supported Azure Region> --subscription <Azure Subscription ID>
    ```

6. Run the following commands to set the appropriate application registration settings for OIDC authentication.

    ```text
    azd env set ENTRA_AUTH_API_INSTANCE <Auth API Instance>
    azd env set ENTRA_AUTH_API_CLIENT_ID <Auth API Client Id>
    azd env set ENTRA_AUTH_API_SCOPES <Auth API Scope>
    azd env set ENTRA_AUTH_API_TENANT_ID <Auth API Tenant ID>

    azd env set ADMIN_GROUP_OBJECT_ID <Admin Group Object Id>

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

    azd env set FOUNDATIONALLM_INSTANCE_ID <guid>
    ```
> [!NOTE]
> You need to manually generate a GUID for `FOUNDATIONALLM_INSTANCE_ID`.
### In Bash:
```bash
  uuidgen
```

### In PowerShell:

```powershell
  [guid]::NewGuid().ToString()
```

> [!IMPORTANT]
> The ADMIN_GROUP_OBJECT_ID in the Entra ID Groups has to be of type `security` NOT `Microsoft 365` and you need to at least add yourself to the group and other members that need to be admins.

1. **Optional**: Bring Your Own Azure OpenAI Instance

    If you have an existing Azure OpenAI instance, you can use it by setting the following environment variables:

    ```text
    azd env set OPENAI_NAME <OpenAI Name>
    azd env set OPENAI_RESOURCE_GROUP <OpenAI Resource Group>
    azd env set OPENAI_SUBSCRIPTION_ID <OpenAI Subscription ID>
    ```
2. Deploy the solution

    After setting the OIDC-specific settings in the AZD environment above, run `azd up` in the same folder location to provision the infrastructure, update the App Configuration entries, deploy the API and web app services, and import files into the storage account.

    ```pwsh
    azd up
    ```

### Running script to allow MS Graph access through Role Permissions

After the deployment is complete, you will need to run the following script to allow MS Graph access through Role Permissions. [Role Permissions Script](/deploy/common/scripts/Assign-MSGraph-Roles.ps1)
This script will need to be executed twice for the principal IDs of the following:
- Core API Managed Identity
- Management API Managed Identity

These can be found in the Azure portal in the main resource group for the deployment.

> [!TIP]
> The user running the script will need to have the appropriate permissions to assign roles to the managed identities. The user will need to be a `Global Administrator` or have the `Privileged Role Administrator` role in the Entra ID tenant.

The syntax for running the script from the `deploy\common\scripts` folder is:

```pwsh
.\Assign-MSGraph-Roles.ps1 -principalId <GUID of the Core API Managed Identity Principal ID>
.\Assign-MSGraph-Roles.ps1 -principalId <GUID of the Management API Managed Identity Principal ID>
```

> [!IMPORTANT]
> For this release, you will need to restart the Auth API container in the resource group to allow the changes to take effect.

# Teardown

To tear down the environment, execute `azd down` in the same folder location.

```pwsh
azd down --purge
```

> [!NOTE]
> The `--purge` argument in the command above. This ensures that resources that would otherwise be soft-deleted are instead completely purged from your Azure subscription.