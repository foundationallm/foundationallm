# Deployment - Standard with AKS

Compared to the quick start deployment using Azure Container Apps (ACA), the Foundationa**LLM** Standard Deployment with AKS is tailored for scaling up to production environments. It leverages Azure Kubernetes Service (AKS) for robust scalability and management, requiring an Azure Subscription with Azure OpenAI access.

Be mindful of the [Azure OpenaAI regional quota limits](https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits) on the number of Azure OpenAI Service instances.

This deployment option for FoundationaLLM uses Azure Kubernetes Service (AKS) to host the applications.  Compared to [Azure Container Apps (ACA) deployment](deployment-quick-start.md), AKS provides more advanced orchestration and scaling capabilities suitable for larger workloads. The Standard Deployment will configure OpenAI instances to use the maximum quota available.  If existing OpenAI resources are already deployed in the subscription, the Standard Deployment will not be able to deploy.  The Standard Deployment should be deployed to a subscription with no existing OpenAI resources, or a new subscription should be created for the Standard Deployment. As a final option, the template can be updated to allocate a smaller quota.

## Prerequisites

You will need the following resources and access to deploy the solution:

- Azure Subscription: An Azure Subscription is a logical container in Microsoft Azure that links to an Azure account and is the basis for billing, resource management, and allocation. It allows users to create and manage Azure resources like virtual machines, databases, and more, providing a way to organize access and costs associated with these resources.
- Subscription access to Azure OpenAI service: Access to Azure OpenAI Service provides users with the ability to integrate OpenAI's advanced AI models and capabilities within Azure. This service combines OpenAI's powerful models with Azure's robust cloud infrastructure and security, offering scalable AI solutions for a variety of applications like natural language processing and generative tasks. **Start here to [Request Access to Azure OpenAI Service](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu)**
- Minimum quota of vCPUs across all VM family types: Azure CPU quotas refer to the limits set on the number and type of virtual CPUs that can be used in an Azure Subscription. For Azure Kubernetes Service **(AKS)** deployment, you will need a quota of around 24 vCPUs across all VM family types to ensure that the solution can be deployed and run successfully as the system deploys 2 clusters with 2 node pools each, each pool is set to scale between 1 and 3 instances where each instance has 2 vCPUS which brings the total to 24 vCPUS.
These quotas are in place to manage resource allocation and ensure fair usage across different users and services. Users can request quota increases if their application or workload requires more CPU resources. **Start here to [Manage VM Quotas](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests)**
- App Registrations created in the Entra ID tenant (formerly Azure Active Directory): Azure App Registrations is a feature in Entra ID that allows developers to register their applications for identity and access management. This registration process enables applications to authenticate users, request and receive tokens, and access Azure resources that are secured by Entra ID. **Follow the instructions in the [Authentication setup document](authentication/index.md) to configure authentication for the solution.**
- User with the proper role assignments: Azure Role-Based Access Control (RBAC) roles are a set of permissions in Azure that control access to Azure resource management. These roles can be assigned to users, groups, and services in Azure, allowing granular control over who can perform what actions within a specific scope, such as a subscription, resource group, or individual resource.
    - Owner on the target subscription
    - Owner on the app registrations described in the Authentication setup document

You will use the following tools during deployment:

- Azure CLI ([v2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli))
- [git](https://git-scm.com/downloads)
- PowerShell 7 ([7.4.1 or greater](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.4))
- [Helm](https://helm.sh/docs/intro/install/) 
- [kubectl](https://kubernetes.io/docs/tasks/tools/) 
- [kubelogin](https://azure.github.io/kubelogin/install.html) 

**Optional** To run or debug the solution locally, you will need to install the following dependencies:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/)

**Optional** To build or test container images, you will need to install the following dependencies:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Pre-Deployment steps

Follow the steps below to deploy the solution to your Azure subscription.

1. Ensure all the prerequisites are met.

2. From a PowerShell prompt, execute the following to clone the repository:

    ```pwsh
      git clone https://github.com/solliancenet/foundationallm.git
      cd foundationallm
      git checkout release/0.9.6
    ```

3. Set up an `azd` environment targeting your Azure subscription and desired deployment region:

    ```pwsh
    # Set your target Subscription and Location
    azd env new --location <Supported Azure Region> --subscription <Azure Subscription ID>
    ```

4.  Provision SSL certificates for the appropriate domains and package them in PFX format.  Place the PFX files in `foundationallm/deploy/standard/certs` following the naming convention below.  The values for `Host Name` and `Domain Name` should match the values you provided in your deployment manifest:

    | Service Name      | Host Name         | Domain Name | File Name                         |
    | ----------------- | ----------------- | ----------- | --------------------------------- |
    | core-api          | api               | example.com | api.example.com.pfx               |
    | management-api    | management-api    | example.com | management-api.example.com.pfx    |
    | chat-ui           | chat              | example.com | chat.example.com.pfx              |
    | management-ui     | management        | example.com | management.example.com.pfx        |

## Provision Infrastructure

5. Provision platform infrastructure with `AZD`:

    ```pwsh
    cd .\deploy\standard
    azd provision
    ```

    The deployment process will take some time.

    The AZD post-provisioning hook script will generate a `hosts` file in the `.\deploy\standard\config` folder describing all the private endpoint IPs and the associated hostnames.  These values can be used to populate your computer's local `hosts` file, or may assist with configuring your organization's DNS system.  This guide will assume that you have taken the contents of the generated file and added them to your local `hosts` file.

## Configure and Deploy

6. Ensure that you have network access to the deployed resources and that DNS resolution to deployed resources is configured (this is environment specific).

7. Deploy to platform infrastructure with `AZD`:

    ```pwsh
    cd .\deploy\standard
    azd deploy
    ```

    The deployment process will take some time.  The process will:
    - Generate the configuration for the system.
    - Load the configuration into App Configuration.
    - Load default system files into Azure Storage.
    - Configure the backend cluster.
      - Create the FLLM namespace in the backend cluster
        - Deploy the backend services to the cluster in the FLLM namespace
      - Create the gateway-system namespace
        - Deploy the secret class provider to the gateway-system namespace
        - Deploy ingress-nginx
        - Deploy Ingress Configurations and External Services
      - Configure the frontend cluster.
        - Create the FLLM namespace in the frontend cluster
          - Deploy the frontend services to the cluster in the FLLM namespace
        - Create the gateway-system namespace
          - Deploy the secret class provider to the gateway-system namespace
          - Deploy ingress-nginx
          - Deploy Ingress Configurations and External Services
      - Generate host file entries for the deployed services on AKS that you can add to your host file or DNS server.

    The AZD deploy hook script will generate a `hosts.ingress` file in the `.\deploy\standard\config` folder describing the api and frontend endpoints and the associated hostnames.  These values can be used to populate your computer's local `hosts` file, or may assist with configuring your organization's DNS system.  This guide will assume that you have taken the contents of the generated file and added them to your local `hosts` file.16.  Update your local `hosts` file with the entries from the generated host file.

### Running script to allow MS Graph access through Role Permissions

8. After the deployment is complete, you will need to run the following script to allow MS Graph access through Role Permissions. (See below)

    > [!IMPORTANT]
    > The user running the script will need to have the appropriate permissions to assign roles to the managed identities. The user will need to be a `Global Administrator` or have the `Privileged Role Administrator` role in the Entra ID tenant.
    The syntax for running the script from the `deploy\standard` folder is:

    ```pwsh
        cd .\deploy\standard
        ..\common\scripts\Set-FllmGraphRoles.ps1 -resourceGroupName rg-<azd env name>
    ```
    Finally, you will need to update the Authorization Callbacks in the App Registrations created in the Entra ID tenant by running the following script:

    ```pwsh
        cd .\deploy\standard
        ..\common\scripts\Update-OAuthCallbackUris.ps1
    ```


## Connect and Test

9. Visit the chat UI in your browser and send a message to verify the deployment.  The message can be very simple like "Who are you?".  The default agent should respond with a message explaining it's persona.