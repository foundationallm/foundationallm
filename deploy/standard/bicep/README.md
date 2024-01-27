# Standard Deployment with AKS (bicep)

This deployment option for FoundationaLLM uses Azure Kubernetes Service (AKS) to host the applications.  Compared to Azure Container Apps (ACA), AKS provides more advanced orchestration and scaling capabilities suitable for larger workloads.

There are Azure Subscription quota limits for the number of Azure OpenAI Service resources that can be deployed. The Standard Deployment will configure OpenAI instances to use the maximum quota available.  If existing OpenAI resources are already deployed in the subscription, the Standard Deployment will not be able to deploy.  The Standard Deployment should be deployed to a subscription with no existing OpenAI resources, or a new subscription should be created for the Standard Deployment.

The provided list outlines the prerequisites for deploying the FoundationLLM application on Azure, including specific requirements related to Azure OpenAI. Below is a summary of the prerequisites along with additional information:

## Prerequisites:

1. **Azure Subscription:**
   - Obtain an Azure Subscription. Ensure that the subscription is whitelisted for Azure OpenAI. You can request access to Azure OpenAI [here](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu).

2. **Azure CLI (v2.51.0 or greater):**
   - Install Azure CLI with a version of [2.51.0 or greater](https://docs.microsoft.com/cli/azure/install-azure-cli). The Azure CLI is essential for managing Azure resources and interacting with the Azure OpenAI service.

3. **Helm (v3.11.1 or greater):**
   - Install Helm with a version of [3.11.1 or greater](https://helm.sh/docs/intro/install/). Helm is a package manager for Kubernetes and may be used for deploying and managing applications on Azure Kubernetes Service (AKS).

4. **Minimum Quota of 65 CPUs:**
   - Ensure that your Azure Subscription has a minimum quota of 65 CPUs across all VM family types. This is necessary for the deployment of resources. [Manage VM quotas as needed](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests).

5. **App Registrations in Entra ID Tenant (Azure AD):**
   - Ability to create App Registrations in the Entra ID tenant.

6.  **User Role Assignments:**
   - Assign the following roles to the user performing the deployment:
      - Owner on the target subscription.
      - Owner on the App Registrations created in the Entra ID tenant.

## Deployment Steps:

Follow the steps below to deploy the solution to your Azure subscription.

1. **Login to your Azure account**

   - Open a PowerShell prompt and execute the following command to login to your Azure account:
     ```powershell
     az login
     ```

     Select your subscription

     ```powershell
     az account set --subscription <subscription_id>
     ```

1. **Clone the Repository:**
   - Execute the following command to clone the repository:
     ```powershell
     git clone https://github.com/solliancenet/foundationallm.git
     ```

2. **Navigate to the bicep deployment directory:**
   - Execute the following command to navigate:
     ```powershell
     cd foundationallm/deploy/standard/bicep
     ```

3. **Load psake into your PowerShell session:**
   - Execute the following command:
     ```powershell
     ./bootstrap.ps1
     ```

4. **Edit psakefile.ps1 with your deployment details:**

   - Edit the following variables in the `psakefile.ps1` file:
     ```powershell
     $aksAdmnistratorObjectId = <Entra object id for the user or group to set as the AKS cluster administrator>
     $environment = <your prefered environment name like demo, dev, prod>
     $location = <Azure region to deploy to>
     $project = <your prefered tag to add to all resource names>
     $subscription = <your subscription id>
     ```
5. **Execute the deployment:**
   - Execute the following command to deploy the solution:
     ```powershell
     Invoke-psake
     ```

### Additional Notes:
- **Azure OpenAI Service Regions:**
  - Ensure that the `<location>` value in your deployment script corresponds to a region that supports Azure OpenAI services. Refer to Azure OpenAI service regions for more information.

## Post-deployment configuration

### Authentication setup

Follow the instructions in the [Authentication setup document](https://docs.foundationallm.ai/deployment/authentication/index.html) to configure authentication for the solution.
