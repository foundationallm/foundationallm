# Quick Start Deployment

The Quick Start deployment uses Azure Container Apps (ACA) for rapid deployment and streamlined development. This option is ideal for proof-of-concept, development, and smaller production workloads.

## Overview

| Aspect | Details |
|--------|---------|
| **Infrastructure** | Azure Container Apps (ACA) |
| **Deployment Time** | ~30 minutes |
| **Scalability** | Auto-scaling built-in |
| **Networking** | Public endpoints |
| **SSL** | Managed certificates |

## Prerequisites

### Azure Requirements

| Requirement | Details |
|-------------|---------|
| **Azure Subscription** | Active subscription with billing enabled |
| **Azure OpenAI Access** | [Request access here](https://customervoice.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR7en2Ais5pxKtso_Pz4b1_xUNTZBNzRKNlVQSFhZMU9aV09EVzYxWFdORCQlQCN0PWcu) |
| **VM Quota** | Minimum 65 vCPUs across all VM families |
| **Role Assignment** | Owner on target subscription |

### Entra ID Requirements

| Requirement | Details |
|-------------|---------|
| **App Registrations** | 6 registrations required (see [Authentication Setup](security-permissions/authentication-authorization/index.md)) |
| **Role Assignment** | Owner on app registrations |

### Required Tools

| Tool | Version | Installation |
|------|---------|--------------|
| Azure Developer CLI | v1.6.1+ | [Install azd](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) |
| Azure CLI | v2.51.0+ | [Install Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) |
| PowerShell | 7.4.1+ | [Install PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) |
| Git | Latest | [Install Git](https://git-scm.com/downloads) |

### Optional Tools

| Tool | Purpose |
|------|---------|
| .NET 8 SDK | Local debugging |
| Visual Studio 2022 | Development |
| Docker Desktop | Container testing |

## Deployment Steps

### Step 1: Pre-Deployment Setup

Complete the [Authentication Setup](security-permissions/authentication-authorization/index.md) to create the required Entra ID app registrations.

> **Important:** App registrations must be created before deployment. You will update some values after deployment.

### Step 2: Clone Repository

```powershell
git clone https://github.com/foundationallm/foundationallm.git
cd foundationallm
git checkout release/0.8.3
```

> **Note:** Use the latest [release branch](https://github.com/foundationallm/foundationallm/releases) for production deployments. The `main` branch is for development.

### Step 3: Install Deployment Utilities

```powershell
cd deploy/common/scripts
./Get-AzCopy.ps1
```

### Step 4: Authenticate

```powershell
cd deploy/quick-start

# Azure CLI
az login

# Azure Developer CLI
azd auth login

# AzCopy
../common/tools/azcopy/azcopy login
```

### Step 5: Configure Environment

```powershell
azd env new --location <azure-region> --subscription <subscription-id>
```

**Supported regions:** Choose a region that supports Azure OpenAI and required models. See [Azure OpenAI regional availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models).

### Step 6: Configure Authentication Settings

```powershell
../common/scripts/Set-AzdEnvEntra.ps1
```

This script prompts for your Entra ID app registration values.

### Step 7: (Optional) Use Existing Azure OpenAI

If you have an existing Azure OpenAI instance:

```powershell
azd env set OPENAI_NAME <openai-name>
azd env set OPENAI_RESOURCE_GROUP <resource-group>
azd env set OPENAI_SUBSCRIPTION_ID <subscription-id>
```

> **Important:** Ensure relevant Managed Identities (LangChain API, Semantic Kernel API, Gateway API) have `OpenAI Reader` role on the Azure OpenAI resource.

### Step 8: Deploy

```powershell
azd up
```

This command:
1. Provisions Azure infrastructure
2. Updates App Configuration entries
3. Deploys API and web services
4. Imports default files to storage

## Post-Deployment Configuration

### Configure MS Graph Permissions

Run this script to grant MS Graph access:

```powershell
cd deploy/quick-start
../common/scripts/Set-FllmGraphRoles.ps1 -resourceGroupName rg-<azd-env-name>
```

> **Requirement:** User must be `Global Administrator` or have `Privileged Role Administrator` role.

### Update OAuth Callback URIs

```powershell
../common/scripts/Update-OAuthCallbackUris.ps1
```

### Verify Deployment

1. Navigate to the Chat Portal URL (from deployment output)
2. Log in with an authorized user
3. Send a test message to verify agent response

## Teardown

To remove all deployed resources:

```powershell
azd down --purge
```

> **Important:** Use `--purge` to permanently delete soft-deleted resources (Azure OpenAI, Key Vault, AI Search, Content Safety).

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Quota exceeded | [Request quota increase](https://learn.microsoft.com/azure/quotas/per-vm-quota-requests) |
| Authentication errors | Verify app registration settings |
| Deployment fails | Check `azd` logs for specific errors |
| Container crashes | Review container logs in Azure Portal |

## Related Topics

- [Standard Deployment](deployment-standard.md)
- [Authentication Setup](security-permissions/authentication-authorization/index.md)
- [App Configuration Values](app-configuration-values.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
