#!/usr/bin/env pwsh

Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$keyvaultName,
    [parameter(Mandatory = $false)][string]$configurationFile="../config/appconfig.json"
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

Push-Location $($MyInvocation.InvocationName | Split-Path)

$config = Get-Content -Raw -Path $configurationFile | ConvertFrom-Json

for ( $idx = 0; $idx -lt $config.count; $idx++ )
{
    Write-Host $config[$idx].key -ForegroundColor Blue
    if ($config[$idx].keyVault)
    {
        Write-Host "Setting Key Vault reference for $($config[$idx].key) to $($config[$idx].value)"
        $secretName = $config[$idx].value
        az appconfig kv set-keyvault `
            --key $config[$idx].key `
            --name $name `
            --secret-identifier https://$($keyvaultName).vault.azure.net/Secrets/$($secretName)/ `
            --yes
    }
    elseif ($config[$idx].featureFlag)
    {
        Write-Host "Setting feature flag $($config[$idx].key) to $($config[$idx].value)"
        az appconfig feature set `
            --feature $config[$idx].value `
            --key $config[$idx].key `
            --name $name `
            --yes
    }
    else
    {
        Write-Host "Setting Key Value $($config[$idx].key) to $($config[$idx].value)"
        az appconfig kv set `
            --key $config[$idx].key `
            --name $name `
            --value "$($config[$idx].value)" `
            --yes
    }

    # Avoid rate limiting :( (This limits to 20 requests per second according to CoPilot but I think it would be 5.  What do I know?)
    Start-Sleep -Milliseconds 200
}

Pop-Location
