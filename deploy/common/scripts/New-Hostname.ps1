#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Script to create a new hostname in the AKS cluster.

.DESCRIPTION
    This script is used to create a new hostname in the AKS cluster. It imports
    a certificate from a specified file, updates the secret provider class,
    creates a new ingress rule, and updates the app registration redirect URIs
    if necessary.

.PARAMETER certificatePath
    The path to the certificate file. This parameter is mandatory and must be a
    valid file path.

.PARAMETER clusterRole
    The role of the cluster. Valid values are "frontend" and "backend". This
    parameter is mandatory.

.PARAMETER id
    The logical ID of the certificate used when saving the certificate to key
    vault. This parameter is mandatory and must be a unique name within the key
    vault.

.PARAMETER servicePath
    The path for the service. This parameter is optional and defaults to "/(.*)"
    if not specified.

.PARAMETER serviceName
    The name of the service that handles requests for the hostname. This
    parameter is mandatory.

.EXAMPLE
    New-Hostname.ps1 -certificatePath "C:\certificates\example.pfx" -clusterRole "frontend" -id "example" -serviceName "example-service"

.NOTES
    This script requires the Azure CLI and kubectl to be installed and
    configured.
    This script must run from an AZD deployment directory initialized with an
    environment. The script will update the AZD environment currently selected
    as the default.

    Author: Jim Counts
    Date: 2024-08-26
#>
param (
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType 'Leaf' })]
    [string]$certificatePath,

    [parameter(Mandatory = $true)]
    [ValidateSet("frontend", "backend")]
    [string]$clusterRole,

    [Parameter(Mandatory = $true)]
    [string]$id,

    [Parameter(Mandatory = $false)]
    [string]$servicePath = "/(.*)",

    [Parameter(Mandatory = $true)]
    [string]$serviceName
)

$TranscriptName = $($MyInvocation.MyCommand.Name) -replace ".ps1", ".transcript.txt"
Start-Transcript -path .\$TranscriptName -Force

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path
. $ScriptDirectory/Function-Library.ps1

$timestamp = Get-Date -Format "yyyy-M-d-H-m"

Show-AzdEnvironments
Import-AzdEnvironment

# Check if required environment variables are set
$requiredEnvVariables = @{
    "FLLM_OPS_KV" = "Unable to determine the Key Vault for certificate upload"
    "FLLM_APP_RG" = "Unable to determine the resource group for the AKS cluster"
}
Test-EnvironmentVariables -envVariables $requiredEnvVariables

# Check if the certificate file is empty
if (-not (Test-Path $certificatePath) -or (Get-Item $certificatePath).length -eq 0) {
    throw "The certificate file is empty or does not exist."
}

# Create the backup folder if it doesn't exist
$backupFolder = "./backup" | Get-AbsolutePath
if (-not (Test-Path $backupFolder)) {
    New-Item -ItemType Directory -Path $backupFolder
}

$deletedCertificate = $null
Invoke-CliCommand "Check for Deleted Certificate" {
    $script:deletedCertificate = az keyvault certificate list-deleted `
        --vault-name $env:FLLM_OPS_KV `
        --query "[?name == '$id'].name | [0]" `
        --output tsv
}

if ($null -ne $deletedCertificate) {
    Invoke-CliCommand "Recover Deleted Certificate" {
        az keyvault certificate recover `
            --name $id `
            --query "id" `
            --vault-name $env:FLLM_OPS_KV `
            --output tsv
    }

    Write-Host "Waiting for the certificate to be recovered..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
}

Invoke-CliCommand "Load Certificate" {
    az keyvault certificate import `
        --file $certificatePath `
        --name $id `
        --vault-name $env:FLLM_OPS_KV `
        --query "id" `
        --output tsv
}

# Login to AKS
Connect-AksCluster `
    -clusterRole $clusterRole `
    -resourceGroup $env:FLLM_APP_RG

# Update secret provider class
$secretProviderClassJson = $null
Invoke-CLICommand "Get secret provider class" {
    $script:secretProviderClassJson = kubectl get SecretProviderClass `
        foundationallm-certificates `
        --namespace gateway-system `
        --output json
}

$secretProviderClassJson | Out-File "$backupFolder/foundationallm-certificates.SecretProviderClass.${timestamp}.json"
$secretProviderClassJson = $secretProviderClassJson | ConvertFrom-Json -Depth 100 -AsHashtable

$kvObject = @"
`n  - |
    objectName: $($id)
    objectType: secret
"@

$secretObject = @{
    secretName = "$($id)-tls"
    type       = "kubernetes.io/tls"
    data       = @(
        @{
            key        = "tls.crt"
            objectName = $id
        },
        @{
            key        = "tls.key"
            objectName = $id
        }
    )
}

if ($secretProviderClassJson.spec.parameters.objects -notmatch $id) {
    $secretProviderClassJson.spec.parameters.objects += $kvObject
}

$existingSecretObject = $secretProviderClassJson.spec.secretObjects | `
    Where-Object { $_.secretName -eq $secretObject.secretName }
if (-not $existingSecretObject) {
    $secretProviderClassJson.spec.secretObjects += $secretObject
}

$secretProviderClassFilePath = "./config/foundationallm-certificates.SecretProviderClass.json" | Get-AbsolutePath
$secretProviderClassJson | ConvertTo-Json -Depth 100 | Out-File $secretProviderClassFilePath
Invoke-CliCommand "Publish secret provider class" {
    kubectl apply `
        --filename $secretProviderClassFilePath `
        --namespace gateway-system
}

# Create new ingress rule
$certificate = Get-Item -Path $certificatePath
$hostname = $certificate.Name -replace "\.pfx$"

$tokens = @{
    ingressName       = $id
    serviceHostname   = $hostname
    serviceName       = $serviceName
    servicePath       = $servicePath
    servicePathType   = "ImplementationSpecific"
    serviceSecretName = $id
}

$serviceIngressTemplatePath = "./config/helm/service-ingress.template.yml" | Get-AbsolutePath
$serviceIngressTemplate = (Get-Content -Path $serviceIngressTemplatePath -Raw) -split "---" | Select-Object -Last 1
$serviceIngressTemplate = $serviceIngressTemplate -replace 'metadata:\s*name:\s*{{serviceName}}', "metadata:`n  name: {{ingressName}}"

$temporaryTemplatePath = "$backupFolder/service-ingress.template.yml" | Get-AbsolutePath
$serviceIngressTemplate | Out-File $temporaryTemplatePath

$ingressRule = Format-Template `
    -template $temporaryTemplatePath `
    -tokens $tokens

$ingressRulePath = "./config/helm/service-ingress.${id}.yml" | Get-AbsolutePath
$ingressRule | Out-File $ingressRulePath
Invoke-CliCommand "Publish Ingress Rule" {
    kubectl apply `
        --filename $ingressRulePath `
        --namespace gateway-system
}

# Update the app registration redirect URIs if necessary
$appRegistration = @{
    "chat-ui"       = "FoundationaLLM-Core-Portal"
    "management-ui" = "FoundationaLLM-Management-Portal"
}
if ($appRegistration.ContainsKey($serviceName)) {
    $objectId = Get-AppRegistrationObjectId `
        -displayName $appRegistration[$serviceName]

    Update-OAuthCallbackUri `
        -applicationId $objectId `
        -redirectUri "https://$hostname/signin-oidc"
}

Stop-Transcript