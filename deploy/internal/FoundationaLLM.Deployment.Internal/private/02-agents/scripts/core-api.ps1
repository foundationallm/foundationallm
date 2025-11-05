function Initialize-CoreAPI {
    param (
        [string]$UniqueName,
        [string]$Location,
        [string]$AdminGroupObjectId,
        [string]$TenantId,
        [string]$SubscriptionId,
        [string]$InstanceId,
        [string]$ContainerImage,
        [string]$FoundationaLLMRepoPath
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    $coreResourceGroupName = $resourceGroupNames.Core

    Write-Host "Ensuring Core API managed identity exists..."
    $managedIdentities = @(
        $resourceNames.CoreAPIManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring Azure role assignments for Core API managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "CoreAPIManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames `
        -AssignGraphRoles $true `
        -AssignCosmosDBRoles $true

    Write-Host "Ensuring Core API container app exists..."

    $secrets = Get-ContainerAppSecrets `
        -ResourceNames $resourceNames
    $environmentVariables = Get-ContainerAppEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.CoreAPIManagedIdentity

    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.CoreAPIContainerApp `
        -ContainerAppIdentity $resourceNames.CoreAPIManagedIdentity `
        -Secrets $secrets `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 1
}

function Restart-CoreAPI {
    param (
        [string]$UniqueName
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    Restart-ContainerApp `
        -ResourceGroupName $resourceGroupNames.Core `
        -ContainerAppName $resourceNames.CoreAPIContainerApp
}

function New-CoreAPIArtifacts {
    param (
        [string]$UniqueName,
        [string]$InstanceId,
        [string]$UserGroupObjectId
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    $managementAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.ManagementAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
    $coreAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.CoreAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)

    # Initialize global variables for the FoundationaLLM.Core module
    $global:InstanceId = $InstanceId
    $global:ManagementAPIBaseUrl = $managementAPIEndpointURL
    $global:ManagementAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"
    $global:CoreAPIBaseUrl = $coreAPIEndpointURL
    $global:CoreAPIInstanceRelativeUri = "/instances/$($global:InstanceId)"

    $coreAPIManagedIdentityObjectId = (az identity show -n $resourceNames.CoreAPIManagedIdentity -g $resourceGroupNames.Core --query principalId -o tsv)
    $packagePath = "$PSScriptRoot\..\packages\core-api"

    Deploy-FoundationaLLMPackage `
        -PackageRoot $packagePath `
        -Parameters @{
            CORE_API_MI_OBJECT_ID = $coreAPIManagedIdentityObjectId
            USER_GROUP_OBJECT_ID  = $UserGroupObjectId
        }
}