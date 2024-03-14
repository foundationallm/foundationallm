#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$aksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$namespace = "fllm",
    [parameter(Mandatory=$false)][bool]$autoscale=$false,
    [parameter(Mandatory=$false)][string]$version="0.5.0"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}

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
        $newcommand = "$newcommand --namespace $namespace --create-namespace"
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

if ($charts.Contains("chat-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - web" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-web oci://ghcr.io/solliancenet/foundationallm/helm/chat-ui --values ../values/chatui-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying chat-ui" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("management-ui") -or  $charts.Contains("*")) {
    Write-Host "Webapp chart - management" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-management oci://ghcr.io/solliancenet/foundationallm/helm/management-ui --values ../values/managementui-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying management-ui" {
        Invoke-Expression "$command"
    }
}

Pop-Location

Write-Host "FoundationaLLM frontend services deployed on AKS" -ForegroundColor Yellow