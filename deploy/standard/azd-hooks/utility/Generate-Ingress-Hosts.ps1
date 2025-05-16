#!/usr/bin/env pwsh

param (
    [parameter(Mandatory = $true)][array]$clusters,
    [parameter(Mandatory = $true)][string]$resourceGroup
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./Invoke-AndRequireSuccess.ps1
}
finally {
    Pop-Location
}

$gatewayNamespace = "gateway-system"

Write-Host ($clusters | ConvertTo-Json)

$hosts = @{}
foreach ($cluster in $clusters) {
    $aksName = $cluster.cluster
    Invoke-AndRequireSuccess "Retrieving credentials for AKS cluster ${aksName}" {
        az aks get-credentials --name $aksName --resource-group $resourceGroup --overwrite-existing
        kubelogin convert-kubeconfig -l azurecli
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
