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

if ($charts.Contains("agent-factory-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-factory-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-agent-factory-api oci://ghcr.io/solliancenet/foundationallm/helm/agent-factory-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying agent-factory-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("agent-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-agent-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/agent-hub-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying agent-hub-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("authorization-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - authorization-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-authorization-api oci://ghcr.io/solliancenet/foundationallm/helm/authorization-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying authorization-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("core-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - core-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-core-api oci://ghcr.io/solliancenet/foundationallm/helm/core-api -f ../values/coreapi-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying core-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("core-job") -or  $charts.Contains("*")) {
    Write-Host "Worker job chart - core-job" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-core-job oci://ghcr.io/solliancenet/foundationallm/helm/core-job -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying core-job" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("data-source-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - data-source-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-data-source-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/data-source-hub-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying data-source-hub-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("gatekeeper-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-gatekeeper-api oci://ghcr.io/solliancenet/foundationallm/helm/gatekeeper-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying gatekeeper-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("gatekeeper-integration-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-integration-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-gatekeeper-integration-api oci://ghcr.io/solliancenet/foundationallm/helm/gatekeeper-integration-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying gatekeeper-integration-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("langchain-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-langchain-api oci://ghcr.io/solliancenet/foundationallm/helm/langchain-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying langchain-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("management-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - management-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-management-api oci://ghcr.io/solliancenet/foundationallm/helm/management-api -f ../values/managementapi-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying management-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("prompt-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - prompt-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-prompt-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/prompt-hub-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying prompt-hub-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("semantic-kernel-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - semantic-kernel-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-semantic-kernel-api oci://ghcr.io/solliancenet/foundationallm/helm/semantic-kernel-api -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying semantic-kernel-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("vectorization-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-api" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-vectorization-api oci://ghcr.io/solliancenet/foundationallm/helm/vectorization-api -f ../values/vectorizationapi-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying vectorization-api" {
        Invoke-Expression "$command"
    }
}

if ($charts.Contains("vectorization-job") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-job" -ForegroundColor Yellow
    $command = "helm upgrade --version $version --install $name-vectorization-job oci://ghcr.io/solliancenet/foundationallm/helm/vectorization-job -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-AndRequireSuccess "Deploying vectorization-job" {
        Invoke-Expression "$command"
    }
}

Pop-Location

Write-Host "FoundationaLLM backend services deployed on AKS" -ForegroundColor Yellow