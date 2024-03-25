#!/usr/bin/env pwsh

Param(
    [Parameter(Mandatory = $true)][string]$envValues
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Set-EnvironmentVariables {
    param(
        [Parameter(Mandatory = $true)][string]$envFile
    )

    if (!(Test-Path $envFile -PathType Leaf)) {
        throw "$envFile is not a file."
    }

    Get-Content $envFile | `
        Where-Object { -not $_.StartsWith('#') } | `
        ForEach-Object `
    {
        $name, $value = $_.split('=')
        $value = $value.Trim('"')
        Set-Content env:\$name $value
        Write-Host "Wrote ${value} to environment variable ${name}." -ForegroundColor Green
    }
}


Set-EnvironmentVariables -envFile $envValues
. ./azd-hooks/postprovision.ps1
