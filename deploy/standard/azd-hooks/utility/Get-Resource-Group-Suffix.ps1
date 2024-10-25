#!/usr/bin/env pwsh

function Get-Resource-Group-Suffix {
    param([Parameter(Mandatory = $true)][string]$workload)

    return "$($env:AZURE_ENV_NAME)-$($env:AZURE_LOCATION)-$($workload)-$($env:FOUNDATIONALLM_PROJECT)"
}
