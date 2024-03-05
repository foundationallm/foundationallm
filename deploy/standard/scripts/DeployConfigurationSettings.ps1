#!/usr/bin/env pwsh

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$keyvaultName,
    [parameter(Mandatory = $false)][string]$configurationFile = "../config/appconfig.json"
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

Push-Location $($MyInvocation.InvocationName | Split-Path)

$config = Get-Content -Raw -Path $configurationFile | ConvertFrom-Json

for ( $idx = 0; $idx -lt $config.count; $idx++ ) {
    Write-Host $config[$idx].key -ForegroundColor Blue

    if ($config[$idx].keyVault) {
        Invoke-AndRequireSuccess "Setting Key Vault reference for $($config[$idx].key) to $($config[$idx].value)" {
            $secretName = $config[$idx].value
            az appconfig kv set-keyvault `
                --key $config[$idx].key `
                --name $name `
                --secret-identifier https://$($keyvaultName).vault.azure.net/Secrets/$($secretName)/ `
                --yes
        }
    }
    elseif ($config[$idx].featureFlag) {
        Invoke-AndRequireSuccess "Setting feature flag $($config[$idx].key) to $($config[$idx].value)" {
            az appconfig feature set `
                --feature $config[$idx].value `
                --key $config[$idx].key `
                --name $name `
                --yes
        }
    }
    else {
        Invoke-AndRequireSuccess "Setting Key Value $($config[$idx].key) to $($config[$idx].value)" {
            az appconfig kv set `
                --key $config[$idx].key `
                --name $name `
                --value "$($config[$idx].value)" `
                --yes
        }
    }

    # Avoid rate limiting :(
    Start-Sleep -Milliseconds 200
}

Pop-Location
