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
        ContextStorageAccount           = "$($UniqueName.ToLower())fllmcontextstorage"

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

function Get-EntraIDAppRegistrationURIs {

    $appRegistrationURIs = @{
        AuthorizationAPI    = "api://FoundationaLLM-Authorization"
        CoreAPI             = "api://FoundationaLLM-Core"
        ManagementAPI       = "api://FoundationaLLM-Management"
    }

    return $appRegistrationURIs
}

function Get-EntraIDAppRegistrationScopes {

    $appRegistrationScopes = @{
        CoreAPI             = "Data.Read"
        ManagementAPI       = "Data.Manage"
        AuthorizationAPI    = "api://FoundationaLLM-Authorization"
    }
    return $appRegistrationScopes
}

function Get-ConfigurationVariables {
    param (
        [string]$TenantId,
        [string]$SubscriptionId,
        [string]$InstanceId,
        [string]$UniqueName
    )

    $ResourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName
    $ResourceNames = Get-ResourceNames -UniqueName $UniqueName
    $AppRegistrationNames = Get-EntraIDAppRegistrationNames
    $AppRegistrationURIs = Get-EntraIDAppRegistrationURIs
    $AppRegistrationScopes = Get-EntraIDAppRegistrationScopes

    $configurationVariables = @{
        AZURE_COSMOS_DB_ENDPOINT = "https://$($ResourceNames.CosmosDBAccount).documents.azure.com:443/"
        AZURE_EVENT_GRID_ID = "/subscriptions/$($SubscriptionId)/resourceGroups/$($ResourceGroupNames.Core)/providers/Microsoft.EventGrid/namespaces/$($ResourceNames.EventGrid)"
        AZURE_KEY_VAULT_ENDPOINT = "https://$($ResourceNames.KeyVault).vault.azure.net/"
        AZURE_STORAGE_ACCOUNT_NAME = $ResourceNames.CoreStorageAccount
        CONTEXTAPI_KNOWLEDGESERVICE_AZURE_STORAGE_ACCOUNT_NAME = $ResourceNames.ContextStorageAccount
        ENTRA_AUTH_API_SCOPES = $AppRegistrationScopes.AuthorizationAPI
        ENTRA_MANAGEMENT_API_CLIENT_ID = ((az ad app list --query "[?displayName=='$($AppRegistrationNames.ManagementAPI)']") | ConvertFrom-Json | Select-Object -ExpandProperty appId)
        ENTRA_MANAGEMENT_API_SCOPES = $AppRegistrationScopes.ManagementAPI
        ENTRA_MANAGEMENT_API_TENANT_ID = $TenantId
        FOUNDATIONALLM_INSTANCE_ID = $InstanceId
        FOUNDATIONALLM_MANAGEMENT_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "management"
        SERVICE_AUTH_API_ENDPOINT_URL = "https://" + (az containerapp ingress show -n $ResourceNames.AuthorizationAPIContainerApp -g $ResourceGroupNames.Core --query "fqdn" -o tsv)
        SERVICE_MANAGEMENT_API_ENDPOINT_URL = "https://" + (az containerapp ingress show -n $ResourceNames.ManagementAPIContainerApp -g $ResourceGroupNames.Core --query "fqdn" -o tsv)
    }

    return $configurationVariables
}