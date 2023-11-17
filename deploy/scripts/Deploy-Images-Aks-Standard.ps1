#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$frontendAksName,
    [parameter(Mandatory=$false)][string]$backendAksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$acrName,
    [parameter(Mandatory=$false)][string]$acrRg,
    [parameter(Mandatory=$false)][string]$tag="latest",
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$valuesFile = "",
    [parameter(Mandatory=$false)][string]$namespace = "",
    [parameter(Mandatory=$false)][string]$frontendHostname="www.internal.foundationallm.ai",
    [parameter(Mandatory=$false)][string]$backendHostname="api.internal.foundationallm.ai",
    [parameter(Mandatory=$false)][bool]$autoscale=$false
)

function validate {
    $valid = $true

    if ([string]::IsNullOrEmpty($frontendAksName)) {
        Write-Host "No frontend AKS name. Use -frontendAksName to specify name" -ForegroundColor Red
        $valid=$false
    }
    if ([string]::IsNullOrEmpty($backendAksName)) {
        Write-Host "No backend AKS name. Use -backendAksName to specify name" -ForegroundColor Red
        $valid=$false
    }
    if ([string]::IsNullOrEmpty($resourceGroup))  {
        Write-Host "No resource group. Use -resourceGroup to specify resource group." -ForegroundColor Red
        $valid=$false
    }

    if ([string]::IsNullOrEmpty($acrLogin))  {
        Write-Host "ACR login server can't be found. Are you using right ACR ($acrName) and RG ($resourceGroup)?" -ForegroundColor Red
        $valid=$false
    }

    if ($valid -eq $false) {
        exit 1
    }
}

function createHelmCommand([string]$command) {   

    $newcommand = $command

    if (-not [string]::IsNullOrEmpty($namespace)) {
        $newcommand = "$newcommand --namespace $namespace" 
    }

    return "$newcommand";
}

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host " Deploying images on cluster $backendAksName"  -ForegroundColor Yellow
Write-Host " "  -ForegroundColor Yellow
Write-Host " Additional parameters are:"  -ForegroundColor Yellow
Write-Host " Release Name: $name"  -ForegroundColor Yellow
Write-Host " AKS to use: $backendAksName in RG $resourceGroup and ACR $acrName"  -ForegroundColor Yellow
Write-Host " Images tag: $tag"  -ForegroundColor Yellow
Write-Host " Namespace (empty means the one in .kube/config): $namespace"  -ForegroundColor Yellow
Write-Host " --------------------------------------------------------" 

az aks get-credentials -n $backendAksName -g $resourceGroup

$acrLogin=$(az acr show -n $acrName -g $acrRg -o json| ConvertFrom-Json).loginServer
Write-Host "acr login server is $acrLogin" -ForegroundColor Yellow

validate

Push-Location $($MyInvocation.InvocationName | Split-Path)
Push-Location $(Join-Path .. helm)

Write-Host "Deploying charts $charts" -ForegroundColor Yellow

if ([String]::IsNullOrEmpty($valuesFile)) {
    $valuesFile="gvalues.yaml"
}

Write-Host "Configuration file used is $valuesFile" -ForegroundColor Yellow

if ($charts.Contains("agent-factory-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-factory-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-factory-api ./agent-factory-api -f ./agent-factory-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/agent-factory-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("agent-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-hub-api ./agent-hub-api -f ./agent-hub-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/agent-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("core-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - core-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-api ./core-api -f ./core-api/standard-values.yaml -f $valuesFile --set ingress.hosts='{$backendHostname}' --set image.repository=$acrLogin/core-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("core-job") -or  $charts.Contains("*")) {
    Write-Host "Job chart - core-job" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-job ./core-job -f $valuesFile --set image.repository=$acrLogin/core-job --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("data-source-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - data-source-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-data-source-hub-api ./data-source-hub-api -f ./data-source-hub-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/data-source-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-api ./gatekeeper-api -f ./gatekeeper-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/gatekeeper-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("langchain-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-langchain-api ./langchain-api -f ./langchain-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/langchain-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("prompt-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - prompt-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-prompt-hub-api ./prompt-hub-api -f ./prompt-hub-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/prompt-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("semantic-kernel-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - semantic-kernel-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-semantic-kernel-api ./semantic-kernel-api -f ./semantic-kernel-api/standard-values.yaml -f $valuesFile --set image.repository=$acrLogin/semantic-kernel-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host " Deploying images on cluster $frontendAksName"  -ForegroundColor Yellow
Write-Host " "  -ForegroundColor Yellow
Write-Host " Additional parameters are:"  -ForegroundColor Yellow
Write-Host " Release Name: $name"  -ForegroundColor Yellow
Write-Host " AKS to use: $frontendAksName in RG $resourceGroup and ACR $acrName"  -ForegroundColor Yellow
Write-Host " Images tag: $tag"  -ForegroundColor Yellow
Write-Host " Namespace (empty means the one in .kube/config): $namespace"  -ForegroundColor Yellow
Write-Host " --------------------------------------------------------" 

az aks get-credentials -n $frontendAksName -g $resourceGroup

if ($charts.Contains("chat-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - web" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-web ./chat-ui -f ./chat-ui/standard-values.yaml -f $valuesFile --set ingress.hosts='{$frontendHostname}' --set image.repository=$acrLogin/chat-ui --set image.tag=$tag  --set hpa.activated=$autoscale"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

Pop-Location
Pop-Location

Write-Host "FoundationaLLM Chat deployed on AKS" -ForegroundColor Yellow