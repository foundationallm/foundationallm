#! /usr/bin/env pwsh

param(
    [parameter(Mandatory = $true)][string]$keyVaultResourceGroup,
    [parameter(Mandatory = $true)][hashtable]$certificates
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)

. ./utility/Invoke-AndRequireSuccess.ps1

$directories = @{
    "certs"  = "../config/certbot/certs"
}

$keyVaultName = Invoke-AndRequireSuccess "Retrieve Key Vault" {
    az keyvault list `
        --resource-group $keyVaultResourceGroup `
        --query "[0].name" `
        --output tsv
}

foreach ($certificate in $certificates.GetEnumerator()) {
    $pfxPath = Join-Path $directories["certs"] $certificate.Value
    $pfx = Get-ChildItem -Path $pfxPath
    $domain = $pfx.BaseName
    $keyName = $certificate.Name

    Invoke-AndRequireSuccess "Load PFX Certificate $($domain) into Azure Key Vault" {
        az keyvault certificate import `
            --file $pfx.FullName `
            --name $keyName `
            --vault-name $keyVaultName
    }
}
