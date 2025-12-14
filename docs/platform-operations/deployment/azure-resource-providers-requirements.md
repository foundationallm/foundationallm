# Azure Resource Provider Requirements

FoundationaLLM requires specific Azure Resource Providers to be registered in your subscription before deployment.

## Overview

Azure Resource Providers enable specific Azure services. If a required provider is not registered, deployment will fail with a "resource provider not registered" error.

## Quick Start Deployment Requirements

The following providers are required for Quick Start (ACA) deployments:

| Resource Provider | Service |
|-------------------|---------|
| `microsoft.alertsmanagement/smartDetectorAlertRules` | Smart detection alerts |
| `Microsoft.App/containerApps` | Container Apps |
| `Microsoft.App/managedEnvironments` | Container Apps environments |
| `Microsoft.AppConfiguration/configurationStores` | App Configuration |
| `Microsoft.CognitiveServices/accounts` | Azure OpenAI, Content Safety |
| `Microsoft.DocumentDB/databaseAccounts` | Cosmos DB |
| `Microsoft.EventGrid/namespaces` | Event Grid |
| `Microsoft.EventGrid/systemTopics` | Event Grid system topics |
| `Microsoft.Insights/components` | Application Insights |
| `Microsoft.KeyVault/vaults` | Key Vault |
| `Microsoft.ManagedIdentity/userAssignedIdentities` | Managed identities |
| `Microsoft.OperationalInsights/workspaces` | Log Analytics |
| `Microsoft.Portal/dashboards` | Azure dashboards |
| `Microsoft.Search/searchServices` | Azure AI Search |
| `Microsoft.Storage/storageAccounts` | Storage accounts |

## Standard Deployment Requirements

The Standard (AKS) deployment requires additional providers:

| Resource Provider | Service |
|-------------------|---------|
| `microsoft.alertsmanagement/smartDetectorAlertRules` | Smart detection alerts |
| `Microsoft.AppConfiguration/configurationStores` | App Configuration |
| `Microsoft.CognitiveServices/accounts` | Azure OpenAI, Content Safety |
| `Microsoft.Compute/virtualMachineScaleSets` | VM scale sets (AKS nodes) |
| `Microsoft.ContainerService/managedClusters` | AKS clusters |
| `Microsoft.DocumentDB/databaseAccounts` | Cosmos DB |
| `Microsoft.EventGrid/namespaces` | Event Grid |
| `Microsoft.EventGrid/systemTopics` | Event Grid system topics |
| `Microsoft.Insights/actiongroups` | Alert action groups |
| `Microsoft.Insights/components` | Application Insights |
| `Microsoft.Insights/metricalerts` | Metric alerts |
| `microsoft.insights/privateLinkScopes` | Private link for monitoring |
| `Microsoft.Insights/scheduledqueryrules` | Log query alerts |
| `Microsoft.KeyVault/vaults` | Key Vault |
| `Microsoft.ManagedIdentity/userAssignedIdentities` | Managed identities |
| `Microsoft.Network/loadBalancers` | Load balancers |
| `Microsoft.Network/networkInterfaces` | Network interfaces |
| `Microsoft.Network/networkSecurityGroups` | NSGs |
| `Microsoft.Network/privateEndpoints` | Private endpoints |
| `Microsoft.Network/publicIPAddresses` | Public IPs |
| `Microsoft.Network/virtualNetworks` | Virtual networks |
| `Microsoft.OperationalInsights/workspaces` | Log Analytics |
| `Microsoft.OperationsManagement/solutions` | Management solutions |
| `Microsoft.Search/searchServices` | Azure AI Search |
| `Microsoft.Storage/storageAccounts` | Storage accounts |

## Checking Provider Registration

### Azure Portal

1. Navigate to your subscription
2. Select **Settings** > **Resource providers**
3. Search for the provider name
4. Check the **Status** column

### Azure CLI

```bash
# List all providers and their status
az provider list --output table

# Check specific provider
az provider show --namespace Microsoft.App --query "registrationState"
```

### PowerShell

```powershell
# List all providers
Get-AzResourceProvider | Select-Object ProviderNamespace, RegistrationState

# Check specific provider
(Get-AzResourceProvider -ProviderNamespace Microsoft.App).RegistrationState
```

## Registering Providers

### Azure Portal

1. Navigate to **Subscription** > **Resource providers**
2. Search for the provider
3. Select the provider
4. Click **Register**

### Azure CLI

```bash
# Register a provider
az provider register --namespace Microsoft.App

# Wait for registration (may take several minutes)
az provider show --namespace Microsoft.App --query "registrationState" --output tsv
```

### PowerShell

```powershell
# Register a provider
Register-AzResourceProvider -ProviderNamespace Microsoft.App

# Check status
(Get-AzResourceProvider -ProviderNamespace Microsoft.App).RegistrationState
```

## Bulk Registration Script

Register all required providers for Standard deployment:

```powershell
$providers = @(
    "Microsoft.App",
    "Microsoft.AppConfiguration",
    "Microsoft.CognitiveServices",
    "Microsoft.Compute",
    "Microsoft.ContainerService",
    "Microsoft.DocumentDB",
    "Microsoft.EventGrid",
    "Microsoft.Insights",
    "Microsoft.KeyVault",
    "Microsoft.ManagedIdentity",
    "Microsoft.Network",
    "Microsoft.OperationalInsights",
    "Microsoft.OperationsManagement",
    "Microsoft.Search",
    "Microsoft.Storage"
)

foreach ($provider in $providers) {
    Write-Host "Registering $provider..."
    Register-AzResourceProvider -ProviderNamespace $provider
}

# Wait and verify
Start-Sleep -Seconds 30
foreach ($provider in $providers) {
    $status = (Get-AzResourceProvider -ProviderNamespace $provider).RegistrationState
    Write-Host "$provider : $status"
}
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Resource provider not registered" | Register the provider shown in error |
| Registration stuck in "Registering" | Wait up to 10 minutes, then retry |
| "Authorization failed" | Ensure you have Contributor role on subscription |
| Provider not available | Check if service is available in your region |

## Required Permissions

To register resource providers, you need:

- **Contributor** or **Owner** role on the subscription, OR
- A custom role with `Microsoft.*/register/action` permission

## Related Topics

- [Quick Start Deployment](deployment-quick-start.md)
- [Standard Deployment](deployment-standard.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
