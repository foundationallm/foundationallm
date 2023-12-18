#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$aksAdmnistratorObjectId = "73d59f98-857b-45e7-950b-5ee30d289bc8"
$environment = "demo"
$location = "eastus"
$project = "fllm"
$skipApp = $false
$skipDns = $false
$skipNetworking = $false
$skipOai = $false
$skipOps = $false
$skipResourceGroups = $false
$skipStorage = $false
$skipVec = $false
$skipAgw = $false
$subscription = "4dae7dc4-ef9c-4591-b247-8eacb27f3c9e"
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

properties {
    $actionGroupId = ""
    $applicationGateways = @{}
    $logAnalyticsWorkspaceId = ""
    $privateDnsZoneId = @{}
    $vnetId = ""
}

$resourceGroups = @{
    agw     = "rg-${environment}-${location}-agw-${project}"
    app     = "rg-${environment}-${location}-app-${project}"
    data    = "rg-${environment}-${location}-data-${project}"
    dns     = "rg-${environment}-${location}-dns-${project}"
    jbx     = "rg-${environment}-${location}-jbx-${project}"
    net     = "rg-${environment}-${location}-net-${project}"
    oai     = "rg-${environment}-${location}-oai-${project}"
    ops     = "rg-${environment}-${location}-ops-${project}"
    storage = "rg-${environment}-${location}-storage-${project}"
    vec     = "rg-${environment}-${location}-vec-${project}"
}

$deployments = @{}
foreach ($resourceGroup in $resourceGroups.GetEnumerator()) {
    $deployments.Add($resourceGroup.Name, "$($resourceGroup.Value)-${timestamp}")
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

    $script:applicationGateways = $(
        az deployment group show `
            --name $deployments["agw"] `
            --output json `
            --query properties.outputs.applicationGateways.value `
            --resource-group $resourceGroups["agw"]
    )
}

task App -depends Agw, ResourceGroups, Ops, Networking, DNS {
    if ($skipApp -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping app creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure app resources exist"
    $privateDnsZones = $($script:privateDnsZoneId | ConvertTo-Json -Compress)


    az deployment group create `
        --name $deployments["app"] `
        --resource-group $resourceGroups["app"] `
        --template-file ./app-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        aksAdmnistratorObjectId=$aksAdmnistratorObjectId `
        applicationGateways=$script:applicationGateways `
        environmentName=$environment `
        location=$location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        logAnalyticsWorkspaceResourceId=$script:logAnalyticsWorkspaceId `
        networkingResourceGroupName="$($resourceGroups["net"])" `
        privateDnsZones=$privateDnsZones `
        project=$project `
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
        --parameters environmentName=$environment location=$location project=$project vnetId=$script:vnetId `
        --resource-group $resourceGroups["dns"] `
        --template-file ./dns-rg.bicep 

    if ($LASTEXITCODE -ne 0) {
        throw "The DNS deployment failed."
    }

    $script:privateDnsZoneId = $(
        az deployment group show `
            --name $deployments["dns"] `
            --output json `
            --query properties.outputs.ids.value `
            --resource-group $resourceGroups["dns"] | ConvertFrom-Json
    )

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

    az deployment group create `
        --name $deployments["net"] `
        --parameters environmentName=$environment location=$location project=$project `
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
                --workspace-name "la-${environment}-${location}-ops-${project}" `
                --query id `
                --output tsv 
        )
    }
    else {
        # psake remembers this in-between invocations 🤯
        Write-Host -ForegroundColor Blue "Log Analytics Workspace found: ${script:logAnalyticsWorkspaceId}."
    }

    $privateDnsZones = $($script:privateDnsZoneId | ConvertTo-Json -Compress)
    
    Write-Host -ForegroundColor Blue "Ensure OpenAI accounts exist"

    az deployment group create `
        --name $deployments["oai"] `
        --resource-group $resourceGroups["oai"] `
        --template-file ./openai-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        environmentName=$environment `
        location=$location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
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

    $opsZones = $($script:privateDnsZoneId | ConvertTo-Json -Compress)

    az deployment group create `
        --name $deployments["ops"] `
        --resource-group $resourceGroups["ops"] `
        --template-file ./ops-rg.bicep `
        --parameters `
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

task ResourceGroups {
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

task Storage -depends ResourceGroups, Ops, Networking, DNS {
    if ($skipStorage -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Storage creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure Storage resources exist"
    $privateDnsZones = $($script:privateDnsZoneId | ConvertTo-Json -Compress)

    az deployment group create `
        --name $deployments["storage"] `
        --resource-group $resourceGroups["storage"] `
        --template-file ./storage-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        environmentName=$environment `
        location=$location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
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
    $privateDnsZones = $($script:privateDnsZoneId | ConvertTo-Json -Compress)

    az deployment group create `
        --name $deployments["vec"] `
        --resource-group $resourceGroups["vec"] `
        --template-file ./vec-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        environmentName=$environment `
        location=$location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        privateDnsZones=$privateDnsZones `
        project=$project `
        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}