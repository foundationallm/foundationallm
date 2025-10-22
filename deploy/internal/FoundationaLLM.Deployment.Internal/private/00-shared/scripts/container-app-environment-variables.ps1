function Get-AuthorizationEnvVars {
    param (
        [string]$ResourceGroupName,
        [hashtable]$ResourceNames,
        [string]$TenantId,
        [string]$ContainerAppIdentity
    )

    Write-Host "Preparing environment variables for Authorization API container app..."
    Write-Host $ResourceGroupName
    Write-Host $ResourceNames
    Write-Host $TenantId
    Write-Host $ContainerAppIdentity

    $envVarsArgs = @()
    $envVarsArgs += "--environment-variables"
    $envVarsArgs += "AZURE_CLIENT_ID=$(az identity show --name $ContainerAppIdentity --resource-group $ResourceGroupName --query clientId -o tsv)"
    $envVarsArgs += "AZURE_TENANT_ID=$TenantId"
    $envVarsArgs += "APPLICATIONINSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show --resource-group $ResourceGroupName --app $ResourceNames.AppInsights --query connectionString -o tsv)"
    $envVarsArgs += "PORT=80"
    $envVarsArgs += "FoundationaLLM_AuthorizationAPI_KeyVaultURI=https://$($ResourceNames.AuthKeyVault).vault.azure.net/"
}