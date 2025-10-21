function Get-ResourceNames {
    param (
        [string]$UniqueName
    )

    $resourceNames = @{
        KeyVault                = "$UniqueName-key-vault"
        AppConfig               = "$UniqueName-app-config"
        AppInsights             = "$UniqueName-app-insights"
        EventGrid               = "$UniqueName-event-grid"
        LogAnalytics             = "$UniqueName-log-analytics"
        ContainerAppsEnvironment = "$UniqueName-container-apps-env"
        CoreStorageAccount      = "$($UniqueName.ToLower())fllmcorestorage"
        CosmosDBAccount         = "$UniqueName-cosmosdb"
        CosmosDBDatabase        = "database"
    }

    return $resourceNames
}