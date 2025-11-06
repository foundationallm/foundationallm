function Initialize-ContextAPI {
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

    Write-Host "Ensuring Context API managed identity exists..."
    $managedIdentities = @(
        $resourceNames.ContextAPIManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring Azure role assignments for Context API managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "ContextAPIManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames `
        -AssignGraphRoles $false `
        -AssignCosmosDBRoles $true

    Write-Host "Ensuring Context API container app exists..."

    $secrets = Get-ContainerAppSecrets `
        -ResourceNames $resourceNames
    $environmentVariables = Get-ContainerAppEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.ContextAPIManagedIdentity

    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.ContextAPIContainerApp `
        -ContainerAppIdentity $resourceNames.ContextAPIManagedIdentity `
        -Secrets $secrets `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 1
}

function Restart-ContextAPI {
    param (
        [string]$UniqueName
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    Restart-ContainerApp `
        -ResourceGroupName $resourceGroupNames.Core `
        -ContainerAppName $resourceNames.ContextAPIContainerApp
}


function New-ContextAPIArtifacts {
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

    $contextAPIManagedIdentityObjectId = (az identity show -n $resourceNames.ContextAPIManagedIdentity -g $resourceGroupNames.Core --query principalId -o tsv)
    $packagePath = "$PSScriptRoot\..\packages\context-api"

    Deploy-FoundationaLLMPackage `
        -PackageRoot $packagePath `
        -Parameters @{
            CONTEXT_API_MI_OBJECT_ID = $contextAPIManagedIdentityObjectId
        }
}
