#! /usr/bin/pwsh

<#
.SYNOPSIS
    This script generates and imports SSL certificates using Certbot and Azure Key Vault.

.DESCRIPTION
    The script generates SSL certificates for a list of domains using Certbot, and then imports the generated certificates into an Azure Key Vault.

.PARAMETER key_vault_name
    The name of the Azure Key Vault where the certificates will be imported.

.NOTES
    - This script requires Certbot and OpenSSL to be installed on the system.
    - The script assumes that the necessary DNS configuration for domain validation is already in place (see references below).

.REFERENCES
    - Certbot DNS Azure documentation: https://docs.certbot-dns-azure.co.uk/en/latest/
    - Certbot DNS Azure GitHub repository: https://github.com/terrycain/certbot-dns-azure

.EXAMPLE
    .\certbot.ps1 -key_vault_name "mykeyvault"
#>

Param(
    [parameter(Mandatory = $true)][string]$key_vault_name
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$domains = @(
    "www.internal.foundationallm.ai",
    "api.internal.foundationallm.ai",
    "management-api.internal.foundationallm.ai",
    "vectorization-api.internal.foundationallm.ai"
)

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

foreach ($domain in $domains) {
    $fullChain = Join-Path $directories["config"] "live" ${domain} "fullchain.pem"
    $key_name = $domain -replace '\.', '-'
    $pfx = Join-Path $directories["certs"] "${domain}.pfx"
    $privKey = Join-Path $directories["config"] "live" ${domain} "privkey.pem"

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
        Write-Error("Failed to generate certificate for ${domain}")
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
    az keyvault certificate import `
        --file ${pfx} `
        --name ${key_name} `
        --vault-name ${key_vault_name}

    if ($LASTEXITCODE -ne 0) {
        Write-Error("Failed to import certificate for ${domain}")
        exit 1
    }
}
