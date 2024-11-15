#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Remove-Hostname.ps1 - Script to remove a hostname from the system.

.DESCRIPTION
    This script is used to remove a hostname from the system. It performs the following steps:
    1. Checks if required environment variables are set.
    2. Creates a backup folder if it doesn't exist.
    3. Removes the callback URL if necessary.
    4. Connects to the AKS cluster.
    5. Removes the ingress rule.
    6. Updates the secret provider class.
    7. Removes the secret object from the secret provider class.
    8. Removes the parameter object from the secret provider class.
    9. Saves the updated secret provider class.
    10. Checks for the certificate to remove.
    11. Removes the certificate if found.

.PARAMETER clusterRole
    Specifies the role of the cluster. Valid values are "frontend" or "backend".

.PARAMETER hostname
    Specifies the hostname to be removed.

.PARAMETER id
    Specifies the ID of the hostname.

.PARAMETER serviceName
    Specifies the name of the service.

.EXAMPLE
    Remove-Hostname.ps1 -clusterRole "frontend" -hostname "example.com" -id "12345" -serviceName "chat-ui"

    This example removes the hostname "example.com" with ID "12345" from the frontend cluster for the "chat-ui" service.

.NOTES
    Author: Jim Counts
    Date: 2024-08-27
#>
param (
    [parameter(Mandatory = $true)]
    [ValidateSet("frontend", "backend")]
    [string]$clusterRole,

    [Parameter(Mandatory = $true)]
    [string]$hostname,

    [Parameter(Mandatory = $true)]
    [string]$id,

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
    "AZURE_SUBSCRIPTION_ID" = "Unable to determine the subscription ID"
    "FLLM_OPS_KV"           = "Unable to determine the Key Vault for certificate upload"
    "FLLM_APP_RG"           = "Unable to determine the resource group for the AKS cluster"
}
Test-EnvironmentVariables -envVariables $requiredEnvVariables

# Create the backup folder if it doesn't exist
$backupFolder = "./backup" | Get-AbsolutePath
if (-not (Test-Path $backupFolder)) {
    New-Item -ItemType Directory -Path $backupFolder
}

# Remove the callback URL if necessary
$appRegistration = @{
    "chat-ui"       = "FoundationaLLM-Core-Portal"
    "management-ui" = "FoundationaLLM-Management-Portal"
}
if ($appRegistration.ContainsKey($serviceName)) {
    $objectId = Get-AppRegistrationObjectId `
        -displayName $appRegistration[$serviceName]

    Remove-OAuthCallbackUri `
        -applicationId $objectId `
        -redirectUri "https://$hostname/signin-oidc"
}

Connect-AksCluster -clusterRole $clusterRole -resourceGroup $env:FLLM_APP_RG
Invoke-CliCommand "Remove Ingress Rule" {
    kubectl delete ingress `
        $id `
        --namespace gateway-system `
        --ignore-not-found
}

# Update secret provider class
$secretProviderClassJson = $null
Invoke-CLICommand "Get SecretProviderClass" {
    $script:secretProviderClassJson = kubectl get SecretProviderClass `
        foundationallm-certificates `
        --namespace gateway-system `
        --output json
}

$secretProviderClassJson | Out-File "$backupFolder/foundationallm-certificates.SecretProviderClass.${timestamp}.json"
$secretProviderClass = $secretProviderClassJson | ConvertFrom-Json -Depth 100 -AsHashtable

# Remove the secret object from the secret provider class
$secretName = "$($id)-tls"
$secretProviderClass.spec.secretObjects = $secretProviderClass.spec.secretObjects | `
    Where-Object { $_.secretName -ne $secretName }

# Remove the parameter object from the secret provider class
$parameterObjects = $secretProviderClass.spec.parameters.objects -split "-\s+\|" | `
    Select-Object -Skip 1 | `
    Where-Object { $_ -notmatch $id } | `
    ForEach-Object {
    @"
  - |
    $($_.Trim())
"@
}

$secretProviderClass.spec.parameters.objects = @"
array:
$($parameterObjects -join "`n")
"@

# Save the updated secret provider class
$secretProviderClassFilePath = "./config/foundationallm-certificates.SecretProviderClass.json" | Get-AbsolutePath
$secretProviderClass | ConvertTo-Json -Depth 100 | Out-File $secretProviderClassFilePath
Invoke-CliCommand "Publish SecretProviderClass" {
    kubectl apply `
        --filename $secretProviderClassFilePath `
        --namespace gateway-system
}

$certificate = $null
Invoke-CliCommand "Check for certificate to remove" {
    $script:certificate = az keyvault certificate list `
        --subscription $env:AZURE_SUBSCRIPTION_ID `
        --vault-name $env:FLLM_OPS_KV `
        --query "[?name == '$id'].name | [0]" `
        --output tsv
}

if ($null -ne $certificate) {
    Invoke-CliCommand "Remove Certificate" {
        az keyvault certificate delete `
            --subscription $env:AZURE_SUBSCRIPTION_ID `
            --vault-name $env:FLLM_OPS_KV `
            --query "id" `
            --name $id `
            --output tsv
    }
}

Stop-Transcript