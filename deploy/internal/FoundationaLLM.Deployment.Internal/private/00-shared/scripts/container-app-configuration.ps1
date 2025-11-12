function Get-AuthorizationEnvVars {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppIdentity
    )

    $envVarsArgs = @()
    $envVarsArgs += "AZURE_CLIENT_ID=$(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query clientId -o tsv)"
    $envVarsArgs += "AZURE_TENANT_ID=$TenantId"
    $envVarsArgs += "APPLICATIONINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)"
    $envVarsArgs += "PORT=80"
    $envVarsArgs += "FoundationaLLM_AuthorizationAPI_KeyVaultURI=https://$($ResourceNames.AuthKeyVault).vault.azure.net/"

    return $envVarsArgs
}

function Get-AuthorizationKeyVaultSecrets {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$InstanceId,
        [string]$EntraIDAppRegistrationName
    )

    $secrets = @{
        "foundationallm-authorizationapi-appinsights-connectionstring"      = (az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)
        "foundationallm-authorizationapi-entra-clientid"                    = (az ad app list --display-name $EntraIDAppRegistrationName --query "[0].appId" -o tsv)
        "foundationallm-authorizationapi-entra-instance"                    = "https://login.microsoftonline.com/"
        "foundationallm-authorizationapi-entra-tenantid"                    = $TenantId
        "foundationallm-authorizationapi-instanceids"                       = $InstanceId
        "foundationallm-authorizationapi-storage-accountname"               = $ResourceNames.AuthStorageAccount
    }

    return $secrets
}

function Get-ContainerAppSecrets {
    param (
        [hashtable]$ResourceNames
    )

    $secrets = @()
    $secrets += "appconfig-connection-string=$(az appconfig credential list --name $ResourceNames.AppConfig --query "[0].connectionString" -o tsv)"
    
    return $secrets
}

function Get-ContainerAppEnvVars {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppIdentity
    )

    $envVarsArgs = @()
    $envVarsArgs += "AZURE_CLIENT_ID=$(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query clientId -o tsv)"
    $envVarsArgs += "AZURE_TENANT_ID=$TenantId"
    $envVarsArgs += "APPLICATIONINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)"
    $envVarsArgs += "PORT=80"
    $envVarsArgs += "FoundationaLLM_AppConfig_ConnectionString=secretref:appconfig-connection-string"

    return $envVarsArgs
}

function Get-PortalEnvVars {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppIdentity
    )

    $envVarsArgs = @()
    $envVarsArgs += "AZURE_CLIENT_ID=$(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query clientId -o tsv)"
    $envVarsArgs += "AZURE_TENANT_ID=$TenantId"
    $envVarsArgs += "APPLICATIONINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)"
    $envVarsArgs += "PORT=80"
    $envVarsArgs += "NUXT_APP_CONFIG_ENDPOINT=secretref:appconfig-connection-string"

    return $envVarsArgs
}

function Get-LangChainContainerAppEnvVars {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppIdentity
    )

    $envVarsArgs = @()
    $envVarsArgs += "AZURE_CLIENT_ID=$(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query clientId -o tsv)"
    $envVarsArgs += "AZURE_TENANT_ID=$TenantId"
    $envVarsArgs += "APPLICATIONINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)"
    $envVarsArgs += "PORT=80"
    $envVarsArgs += "FOUNDATIONALLM_APP_CONFIGURATION_URI=https://$($ResourceNames.AppConfig).azconfig.io"

    return $envVarsArgs
}