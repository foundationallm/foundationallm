function Initialize-ManagementPortal {
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
    $appRegistrationNames = Get-EntraIDAppRegistrationNames

    $coreResourceGroupName = $resourceGroupNames.Core

    Write-Host "Ensuring Management Portal managed identity exists..."
    $managedIdentities = @(
        $resourceNames.ManagementPortalManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring Azure role assignments for Management Portal managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "ManagementPortalManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames

    Write-Host "Ensuring Management Portal container app exists..."

    $secrets = Get-ContainerAppSecrets `
        -ResourceNames $resourceNames
    $environmentVariables = Get-ManagementPortalEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.ManagementPortalManagedIdentity

    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.ManagementPortalContainerApp `
        -ContainerAppIdentity $resourceNames.ManagementPortalManagedIdentity `
        -Secrets $secrets `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 3

    Set-AppRegistrationRedirectURI `
        -AppRegistrationName $appRegistrationNames.ManagementPortal `
        -RedirectURI ("https://" + (az containerapp ingress show -n $resourceNames.ManagementPortalContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv) + "/signin-oidc")
}