#!/usr/bin/env pwsh

function Format-Json {
    param([Parameter(Mandatory = $true, ValueFromPipeline = $true)][string]$json)
    return $json | ConvertFrom-Json -Depth 50 | ConvertTo-Json -Compress -Depth 50 | ForEach-Object { $_ -replace '"', '\"' }
}
