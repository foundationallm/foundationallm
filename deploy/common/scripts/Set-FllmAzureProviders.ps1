#! /usr/bin/pwsh
<#
.SYNOPSIS
    Checks and registers Azure resource providers if they are not already registered.

.DESCRIPTION
    This PowerShell script takes a list of Azure resource providers and checks if they are registered in the current subscription. If any providers are not registered, it registers them. The script can also differentiate between quickstart and standard providers based on a parameter.

.PARAMETER deploymentType
    Specifies whether to use quickstart or standard providers. Acceptable values are 'QuickStart' or 'Standard'.

.PARAMETER listOnly
    Writes the list of standard and quickstart providers to a file then exits.

.EXAMPLE
    ./Register-AzureProviders.ps1 -deploymentType "Standard"
    This example checks and registers standard Azure resource providers if they are not already registered.

.EXAMPLE
    ./Set-FllmAzureProviders.ps1 -deploymentType "QuickStart"
    This example checks and registers quickstart Azure resource providers if they are not already registered.

.NOTES
    This script should be run in a PowerShell Core environment, and uses the Azure CLI to check and register providers.
    This should be run prior to deploying FLLm to ensure to ensure that all required providers are registered.

#>

param (
    [parameter(Mandatory = $true)][string]$deploymentType,
    [parameter(Mandatory = $false)][bool]$listOnly = $false
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Define the list of standard and quickstart providers
$standardProviders = @(
    "microsoft.alertsmanagement/smartDetectorAlertRules",
    "Microsoft.AppConfiguration/configurationStores",
    "Microsoft.CognitiveServices/accounts",
    "Microsoft.Compute/virtualMachineScaleSets",
    "Microsoft.ContainerService/managedClusters",
    "Microsoft.DocumentDB/databaseAccounts",
    "Microsoft.EventGrid/namespaces",
    "Microsoft.EventGrid/systemTopics",
    "Microsoft.Insights/actiongroups",
    "Microsoft.Insights/components",
    "Microsoft.Insights/metricalerts",
    "microsoft.insights/privateLinkScopes",
    "Microsoft.Insights/scheduledqueryrules",
    "Microsoft.KeyVault/vaults",
    "Microsoft.ManagedIdentity/userAssignedIdentities",
    "Microsoft.Network/loadBalancers",
    "Microsoft.Network/networkInterfaces",
    "Microsoft.Network/networkSecurityGroups",
    "Microsoft.Network/privateEndpoints",
    "Microsoft.Network/publicIPAddresses",
    "Microsoft.Network/virtualNetworks",
    "Microsoft.OperationalInsights/workspaces",
    "Microsoft.OperationsManagement/solutions",
    "Microsoft.Search/searchServices",
    "Microsoft.Storage/storageAccounts"
)

$quickStartProviders = @(
    "microsoft.alertsmanagement/smartDetectorAlertRules",
    "Microsoft.App/containerApps",
    "Microsoft.App/managedEnvironments",
    "Microsoft.AppConfiguration/configurationStores",
    "Microsoft.CognitiveServices/accounts",
    "Microsoft.DocumentDB/databaseAccounts",
    "Microsoft.EventGrid/namespaces",
    "Microsoft.EventGrid/systemTopics",
    "Microsoft.Insights/components",
    "Microsoft.KeyVault/vaults",
    "Microsoft.ManagedIdentity/userAssignedIdentities",
    "Microsoft.OperationalInsights/workspaces",
    "Microsoft.Portal/dashboards",
    "Microsoft.Search/searchServices",
    "Microsoft.Storage/storageAccounts"
)

if($listOnly) {
    "Standard Providers:" | Out-File -FilePath "fllm.providers.txt"
    $standardProviders | Out-File -FilePath "fllm.providers.txt" -Append

    "QuickStart Providers:" | Out-File -FilePath "fllm.providers.txt" -Append
    $quickStartProviders | Out-File -FilePath "fllm.providers.txt" -Append
    exit
}

# Select the appropriate list of providers based on the parameter
if ($deploymentType -eq "Standard") {
    $providers = $standardProviders
} elseif ($deploymentType -eq "QuickStart") {
    $providers = $quickStartProviders
} else {
    Write-Host -ForegroundColor Red "Invalid deployment type. Please specify 'QuickStart' or 'Standard'."
    exit
}

# Check and register providers if necessary
foreach ($provider in $providers) {
    $providerNamespace = $provider.Split("/")[0]
    $providerStatus = az provider show --namespace $providerNamespace --query "registrationState" --output tsv

    if ($providerStatus -ne "Registered") {
        Write-Host -ForegroundColor Yellow  "Registering provider: $providerNamespace"
        az provider register --namespace $providerNamespace
    } else {
        Write-Host -ForegroundColor Blue "Provider already registered: $providerNamespace"
    }
}

Write-Host -ForegroundColor Green "Provider registration check and update completed."
