#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$init = $true,
    [parameter(Mandatory = $false)][string]$loginServer = "ghcr.io/solliancenet/foundationallm",
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json",
    [parameter(Mandatory = $false)][string]$updateVersion = "0.4.1"

)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load the Invoke-AndRequireSuccess function
. ./utility/Invoke-AndRequireSuccess.ps1

# Navigate to the script directory so that we can use relative paths.
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    Write-Host "Loading Deployment Manifest ../${manifestName}" -ForegroundColor Blue
    $manifest = $(Get-Content -Raw -Path ../${manifestName} | ConvertFrom-Json)

    $resourceGroup = @{}
    $manifest.resourceGroups.PSObject.Properties | ForEach-Object { $resourceGroup[$_.Name] = $_.Value }

    $backendAks = Invoke-AndRequireSuccess "Get Backend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'backend')].name | [0]" `
            --output tsv
    }

    $chartNames = @{
        "agent-factory-api"          = @{
            image  = "agentfactory-api"
            values = "../config/helm/microservice-values.yml"
        }
        "agent-hub-api"              = @{
            image  = "agenthub-api"
            values = "../config/helm/microservice-values.yml"
        }
        "core-api"                   = @{
            image  = "core-api"
            values = "../config/helm/coreapi-values.yml"
        }
        "core-job"                   = @{
            image  = "core-job"
            values = "../config/helm/microservice-values.yml"
        }
        "data-source-hub-api"        = @{
            image  = "datasourcehub-api"
            values = "../config/helm/microservice-values.yml"
        }
        "gatekeeper-api"             = @{
            image  = "gatekeeper-api"
            values = "../config/helm/microservice-values.yml"
        }
        "gatekeeper-integration-api" = @{
            image  = "gatekeeperintegration-api"
            values = "../config/helm/microservice-values.yml"
        }
        "langchain-api"              = @{
            image  = "langchain-api"
            values = "../config/helm/microservice-values.yml"
        }
        "management-api"             = @{
            image  = "management-api"
            values = "../config/helm/managementapi-values.yml"
        }
        "prompt-hub-api"             = @{
            image  = "prompthub-api"
            values = "../config/helm/microservice-values.yml"
        }
        "semantic-kernel-api"        = @{
            image  = "semantickernel-api"
            values = "../config/helm/microservice-values.yml"
        }
        "vectorization-api"          = @{
            image  = "vectorization-api"
            values = "../config/helm/vectorizationapi-values.yml"
        }
        "vectorization-job"          = @{
            image  = "vectorization-job"
            values = "../config/helm/microservice-values.yml"
        }
    }

    Invoke-AndRequireSuccess "Update Backend" {
        ./deploy/Update-Aks.ps1 `
            -aksName $backendAks `
            -chartNames $chartNames `
            -loginServer $loginServer `
            -resourceGroup $resourceGroup.app `
            -updateVersion $updateVersion
    }

    $frontendAks = Invoke-AndRequireSuccess "Get Frontend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'frontend')].name | [0]" `
            --output tsv
    }

    $chartNames = @{
        "chat-ui"       = @{
            image  = "chat-ui"
            values = "../config/helm/chatui-values.yml"
        }
        "management-ui" = @{
            image  = "management-ui"
            values = "../config/helm/managementui-values.yml"
        }
    }
    Invoke-AndRequireSuccess "Update Frontend" {
        ./deploy/Update-Aks.ps1 `
            -aksName $frontendAks `
            -chartNames $chartNames `
            -loginServer $loginServer `
            -resourceGroup $resourceGroup.app `
            -updateVersion $updateVersion
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}
