#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$aksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$tag="latest",
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$valuesFile = "",
    [parameter(Mandatory=$false)][string]$namespace = "",
    [parameter(Mandatory=$false)][string][ValidateSet('prod','staging','none','custom', IgnoreCase=$false)]$tlsEnv = "prod",
    [parameter(Mandatory=$false)][string]$tlsHost="",
    [parameter(Mandatory=$false)][string]$tlsSecretName="tls-prod",
    [parameter(Mandatory=$false)][bool]$autoscale=$false
)

function validate {
    $valid = $true

    if ([string]::IsNullOrEmpty($aksName)) {
        Write-Host "No AKS name. Use -aksName to specify name" -ForegroundColor Red
        $valid=$false
    }
    if ([string]::IsNullOrEmpty($resourceGroup))  {
        Write-Host "No resource group. Use -resourceGroup to specify resource group." -ForegroundColor Red
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
Write-Host " Deploying images on cluster $aksName"  -ForegroundColor Yellow
Write-Host " "  -ForegroundColor Yellow
Write-Host " Additional parameters are:"  -ForegroundColor Yellow
Write-Host " Release Name: $name"  -ForegroundColor Yellow
Write-Host " AKS to use: $aksName in RG $resourceGroup"  -ForegroundColor Yellow
Write-Host " Images tag: $tag"  -ForegroundColor Yellow
Write-Host " TLS/SSL environment to enable: $tlsEnv"  -ForegroundColor Yellow
Write-Host " Namespace (empty means the one in .kube/config): $namespace"  -ForegroundColor Yellow
Write-Host " --------------------------------------------------------" 

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
    $command = "helm upgrade --install $name-agent-factory-api ./agent-factory-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("agent-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-hub-api ./agent-hub-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("core-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - core-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-api ./core-api -f ../gvalues-ingress.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("core-job") -or  $charts.Contains("*")) {
    Write-Host "Worker job chart - core-job" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-job ./core-job -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("data-source-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - data-source-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-data-source-hub-api ./data-source-hub-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-api ./gatekeeper-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-integration-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-integration-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-integration-api ./gatekeeper-integration-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("langchain-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-langchain-api ./langchain-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("prompt-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - prompt-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-prompt-hub-api ./prompt-hub-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("semantic-kernel-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - semantic-kernel-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-semantic-kernel-api ./semantic-kernel-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("vectorization-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-vectorization-api ./vectorization-api -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("vectorization-job") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-job" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-vectorization-job ./vectorization-job -f ../gvalues.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

# Write-Host " --------------------------------------------------------" 
# Write-Host "Entering holding pattern to wait for proper backend API initialization"
# Write-Host "Attempting to retrieve status from https://$($aksHost)/core/status every 20 seconds with 50 retries"
# Write-Host " --------------------------------------------------------" 
# $apiStatus = "initializing"
# $retriesLeft = 50
# while (($apiStatus.ToString() -ne "ready") -and ($retriesLeft -gt 0)) {
#     Start-Sleep -Seconds 20
    
#     try {
#         $apiStatus = Invoke-RestMethod -Uri "https://$($aksHost)/core/status" -Method GET
#     }
#     catch {
#         Write-Host "The attempt to invoke the API endpoint failed. Will retry."
#     }
#     finally {
#         Write-Host "API endpoint status: $($apiStatus)"
#     }

#     $retriesLeft -= 1
# } 

# if ($apiStatus.ToString() -ne "ready") {
#     throw "The backend API did not enter the ready state."
# }

Pop-Location
Pop-Location

Write-Host "FoundationaLLM Chat deployed on AKS" -ForegroundColor Yellow