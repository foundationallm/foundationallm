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
        KeyVault                            = "$UniqueName-key-vault"
        AppConfig                           = "$UniqueName-app-config"
        AppInsights                         = "$UniqueName-app-insights"
        EventGrid                           = "$UniqueName-event-grid"
        LogAnalytics                        = "$UniqueName-log-analytics"
        ContainerAppsEnvironment            = "$UniqueName-container-apps-env"
        CoreStorageAccount                  = "$($UniqueName.ToLower())core"
        CosmosDBAccount                     = "$UniqueName-cosmosdb"
        CosmosDBDatabase                    = "database"
        AuthStorageAccount                  = "$($UniqueName.ToLower())auth"
        AuthKeyVault                        = "$UniqueName-auth-key-vault"
        ContextStorageAccount               = "$($UniqueName.ToLower())context"
        AIFoundry                           = "$UniqueName-ai-foundry"
        AISearch                            = "$UniqueName-ai-search"
        CodeContainerAppsEnvironment        = "$UniqueName-code-container-apps-env"
        PythonCodeContainerSessionPool      = "$UniqueName-python-code-container"

        AuthorizationAPIManagedIdentity     = "$UniqueName-mi-authorization-api"
        ManagementAPIManagedIdentity        = "$UniqueName-mi-management-api"
        ManagementPortalManagedIdentity     = "$UniqueName-mi-management-portal"
        CoreAPIManagedIdentity              = "$UniqueName-mi-core-api"
        CoreWorkerManagedIdentity           = "$UniqueName-mi-core-worker"
        UserPortalManagedIdentity           = "$UniqueName-mi-user-portal"
        OrchestrationAPIManagedIdentity     = "$UniqueName-mi-orchestration-api"
        LangChainAPIManagedIdentity         = "$UniqueName-mi-langchain-api"
        GatewayAPIManagedIdentity           = "$UniqueName-mi-gateway-api"
        StateAPIManagedIdentity             = "$UniqueName-mi-state-api"
        ContextAPIManagedIdentity           = "$UniqueName-mi-context-api"

        AuthorizationAPIContainerApp        = "$UniqueName-ca-authorization-api"
        ManagementAPIContainerApp           = "$UniqueName-ca-management-api"
        ManagementPortalContainerApp        = "$UniqueName-ca-management-portal"
        CoreAPIContainerApp                 = "$UniqueName-ca-core-api"
        CoreWorkerContainerApp              = "$UniqueName-ca-core-worker"
        UserPortalContainerApp              = "$UniqueName-ca-user-portal"
        OrchestrationAPIContainerApp        = "$UniqueName-ca-orchestration-api"
        LangChainAPIContainerApp            = "$UniqueName-ca-langchain-api"
        GatewayAPIContainerApp              = "$UniqueName-ca-gateway-api"
        StateAPIContainerApp                = "$UniqueName-ca-state-api"
        ContextAPIContainerApp              = "$UniqueName-ca-context-api"
    }

    return $resourceNames
}

function Get-ContainerImageNames {
    param (
        [string]$ContainerRegistry,
        [string]$Version
    )

    $containerImages = @{
        AuthorizationAPI                    = "$ContainerRegistry/authorization-api:$Version"
        ManagementAPI                       = "$ContainerRegistry/management-api:$Version"
        ManagementPortal                    = "$ContainerRegistry/management-ui:$Version"
        CoreAPI                             = "$ContainerRegistry/core-api:$Version"
        CoreWorker                          = "$ContainerRegistry/core-job:$Version"
        UserPortal                          = "$ContainerRegistry/chat-ui:$Version"
        OrchestrationAPI                    = "$ContainerRegistry/orchestration-api:$Version"
        LangChainAPI                        = "$ContainerRegistry/langchain-api:$Version"
        GatewayAPI                          = "$ContainerRegistry/gateway-api:$Version"
        StateAPI                            = "$ContainerRegistry/state-api:$Version"
        ContextAPI                          = "$ContainerRegistry/context-api:$Version"

        PythonCodeSessionAPI                = "$ContainerRegistry/python-codesession-api:$Version"
    }

    return $containerImages
}

function Get-EntraIDAppRegistrationNames {

    $appRegistrationNames = @{
        AuthorizationAPI                    = "FoundationaLLM-Authorization-API"
        CoreAPI                             = "FoundationaLLM-Core-API"
        ManagementAPI                       = "FoundationaLLM-Management-API"
        ManagementPortal                    = "FoundationaLLM-Management-Portal"
        UserPortal                          = "FoundationaLLM-User-Portal"
    }

    return $appRegistrationNames
}

function Get-EntraIDAppRegistrationURIs {

    $appRegistrationURIs = @{
        AuthorizationAPI                    = "api://FoundationaLLM-Authorization"
        CoreAPI                             = "api://FoundationaLLM-Core"
        ManagementAPI                       = "api://FoundationaLLM-Management"
    }

    return $appRegistrationURIs
}

function Get-EntraIDAppRegistrationScopes {

    $appRegistrationScopes = @{
        CoreAPI                             = "Data.Read"
        ManagementAPI                       = "Data.Manage"
        AuthorizationAPI                    = "api://FoundationaLLM-Authorization"
        ManagementPortal                    = "api://FoundationaLLM-Management/Data.Manage"
        UserPortal                          = "api://FoundationaLLM-Core/Data.Read"
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

    $managementAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.ManagementAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
    $coreAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.CoreAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
    $stateAPIEndpointURL = "https://" + (az containerapp ingress show -n $resourceNames.StateAPIContainerApp -g $resourceGroupNames.Core --query "fqdn" -o tsv)
    $pythonCustomContainerEndpointURL = (az containerapp sessionpool show --name $resourceNames.PythonCodeContainerSessionPool --resource-group $resourceGroupNames.Context --query "properties.poolManagementEndpoint" -o tsv)

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
        SERVICE_MANAGEMENT_API_ENDPOINT_URL = $managementAPIEndpointURL

        CONTEXTAPI_API_KEY = New-APIKey
        DATAPIPELINEAPI_API_KEY = New-APIKey
        GATEWAYAPI_API_KEY = New-APIKey

        # -----------------------------------------------------------
        # Management Portal
        # -----------------------------------------------------------
        ENTRA_MANAGEMENT_UI_TENANT_ID = $TenantId
        ENTRA_MANAGEMENT_UI_SCOPES = $appRegistrationScopes.ManagementPortal
        ENTRA_MANAGEMENT_UI_CLIENT_ID = ((az ad app list --query "[?displayName=='$($appRegistrationNames.ManagementPortal)']") | ConvertFrom-Json | Select-Object -ExpandProperty appId)

        # -----------------------------------------------------------
        # Core API
        # -----------------------------------------------------------
        ENTRA_CORE_API_TENANT_ID = $TenantId
        ENTRA_CORE_API_CLIENT_ID = ((az ad app list --query "[?displayName=='$($appRegistrationNames.CoreAPI)']") | ConvertFrom-Json | Select-Object -ExpandProperty appId)
        SERVICE_CORE_API_ENDPOINT_URL = $coreAPIEndpointURL
        FOUNDATIONALLM_CORE_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile-2.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "core"

        ORCHESTRATIONAPI_API_KEY = New-APIKey
        GATEKEEPERAPI_API_KEY = New-APIKey

        # -----------------------------------------------------------
        # Core Worker
        # -----------------------------------------------------------
        COREWORKER_API_KEY = New-APIKey

        # -----------------------------------------------------------
        # User Portal
        # -----------------------------------------------------------
        ENTRA_CHAT_UI_TENANT_ID = $TenantId
        ENTRA_CHAT_UI_CLIENT_ID = ((az ad app list --query "[?displayName=='$($appRegistrationNames.UserPortal)']") | ConvertFrom-Json | Select-Object -ExpandProperty appId)
        ENTRA_CHAT_UI_SCOPES = $appRegistrationScopes.UserPortal

        #------------------------------------------------------------
        # Gateway API
        #------------------------------------------------------------
        AZURE_OPENAI_ID = ((az cognitiveservices account show -g $resourceGroupNames.AIFoundry -n $resourceNames.AIFoundry) | ConvertFrom-Json | Select-Object -ExpandProperty id)
        AZUREOPENAI_API_KEY = (az cognitiveservices account keys list -g $resourceGroupNames.AIFoundry -n $resourceNames.AIFoundry --query "key1" -o tsv)
        AZURECONTENTSAFETY_API_KEY = (az cognitiveservices account keys list -g $resourceGroupNames.AIFoundry -n $resourceNames.AIFoundry --query "key1" -o tsv)
        FOUNDATIONALLM_GATEWAY_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile-empty.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "gateway"
        
        # -----------------------------------------------------------
        # Orchestration API
        #------------------------------------------------------------
        GATEKEEPERINTEGRATIONAPI_API_KEY = New-APIKey
        GATEWAYADAPTER_API_KEY = New-APIKey
        LANGCHAINAPI_API_KEY = New-APIKey
        STATEAPI_API_KEY = New-APIKey
        SEMANTICKERNELAPI_API_KEY = New-APIKey
        DATAPIPELINEAPI_AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.ContextStorageAccount
        DATAPIPELINEFRONTENDWORKER_API_KEY = New-APIKey
        DATAPIPELINEFRONTENDWORKER_AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.ContextStorageAccount
        DATAPIPELINEBACKENDWORKER_API_KEY = New-APIKey
        DATAPIPELINEBACKENDWORKER_AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.ContextStorageAccount
        CONTEXTAPI_FILESERVICE_AZURE_STORAGE_ACCOUNT_NAME = $resourceNames.ContextStorageAccount
        CONTEXTAPI_KNOWLEDGESERVICE_EMBEDDING = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/context-knowledge-service-embedding.json") `
            -replace "{{FOUNDATIONALLM_INSTANCE_ID}}", $InstanceId
        SERVICE_STATE_API_ENDPOINT_URL = $stateAPIEndpointURL
        FOUNDATIONALLM_ORCHESTRATION_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "orch"

        #------------------------------------------------------------
        # Context API
        #------------------------------------------------------------
        CONTEXTAPI_CODEEXECUTION_CUSTOMCONTAINER = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/context-code-execution-custom-container.json") `
            -replace "{{PYTHON_CUSTOM_CONTAINER_ENDPOINT}}", $pythonCustomContainerEndpointURL
        FOUNDATIONALLM_CONTEXT_API_EVENT_GRID_PROFILE = (Get-Content -Raw -LiteralPath "$PSScriptRoot/../data/event-grid-profile.json") `
            -replace "{{SUBSCRIPTION_PREFIX}}", "context"
    }

    return $configurationVariables
}
