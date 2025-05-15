#!/usr/bin/env pwsh

function Get-Dns-Resource-Config {
    param (
    )

    $dnsResourceGroupName = $(azd env get-value FOUNDATIONALLM_DNS_RESOURCE_GROUP)
    if ($LastExitCode -ne 0) {
        $dnsResourceGroupName = Read-Host "Enter Private DNS Zones Resource Group Name: "
        $dnsResourceGroupName = $dnsResourceGroupName.Trim()
        if ($dnsResourceGroupName -eq "") {
            throw "Private DNS Zones Resource Group Name cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_DNS_RESOURCE_GROUP $dnsResourceGroupName
        }
    } else {
        $newDnsResourceGroupName = Read-Host "Enter Private DNS Zones Resource Group Name (Enter for '$dnsResourceGroupName'): "
        $newDnsResourceGroupName = $newDnsResourceGroupName.Trim()
        if ($newDnsResourceGroupName -ne "") {
            $dnsResourceGroupName = $newDnsResourceGroupName
            azd env set FOUNDATIONALLM_DNS_RESOURCE_GROUP $dnsResourceGroupName
        }
    }

    $dnsSubscriptionId = $(azd env get-value FOUNDATIONALLM_DNS_SUBSCRIPTION_ID)
    if ($LastExitCode -ne 0) {
        $dnsSubscriptionId = Read-Host "Enter Private DNS Zones Subscription Id: "
        $dnsSubscriptionId = $dnsSubscriptionId.Trim()
        if ($dnsSubscriptionId -eq "") {
            throw "Private DNS Zones Subscription Id cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_DNS_SUBSCRIPTION_ID $dnsSubscriptionId
        }
    } else {
        $newDnsSubscriptionId = Read-Host "Enter Private DNS Zones Subscription Id (Enter for '$dnsSubscriptionId'): "
        $newDnsSubscriptionId = $newDnsSubscriptionId.Trim()
        if ($newDnsSubscriptionId -ne "") {
            $dnsSubscriptionId = $newDnsSubscriptionId
            azd env set FOUNDATIONALLM_DNS_SUBSCRIPTION_ID $dnsSubscriptionId
        }
    }

    $dnsTenantId = $(azd env get-value FOUNDATIONALLM_DNS_TENANT_ID)
    if ($LastExitCode -ne 0) {
        $dnsTenantId = Read-Host "Enter Private DNS Zones Tenant Id: "
        $dnsTenantId = $dnsTenantId.Trim()
        if ($dnsTenantId -eq "") {
            throw "Private DNS Zones Tenant Id cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_DNS_TENANT_ID $dnsTenantId
        }
    } else {
        $newDnsTenantId = Read-Host "Enter Private DNS Zones Tenant Id (Enter for '$dnsTenantId'): "
        $newDnsTenantId = $newDnsTenantId.Trim()
        if ($newDnsTenantId -ne "") {
            $dnsTenantId = $newDnsTenantId
            azd env set FOUNDATIONALLM_DNS_TENANT_ID $dnsTenantId
        }
    }

    Write-Host "DNS Zone Resource Group Name: $dnsResourceGroupName" -ForegroundColor Green
    Write-Host "DNS Zone Subscription Id:     $dnsSubscriptionId" -ForegroundColor Green
    Write-Host "DNS Zone Tenant Id:           $dnsTenantId" -ForegroundColor Green
}