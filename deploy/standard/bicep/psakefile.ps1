#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$skipApp = $false
$skipDns = $false
$skipNetworking = $false
$skipOai = $false
$skipOps = $false
$skipResourceGroups = $false
$skipStorage = $false
$skipVec = $false
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

# Properties
properties {
    # Override example: Invoke-Psake -properties @{ "manifestName" = "My-Deployment-Manifest.json" }
    $manifestName = "Deployment-Manifest.json"
}

task default -depends Storage, App, DNS, Networking, OpenAI, Ops, ResourceGroups, Vec, Configuration

task App -depends ResourceGroups, Ops, Networking, DNS, Configuration, Vec {
    if ($skipApp -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping app creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure app resources exist"

    az deployment group create --name  $script:deployments["app"] `
        --resource-group $resourceGroups.app `
        --template-file ./app-rg.bicep `
        --parameters actionGroupId=$script:actionGroupId `
        administratorObjectId=$script:administratorObjectId `
        chatUiClientSecret=$script:chatUiClientSecret `
        coreApiClientSecret=$script:coreApiClientSecret `
        dnsResourceGroupName=$($resourceGroups.dns) `
        environmentName=$script:environment `
        k8sNamespace=$script:k8sNamespace `
        location=$script:location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        logAnalyticsWorkspaceResourceId=$script:logAnalyticsWorkspaceId `
        managementUiClientSecret=$script:managementUiClientSecret `
        managementApiClientSecret=$script:managementApiClientSecret `
        networkingResourceGroupName=$($script:resourceGroups.net) `
        opsResourceGroupName=$($script:resourceGroups.ops) `
        vectorizationResourceGroupName=$($script:resourceGroups.vec) `
        project=$script:project `
        storageResourceGroupName=$($script:resourceGroups.storage) `
        vectorizationApiClientSecret=$script:vectorizationApiClientSecret `
        vnetName=$script:vnetName

    if ($LASTEXITCODE -ne 0) {
        throw "The app deployment failed."
    }
}

task Clean -depends Configuration {
    Write-Host -ForegroundColor Blue "Removing OpenAI model deployments..."

    # List OpenAI resources in the oai resource group
    $openAIResources = az cognitiveservices account list `
        --resource-group $resourceGroups.oai `
        --output json | ConvertFrom-Json

    # for each resource, list the deployments
    foreach ($resource in $openAIResources) {
        $deployments = az cognitiveservices account deployment list `
            --name $resource.name `
            --resource-group $resourceGroups.oai `
            --output json | ConvertFrom-Json

        # for each deployment, delete it
        foreach ($deployment in $deployments) {
            Write-Host -ForegroundColor Magenta "Deleting $($deployment.name) in $($resource.name)..."
            az cognitiveservices account deployment delete `
                --name $resource.name `
                --resource-group $resourceGroups.oai `
                --deployment-name $deployment.name `
                --output json
        }
    }

    Write-Host -ForegroundColor Blue "Deleting all resource groups..."

    foreach ($property in $script:resourceGroups.GetEnumerator()) {
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

task Configuration {
    Write-Host -ForegroundColor Blue "Loading Deployment Manifest ../$($manifestName)"
    $manifest = $(Get-Content -Raw -Path ../$($manifestName) | ConvertFrom-Json)

    $resourceGroups = $manifest.resourceGroups

    $script:administratorObjectId = $manifest.adminObjectId
    $script:chatUiClientSecret = "CHAT-CLIENT-SECRET"
    $script:coreApiClientSecret = "CORE-API-CLIENT-SECRET"
    $script:createVpnGateway = $manifest.createVpnGateway
    $script:environment = $manifest.environment
    $script:k8sNamespace = $manifest.k8sNamespace
    $script:location = $manifest.location
    $script:managementApiClientSecret = "MGMT-API-CLIENT-SECRET"
    $script:managementUiClientSecret = "MGMT-CLIENT-SECRET"
    $script:project = $manifest.project
    $script:subscription = $manifest.subscription
    $script:vectorizationApiClientSecret = "VEC-API-CLIENT-SECRET"

    $script:deployments = @{}
    $script:resourceGroups = @{}
    foreach ($property in $resourceGroups.PSObject.Properties) {
        $deployments.Add($property.Name, "$($property.Value)-${timestamp}")
        $script:resourceGroups.Add($property.Name, $property.Value)
    }
    Write-Host -ForegroundColor Blue "Configuration complete."
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

task Auth -depends App, ResourceGroups, Networking, DNS {
    if ($skipAuth -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Auth creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure Auth resources exist"

    az deployment group create --name  $deployments["auth"] `
                        --resource-group $resourceGroups.auth `
                        --template-file ./auth-rg.bicep `
                        --parameters actionGroupId=$script:actionGroupId `
                                    administratorObjectId=$administratorObjectId `
                                    appResourceGroupName=$($resourceGroups.app) `
                                    dnsResourceGroupName=$($resourceGroups.dns) `
                                    opsResourceGroupName=$($resourceGroups.ops) `
                                    authAppRegistrationInstance=$($manifest.entraInstances.authorization) `
                                    authAppRegistrationTenantId=$($manifest.tenantId) `
                                    authAppRegistrationClientId=$($manifest.entraClientIds.authorization) `
                                    authClientSecret=$($manifest.entraClientSecrets.authorization) `
                                    authAppRegistrationScopes=$($manifest.entraClientScopes.authorization) `
                                    instanceId=$($manifest.instanceId) `
                                    environmentName=$environment `
                                    k8sNamespace=$script:k8sNamespace `
                                    location=$location `
                                    logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
                                    project=$project `
                                    vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The auth deployment failed."
    }
}

task DNS -depends ResourceGroups, Networking {
    if ($skipDns -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping DNS Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure DNS resources exist"

    az deployment group create `
        --name $script:deployments["dns"] `
        --parameters `
        environmentName=$script:environment `
        location=$script:location `
        project=$script:project `
        networkResourceGroupName=$($script:resourceGroups.net) `
        vnetName=$script:vnetName `
        --resource-group $script:resourceGroups.dns `
        --template-file ./dns-rg.bicep

    if ($LASTEXITCODE -ne 0) {
        throw "The DNS deployment failed."
    }
}

task Networking -depends ResourceGroups, Configuration {
    if ($skipNetworking -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Network Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure networking resources exist"

    az deployment group create `
        --name $script:deployments["net"] `
        --parameters `
        environmentName=$script:environment `
        location=$script:location `
        project=$script:project `
        createVpnGateway=$script:createVpnGateway `
        --resource-group $script:resourceGroups.net `
        --template-file ./networking-rg.bicep

    if ($LASTEXITCODE -ne 0) {
        throw "The networking deployment failed."
    }

    $vnet = $(
        az deployment group show `
            --name $script:deployments["net"] `
            --output json `
            --query "{id:properties.outputs.vnetId.value,name:properties.outputs.vnetName.value}" `
            --resource-group $script:resourceGroups.net | `
            ConvertFrom-Json
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The VNet details could not be retrieved."
    }

    $script:vnetId = $vnet.id
    $script:vnetName = $vnet.name
}

task OpenAI -depends ResourceGroups, Ops, Networking, DNS, Configuration {
    if ($skipOai -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping OpenAI Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure OpenAI accounts exist"

    az deployment group create --name $script:deployments.oai `
        --resource-group  $script:resourceGroups.oai `
        --template-file ./openai-rg.bicep `
        --parameters actionGroupId=$script:actionGroupId `
        administratorObjectId=$script:administratorObjectId `
        dnsResourceGroupName=$($script:resourceGroups.dns) `
        environmentName=$script:environment `
        location=$script:location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        opsResourceGroupName=$($script:resourceGroups.ops) `
        project=$script:project `
        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The OpenAI deployment failed."
    }
}

task Ops -depends ResourceGroups, Networking, DNS, Configuration {
    if ($skipOps -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping ops creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure ops resources exist"

    az deployment group create `
        --name $script:deployments["ops"] `
        --resource-group $script:resourceGroups.ops `
        --template-file ./ops-rg.bicep `
        --parameters `
        administratorObjectId=$script:administratorObjectId `
        dnsResourceGroupName=$($script:resourceGroups.dns) `
        environmentName=$script:environment `
        location=$script:location `
        project=$script:project `
        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The ops deployment failed."
    }

    $script:actionGroupId = $(
        az deployment group show `
            --name $script:deployments["ops"] `
            --output tsv `
            --query properties.outputs.actionGroupId.value `
            --resource-group $script:resourceGroups.ops
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Action Group ID could not be retrieved."
    }

    $script:logAnalyticsWorkspaceId = $(
        az deployment group show `
            --name $script:deployments["ops"] `
            --output tsv `
            --query properties.outputs.logAnalyticsWorkspaceId.value `
            --resource-group $script:resourceGroups.ops
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The Log Analytics Workspace ID could not be retrieved."
    }
}

task ResourceGroups -depends Configuration {
    if ($skipResourceGroups -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping resource group creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure resource groups exist"

    foreach ($property in $script:resourceGroups.GetEnumerator()) {
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

task Storage -depends ResourceGroups, Ops, Networking, DNS, Configuration {
    if ($skipStorage -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Storage creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure Storage resources exist"

    az deployment group create `
        --name $script:deployments["storage"] `
        --resource-group $script:resourceGroups.storage `
        --template-file ./storage-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        environmentName=$script:environment `
        location=$script:location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        dnsResourceGroupName=$($script:resourceGroups.dns) `
        opsResourceGroupName=$($script:resourceGroups.ops) `
        project=$script:project `
        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The storage deployment failed."
    }
}

task Vec -depends ResourceGroups, Ops, Networking, DNS, Configuration {
    if ($skipVec -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Vec creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ensure vec resources exist"

    az deployment group create `
        --name $script:deployments.vec `
        --resource-group $script:resourceGroups.vec `
        --template-file ./vec-rg.bicep `
        --parameters `
        actionGroupId=$script:actionGroupId `
        dnsResourceGroupName=$($script:resourceGroups.dns) `
        environmentName=$script:environment `
        location=$script:location `
        logAnalyticsWorkspaceId=$script:logAnalyticsWorkspaceId `
        project=$script:project `
        vnetId=$script:vnetId

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}
