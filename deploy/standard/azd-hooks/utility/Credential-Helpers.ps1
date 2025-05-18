#!/usr/bin/env pwsh

function Get-Service-Principal-Credentials {
    param (
        [Parameter(Mandatory = $true, Position = 0)][string]$configPath,
        [Parameter(Mandatory = $true, Position = 1)][string]$baseName
    )

    if (Test-Path "$configPath/$baseName.json") {
        $credentials = Get-Content -Raw -Path "$configPath/$baseName.json" | ConvertFrom-Json
    } else {
        $credentials = @{}
        $credentials.clientId = Read-Host "Enter Azure Client ID: "
        $credentials.clientSecret = Read-Host "Enter Azure Client Secret: "
        $credentials.tenantId = Read-Host "Enter Azure Tenant ID: "
        $credentials.subscriptionId = Read-Host "Enter Azure Subscription ID: "
        $credentials | ConvertTo-Json -depth 2 | Out-File "$configPath/$baseName.json"
    }

    Write-Host "Azure Client Id:       $($credentials.clientId)" -ForegroundColor Green
    Write-Host "Azure Tenant Id:       $($credentials.tenantId)" -ForegroundColor Green
    Write-Host "Azure Subscription Id: $($credentials.subscriptionId)" -ForegroundColor Green
    return $credentials
}