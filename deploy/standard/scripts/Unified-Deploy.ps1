#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$stepDeployCerts = $false,
    [parameter(Mandatory = $false)][bool]$stepDeployImages = $false,
    [parameter(Mandatory = $false)][bool]$stepUploadSystemPrompts = $false,
    [parameter(Mandatory = $false)][bool]$stepLoginAzure = $false
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

Push-Location $($MyInvocation.InvocationName | Split-Path)

# Update the extension to make sure you have the latest version installed
az extension add --name aks-preview
az extension update --name aks-preview

az extension add --name  application-insights
az extension update --name  application-insights

az extension add --name storage-preview
az extension update --name storage-preview

if ($stepLoginAzure) {
    # Write-Host "Login in your account" -ForegroundColor Yellow
    az login
}

$project = "wtw01"
$environment = "stg"
$location = "eastus2"

$resourceGroups = @{
    agw     = "EBTICP-D-NA24-AIAGW-RGRP"
    app     = "EBTICP-D-NA24-AIApp-RGRP"
    data    = "EBTICP-D-NA24-AIData-RGRP"
    dns     = "EBTICP-D-NA24-AIDNS-RGRP"
    jbx     = "EBTICP-D-NA24-AIJBX-RGRP"
    net     = "EBTICP-D-NA24-AI-RGRP"
    oai     = "EBTICP-D-NA24-AIOpenAI-RGRP"
    ops     = "EBTICP-D-NA24-AIOps-RGRP"
    storage = "EBTICP-D-NA24-AIStorage-RGRP"
    vec     = "EBTICP-D-NA24-AIVector-RGRP"
}

$domains = @{
    chatui = "www.internal.foundationallm.ai"
    coreapi = "api.internal.foundationallm.ai"
    managementapi = "management-api.internal.foundationallm.ai"
    managementui = "management.internal.foundationallm.ai"
}

$entraClientIds = @{
    chat = "0b08d115-b517-4d7f-b883-a1665191d14d"
    core = "b7bfdfd8-fd88-4bec-a6db-7fd1ecac40db"
    managementui = "aa5cba99-e753-4d91-b2f8-85a6b650d022"
    managementapi = "dc4b7d98-e404-4044-8040-4c7a5551e862"
    vectorizationapi = ""
}

if ($stepUploadSystemPrompts) {
    # Upload System Prompts
    #& ./UploadSystemPrompts.ps1 -resourceGroup $resourceGroup -location $location
}

# Generate Config
& ./Generate-Config.ps1 `
    -entraClientIds $entraClientIds `
    -resourceGroups $resourceGroups `
    -resourceSuffix "$project-$environment-$location" `
    -domains $domains
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generating config" -ForegroundColor Red
    exit $LASTEXITCODE
}

if ($stepDeployCerts) {
    # TODO Deploy Certs to AGWs
}

if ($stepDeployImages) {
    # Deploy images in AKS
    $chartsToDeploy = "*"

    #& ./Deploy-Images-Aks-Standard.ps1 -aksName $aksName -resourceGroup $resourceGroup -charts $chartsToDeploy
}

# Write-Host "===========================================================" -ForegroundColor Yellow
# Write-Host "The frontend is hosted at https://$webappHostname" -ForegroundColor Yellow
# Write-Host "The Core API is hosted at $coreApiUri" -ForegroundColor Yellow
# Write-Host "===========================================================" -ForegroundColor Yellow

Pop-Location
