#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$script:chatUiClientSecret="CHAT-CLIENT-SECRET"
$script:coreApiClientSecret="CORE-CLIENT-SECRET"
$script:managementUiClientSecret="MGMT-CLIENT-SECRET"
$script:managementApiClientSecret="MGMT-CLIENT-SECRET"
$script:k8sNamespace="default"
$administratorObjectId = "d3bd4e8e-d413-477d-a420-0792b0504adf"
$environment = "stg"
$location = "eastus2"
$project = "wtw01"
$skipResourceGroups = $false
$skipNetworking = $false
$skipOps = $false
$skipDns = $false
$skipAgw = $false
$skipStorage = $false
$regenerateScripts = $false
$skipApp = $false
$skipOai = $false
$skipVec = $false

$createVpnGateway = $true

$subscription = "0a03d4f9-c6e4-4ee1-87fb-e2005d2c213d"
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

properties {
    $agws = @{}
    $actionGroupId = ""
    $applicationGateways = @{}
    $logAnalyticsWorkspaceId = ""
    $privateDnsZoneId = @{}
    $vnetId = ""
}

$resourceGroups = @{
    agw     = "EBTICP-D-NA24-AIAGW-RGRP"
    app     = "EBTICP-D-NA24-AIApp-RGRP"
    data    = "EBTICP-D-NA24-AIData-RGRP"
    dns     = "EBTICP-D-NA24-AIDNS-RGRP"
    jbx     = "EBTICP-D-NA24-AIJBX-RGRP"
    net     = "EBTICP-D-NA24-AI-RGRP"
    oai     = "EBTICP-D-NA24-AIOpenAI-RGRP"
    ops     = "EBTICP-D-NA24-AIOps-RGRP"
    storage = "EBTICP-D-NA24-AIStorage-RGRP"
    vec     = "EBTICP-D-NA24-AIVector-RGRP"
}

$dnsResourceGroupName = $resourceGroups["dns"]

$vnetName = "EBTICP-D-NA24-AI-VNET"

$deployments = @{}
foreach ($resourceGroup in $resourceGroups.GetEnumerator()) {
    $deployments.Add($resourceGroup.Name, "$($resourceGroup.Value)-${timestamp}")
}
    #DPS:  appGateways were breaking when the prior script was used, so 'hard coded' here
    #$appGateways =  "[{`"key`":`"api`",`"id`":`"/subscriptions/$($subscription)/resourceGroups/EBTICP-D-NA24-AIAGW-RGRP/providers/Microsoft.Network/applicationGateways/agw-api-dev-canadaeast-agw-wtwAI`",`"resourceGroup`":`"EBTICP-D-NA24-AIAGW-RGRP`"},{`"key`":`"www`",`"id`":`"/subscriptions/$($subscription)/resourceGroups/EBTICP-D-NA24-AIAGW-RGRP/providers/Microsoft.Network/applicationGateways/agw-www-dev-canadaeast-agw-wtwAI`",`"resourceGroup`":`"EBTICP-D-NA24-AIAGW-RGRP`"}]"
    #$script:applicationGateways = $appGateways
    #DPS:  moved zones here for re-use compatible with secondary executions & skip logic
    $zones       =  "[{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.agentsvc.azure-automation.net`",`"key`":`"agentsvc`",`"name`":`"privatelink.agentsvc.azure-automation.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.canadaeast.azmk8s.io`",`"key`":`"aks`",`"name`":`"privatelink.canadaeast.azmk8s.io`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.blob.core.windows.net`",`"key`":`"blob`",`"name`":`"privatelink.blob.core.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.cognitiveservices.azure.com`",`"key`":`"cognitiveservices`",`"name`":`"privatelink.cognitiveservices.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.azconfig.io`",`"key`":`"configuration_stores`",`"name`":`"privatelink.azconfig.io`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.documents.azure.com`",`"key`":`"cosmosdb`",`"name`":`"privatelink.documents.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.azurecr.io`",`"key`":`"cr`",`"name`":`"privatelink.azurecr.io`"},{`"id`":`"/subscriptions/4dae7dc4-ef9c-4591-b247-8eacb27f3c9e/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/canadaeast.privatelink.azurecr.io`",`"key`":`"cr_region`",`"name`":`"canadaeast.privatelink.azurecr.io`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.dfs.core.windows.net`",`"key`":`"dfs`",`"name`":`"privatelink.dfs.core.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.file.core.windows.net`",`"key`":`"file`",`"name`":`"privatelink.file.core.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.azure-api.net`",`"key`":`"gateway`",`"name`":`"privatelink.azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/developer.azure-api.net`",`"key`":`"gateway_developer`",`"name`":`"developer.azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/management.azure-api.net`",`"key`":`"gateway_management`",`"name`":`"management.azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/portal.azure-api.net`",`"key`":`"gateway_portal`",`"name`":`"portal.azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/azure-api.net`",`"key`":`"gateway_public`",`"name`":`"azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/scm.azure-api.net`",`"key`":`"gateway_scm`",`"name`":`"scm.azure-api.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.monitor.azure.com`",`"key`":`"monitor`",`"name`":`"privatelink.monitor.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.ods.opinsights.azure.com`",`"key`":`"ods`",`"name`":`"privatelink.ods.opinsights.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.oms.opinsights.azure.com`",`"key`":`"oms`",`"name`":`"privatelink.oms.opinsights.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.prometheus.monitor.azure.com`",`"key`":`"openai`",`"name`":`"privatelink.openai.azure.com`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.queue.core.windows.net`",`"key`":`"queue`",`"name`":`"privatelink.queue.core.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.search.windows.net`",`"key`":`"search`",`"name`":`"privatelink.search.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.azurewebsites.net`",`"key`":`"sites`",`"name`":`"privatelink.azurewebsites.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.database.windows.net`",`"key`":`"sql_server`",`"name`":`"privatelink.database.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.table.core.windows.net`",`"key`":`"table`",`"name`":`"privatelink.table.core.windows.net`"},{`"id`":`"/subscriptions/$($subscription)/resourceGroups/$($dnsResourceGroupName)/providers/Microsoft.Network/privateDnsZones/privatelink.vaultcore.azure.net`",`"key`":`"vault`",`"name`":`"privatelink.vaultcore.azure.net`"}]" 
    $script:privateDnsZones = $zones 
    
task default -depends Agw, Storage, App, DNS, Networking, OpenAI, Ops, ResourceGroups, Vec

task Agw -depends ResourceGroups, Ops, Networking {
    if ($skipAgw -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping agw creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure agw resources exist"

    az deployment group create `
        --name $deployments["agw"] `
        --resource-group $resourceGroups["agw"] `
        --template-file ./agw-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        environmentName=$environment `
        location=$location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        networkingResourceGroupName="$($resourceGroups["net"])" `
        opsResourceGroupName="$($resourceGroups["ops"])" `
        project=$project `
        vnetId=$script:vnetId 

    if ($LASTEXITCODE -ne 0) {
        throw "The agw deployment failed."
    }
   
    $script:agws = $(
        az deployment group show `
            --name $deployments["agw"] `
            --output json `
            --query properties.outputs.applicationGateways.value `
            --resource-group $resourceGroups["agw"] | ConvertFrom-Json) | ConvertTo-Json -Compress

    if ($LASTEXITCODE -ne 0) {
        throw "The Application Gateways could not be retrieved."
    }
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
    $appGateways = $($script:agws | ConvertTo-Json -Compress)

    az deployment group create --name  $deployments["app"] `
                        --resource-group $resourceGroups["app"] `
                        --template-file ./app-rg.bicep `
                        --parameters actionGroupId=$script:actionGroupId `
                                    administratorObjectId=$administratorObjectId `
                                    agwResourceGroupName=$($resourceGroups["agw"]) `
                                    applicationGateways="$appGateways" `
                                    chatUiClientSecret=$script:chatUiClientSecret `
                                    coreApiClientSecret=$script:coreApiClientSecret `
                                    dnsResourceGroupName=$dnsResourceGroupName `
                                    environmentName=$environment `
                                    k8sNamespace=$script:k8sNamespace `
                                    location=$location `
                                    logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
                                    logAnalyticsWorkspaceResourceId=$script:logAnalyticsWorkspaceId `
                                    managementUiClientSecret=$script:managementUiClientSecret `
                                    managementApiClientSecret=$script:managementApiClientSecret `
                                    networkingResourceGroupName=$($resourceGroups["net"]) `
                                    opsResourceGroupName=$($resourceGroups["ops"]) `
                                    privateDnsZones=$privateDnsZones `
                                    project=$project `
                                    storageResourceGroupName=$($resourceGroups["storage"]) `
                                    vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The app deployment failed."
    }

}

#DpS:  Added respect for $skipDns Flag
task DNS -depends ResourceGroups, Networking {
    if ($skipDns -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping DNS creation."
        return;
    }

    az deployment group create `
        --name $deployments["dns"] `
        --parameters `
            environmentName=$environment `
            location=$location `
            project=$project `
            vnetId=$script:vnetId `
        --resource-group $resourceGroups["dns"] `
        --template-file ./dns-rg.bicep

    if ($LASTEXITCODE -ne 0) {
        throw "The Private DNS Zones deployment failed."
    }

    $script:privateDnsZones = $(
        az deployment group show `
            --name $deployments["dns"] `
            --output json `
            --query properties.outputs.ids.value `
            --resource-group $resourceGroups["dns"] | ConvertFrom-Json) | ConvertTo-Json -Compress

    if ($LASTEXITCODE -ne 0) {
        throw "The Private DNS Zones could not be retrieved."
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
            vnetName=$vnetName `
        --resource-group $resourceGroups["net"] `
        --template-file ./networking-rg.bicep 

    if ($LASTEXITCODE -ne 0) {
        throw "The networking deployment failed."
    }

    $script:vnetId = $(
        az deployment group show `
            --name $deployments["net"] `
            --output tsv `
            --query properties.outputs.vnetId.value `
            --resource-group $resourceGroups["net"] 
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
                --resource-group $resourceGroups["ops"] `
                --workspace-name "la-${environment}-${location}-ops" `
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

    az deployment group create --name $deployments["oai"]`
        --resource-group  $resourceGroups["oai"] `
        --template-file ./openai-rg.bicep `
        --parameters actionGroupId=$script:actionGroupId `
                        administratorObjectId=$administratorObjectId `
                        dnsResourceGroupName=$dnsResourceGroupName `
                        environmentName=$environment `
                        location=$location `
                        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
                        opsResourceGroupName=$($resourceGroups["ops"]) `
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
    #DPS:  Attempt to resolve issue (doesn't seem to work due to purge protection) where app config fails on re-deploy when already exists
    $resourceSuffix = "$environment-$location-ops"
    az appconfig delete --name "appConfig-$resourceSuffix" --resource-group $resourceGroups["ops"]  --yes
    $opsZones = $($script:privateDnsZones | ConvertTo-Json -Compress)

    az deployment group create `
        --name $deployments["ops"] `
        --resource-group $resourceGroups["ops"] `
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
            --resource-group $resourceGroups["ops"] 
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Action Group ID could not be retrieved."
    }

    $script:logAnalyticsWorkspaceId = $(
        az deployment group show `
            --name $deployments["ops"] `
            --output tsv `
            --query properties.outputs.logAnalyticsWorkspaceId.value `
            --resource-group $resourceGroups["ops"] 
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Log Analytics Workspace ID could not be retrieved."
    }
}

task ResourceGroups -depends SetSubscription {
    if ($skipResourceGroups -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping resource group creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure resource groups exist"

    foreach ($resourceGroup in $resourceGroups.values) {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup)) {
            Write-Host "The resource group $resourceGroup was not found, creating it..."
            az group create -g $resourceGroup -l $location --subscription $subscription

            if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup)) {
                throw "The resource group $resourceGroup was not found, and could not be created."
            } 
        }
        else {
            Write-Host -ForegroundColor Blue "The resource group $resourceGroup was found."
        }
    }
}

task SetSubscription {
    az account set -s $($subscription)
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

    az deployment group create --name $deployments["storage"] `
        --resource-group $resourceGroups["storage"] `
        --template-file ./storage-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            opsResourceGroupName=$($resourceGroups["ops"]) `
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
        --name $deployments["vec"] `
        --resource-group $resourceGroups["vec"] `
        --template-file ./vec-rg.bicep `
        --parameters `
            actionGroupId=$script:actionGroupId `
            environmentName=$environment `
            location=$location `
            logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
            opsResourceGroupName=$($resourceGroups["ops"]) `
            privateDnsZones=$privateDnsZones `
            project=$project `
            vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}