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

function New-APIKey {

    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Get-ConfigurationVariables {
    param (
        [string]$TenantId,
        [string]$SubscriptionId,
        [string]$InstanceId,
        [string]$UniqueName
    )

    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName
    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $appRegistrationNames = Get-EntraIDAppRegistrationNames
    $appRegistrationScopes = Get-EntraIDAppRegistrationScopes

    $configurationVariables = @{

        # -----------------------------------------------------------
        # Management API
        # -----------------------------------------------------------

        APP_INSIGHTS_CONNECTION_STRING = (az monitor app-insights component show --app $resourceNames.AppInsights --resource-group $resourceGroupNames.Core --query "connectionString" -o tsv)
        AZURE_COSMOS_DB_ENDPOINT = "https://$($resourceNames.CosmosDBAccount).documents.azure.com:443/"
        AZURE_EVENT_GRID_ID = "/subscriptions/$($SubscriptionId)/resourceGroups/$($resourceGroupNames.Core)/providers/Microsoft.EventGrid/namespaces/$($resourceNames.EventGrid)"
        AZUREEVENTGRID_API_KEY = (az eventgrid namespace list-key -g $resourceGroupNames.Core --namespace-name $resourceNames.EventGrid --query "key1" -o tsv)
        AZURE_KEY_VAULT_ENDPOINT = "https://$($resourceNames.KeyVault).vault.azure.net/"
        AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.CoreStorageAccount
        CONTEXTAPI_KNOWLEDGESERVICE_AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.ContextStorageAccount
        ENTRA_AUTH_API_SCOPES = $appRegistrationScopes.AuthorizationAPI
        ENTRA_MANAGEMENT_API_CLIENT_ID = ((az ad app list --query "[?displayName=='$($appRegistrationNames.ManagementAPI)']") | ConvertFrom-Json | Select-Object -ExpandProperty appId)
        ENTRA_MANAGEMENT_API_SCOPES = $appRegistrationScopes.ManagementAPI
        ENTRA_MANAGEMENT_API_TENANT_ID = $TenantId
        FOUNDATIONALLM_INSTANCE_ID = $InstanceId
        FOUNDATIONALLM_MANAGEMENT_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "management"
        SERVICE_AUTH_API_ENDPOINT_URL = "https://" + (az containerapp ingress show -n $resourceNames.AuthorizationAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
        SERVICE_MANAGEMENT_API_ENDPOINT_URL = "https://" + (az containerapp ingress show -n $resourceNames.ManagementAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)

        CONTEXTAPI_API_KEY = New-APIKey
        DATAPIPELINEAPI_API_KEY = New-APIKey
        GATEWAYAPI_API_KEY = New-APIKey
    }

    return $configurationVariables
}

