#!/usr/bin/env pwsh

function Get-Hub-Resource-Config {
    param (
    )

    $hubResourceGroupName = $(azd env get-value FOUNDATIONALLM_HUB_RESOURCE_GROUP)
    if ($LastExitCode -ne 0) {
        $hubResourceGroupName = Read-Host "Enter HUB VNET Resource Group Name: "
        if ($hubResourceGroupName -eq "") {
            throw "HUB VNET Resource Group Name cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_HUB_RESOURCE_GROUP $hubResourceGroupName
        }
    } else {
        $newHubResourceGroupName = Read-Host "Enter HUB VNET Resource Group Name (Enter for '$hubResourceGroupName'): "
        if ($newHubResourceGroupName -ne "") {
            $hubResourceGroupName = $newHubResourceGroupName
            azd env set FOUNDATIONALLM_HUB_RESOURCE_GROUP $hubResourceGroupName
        }
    }

    $hubSubscriptionId = $(azd env get-value FOUNDATIONALLM_HUB_SUBSCRIPTION_ID)
    if ($LastExitCode -ne 0) {
        $hubSubscriptionId = Read-Host "Enter HUB VNET Subscription Id: "
        if ($hubSubscriptionId -eq "") {
            throw "HUB VNET Subscription Id cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_HUB_SUBSCRIPTION_ID $hubSubscriptionId
        }
    } else {
        $newHubSubscriptionId = Read-Host "Enter HUB VNET Subscription Id (Enter for '$hubSubscriptionId'): "
        if ($newHubSubscriptionId -ne "") {
            $hubSubscriptionId = $newHubSubscriptionId
            azd env set FOUNDATIONALLM_HUB_SUBSCRIPTION_ID $hubSubscriptionId
        }
    }

    $hubTenantId = $(azd env get-value FOUNDATIONALLM_HUB_TENANT_ID)
    if ($LastExitCode -ne 0) {
        $hubTenantId = Read-Host "Enter HUB VNET Tenant Id: "
        if ($hubTenantId -eq "") {
            throw "HUB VNET Tenant Id cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_HUB_TENANT_ID $hubTenantId
        }
    } else {
        $newHubTenantId = Read-Host "Enter HUB VNET Tenant Id (Enter for '$hubTenantId'): "
        if ($newHubTenantId -ne "") {
            $hubTenantId = $newHubTenantId
            azd env set FOUNDATIONALLM_HUB_TENANT_ID $hubTenantId
        }
    }

    $hubVnetName = $(azd env get-value FOUNDATIONALLM_HUB_VNET_NAME)
    if ($LastExitCode -ne 0) {
        $hubVnetName = Read-Host "Enter HUB VNET Name: "
        if ($hubVnetName -eq "") {
            throw "HUB VNET Name cannot be empty."
        } else {
            azd env set FOUNDATIONALLM_HUB_VNET_NAME $hubVnetName
        }
    } else {
        $newHubVnetName = Read-Host "Enter HUB VNET Name (Enter for '$hubVnetName'): "
        if ($newHubVnetName -ne "") {
            $hubVnetName = $newHubVnetName
            azd env set FOUNDATIONALLM_HUB_VNET_NAME $hubVnetName
        }
    }

    Write-Host "HUB VNET Resource Group Name: $hubResourceGroupName" -ForegroundColor Green
    Write-Host "HUB VNET Subscription Id:     $hubSubscriptionId" -ForegroundColor Green
    Write-Host "HUB VNET Tenant Id:           $hubTenantId" -ForegroundColor Green
    Write-Host "HUB VNET Name:                $hubVnetName" -ForegroundColor Green
}

function Get-Network-Config {
    param (
    )

    $fllmAksServiceCidr = $(azd env get-value FLLM_AKS_SERVICE_CIDR)
    if ($LastExitCode -ne 0) {
        $fllmAksServiceCidr = Read-Host "Enter AKS Service CIDR (Enter for 10.100.0.0/16): "
        $fllmAksServiceCidr = $fllmAksServiceCidr.Trim()
        if ($fllmAksServiceCidr -eq "") {
            $fllmAksServiceCidr = "10.100.0.0/16"
        } elseif ($fllmAksServiceCidr -notmatch "^(\d{1,3}\.){3}\d{1,3}\/\d{1,2}$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        }

        azd env set FLLM_AKS_SERVICE_CIDR $fllmAksServiceCidr
    } else {
        $newFllmAksServiceCidr = Read-Host "Enter AKS Service CIDR (Enter for '$fllmAksServiceCidr'): "
        $newFllmAksServiceCidr = $newFllmAksServiceCidr.Trim()
        if ($newFllmAksServiceCidr -ne "" -and $newFllmAksServiceCidr -notmatch "^(\d{1,3}\.){3}\d{1,3}\/\d{1,2}$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        } elseif ($newFllmAksServiceCidr -ne "") {
            $fllmAksServiceCidr = $newFllmAksServiceCidr
            azd env set FLLM_AKS_SERVICE_CIDR $fllmAksServiceCidr
        }
    }

    $fllmVnetCidr = $(azd env get-value FLLM_VNET_CIDR)
    if ($LastExitCode -ne 0) {
        $fllmVnetCidr = Read-Host "Enter VNET CIDR (Enter for 10.220.128.0/20): "
        $fllmVnetCidr = $fllmVnetCidr.Trim()
        if ($fllmVnetCidr -eq "") {
            $fllmVnetCidr = "10.220.128.0/20"
        } elseif ($fllmVnetCidr -notmatch "^(\d{1,3}\.){3}\d{1,3}\/\d{1,2}$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        }

        azd env set FLLM_VNET_CIDR $fllmVnetCidr
    } else {
        $newFllmVnetCidr = Read-Host "Enter VNET CIDR (Enter for '$fllmVnetCidr'): "
        $newFllmVnetCidr = $newFllmVnetCidr.Trim()
        if ($newFllmVnetCidr -ne "" -and $newFllmVnetCidr -notmatch "^(\d{1,3}\.){3}\d{1,3}\/\d{1,2}$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        } elseif ($newFllmVnetCidr -ne "") {
            $fllmVnetCidr = $newFllmVnetCidr
            azd env set FLLM_VNET_CIDR $fllmVnetCidr
        }
    }

    $fllmAllowedExternalCidrs = $(azd env get-value FLLM_ALLOWED_CIDR)
    if ($LastExitCode -ne 0) {
        $fllmAllowedExternalCidrs = Read-Host "Enter comma separated Allowed External CIDRs or IPs (Enter for 192.168.100.0/24,192.168.101.0/28): "
        $fllmAllowedExternalCidrs = $fllmAllowedExternalCidrs.Trim()
        if ($fllmAllowedExternalCidrs -eq "") {
            $fllmAllowedExternalCidrs = "192.168.100.0/24,192.168.101.0/28"
        } elseif ($fllmAllowedExternalCidrs -notmatch "^(,*(\d{1,3}\.){3}\d{1,3}(\/\d{1,2}){0,1})+$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        }

        azd env set FLLM_ALLOWED_CIDR $fllmAllowedExternalCidrs
    } else {
        $newFllmAllowedExternalCidrs = Read-Host "Enter comma separated Allowed External CIDRs or IPs (Enter for '$fllmAllowedExternalCidrs'): "
        $newFllmAllowedExternalCidrs = $newFllmAllowedExternalCidrs.Trim()
        if ($newFllmAllowedExternalCidrs -ne "" -and $newFllmAllowedExternalCidrs -notmatch "^(,*(\d{1,3}\.){3}\d{1,3}(\/\d{1,2}){0,1})+$") {
            throw "Invalid CIDR format. Please enter a valid CIDR."
        } elseif ($newFllmAllowedExternalCidrs -ne "") {
            $fllmAllowedExternalCidrs = $newFllmAllowedExternalCidrs
            azd env set FLLM_ALLOWED_CIDR $fllmAllowedExternalCidrs
        }
    }

    Write-Host "AKS Service CIDR: $fllmAksServiceCidr" -ForegroundColor Green
    Write-Host "VNET CIDR: $fllmVnetCidr" -ForegroundColor Green
    Write-Host "Allowed External CIDR: $fllmAllowedExternalCidrs" -ForegroundColor Green
}

function Get-Hostname-Config {
    param (

    )

    $hostnames = @{}
    $hostIds = @{
        core_api    = "core-api"
        mgmt_api    = "management-api"
        mgmt_portal = "management-ui"
        chat_portal = "chat-ui"
    }

    foreach ($hostId in $hostIds.GetEnumerator()) {
        $hostname = $(azd env get-value FLLM_$($hostId.Key.ToUpper())_HOSTNAME)
        if ($LastExitCode -ne 0) {
            $hostname = Read-Host -Prompt "Please enter the hostname for $($hostId.Value) service: "
            $hostname = $hostname.Trim()
            if ($hostname -eq "") {
                throw "$($hostId.Value) hostname cannot be empty."
            } else {
                azd env set FLLM_$($hostId.Key.ToUpper())_HOSTNAME $hostname
            }
        } else {
            $newHostname = Read-Host -Prompt "Enter $($hostId.Value) hostname (Enter for '$hostname'): "
            $newHostname = $newHostname.Trim()
            if ($newHostname -ne "") {
                $hostname = $newHostname
                azd env set FLLM_$($hostId.Key.ToUpper())_HOSTNAME $hostname
            }
        }
    }
}