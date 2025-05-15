#!/bin/usr/env pwsh

function Get-FllmAksNodePoolSkus {
    param()

    $backendUserAksNodeSku = $(azd env get-value FLLM_BACKEND_AKS_NODE_SKU)
    if ($LastExitCode -ne 0) {
        $backendUserAksNodeSku = Read-Host "Enter Backend User AKS Node SKU (Enter for Standard_D8_v5): "
        $backendUserAksNodeSku = $backendUserAksNodeSku.Trim()
        if ($backendUserAksNodeSku -eq "") {
            $backendUserAksNodeSku = "Standard_D8_v5"
            Write-Host "Defaulting to $backendUserAksNodeSku for Backend User AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
        }            
        
        azd env set FLLM_BACKEND_AKS_NODE_SKU $backendUserAksNodeSku
    } else {
        $newBackendUserAksNodeSku = Read-Host "Enter Backend User AKS Node SKU (Enter for '$backendUserAksNodeSku'): "
        $newBackendUserAksNodeSku = $newBackendUserAksNodeSku.Trim()
        if ($newBackendUserAksNodeSku -ne "") {
            $backendUserAksNodeSku = $newBackendUserAksNodeSku
            Write-Host "Setting to $backendUserAksNodeSku for Backend User AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
            azd env set FLLM_BACKEND_AKS_NODE_SKU $backendUserAksNodeSku
        }
    }

    $backendSystemAksNodeSku = $(azd env get-value FLLM_BACKEND_SYSTEM_AKS_NODE_SKU)
    if ($LastExitCode -ne 0) {
        $backendSystemAksNodeSku = Read-Host "Enter Backend System AKS Node SKU (Enter for Standard_D2_v5): "
        $backendSystemAksNodeSku = $backendSystemAksNodeSku.Trim()
        if ($backendSystemAksNodeSku -eq "") {
            $backendSystemAksNodeSku = "Standard_D2_v5"
            Write-Host "Defaulting to $backendSystemAksNodeSku for Backend System AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
        }            
        
        azd env set FLLM_BACKEND_SYSTEM_AKS_NODE_SKU $backendSystemAksNodeSku
    } else {
        $newBackendSystemAksNodeSku = Read-Host "Enter Backend System AKS Node SKU (Enter for '$backendSystemAksNodeSku'): "
        $newBackendSystemAksNodeSku = $newBackendSystemAksNodeSku.Trim()
        if ($newBackendSystemAksNodeSku -ne "") {
            $backendSystemAksNodeSku = $newBackendSystemAksNodeSku
            Write-Host "Setting to $backendSystemAksNodeSku for Backend System AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
            azd env set FLLM_BACKEND_SYSTEM_AKS_NODE_SKU $backendSystemAksNodeSku
        }
    }

    $frontendUserAksNodeSku = $(azd env get-value FLLM_FRONTEND_AKS_NODE_SKU)
    if ($LastExitCode -ne 0) {
        $frontendUserAksNodeSku = Read-Host "Enter Frontend User AKS Node SKU (Enter for Standard_D2_v5): "
        $frontendUserAksNodeSku = $frontendUserAksNodeSku.Trim()
        if ($frontendUserAksNodeSku -eq "") {
            $frontendUserAksNodeSku = "Standard_D2_v5"
            Write-Host "Defaulting to $frontendUserAksNodeSku for Frontend User AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
        }            
        
        azd env set FLLM_FRONTEND_AKS_NODE_SKU $frontendUserAksNodeSku
    } else {
        $newFrontendUserAksNodeSku = Read-Host "Enter Frontend User AKS Node SKU (Enter for '$frontendUserAksNodeSku'): "
        $newFrontendUserAksNodeSku = $newFrontendUserAksNodeSku.Trim()
        if ($newFrontendUserAksNodeSku -ne "") {
            $frontendUserAksNodeSku = $newFrontendUserAksNodeSku
            Write-Host "Setting to $frontendUserAksNodeSku for Frontend User AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
            azd env set FLLM_FRONTEND_AKS_NODE_SKU $frontendUserAksNodeSku
        }
    }

    $frontendSystemAksNodeSku = $(azd env get-value FLLM_FRONTEND_SYSTEM_AKS_NODE_SKU)
    if ($LastExitCode -ne 0) {
        $frontendSystemAksNodeSku = Read-Host "Enter Frontend System AKS Node SKU (Enter for Standard_D2_v5): "
        $frontendSystemAksNodeSku = $frontendSystemAksNodeSku.Trim()
        if ($frontendSystemAksNodeSku -eq "") {
            $frontendSystemAksNodeSku = "Standard_D2_v5"
            Write-Host "Defaulting to $frontendSystemAksNodeSku for Frontend System AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
        }            
        
        azd env set FLLM_FRONTEND_SYSTEM_AKS_NODE_SKU $frontendSystemAksNodeSku
    } else {
        $newFrontendSystemAksNodeSku = Read-Host "Enter Frontend System AKS Node SKU (Enter for '$frontendSystemAksNodeSku'): "
        $newFrontendSystemAksNodeSku = $newFrontendSystemAksNodeSku.Trim()
        if ($newFrontendSystemAksNodeSku -ne "") {
            $frontendSystemAksNodeSku = $newFrontendSystemAksNodeSku
            Write-Host "Setting to $frontendSystemAksNodeSku for Frontend System AKS Node SKU." -ForegroundColor Yellow
            Write-Host "Please ensure availablility and quota in your region." -ForegroundColor Yellow
            azd env set FLLM_FRONTEND_SYSTEM_AKS_NODE_SKU $frontendSystemAksNodeSku
        }
    }

    $vmAvailabilityZones = $(azd env get-value FLLM_VM_AVAILABILITY_ZONES)
    if ($LastExitCode -ne 0) {
        $vmAvailabilityZones = Read-Host "Enter VM Availability Zones (Enter for '1,2,3'): "
        $vmAvailabilityZones = $vmAvailabilityZones.Trim()
        if ($vmAvailabilityZones -eq "") {
            $vmAvailabilityZones = "1,2,3"
            Write-Host "Defaulting to $vmAvailabilityZones for VM Availability Zones." -ForegroundColor Yellow
        } elseif ($vmAvailabilityZones -notmatch "^(,*(\d{1,2}))+$") {
            throw "Invalid Availability Zones format. Please enter a valid format."
        }
        
        azd env set FLLM_VM_AVAILABILITY_ZONES $vmAvailabilityZones
    } else {
        $newVmAvailabilityZones = Read-Host "Enter VM Availability Zones (Enter for '$vmAvailabilityZones'): "
        $newVmAvailabilityZones = $newVmAvailabilityZones.Trim()
        if ($newVmAvailabilityZones -ne "" -and $newVmAvailabilityZones -notmatch "^(,*(\d{1,2}))+$") {
            throw "Invalid Availability Zones format. Please enter a valid format."
        } elseif ($newVmAvailabilityZones -ne "") {
            $vmAvailabilityZones = $newVmAvailabilityZones
            Write-Host "Setting to $vmAvailabilityZones for VM Availability Zones." -ForegroundColor Yellow
            azd env set FLLM_VM_AVAILABILITY_ZONES $vmAvailabilityZones
        }
    }

    Write-Host "Backend User AKS Node SKU: $backendUserAksNodeSku" -ForegroundColor Green
    Write-Host "Backend System AKS Node SKU: $backendSystemAksNodeSku" -ForegroundColor Green
    Write-Host "Frontend User AKS Node SKU: $frontendUserAksNodeSku" -ForegroundColor Green
    Write-Host "Frontend System AKS Node SKU: $frontendSystemAksNodeSku" -ForegroundColor Green
    Write-Host "VM Availability Zones: $vmAvailabilityZones" -ForegroundColor Green
}

function Get-ProjectId {
    param()

    $projectId = $(azd env get-value FOUNDATIONALLM_PROJECT)
    if ($LastExitCode -ne 0) {
        $projectId = Read-Host "Enter Project ID: "
        $projectId = $projectId.Trim()
        if ($projectId -eq "") {
            throw "Project ID cannot be empty."
        }
        azd env set FOUNDATIONALLM_PROJECT $projectId
    } else {
        $newProjectId = Read-Host "Enter Project ID (Enter for '$projectId'): "
        $newProjectId = $newProjectId.Trim()
        if ($newProjectId -ne "") {
            $projectId = $newProjectId
            azd env set FOUNDATIONALLM_PROJECT $projectId
        }
    }

    Write-Host "Project ID: $projectId" -ForegroundColor Green
}

function Get-Location {
    param()

    $location = $(azd env get-value AZURE_LOCATION)
    if ($LastExitCode -ne 0) {
        $location = Read-Host "Enter Location (Enter for eastus2): "
        $location = $location.Trim()
        if ($location -eq "") {
            $location = "eastus2"
            Write-Host "Defaulting to $location for Location." -ForegroundColor Yellow
        }
        azd env set AZURE_LOCATION $location
    } else {
        $newLocation = Read-Host "Enter Location (Enter for '$location'): "
        $newLocation = $newLocation.Trim()
        if ($newLocation -ne "") {
            $location = $newLocation
            azd env set AZURE_LOCATION $location
        }
    }

    Write-Host "Location: $location" -ForegroundColor Green
}

function Get-ResourceGroups {
    param(
    )

    $workloads = Get-Content -Path "./config/workloads.json" -Raw | ConvertFrom-Json
    $projectId = $(azd env get-value FOUNDATIONALLM_PROJECT)
    $location = $(azd env get-value AZURE_LOCATION)
    $environment = $(azd env get-value AZURE_ENV_NAME)

    $resourceGroups = @{}
    foreach ($workload in $workloads) {
        $resourceGroup = azd env get-value "FLLM_$($workload.ToUpper())_RG"
        if ($LastExitCode -ne 0) {
            $resourceGroup = Read-Host "Enter Resource Group for $($workload) (Enter for rg-$environment-$location-$workload-$projectId): "
            $resourceGroup = $resourceGroup.Trim()
            if ($resourceGroup -eq "") {
                $resourceGroup = "rg-$environment-$location-$workload-$projectId"
                Write-Host "Defaulting to $resourceGroup for $($workload) Resource Group." -ForegroundColor Yellow
            }
            azd env set "FLLM_$($workload.ToUpper())_RG" $resourceGroup
        } else {
            $newResourceGroup = Read-Host "Enter Resource Group for $($workload) (Enter for '$resourceGroup'): "
            $newResourceGroup = $newResourceGroup.Trim()
            if ($newResourceGroup -ne "") {
                $resourceGroup = $newResourceGroup
                azd env set "FLLM_$($workload.ToUpper())_RG" $resourceGroup
            }
        }
        Write-Host "$($workload.ToUpper()) Resource Group: $resourceGroup" -ForegroundColor Green
        $resourceGroups[$workload] = $resourceGroup
    }

    foreach ($resourceGroup in $resourceGroups.GetEnumerator()) {
        Write-Host "$($resourceGroup.Key.ToUpper()) Resource Group: $($resourceGroup.Value)" -ForegroundColor Green
    }
}