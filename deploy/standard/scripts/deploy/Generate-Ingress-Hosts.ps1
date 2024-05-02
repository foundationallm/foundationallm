#!/usr/bin/env pwsh

param (
    [parameter(Mandatory = $true)][array]$clusters,
    [parameter(Mandatory = $true)][string]$resourceGroup
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Function to invoke a script block and require success
function Invoke-AndRequireSuccess {
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed: ${message}" -ForegroundColor Red
        Write-Host "Script Block Output: ${result}"
        exit $LASTEXITCODE
    }

    return $result
}

$gatewayNamespace = "gateway-system"

Write-Host $clusters | ConvertTo-Json

$hosts = @{}
foreach ($cluster in $clusters) {
    $aksName = $cluster.cluster
    Invoke-AndRequireSuccess "Retrieving credentials for AKS cluster ${aksName}" {
        az aks get-credentials --name $aksName --resource-group $resourceGroup
    }

    $ingressIp = Invoke-AndRequireSuccess "Get Ingress IP" {
        kubectl get service gateway-ingress-nginx-controller `
            --namespace ${gatewayNamespace} `
            --output jsonpath='{.status.loadBalancer.ingress[0].ip}'
    }

    foreach ($hostName in $cluster.hosts) {
        $hosts[$hostName] = $ingressIp
    }
}

$hostFile = @()
foreach ($endpoint in $hosts.GetEnumerator()) {
    $hostFile += "$($endpoint.Value)  $($endpoint.Key)"
}

$hostFilePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath("../config/hosts.ingress")
Write-Host "Writing hosts file to ${hostFilePath}" -ForegroundColor Green
$hostFile | Sort-Object | Out-File -FilePath $hostFilePath -Encoding ascii -Force
