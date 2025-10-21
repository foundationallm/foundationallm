function Initialize-CommonServices {
    param (
        [string]$UniqueName,
        [string]$Location
    )

    $resourceGroupName = "$UniqueName-core"
    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $keyVaultName = $resourceNames.KeyVault
    $appConfigName = $resourceNames.AppConfig
    $appInsightsName = $resourceNames.AppInsights
    $eventGridName = $resourceNames.EventGrid
    $logAnalyticsName = $resourceNames.LogAnalytics

    Write-Host "Registering namespaces for common services..."
    az provider register --namespace Microsoft.KeyVault
    az provider register --namespace Microsoft.AppConfiguration

    Write-Host "Ensuring Key Vault $keyVaultName exists in resource group '$resourceGroupName'..."
    if ((az keyvault list `
        -g $resourceGroupName `
        --query "[?name=='$($keyVaultName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Key Vault $keyVaultName in resource group $resourceGroupName..."
        az keyvault create `
            --name $keyVaultName `
            --resource-group $resourceGroupName `
            --location $Location | Out-Null
        Write-Host "Key Vault '$keyVaultName' created."
    }

    Write-Host "Ensuring App Configuration $appConfigName exists in resource group '$resourceGroupName'..."
    if ((az appconfig list `
        -g $resourceGroupName `
        --query "[?name=='$($appConfigName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating App Configuration $appConfigName in resource group $resourceGroupName..."
        az appconfig create `
            --name $appConfigName `
            --resource-group $resourceGroupName `
            --location $Location | Out-Null
        Write-Host "App Configuration '$appConfigName' created."
    }

    Write-Host "Ensuring Application Insights $appInsightsName exists in resource group '$resourceGroupName'..."
    if ((az monitor app-insights component show `
        -g $resourceGroupName `
        --query "[?name=='$($appInsightsName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Application Insights $appInsightsName in resource group $resourceGroupName..."
        az monitor app-insights component create `
            --app $appInsightsName `
            --resource-group $resourceGroupName `
            --location $Location `
            --application-type web `
            --workspace $logAnalyticsName| Out-Null
        Write-Host "Application Insights '$appInsightsName' created."
    }
    
    Write-Host "Ensuring Event Grid Namespace $eventGridName exists in resource group '$resourceGroupName'..."
    if ((az eventgrid namespace list `
        -g $resourceGroupName `
        --query "[?name=='$($eventGridName)']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating Event Grid Namespace $eventGridName in resource group $resourceGroupName..."
        az eventgrid namespace create `
            --name $eventGridName `
            --resource-group $resourceGroupName `
            --location $Location | Out-Null
        Write-Host "Event Grid Namespace '$eventGridName' created."
    }

    $topics = @("resource-providers", "api-statistics")
    foreach ($topic in $topics) {
        Write-Host "Ensuring Event Grid Namespace Topic $topic exists in resource group '$resourceGroupName'..."
        if ((az eventgrid namespace topic list `
            -g $resourceGroupName `
            --namespace-name $eventGridName `
            --query "[?name=='$($topic)']" -o tsv).Count -eq 0) {

            Write-Host "Creating Event Grid Topic $topic in resource group $resourceGroupName..."
            az eventgrid namespace topic create `
                --name $topic `
                --namespace-name $eventGridName `
                --resource-group $resourceGroupName | Out-Null
            Write-Host "Event Grid Topic '$topic' created."
        }
    }
}