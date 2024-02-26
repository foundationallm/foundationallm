#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$stepDeployCerts = $false,
    [parameter(Mandatory = $false)][bool]$stepDeployImages = $false,
    [parameter(Mandatory = $false)][bool]$stepUploadSystemPrompts = $false,
    [parameter(Mandatory = $false)][bool]$stepLoginAzure = $false,
    [parameter(Mandatory = $false)][bool]$init = $true
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed: ${message}" -ForegroundColor Red
        exit $LASTEXITCODE
    }

    return $result
}

Push-Location $($MyInvocation.InvocationName | Split-Path)

if ($init) {
    # Update the extension to make sure you have the latest version installed
    az extension add --name aks-preview
    az extension update --name aks-preview

    az extension add --name  application-insights
    az extension update --name  application-insights

    az extension add --name storage-preview
    az extension update --name storage-preview
}

if ($stepLoginAzure) {
    # Write-Host "Login in your account" -ForegroundColor Yellow
    az login
}

$manifest = $(Get-Content -Raw -Path ../Deployment-Manifest.json | ConvertFrom-Json)

$instanceId = $manifest.instanceId
$ingress = $manifest.ingress
$entraClientIds = $manifest.entraClientIds
$environment = $manifest.environment
$location = $manifest.location
$project = $manifest.project
$resourceGroups = $manifest.resourceGroups

if ($stepUploadSystemPrompts) {
    # Upload System Prompts
    #& ./UploadSystemPrompts.ps1 -resourceGroup $resourceGroup -location $location
}

Write-Host "Generate Configuration" -ForegroundColor Blue
Invoke-AndRequireSuccess "Generate Configuration" {
    & ./Generate-Config.ps1 `
        -instanceId $instanceId `
        -entraClientIds $entraClientIds `
        -resourceGroups $resourceGroups `
        -subscriptionId $manifest.subscription `
        -resourceSuffix "$project-$environment-$location" `
        -ingress $ingress
}

Write-Host "Generate Host File" -ForegroundColor Blue
Invoke-AndRequireSuccess "Generate Host File" {
    & ./Generate-Hosts.ps1 -subscription $manifest.subscription
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
