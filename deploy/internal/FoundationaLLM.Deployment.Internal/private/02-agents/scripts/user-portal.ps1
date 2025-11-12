function Initialize-UserPortal {
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

    Write-Host "Ensuring User Portal managed identity exists..."
    $managedIdentities = @(
        $resourceNames.UserPortalManagedIdentity
    )
    Initialize-ManagedIdentities `
        -ManagedIdentityNames $managedIdentities `
        -ResourceGroupName $coreResourceGroupName `
        -Location $Location

    Write-Host "Ensuring Azure role assignments for User Portal managed identity exist..."
    Set-ManagedIdentityAzureRoleAssignments `
        -SubscriptionId $SubscriptionId `
        -ManagedIdentityType "UserPortalManagedIdentity" `
        -ResourceGroupNames $resourceGroupNames `
        -ResourceNames $resourceNames

    Write-Host "Ensuring User Portal container app exists..."

    $secrets = Get-ContainerAppSecrets `
        -ResourceNames $resourceNames
    $environmentVariables = Get-PortalEnvVars `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppIdentity $resourceNames.UserPortalManagedIdentity

    Initialize-ContainerApp `
        -ResourceGroupName $coreResourceGroupName `
        -ResourceNames $resourceNames `
        -TenantId $TenantId `
        -ContainerAppName $resourceNames.UserPortalContainerApp `
        -ContainerAppIdentity $resourceNames.UserPortalManagedIdentity `
        -Secrets $secrets `
        -EnvironmentVariables $environmentVariables `
        -ContainerImage $ContainerImage `
        -MinReplicas 1 `
        -MaxReplicas 1 `
        -CPUCores 1 `
        -Memory 2

    Set-AppRegistrationRedirectURI `
        -AppRegistrationName $appRegistrationNames.UserPortal `
        -RedirectURI ("https://" + (az containerapp ingress show -n $resourceNames.UserPortalContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv) + "/signin-oidc")
}