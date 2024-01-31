#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$aksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$namespace = "fllm",
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
Write-Host " Namespace (empty means the one in .kube/config): $namespace"  -ForegroundColor Yellow
Write-Host " --------------------------------------------------------"

validate

Push-Location $($MyInvocation.InvocationName | Split-Path)

Write-Host "Deploying charts $charts" -ForegroundColor Yellow

Write-Host "Configuration file used is $valuesFile" -ForegroundColor Yellow

if ($charts.Contains("chat-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - web" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-web oci://ghcr.io/solliancenet/foundationallm/helm/chat-ui --values ../values/chatui-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("management-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - management" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-management oci://ghcr.io/solliancenet/foundationallm/helm/management-ui --values ../values/managementui-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

Pop-Location

Write-Host "FoundationaLLM frontend services deployed on AKS" -ForegroundColor Yellow