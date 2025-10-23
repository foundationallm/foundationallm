function Get-ResourceGroupNames {
    param (
        [Parameter(Mandatory = $true)]
        [string]$UniqueName
    )

    $resourceGroupNames = @{
        Core            = "$UniqueName-core"
        Authorization   = "$UniqueName-auth"
        Context         = "$UniqueName-context"
        Data            = "$UniqueName-data"
        AIFoundry       = "$UniqueName-ai-foundry"
    }

    return $resourceGroupNames
}

function Get-ResourceNames {
    param (
        [string]$UniqueName
    )

    $resourceNames = @{
        KeyVault                        = "$UniqueName-key-vault"
        AppConfig                       = "$UniqueName-app-config"
        AppInsights                     = "$UniqueName-app-insights"
        EventGrid                       = "$UniqueName-event-grid"
        LogAnalytics                    = "$UniqueName-log-analytics"
        ContainerAppsEnvironment        = "$UniqueName-container-apps-env"
        CoreStorageAccount              = "$($UniqueName.ToLower())fllmcorestorage"
        CosmosDBAccount                 = "$UniqueName-cosmosdb"
        CosmosDBDatabase                = "database"
        AuthStorageAccount              = "$($UniqueName.ToLower())fllmauthstorage"
        AuthKeyVault                    = "$UniqueName-auth-key-vault"

        AuthorizationAPIManagedIdentity = "$UniqueName-mi-authorization-api"
        ManagementAPIManagedIdentity    = "$UniqueName-mi-management-api"

        AuthorizationAPIContainerApp    = "$UniqueName-ca-authorization-api"
        ManagementAPIContainerApp       = "$UniqueName-ca-management-api"
    }

    return $resourceNames
}

function Get-ContainerImageNames {
    param (
        [string]$ContainerRegistry,
        [string]$Version
    )

    $containerImages = @{
        AuthorizationAPI = "$ContainerRegistry/authorization-api:$Version"
        ManagementAPI    = "$ContainerRegistry/management-api:$Version"
    }

    return $containerImages
}

function Get-EntraIDAppRegistrationNames {

    $appRegistrationNames = @{
        AuthorizationAPI    = "FoundationaLLM-Authorization-API"
        CoreAPI             = "FoundationaLLM-Core-API"
        ManagementAPI       = "FoundationaLLM-Management-API"
        ManagementPortal    = "FoundationaLLM-Management-Portal"
        UserPortal          = "FoundationaLLM-User-Portal"
    }

    return $appRegistrationNames
}