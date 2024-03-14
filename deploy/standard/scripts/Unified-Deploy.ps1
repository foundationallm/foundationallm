#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $false)][bool]$stepDeployCerts = $false,
    [parameter(Mandatory = $false)][bool]$stepDeployImages = $false,
    [parameter(Mandatory = $false)][bool]$init = $true,
    [parameter(Mandatory = $false)][string]$manifestName = "Deployment-Manifest.json"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
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

    # Invoke-AndRequireSuccess "Deploy Certificates to Application Gateway" {
    #     ./Deploy-Certs.ps1 `
    #         -resourceGroup $resourceGroup["network"] `
    #         -subscriptionId $manifest.subscription `
    #         -resourceSuffix $resourceSuffix
    # }

    Invoke-AndRequireSuccess "Generate Configuration" {
        ./Generate-Config.ps1 `
            -instanceId $manifest.instanceId `
            -entraClientIds $manifest.entraClientIds `
            -resourceGroups $resourceGroup `
            -subscriptionId $manifest.subscription `
            -resourceSuffix $resourceSuffix `
            -ingress $manifest.ingress
    }

    Invoke-AndRequireSuccess "Generate Host File" {
        ./Generate-Hosts.ps1 `
            -resourceGroup $resourceGroup `
            -subscription $manifest.subscription
    }

    Invoke-AndRequireSuccess "Uploading System Prompts" {
        ./Upload-AuthStoreData.ps1 `
            -resourceGroup $resourceGroup["auth"]
    }

    Invoke-AndRequireSuccess "Uploading System Prompts" {
        ./UploadSystemPrompts.ps1 `
            -resourceGroup $resourceGroup["storage"] `
            -location $manifest.location
    }
}
finally {
    Pop-Location
    Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
}


# $instanceId = $manifest.instanceId
# $ingress = $manifest.ingress
# $entraClientIds = $manifest.entraClientIds
# $environment = $manifest.environment
# $location = $manifest.location
# $project = $manifest.project
# $resourceGroups = $manifest.resourceGroups





# if ($stepDeployCerts) {
#     # TODO Deploy Certs to AGWs
# }

# if ($stepDeployImages) {
#     # Deploy images in AKS
#     $chartsToDeploy = "*"

#     #& ./Deploy-Images-Aks-Standard.ps1 -aksName $aksName -resourceGroup $resourceGroup -charts $chartsToDeploy
# }

# # Write-Host "===========================================================" -ForegroundColor Yellow
# # Write-Host "The frontend is hosted at https://$webappHostname" -ForegroundColor Yellow
# # Write-Host "The Core API is hosted at $coreApiUri" -ForegroundColor Yellow
# # Write-Host "===========================================================" -ForegroundColor Yellow
