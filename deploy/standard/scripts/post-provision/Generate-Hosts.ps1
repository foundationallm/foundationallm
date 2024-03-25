#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Generates a hosts file based on the private endpoints and APIM instances in the deployment manifest.

.DESCRIPTION
    This script generates a hosts file that maps fully qualified domain names (FQDNs) to private IP addresses.
    It retrieves the private endpoints and APIM instances from the deployment manifest and uses Azure CLI commands
    to get the necessary information. The hosts file is then written to the specified location.

.PARAMETER subscription
    The Azure subscription ID.

.EXAMPLE
    Generate-Hosts.ps1 -subscription "12345678-1234-1234-1234-1234567890ab"
#>

param (
    [parameter(Mandatory = $true)][object]$resourceGroup,
    [parameter(Mandatory = $true)][string]$subscription
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
        exit $LASTEXITCODE
    }

    return $result
}

# Function to get hosts for AKS private endpoints
function Get-Hosts-Aks-Strategy {
    param(
        [parameter(Mandatory = $true)][string]$privateEndpointName,
        [parameter(Mandatory = $true)][string]$privateEndpointServiceId,
        [parameter(Mandatory = $true)][string]$privateEndpointNetworkInterfaceId
    )

    $aksInstance = Invoke-AndRequireSuccess "Get AKS Instances for Private Endpoint: $($privateEndpointName)" {
        az aks list `
            --resource-group $resourceGroup.Value `
            --query '[].{id:id, name:name, privateFqdn:privateFqdn}' `
            --output json | `
            ConvertFrom-Json | `
            Where-Object { $_.id -eq $privateEndpointServiceId } | `
            Select-Object -Property name, @{Name = 'fqdn'; Expression = { $_.privateFqdn } } | `
            Add-Member -MemberType NoteProperty -Name nicId -Value $privateEndpointNetworkInterfaceId -PassThru
    }

    $networkInterfaceFqdns = Invoke-AndRequireSuccess "Get Network Interface FQDNs for AKS Instance: $($aksInstance.name)" {
        az network nic show `
            --ids $aksInstance.nicId `
            --query 'ipConfigurations[].{privateIPAddress:privateIPAddress}' `
            --output json | `
            ConvertFrom-Json | `
            Add-Member -MemberType NoteProperty -Name fqdn -Value $aksInstance.fqdn -PassThru
    }

    return $networkInterfaceFqdns
}

# Function to get hosts for default private endpoints
function Get-Hosts-Default-Strategy {
    param (
        [parameter(Mandatory = $true)][string]$id
    )

    $networkInterfaceFqdns = Invoke-AndRequireSuccess "Get Network Interface FQDNs" {
        az network nic show `
            --ids $id `
            --query 'ipConfigurations[].{
                fqdn:privateLinkConnectionProperties.fqdns[0],
                groupId:privateLinkConnectionProperties.groupId,
                nicId:id,
                nicName:name,
                peId:privateEndpoint.id
                privateIPAddress:privateIPAddress
            }' `
            --output json | `
            ConvertFrom-Json
    }

    return $networkInterfaceFqdns
}

$hosts = @{}
foreach ($resourceGroup in $resourceGroup.GetEnumerator()) {
    Write-Host "Finding Private Endpoints in Resource Group: $($resourceGroup.Name)" -ForegroundColor Green

    $privateEndpoints = Invoke-AndRequireSuccess "Get Private Endpoints in Resource Group: $($resourceGroup.Name)" {
        az network private-endpoint list `
            --resource-group $resourceGroup.Value `
            --query "[].{
                groupId:privateLinkServiceConnections[0].groupIds[0],
                nicId:networkInterfaces[0].id,
                peName:name,
                serviceId:privateLinkServiceConnections[0].privateLinkServiceId
            }" `
            --subscription $subscription `
            --output json | `
            ConvertFrom-Json
    }

    if ($null -eq $privateEndpoints -or $privateEndpoints.Count -lt 1) {
        Write-Host "No Private Endpoints found in Resource Group: $($resourceGroup.Name)" -ForegroundColor Yellow
        continue
    }

    foreach ($privateEndpoint in $privateEndpoints) {
        $networkInterfaceFqdns = @()
        switch ($privateEndpoint.groupId) {
            "management" {
                Write-Host "Found AKS Management Private Endpoint: $($privateEndpoint.peName)" -ForegroundColor Yellow
                $networkInterfaceFqdns = Get-Hosts-Aks-Strategy `
                    -privateEndpointName $privateEndpoint.peName `
                    -privateEndpointServiceId $privateEndpoint.serviceId `
                    -privateEndpointNetworkInterfaceId $privateEndpoint.nicId
            }
            default {
                Write-Host "Found Default Private Endpoint: $($privateEndpoint.peName)" -ForegroundColor Yellow
                $networkInterfaceFqdns = Get-Hosts-Default-Strategy -id $privateEndpoint.nicId
            }
        }

        foreach ($fqdn in $networkInterfaceFqdns) {
            $hosts[$fqdn.fqdn] = $fqdn.privateIPAddress
        }
    }

    Write-Host "Checking for APIM instances..." -ForegroundColor Green
    $apimInstances = Invoke-AndRequireSuccess "Get APIM Instances" {
        az apim list `
            --resource-group $resourceGroup.Value `
            --query '[].{name:name, privateIPAddress:privateIpAddresses[0], fqdn:hostnameConfigurations[0].hostName}' `
            --output json | `
            ConvertFrom-Json
    }

    foreach ($apimInstance in $apimInstances) {
        Write-Host "Found APIM Instance: $($apimInstance.name)" -ForegroundColor Yellow
        $hosts[$apimInstance.fqdn] = $apimInstance.privateIPAddress
    }
}

$hostFile = @()
foreach ($endpoint in $hosts.GetEnumerator()) {
    $hostFile += "$($endpoint.Value)  $($endpoint.Key)"
}

Write-Host "Writing hosts file" -ForegroundColor Green
$hostFile | Sort-Object | Out-File -FilePath "../config/hosts" -Encoding ascii -Force
