#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$frontendAksName,
    [parameter(Mandatory=$false)][string]$backendAksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$namespace = "fllm",
    [parameter(Mandatory=$false)][string]$frontendHostname="www.internal.foundationallm.ai",
    [parameter(Mandatory=$false)][string]$backendHostname="api.internal.foundationallm.ai",
    [parameter(Mandatory=$false)][bool]$autoscale=$false
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

az aks get-credentials -n $backendAksName -g $resourceGroup

& ./Deploy-Backend-Images-Aks.ps1 -name $name -aksName $backendAksName -resourceGroup $resourceGroup -namespace $namespace -charts $charts

Pop-Location

Push-Location $($MyInvocation.InvocationName | Split-Path)

az aks get-credentials -n $frontendAksName -g $resourceGroup

& ./Deploy-Frontend-Images-Aks.ps1 -name $name -aksName $frontendAksName -resourceGroup $resourceGroup -namespace $namespace -charts $charts

Pop-Location
