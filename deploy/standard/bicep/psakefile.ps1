#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$manifest = $(Get-Content ../Deployment-Manifest.json | ConvertFrom-Json)

$administratorObjectId = $manifest.adminObjectId
$environment = $manifest.environment
$location = $manifest.location
$project = $manifest.project
$regenerateScripts = $false
$script:chatUiClientSecret="CHAT-CLIENT-SECRET"
$script:coreApiClientSecret="CORE-API-CLIENT-SECRET"
$script:k8sNamespace=$manifest.k8sNamespace
$script:managementApiClientSecret="MGMT-API-CLIENT-SECRET"
$script:managementUiClientSecret="MGMT-CLIENT-SECRET"
$script:vectorizationApiClientSecret="VEC-API-CLIENT-SECRET"
$skipAgw = $false
$skipApp = $false
$skipDns = $false
$skipNetworking = $false
$skipOai = $false
$skipOps = $false
$skipResourceGroups = $false
$skipStorage = $false
$skipVec = $false
$subscription = $manifest.subscription
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

properties {
    $actionGroupId = ""
    $logAnalyticsWorkspaceId = ""
    $vnetId = ""
}

$resourceGroups = $manifest.resourceGroups
$createVpnGateway = $manifest.createVpnGateway

$deployments = @{}
$resourceGroups.PSObject.Properties | ForEach-Object {
    $deployments.Add($_.Name, "$($_.Value)-${timestamp}")
}

task default -depends Agw, Storage, App, DNS, Networking, OpenAI, Ops, ResourceGroups, Vec

task Clean {
    Write-Host -ForegroundColor Blue "Deleting all resource groups..."

    foreach ($property in $resourceGroups.PSObject.Properties) {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $property.Value)) {
            Write-Host -ForegroundColor Blue "The resource group $($property.Value) was not found."
        }
        else {
            Write-Host -ForegroundColor Magenta "Deleting $($property.Value)..."
            az group delete `
                --name $property.Value `
                --yes `
                --no-wait
        }
    }

    $deleteMessage = @"
Sent delete requests for all resource groups.  Deletion can take up to an hour.
Some resources are only soft-deleted and may need to be purged to completely remove them.
Check the Azure Portal for status.
"@

    Write-Host -ForegroundColor Blue $deleteMessage
}

task Agw -depends ResourceGroups, Ops, Networking {
    if ($skipAgw -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping agw creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure agw resources exist"

    az deployment group create `
        --name $deployments["agw"] `
        --resource-group $resourceGroups.agw `
        --template-file ./agw-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            networkingResourceGroupName="$($resourceGroups.net)" `
            opsResourceGroupName="$($resourceGroups.ops)" `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The agw deployment failed."
    }
}

task App -depends Agw, ResourceGroups, Ops, Networking, DNS {
    if ($skipApp -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping app creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure app resources exist"

    az deployment group create --name  $deployments["app"] `
                        --resource-group $resourceGroups.app `
                        --template-file ./app-rg.bicep `
                        --parameters actionGroupId=$script:actionGroupId `
                                    administratorObjectId=$administratorObjectId `
                                    agwResourceGroupName=$($resourceGroups.agw) `
                                    chatUiClientSecret=$script:chatUiClientSecret `
                                    coreApiClientSecret=$script:coreApiClientSecret `
                                    dnsResourceGroupName=$($resourceGroups.dns) `
                                    environmentName=$environment `
                                    k8sNamespace=$script:k8sNamespace `
                                    location=$location `
                                    logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
                                    logAnalyticsWorkspaceResourceId=$script:logAnalyticsWorkspaceId `
                                    managementUiClientSecret=$script:managementUiClientSecret `
                                    managementApiClientSecret=$script:managementApiClientSecret `
                                    networkingResourceGroupName=$($resourceGroups.net) `
                                    opsResourceGroupName=$($resourceGroups.ops) `
                                    project=$project `
                                    storageResourceGroupName=$($resourceGroups.storage) `
                                    vectorizationApiClientSecret=$script:vectorizationApiClientSecret `
                                    vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The app deployment failed."
    }
}

task DNS -depends ResourceGroups, Networking {
    if ($skipDns -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping DNS Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure DNS resources exist"

    az deployment group create `
        --name $deployments["dns"] `
        --parameters `
            environmentName=$environment `
            location=$location `
            project=$project `
            vnetId=$script:vnetId `
        --resource-group $resourceGroups.dns `
        --template-file ./dns-rg.bicep

    if ($LASTEXITCODE -ne 0) {
        throw "The DNS deployment failed."
    }
}

task Networking -depends ResourceGroups {
    if ($skipNetworking -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Network Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure networking resources exist"

    az deployment group create `
        --name $deployments["net"] `
        --parameters `
            environmentName=$environment `
            location=$location `
            project=$project `
            createVpnGateway=$createVpnGateway `
        --resource-group $resourceGroups.net `
        --template-file ./networking-rg.bicep

    if ($LASTEXITCODE -ne 0) {
        throw "The networking deployment failed."
    }

    $script:vnetId = $(
        az deployment group show `
            --name $deployments["net"] `
            --output tsv `
            --query properties.outputs.vnetId.value `
            --resource-group $resourceGroups.net
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The VNet ID could not be retrieved."
    }
}

task OpenAI -depends ResourceGroups, Ops, Networking, DNS {
    if ($skipOai -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping OpenAI Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure OpenAI accounts exist"

    az deployment group create --name $deployments.oai `
        --resource-group  $resourceGroups.oai `
        --template-file ./openai-rg.bicep `
        --parameters actionGroupId=$script:actionGroupId `
                        administratorObjectId=$administratorObjectId `
                        dnsResourceGroupName=$($resourceGroups.dns) `
                        environmentName=$environment `
                        location=$location `
                        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
                        opsResourceGroupName=$($resourceGroups.ops) `
                        project=$project `
                        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The OpenAI deployment failed."
    }
}

task Ops -depends ResourceGroups, Networking, DNS {
    if ($skipOps -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping ops creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure ops resources exist"

    az deployment group create `
        --name $deployments["ops"] `
        --resource-group $resourceGroups.ops `
        --template-file ./ops-rg.bicep `
        --parameters `
            administratorObjectId=$administratorObjectId `
            dnsResourceGroupName=$($resourceGroups.dns) `
            environmentName=$environment `
            location=$location `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The ops deployment failed."
    }

    $script:actionGroupId = $(
        az deployment group show `
            --name $deployments["ops"] `
            --output tsv `
            --query properties.outputs.actionGroupId.value `
            --resource-group $resourceGroups.ops
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Action Group ID could not be retrieved."
    }

    $script:logAnalyticsWorkspaceId = $(
        az deployment group show `
            --name $deployments["ops"] `
            --output tsv `
            --query properties.outputs.logAnalyticsWorkspaceId.value `
            --resource-group $resourceGroups.ops
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Log Analytics Workspace ID could not be retrieved."
    }
}

task ResourceGroups {
    if ($skipResourceGroups -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping resource group creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure resource groups exist"

    foreach ($property in $resourceGroups.PSObject.Properties) {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $property.Value)) {
            Write-Host "The resource group $($property.Value) was not found, creating it..."
            az group create -g $property.Value -l $location --subscription $subscription

            if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $property.Value)) {
                throw "The resource group $($property.Value) was not found, and could not be created."
            }
        }
        else {
            Write-Host -ForegroundColor Blue "The resource group $($property.Value) was found."
        }
    }
}

task Storage -depends ResourceGroups, Ops, Networking, DNS {
    if ($skipStorage -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Storage creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure Storage resources exist"

    az deployment group create `
        --name $deployments["storage"] `
        --resource-group $resourceGroups.storage `
        --template-file ./storage-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            dnsResourceGroupName=$($resourceGroups.dns) `
            opsResourceGroupName=$($resourceGroups.ops) `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The storage deployment failed."
    }
}

task Vec -depends ResourceGroups, Ops, Networking, DNS {
    if ($skipVec -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Vec creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure vec resources exist"

    az deployment group create `
        --name $deployments.vec `
        --resource-group $resourceGroups.vec `
        --template-file ./vec-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            dnsResourceGroupName=$($resourceGroups.dns) `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            opsResourceGroupName=$($resourceGroups.ops) `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}