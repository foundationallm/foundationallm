#! /usr/bin/pwsh

<#
.SYNOPSIS
    This script generates and imports SSL certificates using Certbot and Azure Key Vault.

.DESCRIPTION
    The script generates SSL certificates for a list of domains using Certbot, and then imports the generated certificates into an Azure Key Vault.

.PARAMETER baseDomain
    The base domain for the certificates.

.PARAMETER keyVaultName
    The name of the Azure Key Vault where the certificates will be imported.

.PARAMETER subdomainPrefix
    The prefix to be added to the subdomains.

.NOTES
    - This script requires Certbot and OpenSSL to be installed on the system.
    - The script assumes that the Azure CLI is installed on the system.
    - The script assumes that the necessary DNS configuration for domain validation is already in place (see references below).
    - Certbot DNS Azure documentation: https://docs.certbot-dns-azure.co.uk/en/latest/
    - Certbot DNS Azure GitHub repository: https://github.com/terrycain/certbot-dns-azure

.EXAMPLE
    .\Generate-Certs.ps1 -baseDomain "example.com" -keyVaultName "mykeyvault"

.EXAMPLE
    .\Generate-Certs.ps1 -baseDomain "example.com" -keyVaultName "mykeyvault" -subdomainPrefix "dev-"
#>

Param(
    [parameter(Mandatory = $true)][string]$baseDomain,
    [parameter(Mandatory = $false)][string]$keyVaultName="",
    [parameter(Mandatory = $false)][string]$subdomainPrefix=""
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)

$hosts = @{
    "${subdomainPrefix}api"               = @("coreapi", "managementapi", "vectorizationapi")
    "${subdomainPrefix}management"        = $null
    "${subdomainPrefix}management-api"    = $null
    "${subdomainPrefix}vectorization-api" = $null
    "${subdomainPrefix}www"               = @("chatui", "managementui")
}

$directories = @{
    "config" = "./certbot/config"
    "work"   = "./certbot/work"
    "log"    = "./certbot/log"
    "certs"  = "./certbot/certs"
}

foreach ($directory in $directories.GetEnumerator()) {
    if (!(Test-Path $directory.Value)) {
        New-Item -ItemType Directory -Force -Path $directory.Value
    }
}

foreach ($hostName in $hosts.GetEnumerator()) {
    $domain = "$($hostName.Key).${baseDomain}"
    $fullChain = Join-Path $directories["config"] "live" "${domain}" "fullchain.pem"
    $pfx = Join-Path $directories["certs"] "${domain}.pfx"
    $privKey = Join-Path $directories["config"] "live" "${domain}" "privkey.pem"

    # Generate certificate using letsencrypt
    & certbot certonly `
        --authenticator dns-azure `
        --config-dir $directories["config"] `
        --dns-azure-config certbot.ini `
        --logs-dir $directories["log"] `
        --preferred-challenges dns `
        --work-dir $directories["work"] `
        -d $domain

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to generate certificate for ${domain}"
        exit 1
    }

    # Export certificate to PFX
    & openssl pkcs12 `
        -export `
        -inkey $privKey `
        -in $fullChain `
        -out $pfx `
        -passout pass:

    if ($LASTEXITCODE -ne 0) {
        Write-Error("Failed to export certificate for ${domain}")
        exit 1
    }

    # Verify certificate
    & openssl pkcs12 `
        -info `
        -in ${pfx} `
        -nokeys `
        -passin pass: `
        -passout pass:

    if ($LASTEXITCODE -ne 0) {
        Write-Error("Failed to verify certificate for ${domain}")
        exit 1
    }

    # Import certificate into Azure Key Vault
    if ($keyVaultName -eq "") {
        continue
    }

    $keyVaultAliases = $hostName.Value
    if ($null -eq $keyVaultAliases) {
        $keyVaultAliases = @($($domain -replace '\.', '-'))
    }

    foreach ($alias in $keyVaultAliases) {
        & az keyvault certificate import `
            --file ${pfx} `
            --name ${alias} `
            --vault-name ${keyVaultName}

        if ($LASTEXITCODE -ne 0) {
            Write-Error("Failed to import certificate for ${domain}")
            exit 1
        }
    }
}
