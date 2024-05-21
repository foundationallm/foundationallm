#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function New-Bicepparams {
    param(
        [string] $templateFile,
        [hashtable] $parameters
    )

    $templateName = [System.IO.Path]::GetFileNameWithoutExtension($templateFile)
    $paramsFile = "$($templateName).parameters.bicepparam"
    $bicepParams = @()
    $bicepParams += "using '$templateFile'"
    foreach ($p in $parameters.GetEnumerator()) {
        switch ($p.Value.type) {
            bool {
                $bicepParams += "param $($p.Name) = $($p.Value.value.ToString().ToLower())"
            }
            Default {
                $bicepParams += "param $($p.Name) = '$($p.Value.value)'"
            }
        }
    }

    Write-Host -ForegroundColor Blue "Write ${paramsFile}"
    $bicepParams | Out-File -FilePath $paramsFile -Encoding ascii -Force
    return $paramsFile
}

$skipApp = $false
$skipAuth = $false
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

task default -depends App, Auth, Configuration, DNS, Networking, OpenAI, Ops, ResourceGroups, Storage, Vec

task App -depends ResourceGroups, Ops, Networking, DNS, Configuration, Vec, Storage {
    if ($skipApp -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping app creation."
        return;
    }

    Write-Host -ForegroundColor Blue "App Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["app"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.app)"

    $templateFile = "app-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        actionGroupId                   = @{
            type  = "string"
            value = $script:actionGroupId
        }
        administratorObjectId           = @{
            type  = "string"
            value = $script:administratorObjectId
        }
        chatUiClientSecret              = @{
            type  = "string"
            value = $script:chatUiClientSecret
        }
        coreApiClientSecret             = @{
            type  = "string"
            value = $script:coreApiClientSecret
        }
        dnsResourceGroupName            = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName                 = @{
            type  = "string"
            value = $script:environment
        }
        k8sNamespace                    = @{
            type  = "string"
            value = $script:k8sNamespace
        }
        location                        = @{
            type  = "string"
            value = $script:location
        }
        logAnalyticsWorkspaceId         = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        logAnalyticsWorkspaceResourceId = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        managementUiClientSecret        = @{
            type  = "string"
            value = $script:managementUiClientSecret
        }
        managementApiClientSecret       = @{
            type  = "string"
            value = $script:managementApiClientSecret
        }
        networkingResourceGroupName     = @{
            type  = "string"
            value = $script:resourceGroups.net
        }
        opsResourceGroupName            = @{
            type  = "string"
            value = $script:resourceGroups.ops
        }
        project                         = @{
            type  = "string"
            value = $script:project
        }
        storageResourceGroupName        = @{
            type  = "string"
            value = $script:resourceGroups.storage
        }
        vectorizationResourceGroupName  = @{
            type  = "string"
            value = $script:resourceGroups.vec
        }
        vectorizationApiClientSecret    = @{
            type  = "string"
            value = $script:vectorizationApiClientSecret
        }
        vnetName                        = @{
            type  = "string"
            value = $script:vnetName
        }
        openAiResourceGroupName         = @{
            type  = "string"
            value = $script:resourceGroups.oai
        }
    }

    az deployment group create `
        --name  $script:deployments["app"] `
        --parameters $paramsFile `
        --resource-group $resourceGroups.app `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The app deployment failed."
    }
}

task Auth -depends App, ResourceGroups, Networking, Ops, DNS, Configuration {
    if ($skipAuth -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Auth creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Auth Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["auth"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.auth)"

    $templateFile = "auth-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        actionGroupId               = @{
            type  = "string"
            value = $script:actionGroupId
        }
        administratorObjectId       = @{
            type  = "string"
            value = $script:administratorObjectId
        }
        appResourceGroupName        = @{
            type  = "string"
            value = $script:resourceGroups.app
        }
        authAppRegistrationClientId = @{
            type  = "string"
            value = $script:entraClientIds.authorization
        }
        authAppRegistrationInstance = @{
            type  = "string"
            value = $script:entraInstances.authorization
        }
        authAppRegistrationScopes   = @{
            type  = "string"
            value = $script:entraClientScopes.authorization
        }
        authAppRegistrationTenantId = @{
            type  = "string"
            value = $script:tenantId
        }
        authClientSecret            = @{
            type  = "string"
            value = $script:entraClientSecrets.authorization
        }
        dnsResourceGroupName        = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName             = @{
            type  = "string"
            value = $script:environment
        }
        instanceId                  = @{
            type  = "string"
            value = $script:instanceId
        }
        k8sNamespace                = @{
            type  = "string"
            value = $script:k8sNamespace
        }
        location                    = @{
            type  = "string"
            value = $script:location
        }
        logAnalyticsWorkspaceId     = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        opsResourceGroupName        = @{
            type  = "string"
            value = $script:resourceGroups.ops
        }
        project                     = @{
            type  = "string"
            value = $script:project
        }
        vnetId                      = @{
            type  = "string"
            value = $script:vnetId
        }
    }

    az deployment group create `
        --name $script:deployments["auth"] `
        --parameters $paramsFile `
        --resource-group $resourceGroups.auth `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The auth deployment failed."
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

    while ($true) {
        Write-Host -ForegroundColor Blue "Deleting all resource groups..."

        $count = 0
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
                $count++
            }
        }

        $deleteMessage = @"
Sent delete requests for all resource groups.  Deletion can take up to an hour.
Some resources are only soft-deleted and may need to be purged to completely remove them.
Check the Azure Portal for status.
"@
        Write-Host -ForegroundColor Blue $deleteMessage

        if($count -eq 0) {
            Write-Host -ForegroundColor Green "All resource groups have been deleted."
            break
        }

        Write-Host -ForegroundColor Blue "Waiting 10 seconds for ${count} resource groups to delete..."
        Start-Sleep -Seconds 10
    }
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
    $script:instanceId = $manifest.instanceId

    $script:tenantId = $(
        az account show `
            --query tenantId `
            --output tsv
    )

    $script:deployments = @{}
    $script:resourceGroups = @{}
    foreach ($property in $resourceGroups.PSObject.Properties) {
        $script:deployments.Add($property.Name, "$($property.Value)-${timestamp}")
        $script:resourceGroups.Add($property.Name, $property.Value)
    }

    $script:entraClientIds = @{}
    foreach ($property in $manifest.entraClientIds.PSObject.Properties) {
        $script:entraClientIds.Add($property.Name, $property.Value)
    }

    $script:entraInstances = @{}
    foreach ($property in $manifest.entraInstances.PSObject.Properties) {
        $script:entraInstances.Add($property.Name, $property.Value)
    }

    $script:entraClientScopes = @{}
    foreach ($property in $manifest.entraScopes.PSObject.Properties) {
        $script:entraClientScopes.Add($property.Name, $property.Value)
    }

    $script:entraClientSecrets = @{}
    foreach ($property in $manifest.entraClientSecrets.PSObject.Properties) {
        $script:entraClientSecrets.Add($property.Name, $property.Value)
    }


    Write-Host -ForegroundColor Blue "Configuration complete."
}

task DNS -depends ResourceGroups, Networking, Configuration {
    if ($skipDns -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping DNS Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "DNS Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["dns"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.dns)"

    $templateFile = "dns-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        environmentName          = @{
            type  = "string"
            value = $script:environment
        }
        location                 = @{
            type  = "string"
            value = $script:location
        }
        project                  = @{
            type  = "string"
            value = $script:project
        }
        networkResourceGroupName = @{
            type  = "string"
            value = $script:resourceGroups.net
        }
        vnetName                 = @{
            type  = "string"
            value = $script:vnetName
        }
    }

    az deployment group create `
        --name $script:deployments["dns"] `
        --parameters $paramsFile `
        --resource-group $script:resourceGroups.dns `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The DNS deployment failed."
    }
}

task Networking -depends ResourceGroups, Configuration {
    if ($skipNetworking -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Network Creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Networking Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["net"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.net)"

    $templateFile = "networking-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        createVpnGateway = @{
            type  = "bool"
            value = $script:createVpnGateway
        }
        environmentName  = @{
            type  = "string"
            value = $script:environment
        }
        location         = @{
            type  = "string"
            value = $script:location
        }
        project          = @{
            type  = "string"
            value = $script:project
        }
    }

    az deployment group create `
        --name $script:deployments["net"] `
        --resource-group $script:resourceGroups.net `
        --template-file ./$templateFile `
        --parameters $paramsFile


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

    Write-Host -ForegroundColor Blue "OpenAI Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments.oai)"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.oai)"

    $templateFile = "openai-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        actionGroupId           = @{
            type  = "string"
            value = $script:actionGroupId
        }
        administratorObjectId   = @{
            type  = "string"
            value = $script:administratorObjectId
        }
        dnsResourceGroupName    = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName         = @{
            type  = "string"
            value = $script:environment
        }
        location                = @{
            type  = "string"
            value = $script:location
        }
        logAnalyticsWorkspaceId = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        opsResourceGroupName    = @{
            type  = "string"
            value = $script:resourceGroups.ops
        }
        project                 = @{
            type  = "string"
            value = $script:project
        }
        vnetId                  = @{
            type  = "string"
            value = $script:vnetId
        }
    }

    az deployment group create `
        --name $script:deployments.oai `
        --parameters $paramsFile `
        --resource-group  $script:resourceGroups.oai `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The OpenAI deployment failed."
    }
}

task Ops -depends ResourceGroups, Networking, DNS, Configuration {
    if ($skipOps -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping ops creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Ops Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["ops"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.ops)"

    $templateFile = "ops-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        administratorObjectId = @{
            type  = "string"
            value = $script:administratorObjectId
        }
        dnsResourceGroupName  = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName       = @{
            type  = "string"
            value = $script:environment
        }
        location              = @{
            type  = "string"
            value = $script:location
        }
        project               = @{
            type  = "string"
            value = $script:project
        }
        vnetId                = @{
            type  = "string"
            value = $script:vnetId
        }
    }

    az deployment group create `
        --name $script:deployments["ops"] `
        --parameters $paramsFile `
        --resource-group $script:resourceGroups.ops `
        --template-file ./$templateFile

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

    $script:opsKeyVaultName = $(
        az deployment group show `
            --name $script:deployments["ops"] `
            --output tsv `
            --query properties.outputs.keyVaultName.value `
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

    Write-Host -ForegroundColor Blue "Storage Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments["storage"])"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.storage)"

    $templateFile = "storage-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        actionGroupId           = @{
            type  = "string"
            value = $script:actionGroupId
        }
        dnsResourceGroupName    = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName         = @{
            type  = "string"
            value = $script:environment
        }
        location                = @{
            type  = "string"
            value = $script:location
        }
        logAnalyticsWorkspaceId = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        project                 = @{
            type  = "string"
            value = $script:project
        }
        vnetId                  = @{
            type  = "string"
            value = $script:vnetId
        }
    }

    az deployment group create `
        --name $script:deployments["storage"] `
        --parameters $paramsFile `
        --resource-group $script:resourceGroups.storage `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The storage deployment failed."
    }
}

task Vec -depends ResourceGroups, Ops, Networking, DNS, Configuration {
    if ($skipVec -eq $true) {
        Write-Host -ForegroundColor Yellow "Skipping Vec creation."
        return;
    }

    Write-Host -ForegroundColor Blue "Vec Deployment"
    Write-Host -ForegroundColor Green "Deployment Name: $($script:deployments.vec)"
    Write-Host -ForegroundColor Green "Resource Group: $($script:resourceGroups.vec)"

    $templateFile = "vec-rg.bicep"
    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters @{
        actionGroupId           = @{
            type  = "string"
            value = $script:actionGroupId
        }
        dnsResourceGroupName    = @{
            type  = "string"
            value = $script:resourceGroups.dns
        }
        environmentName         = @{
            type  = "string"
            value = $script:environment
        }
        location                = @{
            type  = "string"
            value = $script:location
        }
        logAnalyticsWorkspaceId = @{
            type  = "string"
            value = $script:logAnalyticsWorkspaceId
        }
        project                 = @{
            type  = "string"
            value = $script:project
        }
        vnetId                  = @{
            type  = "string"
            value = $script:vnetId
        }
    }

    az deployment group create `
        --name $script:deployments.vec `
        --parameters $paramsFile `
        --resource-group $script:resourceGroups.vec `
        --template-file ./$templateFile

    if ($LASTEXITCODE -ne 0) {
        throw "The vec deployment failed."
    }
}
