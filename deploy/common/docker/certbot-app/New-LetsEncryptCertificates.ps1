#! /usr/bin/env pwsh

<#
.SYNOPSIS
    This script generates Let's Encrypt certificates for a given base domain and a list of subdomains.

.DESCRIPTION
    The script generates Let's Encrypt certificates for a given base domain and a list of subdomains.
    It uses Certbot and the dns-azure plugin for DNS authentication.

.PARAMETER baseDomain
    The base domain for which the certificates will be generated.

.PARAMETER email
    The email address to be used for Let's Encrypt registration and notifications.

.PARAMETER subdomainPrefix
    An optional prefix to be added to each subdomain.

.NOTES
    - This script requires Certbot and the dns-azure plugin to be installed.
    - The script generates certificates for the following subdomains:
        - api
        - management
        - management-api
        - vectorization-api
        - www
    - Certbot DNS Azure documentation: https://docs.certbot-dns-azure.co.uk/en/latest/
    - Certbot DNS Azure GitHub repository: https://github.com/terrycain/certbot-dns-azure

.EXAMPLE
    Get-LetsEncryptCertificates.ps1 -baseDomain example.com -email admin@example.com

#>

param(
    [parameter(Mandatory = $true)][string]$baseDomain,
    [parameter(Mandatory = $true)][string]$email,
    [parameter(Mandatory = $false)][string]$subdomainPrefix = ''
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path
. $ScriptDirectory/Function-Library.ps1

$basenames = @{
    "api"            = "coreapi"
    "management"     = "managementui"
    "management-api" = "managementapi"
    "www"            = "chatui"
}

$directories = @{
    "config" = "/app/config/certbot/config" | Get-AbsolutePath
    "work"   = "/app/config/certbot/work" | Get-AbsolutePath
    "log"    = "/app/config/certbot/log" | Get-AbsolutePath
    "certs"  = "/app/config/certs" | Get-AbsolutePath
}

foreach ($directory in $directories.GetEnumerator()) {
    if (!(Test-Path $directory.Value)) {
        New-Item -ItemType Directory -Force -Path $directory.Value
    }
}

Invoke-CLICommand "Azure Login" {
    az login
}

foreach ($basename in $basenames.GetEnumerator()) {
    # Domain Name
    if('' -ne $subdomainPrefix) {
        $hostname = @($subdomainPrefix, $basename.Name) | Join-String -Separator "-"
    } else {
        $hostname = $basename.Name
    }

    $fqdn = @($hostname, $baseDomain) | Join-String -Separator "."

    # File Paths
    $paths = @{
        "pemFullChain" = Join-Path $directories["config"] "live" $fqdn "fullchain.pem"
        "pemPrivKey"   = Join-Path $directories["config"] "live" $fqdn "privkey.pem"
        "pfx"          = Join-Path $directories["certs"] $basename.Value "${fqdn}.pfx"
        "pfxDir"       = Join-Path $directories["certs"] $basename.Value
    }

    if (!(Test-Path $paths["pfxDir"])) {
        New-Item -ItemType Directory -Force -Path $paths["pfxDir"]
    }

    Invoke-CLICommand "Generate certificate for ${fqdn}" {
        certbot certonly `
            --agree-tos `
            --email $email `
            --authenticator dns-azure `
            --config-dir $directories["config"] `
            --dns-azure-config /app/config/certbot.ini `
            --domain $fqdn `
            --keep-until-expiring `
            --logs-dir $directories["log"] `
            --non-interactive `
            --preferred-challenges dns `
            --quiet `
            --work-dir $directories["work"]
    }

    Invoke-CLICommand "Export certificate for ${fqdn}" {
        openssl pkcs12 `
            -export `
            -inkey $paths["pemPrivKey"] `
            -in $paths["pemFullChain"] `
            -out $paths["pfx"] `
            -passout pass:
    }

    Invoke-CLICommand "Verify certificate for ${fqdn}" {
        openssl pkcs12 `
            -info `
            -in $paths["pfx"] `
            -nokeys `
            -passin pass: `
            -passout pass:
    }
}
