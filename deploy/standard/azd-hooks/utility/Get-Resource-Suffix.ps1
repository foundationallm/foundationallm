#!/usr/bin/env pwsh

function Get-Resource-Suffix {
    return "$($env:FOUNDATIONALLM_PROJECT)-$($env:AZURE_ENV_NAME)-$($env:AZURE_LOCATION)"
}
