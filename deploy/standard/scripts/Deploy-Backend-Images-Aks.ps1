#! /usr/bin/pwsh

Param(
    [parameter(Mandatory=$false)][string]$name = "foundationallm",
    [parameter(Mandatory=$false)][string]$aksName,
    [parameter(Mandatory=$false)][string]$resourceGroup,
    [parameter(Mandatory=$false)][string]$charts = "*",
    [parameter(Mandatory=$false)][string]$namespace = "fllm",
    [parameter(Mandatory=$true)][string]$version
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

if ($charts.Contains("agent-factory-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-factory-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-factory-api oci://ghcr.io/solliancenet/foundationallm/helm/agent-factory-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("agent-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - agent-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-agent-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/agent-hub-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("authorization-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - authorization-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-authorization-api oci://ghcr.io/solliancenet/foundationallm/helm/authorization-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("core-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - core-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-api oci://ghcr.io/solliancenet/foundationallm/helm/core-api --version $version -f ../values/coreapi-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("core-job") -or  $charts.Contains("*")) {
    Write-Host "Worker job chart - core-job" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-core-job oci://ghcr.io/solliancenet/foundationallm/helm/core-job --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("data-source-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - data-source-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-data-source-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/data-source-hub-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-api oci://ghcr.io/solliancenet/foundationallm/helm/gatekeeper-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("gatekeeper-integration-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-integration-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-gatekeeper-integration-api oci://ghcr.io/solliancenet/foundationallm/helm/gatekeeper-integration-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("langchain-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - gatekeeper-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-langchain-api oci://ghcr.io/solliancenet/foundationallm/helm/langchain-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("management-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - management-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-management-api oci://ghcr.io/solliancenet/foundationallm/helm/management-api --version $version -f ../values/managementapi-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("prompt-hub-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - prompt-hub-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-prompt-hub-api oci://ghcr.io/solliancenet/foundationallm/helm/prompt-hub-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("semantic-kernel-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - semantic-kernel-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-semantic-kernel-api oci://ghcr.io/solliancenet/foundationallm/helm/semantic-kernel-api --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

if ($charts.Contains("vectorization-api") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-api" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-vectorization-api oci://ghcr.io/solliancenet/foundationallm/helm/vectorization-api --version $version -f ../values/vectorizationapi-values.yml"
    $command = createHelmCommand $command 
    Invoke-Expression "$command"
}

if ($charts.Contains("vectorization-job") -or  $charts.Contains("*")) {
    Write-Host "API chart - vectorization-job" -ForegroundColor Yellow
    $command = "helm upgrade --install $name-vectorization-job oci://ghcr.io/solliancenet/foundationallm/helm/vectorization-job --version $version -f ../values/microservice-values.yml"
    $command = createHelmCommand $command
    Invoke-Expression "$command"
}

Pop-Location

Write-Host "FoundationaLLM backend services deployed on AKS" -ForegroundColor Yellow