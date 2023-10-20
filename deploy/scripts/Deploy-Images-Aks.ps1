#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$aksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$acrName,
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

    if ([string]::IsNullOrEmpty($aksHost) -and $tlsEnv -ne "custom")  {
        Write-Host "AKS host of HttpRouting can't be found. Are you using right AKS ($aksName) and RG ($resourceGroup)?" -ForegroundColor Red
        $valid=$false
    }     
    if ([string]::IsNullOrEmpty($acrLogin))  {
        Write-Host "ACR login server can't be found. Are you using right ACR ($acrName) and RG ($resourceGroup)?" -ForegroundColor Red
        $valid=$false
    }

    if ($tlsEnv -eq "custom" -and [string]::IsNullOrEmpty($tlsSecretName)) {
        Write-Host "If tlsEnv is custom must use -tlsSecretName to set the TLS secret name (you need to install this secret manually)"
        $valid=$false
    }

    if ($tlsEnv -eq "custom" -and [string]::IsNullOrEmpty($tlsHost)) {
        Write-Host "If tlsEnv is custom must use -tlsHost to set the hostname of AKS (inferred name of Http Application Routing won't be used)"
        $valid=$false
    }

    if ($valid -eq $false) {
        exit 1
    }
}

function createHelmCommand([string]$command) {
    $tlsSecretNameToUse = ""
    if ($tlsEnv -eq "staging") {
        $tlsSecretNameToUse = "tls-staging"
    }
    if ($tlsEnv -eq "prod") {
        $tlsSecretNameToUse = "tls-prod"
    }
    if ($tlsEnv -eq "custom") {
        $tlsSecretNameToUse=$tlsSecretName
    }	    

    $newcommand = $command

    if (-not [string]::IsNullOrEmpty($namespace)) {
        $newcommand = "$newcommand --namespace $namespace" 
    }

    if (-not [string]::IsNullOrEmpty($tlsSecretNameToUse)) {
        $newcommand = "$newcommand --set ingress.tls[0].secretName=$tlsSecretNameToUse --set ingress.tls[0].hosts='{$aksHost}'"
    }

    return "$newcommand";
}

Write-Host "--------------------------------------------------------" -ForegroundColor Yellow
Write-Host " Deploying images on cluster $aksName"  -ForegroundColor Yellow
Write-Host " "  -ForegroundColor Yellow
Write-Host " Additional parameters are:"  -ForegroundColor Yellow
Write-Host " Release Name: $name"  -ForegroundColor Yellow
Write-Host " AKS to use: $aksName in RG $resourceGroup and ACR $acrName"  -ForegroundColor Yellow
Write-Host " Images tag: $tag"  -ForegroundColor Yellow
Write-Host " TLS/SSL environment to enable: $tlsEnv"  -ForegroundColor Yellow
Write-Host " Namespace (empty means the one in .kube/config): $namespace"  -ForegroundColor Yellow
Write-Host " --------------------------------------------------------" 

$acrLogin=$(az acr show -n $acrName -g $resourceGroup -o json| ConvertFrom-Json).loginServer

if ($tlsEnv -ne "custom" -and [String]::IsNullOrEmpty($tlsHost)) {
    $aksHost=$(az aks show -n $aksName -g $resourceGroup --query addonProfiles.httpapplicationrouting.config.HTTPApplicationRoutingZoneName -o json | ConvertFrom-Json)

    if (-not $aksHost) {
        $aksHost=$(az aks show -n $aksName -g $resourceGroup --query addonProfiles.httpApplicationRouting.config.HTTPApplicationRoutingZoneName -o json | ConvertFrom-Json)
    }

    Write-Host "acr login server is $acrLogin" -ForegroundColor Yellow
    Write-Host "aksHost is $aksHost" -ForegroundColor Yellow
}
else {
    $aksHost=$tlsHost
}

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
    $command = "helm upgrade --install $name-agent-factory-api ./agent-factory-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/agent-factory-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("agent-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-hub-api ./agent-hub-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/agent-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("core-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - core-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-api ./core-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/core-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("data-source-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - data-source-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-data-source-hub-api ./data-source-hub-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/data-source-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-api ./gatekeeper-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/gatekeeper-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("langchain-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-langchain-api ./langchain-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/langchain-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("prompt-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - prompt-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-prompt-hub-api ./prompt-hub-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/prompt-hub-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("semantic-kernel-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - semantic-kernel-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-semantic-kernel-api ./semantic-kernel-api -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/semantic-kernel-api --set image.tag=$tag --set hpa.activated=$autoscale"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("chat-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - web" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-web ./chat-ui -f $valuesFile --set ingress.hosts='{$aksHost}' --set image.repository=$acrLogin/chat-ui --set image.tag=$tag  --set hpa.activated=$autoscale"
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