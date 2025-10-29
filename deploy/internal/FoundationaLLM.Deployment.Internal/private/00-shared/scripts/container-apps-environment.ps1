function Initialize-ContainerAppsEnvironment {
    param (
        [string]$UniqueName,
        [string]$Location
    )

    $resourceGroupName = "$UniqueName-core"
    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $logAnalyticsName = $resourceNames.LogAnalytics
    $containerAppsEnvironmentName = $resourceNames.ContainerAppsEnvironment
    $containerAppsWorkloadProfileName = "Warm"
    
    Write-Host "Ensuring log analytics workspace $($logAnalyticsName) exists in resource group '$resourceGroupName'..."
    if ((az monitor log-analytics workspace list `
        -g $resourceGroupName `
        --query "[?name=='$($logAnalyticsName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Log Analytics workspace $logAnalyticsName in resource group $resourceGroupName..."
        az monitor log-analytics workspace create `
            --resource-group $resourceGroupName `
            --workspace-name $logAnalyticsName `
            --location $Location | Out-Null
        Write-Host "Log Analytics workspace '$logAnalyticsName' created."
    }

    $logAnalyticsWorkspaceId = (az monitor log-analytics workspace show `
        --resource-group $resourceGroupName `
        --workspace-name $logAnalyticsName `
        --query customerId -o tsv)
    $logAnalyticsKey = (az monitor log-analytics workspace get-shared-keys `
        --resource-group $resourceGroupName `
        --workspace-name $logAnalyticsName `
        --query primarySharedKey -o tsv)

    Write-Host "Ensuring container apps environment $containerAppsEnvironmentName exists in resource group $resourceGroupName..."
    if ((az containerapp env list `
        -g $resourceGroupName `
        --query "[?name=='$($containerAppsEnvironmentName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Container Apps environment $containerAppsEnvironmentName in resource group $resourceGroupName..."
        az containerapp env create `
            -g $resourceGroupName `
            -n $containerAppsEnvironmentName `
            -l $Location `
            --logs-workspace-id $logAnalyticsWorkspaceId `
            --logs-workspace-key $logAnalyticsKey `
            --enable-workload-profiles | Out-Null
        Write-Host "Container Apps environment '$containerAppsEnvironmentName' created."
    }

    Write-Host "Ensuring container apps workload profile $containerAppsWorkloadProfileName exists in environment $containerAppsEnvironmentName..."
    if ((az containerapp env workload-profile list `
        -g $resourceGroupName `
        -n $containerAppsEnvironmentName `
        --query "[?name=='$containerAppsWorkloadProfileName']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Container Apps workload profile $containerAppsWorkloadProfileName in environment $containerAppsEnvironmentName..."
        az containerapp env workload-profile add `
            -g $resourceGroupName `
            -n $containerAppsEnvironmentName `
            --workload-profile-name $containerAppsWorkloadProfileName `
            --workload-profile-type D4 `
            --min-nodes 1 `
            --max-nodes 20 | Out-Null
        Write-Host "Container Apps workload profile $containerAppsWorkloadProfileName created."
    }
}

