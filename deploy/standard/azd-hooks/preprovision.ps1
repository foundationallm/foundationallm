#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Load-Utility-Functions.ps1
}
finally {
    Pop-Location
}

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Invoke-AndRequireSuccess "Download AzCopy for the FoundationaLLM solution" {
        Push-Location ../../common/scripts
        ./Get-AzCopy.ps1
        Pop-Location
    }
}
finally {
    Pop-Location
}

Write-Host "Fetching FLLM Version..." -ForegroundColor Blue
$fllmVersionConfigPath = "./config/version.json"
$fllmVersionConfig = (Get-content $fllmVersionConfigPath | ConvertFrom-Json)

Write-Host "Setting FLLM Version to $($fllmVersionConfig.version)..." -ForegroundColor Blue
azd env set FLLM_VERSION "$($fllmVersionConfig.version)"

# Check for deployment service principal credentials
$credentials = Get-Service-Principal-Credentials ./config credentials

# Authenticating with Azure
az login --service-principal `
    --username $credentials.clientId `
    --password $credentials.clientSecret `
    --tenant $credentials.tenantId

az account set --subscription $credentials.subscriptionId

# Authenticating with AZD
azd auth login `
    --client-id $credentials.clientId `
    --client-secret $credentials.clientSecret `
    --tenant-id $credentials.tenantId

# Authenticating with AzCopy
$env:AZCOPY_SPA_CLIENT_SECRET="$($credentials.clientSecret)"
../common/tools/azcopy/azcopy login `
    --application-id $credentials.clientId `
    --tenant-id $credentials.tenantId `
    --login-type=spn

Get-ProjectId
Get-Location
Get-ResourceGroups
Get-Entra-Config
Get-Dns-Resource-Config
Get-Hub-Resource-Config
Get-Network-Config
Get-Hostname-Config
Get-FllmAksNodePoolSkus
if (Get-Escrow-Config)
{
    Escrow-FoundationaLLM-Images -version $fllmVersionConfig.version
    Escrow-FoundationaLLM-Helm-Charts -version $fllmVersionConfig.version
    Escrow-FoundationaLLM-Dependencies
}

$fllmProject = $(azd env get-value FOUNDATIONALLM_PROJECT)

$readerClientId = $(azd env get-value ENTRA_READER_CLIENT_ID)
if ($LastExitCode -eq 0)
{
    Write-Host "Setting FoundationaLLM Reader Role"
    az role assignment create --assignee $($readerClientId) --role Reader --scope /subscriptions/$($env:AZURE_SUBSCRIPTION_ID)

    $pal = $(azd env get-value FOUNDATIONALLM_PAL)
    if ($LastExitCode -eq 0) {
        az extension add --name managementpartner
        az managementpartner update --partner-id $pal
    }
}

$instanceId = $(azd env get-value FOUNDATIONALLM_INSTANCE_ID)
if ($LastExitCode -eq 0) 
{
    # TODO: Validate that it is a proper GUID
}
else
{
    $instanceId = $((New-Guid).Guid)
    azd env set FOUNDATIONALLM_INSTANCE_ID $instanceId
}

Write-Host "FoundationaLLM Instance ID: $($instanceId)"

Write-Host "Getting Current User UPN..." -ForegroundColor Blue
$upn = $(az account show --query user.name --output tsv)
azd env set FOUNDATIONALLM_OWNER $upn

$adminGroupObjectId = $(azd env get-value ADMIN_GROUP_OBJECT_ID)
$userObjectId = $(az ad sp list --filter "appId eq '$($credentials.clientId)'" --query "[].id" --output tsv)
az ad group member add --group $adminGroupObjectId --member-id $userObjectId
