function Initialize-ContextServices {
    param (
        [string]$UniqueName,
        [string]$Location,
        [string]$PythonContainerImage
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    Write-Host "Ensuring AI Search $($resourceNames.AISearch) exists in resource group $($resourceGroupNames.Context)..."
    if ((az search service list `
        -g $resourceGroupNames.Context `
        --query "[?name=='$($resourceNames.AISearch)']" -o tsv).Count -eq 0) {

        Write-Host "Creating AI Search $($resourceNames.AISearch) in resource group $($resourceGroupNames.Context)..."
        az search service create `
            --name $resourceNames.AISearch `
            --resource-group $resourceGroupNames.Context `
            --location $Location `
            --sku basic `
            --semantic-search standard `
            | Out-Null
        Write-Host "AI Search '$($resourceNames.AISearch)' created."
    } else {
        Write-Host "AI Search $($resourceNames.AISearch) already exists in resource group $($resourceGroupNames.Context)."
    }

    $logAnalyticsWorkspaceId = (az monitor log-analytics workspace show `
        --resource-group $resourceGroupNames.Core `
        --workspace-name $resourceNames.LogAnalytics `
        --query customerId -o tsv)
    $logAnalyticsKey = (az monitor log-analytics workspace get-shared-keys `
        --resource-group $resourceGroupNames.Core `
        --workspace-name $resourceNames.LogAnalytics `
        --query primarySharedKey -o tsv)

    Write-Host "Ensuring container apps environment $($resourceNames.CodeContainerAppsEnvironment) exists in resource group $($resourceGroupNames.Context)..."
    if ((az containerapp env list `
        -g $resourceGroupNames.Context `
        --query "[?name=='$($resourceNames.CodeContainerAppsEnvironment)']" -o tsv).Count -eq 0) {

        Write-Host "Creating Container Apps environment $($resourceNames.CodeContainerAppsEnvironment) in resource group $($resourceGroupNames.Context)..."
        az containerapp env create `
            -g $resourceGroupNames.Context `
            -n $resourceNames.CodeContainerAppsEnvironment `
            -l $Location `
            --logs-workspace-id $logAnalyticsWorkspaceId `
            --logs-workspace-key $logAnalyticsKey `
            --enable-workload-profiles | Out-Null
        Write-Host "Container Apps environment '$($resourceNames.CodeContainerAppsEnvironment)' created."
    }
    Write-Host "Ensuring custom session pool for Python code sessions exists in Container Apps environment $($resourceNames.CodeContainerAppsEnvironment)..."
    if ((az containerapp sessionpool list `
        --resource-group $resourceGroupNames.Context `
        --query "[?name=='$($resourceNames.PythonCodeContainerSessionPool)']" -o tsv).Count -eq 0) {

            Write-Host "Creating custom session pool for Python code sessions in Container Apps environment $($resourceNames.CodeContainerAppsEnvironment)..."
            az containerapp sessionpool create `
                --name $resourceNames.PythonCodeContainerSessionPool `
                --resource-group $resourceGroupNames.Context `
                --environment $resourceNames.CodeContainerAppsEnvironment `
                --container-type CustomContainer `
                --image $PythonContainerImage `
                --cpu 0.25 --memory 0.5Gi `
                --target-port 80 `
                --cooldown-period 300 `
                --network-status EgressDisabled `
                --max-sessions 10 `
                --ready-sessions 5 `
                --location $Location
                | Out-Null
            Write-Host "Custom session pool for Python code sessions created."
        } else {
            Write-Host "Custom session pool for Python code sessions already exists in Container Apps environment $($resourceNames.CodeContainerAppsEnvironment)."
    }
}