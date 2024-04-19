#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$init = $true,
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json"
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

    if ($init) {
        $extensions = @("aks-preview", "application-insights", "storage-preview", "eventgrid")
        foreach ($extension in $extensions) {
            Invoke-AndRequireSuccess "Install $extension extension" {
                az extension add --name $extension --allow-preview true --yes
                az extension update --name $extension --allow-preview true
            }
        }

        Invoke-AndRequireSuccess "Login to Azure" {
            az login
            az account set --subscription $manifest.subscription
            az account show
        }
    }

    # Convert the manifest resource groups to a hashtable for easier access
    $resourceGroup = @{}
    $manifest.resourceGroups.PSObject.Properties | ForEach-Object { $resourceGroup[$_.Name] = $_.Value }
    $resourceSuffix = "$($manifest.project)-$($manifest.environment)-$($manifest.location)"

    Invoke-AndRequireSuccess "Generate Configuration" {
        ./deploy/Generate-Config.ps1 `
            -adminGroupObjectId $manifest.adminObjectId `
            -entraClientIds $manifest.entraClientIds `
            -entraScopes $manifest.entraScopes `
            -instanceId $manifest.instanceId `
            -resourceGroups $resourceGroup `
            -resourceSuffix $resourceSuffix `
            -serviceNamespaceName $manifest.k8sNamespace `
            -subscriptionId $manifest.subscription `
            -ingress $manifest.ingress
    }

    $appConfigName = Invoke-AndRequireSuccess "Get AppConfig" {
        az appconfig list `
            --resource-group $($resourceGroup.ops) `
            --query "[0].name" `
            --output tsv
    }

    $configurationFile = Resolve-Path "../config/appconfig.json"
    Invoke-AndRequireSuccess "Loading AppConfig Values" {
        az appconfig kv import `
            --profile appconfig/kvset `
            --name $appConfigName `
            --source file `
            --path $configurationFile `
            --format json `
            --yes `
            --output none
    }

    Invoke-AndRequireSuccess "Uploading Auth Store Data" {
        ./Upload-AuthStoreData.ps1 `
            -resourceGroup $resourceGroup["auth"]
    }

    Invoke-AndRequireSuccess "Uploading System Prompts" {
        ./deploy/UploadSystemPrompts.ps1 `
            -resourceGroup $resourceGroup["storage"] `
            -location $manifest.location
    }

    $backendAks = Invoke-AndRequireSuccess "Get Backend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'backend')].name | [0]" `
            --output tsv
    }

    $secretProviderClassManifestBackend = Resolve-Path "../config/kubernetes/spc.foundationallm-certificates.backend.yml"
    $ingressNginxValuesBackend = Resolve-Path "../config/helm/ingress-nginx.values.backend.yml"
    Invoke-AndRequireSuccess "Deploy Backend" {
        ./deploy/Deploy-Backend-Aks.ps1 `
            -aksName $backendAks `
            -resourceGroup $resourceGroup.app `
            -secretProviderClassManifest $secretProviderClassManifestBackend `
            -ingressNginxValues $ingressNginxValuesBackend
    }

    $frontendAks = Invoke-AndRequireSuccess "Get Frontend AKS" {
        az aks list `
            --resource-group $($resourceGroup.app) `
            --query "[?contains(name, 'frontend')].name | [0]" `
            --output tsv
    }

    $secretProviderClassManifestFrontend = Resolve-Path "../config/kubernetes/spc.foundationallm-certificates.frontend.yml"
    $ingressNginxValuesFrontend = Resolve-Path "../config/helm/ingress-nginx.values.frontend.yml"
    Invoke-AndRequireSuccess "Deploy Frontend" {
        ./deploy/Deploy-Frontend-Aks.ps1 `
            -aksName $frontendAks `
            -resourceGroup $resourceGroup.app `
            -secretProviderClassManifest $secretProviderClassManifestFrontend `
            -ingressNginxValues $ingressNginxValuesFrontend
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}








# # Write-Host "===========================================================" -ForegroundColor Yellow
# # Write-Host "The frontend is hosted at https://$webappHostname" -ForegroundColor Yellow
# # Write-Host "The Core API is hosted at $coreApiUri" -ForegroundColor Yellow
# # Write-Host "===========================================================" -ForegroundColor Yellow
