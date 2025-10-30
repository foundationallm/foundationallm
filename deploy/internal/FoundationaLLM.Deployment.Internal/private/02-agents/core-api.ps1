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
        -ResourceNames $resourceNames

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
        [string]$UniqueName
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    $eventGridHostName = (az eventgrid namespace show -g $resourceGroupNames.Core -n $resourceNames.EventGrid --query "topicsConfiguration.hostname" -o tsv)
    $packagePath = "$PSScriptRoot\..\"

    Deploy-FoundationaLLMPackage `
        -PackageRoot $packagePath `
        -Parameters @{
            EVENT_GRID_HOSTNAME = $eventGridHostName
        }
}