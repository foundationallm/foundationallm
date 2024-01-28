#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$manifest = $(Get-Content ../scripts/Deployment-Manifest.json | ConvertFrom-Json)

$administratorObjectId = $manifest.adminObjectId
$environment = $manifest.environment
$location = $manifest.location
$project = $manifest.project
$regenerateScripts = $false
$script:chatUiClientSecret="CHAT-CLIENT-SECRET"
$script:coreApiClientSecret="CORE-CLIENT-SECRET"
$script:k8sNamespace=$manifest.k8sNamespace
$script:managementApiClientSecret="MGMT-CLIENT-SECRET"
$script:managementUiClientSecret="MGMT-CLIENT-SECRET"
$skipAgw = $false
$skipAgw = $false
$skipApp = $false
$skipDns = $false
$skipDns = $false
$skipNetworking = $false
$skipNetworking = $false
$skipOai = $false
$skipOps = $false
$skipOps = $false
$skipResourceGroups = $false
$skipResourceGroups = $false
$skipStorage = $false
$skipStorage = $false
$skipVec = $false
$subscription = $manifest.subscription
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)
$vnetName = $manifest.vnetName

properties {
    $actionGroupId = ""
    $applicationGateways = @{}
    $logAnalyticsWorkspaceId = ""
    $privateDnsZones = @{}
    $vnetId = ""
}

$resourceGroups = $manifest.resourceGroups
$createVpnGateway = $manifest.createVpnGateway

$deployments = @{}
$resourceGroups.PSObject.Properties | ForEach-Object {
    $deployments.Add($_.Name, "$($_.Value)-${timestamp}")
}

task default -depends Agw, Storage, App, DNS, Networking, OpenAI, Ops, ResourceGroups, Vec

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

    $script:applicationGateways = $(
        az deployment group show `
            --name $deployments["agw"] `
            --output json `
            --query properties.outputs.applicationGateways.value `
            --resource-group $resourceGroups.agw | ConvertFrom-Json
    ) | ConvertTo-Json -Compress
}

task App -depends Agw, ResourceGroups, Ops, Networking, DNS {
    if ($skipApp -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping app creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure app resources exist"
    $dnsZoneTypes = @("aks")
    $aksDnsZones = ($script:privateDnsZones | ConvertFrom-Json).where({ $dnsZoneTypes -Contains $_.key })
    $privateDnsZones = $("[$($aksDnsZones | ConvertTo-Json -Compress)]") | ConvertTo-Json
    $appGateways = $($script:applicationGateways | ConvertTo-Json -Compress)

    az deployment group create --name  $deployments["app"] `
                        --resource-group $resourceGroups.app `
                        --template-file ./app-rg.bicep `
                        --parameters actionGroupId=$script:actionGroupId `
                                    administratorObjectId=$administratorObjectId `
                                    agwResourceGroupName=$($resourceGroups.agw) `
                                    applicationGateways="$appGateways" `
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
                                    privateDnsZones=$privateDnsZones `
                                    project=$project `
                                    storageResourceGroupName=$($resourceGroups.storage) `
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

    $script:privateDnsZones = $(
        az deployment group show `
            --name $deployments["dns"] `
            --output json `
            --query properties.outputs.ids.value `
            --resource-group $resourceGroups.dns | ConvertFrom-Json
    ) | ConvertTo-Json -Compress

    if ($LASTEXITCODE -ne 0) {
        throw "The private DNS zone IDs could not be retrieved."
    }
}

task Networking -depends ResourceGroups {
    if ($skipNetworking -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Network Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure networking resources exist"

    if ($vnetName -eq $null) {
        $vnetName = "vnet-${environment}-${location}-net"
    }

    az deployment group create `
        --name $deployments["net"] `
        --parameters `
            environmentName=$environment `
            location=$location `
            project=$project `
            createVpnGateway=$createVpnGateway `
            vnetName=$vnetName `
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

    Write-Host -ForegroundColor Blue "Checking deployment prerequisites..."
    if ( $script:logAnalyticsWorkspaceId -eq "") {
        Write-Host -ForegroundColor Yellow "Log Analytics Workspace not found.  Attempting to locate it..."
        $script:logAnalyticsWorkspaceId = $(
            az monitor log-analytics workspace show `
                --resource-group $resourceGroups.ops `
                --workspace-name "la-${project}-${environment}-${location}-ops" `
                --query id `
                --output tsv
        )
    }
    else {
        # psake remembers this in-between invocations 🤯
        Write-Host -ForegroundColor Blue "Log Analytics Workspace found: ${script:logAnalyticsWorkspaceId}."
    }

    $dnsZoneTypes = @("cognitiveservices","gateway_developer","gateway_management","gateway_portal","gateway_public","gateway_scm","openai","vault")
    $openAiDnsZones = ($script:privateDnsZones | ConvertFrom-Json).where({ $dnsZoneTypes -Contains $_.key })
    $privateDnsZones = $($openAiDnsZones | ConvertTo-Json -Compress) | ConvertTo-Json

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
                        privateDnsZones=$privateDnsZones `
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

    $opsZones = $($script:privateDnsZones | ConvertTo-Json -Compress)

    az deployment group create `
        --name $deployments["ops"] `
        --resource-group $resourceGroups.ops `
        --template-file ./ops-rg.bicep `
        --parameters `
            administratorObjectId=$administratorObjectId `
            environmentName=$environment `
            location=$location `
            privateDnsZones=$opsZones `
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

    $resourceGroups.PSObject.Properties | ForEach-Object {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $($_.Value))) {
            Write-Host "The resource group $($_.Value) was not found, creating it..."
            az group create -g $($_.Value) -l $location --subscription $subscription

            if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $($_.Value))) {
                throw "The resource group $($_.Value) was not found, and could not be created."
            }
        }
        else {
            Write-Host -ForegroundColor Blue "The resource group $($_.Value) was found."
        }
    }
}

task Storage -depends ResourceGroups, Ops, Networking, DNS {
    if ($skipStorage -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Storage creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure Storage resources exist"
    $dnsZoneTypes = @("blob","cosmosdb","dfs","file","queue","table","web")
    $storageDnsZones = ($script:privateDnsZones | ConvertFrom-Json).where({ $dnsZoneTypes -Contains $_.key })
    $privateDnsZones = $($storageDnsZones | ConvertTo-Json -Compress) | ConvertTo-Json

    az deployment group create `
        --name $deployments["storage"] `
        --resource-group $resourceGroups.storage `
        --template-file ./storage-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            opsResourceGroupName=$($resourceGroups.ops) `
            privateDnsZones=$privateDnsZones `
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
    $dnsZoneTypes = @("search")
    $vecDnsZones = ($script:privateDnsZones | ConvertFrom-Json).where({ $dnsZoneTypes -Contains $_.key })
    $privateDnsZones = $("[$($vecDnsZones | ConvertTo-Json -Compress)]") | ConvertTo-Json

    az deployment group create `
        --name $deployments.vec `
        --resource-group $resourceGroups.vec `
        --template-file ./vec-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            opsResourceGroupName=$($resourceGroups.ops) `
            privateDnsZones=$privateDnsZones `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}