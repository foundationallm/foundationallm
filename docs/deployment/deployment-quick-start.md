# Deployment - Quick Start

Foundationa**LLM** is designed for seamless deployment within your Azure Subscription. It initially utilizes Azure Container Apps (ACA) for rapid deployment and streamlined development. For scaling up to production environments, FoundationaLLM also supports deployment on Azure Kubernetes Service (AKS), offering robust scalability and management features.

Be mindful of the [Azure OpenaAI regional quota limits](https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits) on the number of Azure OpenAI Service instances. To optimize resource usage, FoundationaLLM offers the flexibility to connect to an existing Azure OpenAI Service resource, thereby avoiding the creation of additional instances during deployment. This feature is particularly useful for managing resource allocation and ensuring efficient Azure OpenAI Service quota utilization.

## Prerequisites

You will need the following resources and access to deploy the solution:

- Azure Subscription: An Azure Subscription is a logical container in Microsoft Azure that links to an Azure account and is the basis for billing, resource management, and allocation. It allows users to create and manage Azure resources like virtual machines, databases, and more, providing a way to organize access and costs associated with these resources.
- Subscription access to Azure OpenAI service: Access to Azure OpenAI Service provides users with the ability to integrate OpenAI's advanced AI models and capabilities within Azure. This service combines OpenAI's powerful models with Azure's robust cloud infrastructure and security, offering scalable AI solutions for a variety of applications like natural language processing and generative tasks. **Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu)**
- Minimum quota of 65 CPUs across all VM family types: Azure CPU quotas refer to the limits set on the number and type of virtual CPUs that can be used in an Azure Subscription. These quotas are in place to manage resource allocation and ensure fair usage across different users and services. Users can request quota increases if their application or workload requires more CPU resources. **Start here to [Manage VM Quotas](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests)**
- App Registrations created in the Entra ID tenant (formerly Azure Active Directory): Azure App Registrations is a feature in Entra ID that allows developers to register their applications for identity and access management. This registration process enables applications to authenticate users, request and receive tokens, and access Azure resources that are secured by Entra ID. **Follow the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution.**
- User with the proper role assignments: Azure Role-Based Access Control (RBAC) roles are a set of permissions in Azure that control access to Azure resource management. These roles can be assigned to users, groups, and services in Azure, allowing granular control over who can perform what actions within a specific scope, such as a subscription, resource group, or individual resource.
    - Owner on the target subscription
    - Owner on the app registrations described in the Authentication setup document

You will use the following tools during deployment:
- Azure Developer CLI ([v1.6.1 or greater](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd)): The Azure Developer CLI (azd) is a command-line tool designed to streamline the development and deployment of applications on Microsoft's Azure cloud platform. It simplifies various tasks such as setting up development environments, managing resources, and deploying applications by providing a more developer-friendly interface. The Azure Dev CLI aims to enhance productivity by abstracting complex cloud management tasks into simpler, more intuitive commands, making it easier for developers to integrate Azure services into their workflows.
- Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli)): The Azure Command Line Interface (CLI) is a set of commands used to manage Azure resources directly from the command line. It provides a simple and efficient way to automate tasks and manage Azure services, supporting both Windows, macOS, and Linux platforms. Azure CLI is particularly useful for scripting and executing batch operations, offering a comprehensive set of commands that cover almost all aspects of Azure resource management.
- [git](https://git-scm.com/downloads): Git is a distributed version control system designed to handle everything from small to very large projects with speed and efficiency. It allows multiple developers to work on the same codebase simultaneously, tracking and merging changes, and maintaining a complete history of all file revisions. Git is essential for modern software development, supporting branching and merging strategies, and is widely used for its robustness, flexibility, and remote collaboration capabilities.
- PowerShell 7 ([7.4.1 or greater](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.4)):
PowerShell 7 is a cross-platform (Windows, macOS, and Linux) automation tool and scripting language, an evolution of PowerShell that works with the .NET Core framework. It offers enhanced features and performance improvements over its predecessors and is designed for heterogeneous environments and the hybrid cloud. In PowerShell 7, the command-line executable is referred to as pwsh, an alias that is essential for integration with Azure Developer CLI (AZD) hooks and other modern automation scenarios.

**Optional** To run or debug the solution locally, you will need to install the following dependencies:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0): The .NET 8 SDK is the latest iteration of Microsoft's .NET software development kit, offering enhanced features and improvements for building applications across various platforms, including web, mobile, desktop, cloud, and IoT. It provides a comprehensive set of libraries, runtime features, and APIs, supporting multiple programming languages like C#, F#, and Visual Basic. This SDK is designed for high performance and efficiency, catering to modern development needs with support for cloud-native applications, microservices, and machine learning.
- Visual Studio 2022: Visual Studio 2022 is an advanced integrated development environment (IDE) from Microsoft, offering robust tools for developing applications on the .NET platform and other technologies. It brings improved performance, better usability, and enhanced collaboration features, supporting a wide range of programming languages and frameworks. Visual Studio 2022 is tailored for both individual developers and teams, integrating seamlessly with modern workflows and cloud services, and providing powerful debugging, code navigation, and refactoring capabilities.

**Optional** To build or test container images, you will need to install the following dependencies:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/): Docker Desktop is an application for MacOS and Windows machines for the building and sharing of containerized applications and microservices. It provides an integrated environment to use Docker containers, simplifying the process of building, testing, and deploying applications in a consistent and isolated environment. Docker Desktop includes Docker Engine, Docker CLI client, Docker Compose, and other Docker tools, making it a key tool for developers working with container-based applications.

## Deployment steps

Follow the steps below to deploy the solution to your Azure subscription.

1. Ensure all the prerequisites are met.

2. From a PowerShell prompt, execute the following to clone the repository:

    ```cmd
    git clone https://github.com/solliancenet/foundationallm.git
    git checkout release/0.4.0
    ```

3. Run the following commands to set the appropriate application registration settings for OIDC authentication.

    ```text
    cd foundationallm/deploy/quick-start

    az login            # Log into Azure CLI
    azd auth login      # Log into Azure Developer CLI

    ./scripts/bootstrap.ps1     # Bootstrap environment dependencies (AzCopy)
                                # This will prompt you for an AzCopy login to Azure

    # Set your target Subscription and Location
    azd env new --location <Supported Azure Region> --subscription <Azure Subscription ID>

    azd env set ENTRA_AUTH_API_INSTANCE <Auth API Instance>
    azd env set ENTRA_AUTH_API_CLIENT_ID <Auth API Client Id>
    azd env set ENTRA_AUTH_API_CLIENT_SECRET <Auth API Client Secret>
    azd env set ENTRA_AUTH_API_SCOPES <Auth API Scope>
    azd env set ENTRA_AUTH_API_TENANT_ID <Auth API Tenant ID>

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

    azd env set FOUNDATIONALLM_INSTANCE_ID <guid>
    ```

    >[!NOTE]
    > You need to manually generate a GUID for `FOUNDATIONALLM_INSTANCE_ID`.

    Bash:

    ```bash
    uuidgen
    ```

    PowerShell:

    ```powershell
    [guid]::NewGuid().ToString()
    ```

4. **Optional**: Bring Your Own Azure OpenAI Instance

    If you have an existing Azure OpenAI instance, you can use it by setting the following environment variables:

    ```text
    azd env set OPENAI_NAME <OpenAI Name>
    azd env set OPENAI_RESOURCE_GROUP <OpenAI Resource Group>
    azd env set OPENAI_SUBSCRIPTION_ID <OpenAI Subscription ID>
    ```
5. Deploy the solution

    After setting the OIDC-specific settings in the AZD environment above, run `azd up` in the same folder location to provision the infrastructure, update the App Configuration entries, deploy the API and web app services, and import files into the storage account.

    ```pwsh
    azd up
    ```

### Authentication setup

Follow the instructions in the [Authentication setup document](authentication/index.md) to finalizie authentication for the solution.

# Teardown

To tear down the environment, execute `azd down` in the same folder location.

```pwsh
azd down --purge
```

> Note the `--purge` argument in the command above. This ensures that resources that would otherwise be soft-deleted are instead completely purged from your Azure subscription.