function Initialize-OrchestrationAPI {
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

    Write-Host "Ensuring Orchestration API managed identity exists..."
    $managedIdentities = @(
        $resourceNames.OrchestrationAPIManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring Azure role assignments for Orchestration API managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "OrchestrationAPIManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames `
        -AssignGraphRoles $false `
        -AssignCosmosDBRoles $true

    Write-Host "Ensuring Orchestration API container app exists..."

    $secrets = Get-ContainerAppSecrets `
        -ResourceNames $resourceNames
    $environmentVariables = Get-ContainerAppEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.OrchestrationAPIManagedIdentity

    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.OrchestrationAPIContainerApp `
        -ContainerAppIdentity $resourceNames.OrchestrationAPIManagedIdentity `
        -Secrets $secrets `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 1
}

function Restart-OrchestrationAPI {
    param (
        [string]$UniqueName
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    Restart-ContainerApp `
        -ResourceGroupName $resourceGroupNames.Core `
        -ContainerAppName $resourceNames.OrchestrationAPIContainerApp
}


function New-OrchestrationAPIArtifacts {
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

    $orchestrationAPIManagedIdentityObjectId = (az identity show -n $resourceNames.OrchestrationAPIManagedIdentity -g $resourceGroupNames.Core --query principalId -o tsv)
    $packagePath = "$PSScriptRoot\..\packages\orchestration-api"

    Deploy-FoundationaLLMPackage `
        -PackageRoot $packagePath `
        -Parameters @{
            ORCHESTRATION_API_MI_OBJECT_ID = $orchestrationAPIManagedIdentityObjectId
        }
}
