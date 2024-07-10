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

$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

# Properties
properties {
    # Override example: Invoke-Psake -properties @{ "manifestName" = "My-Deployment-Manifest.json" }
    $manifestName = "Deployment-Manifest.json"
}

task default -depends Configuration, Main

task Main -depends Configuration {
    $deployment = "main-${timestamp}"
    $templateFile = "main.bicep"

    $parameters = @{
        administratorObjectId       = @{
            type  = "string"
            value = $script:administratorObjectId
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
        createVpnGateway            = @{
            type  = "bool"
            value = $script:createVpnGateway
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
        project                     = @{
            type  = "string"
            value = $script:project
        }
    }

    if ($script:useExternalDns) {
        $parameters.Add("externalDnsResourceGroupName", @{
                type  = "string"
                value = $script:externalResourceGroups.dns
            })
    }

    if ($script:useExternalNetworking) {
        $parameters.Add("externalNetworkingResourceGroupName", @{
                type  = "string"
                value = $script:externalResourceGroups.net
            })
    }

    if ($script:networkName -ne $null) {
        $parameters.Add("networkName", @{
                type  = "string"
                value = $script:networkName
            })
    }

    $paramsFile = New-Bicepparams -templateFile $templateFile -parameters $parameters

    $outputs = az deployment sub create `
        --name $deployment `
        --location $script:location `
        --subscription $script:subscription `
        --template-file ./$templateFile `
        --output json `
        --query 'properties.outputs' `
        --parameters $paramsFile |
    ConvertFrom-Json -Depth 10

    if ($LASTEXITCODE -ne 0) {
        throw "The main deployment failed."
    }

    Copy-Item -Path ../$($manifestName) -Destination ../$($manifestName)-${timestamp}.backup
    $manifest = $(Get-Content -Raw -Path ../$($manifestName) | ConvertFrom-Json)
    $manifest.resourceGroups = $outputs.managedResourceGroupNames.value
    $manifest | ConvertTo-Json -Depth 10 | Out-File -FilePath ../$($manifestName) -Encoding ascii -Force
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

    # Remove the external resource groups from the resource group collection and iterate over the remaining resource groups
    if ($script:externalResourceGroups -ne $null) {
        foreach ($property in $script:externalResourceGroups.GetEnumerator()) {
            $script:resourceGroups.Remove($property.Name)
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

        if ($count -eq 0) {
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

    $script:networkName = $null
    if ($manifest.PSobject.Properties.Name -contains "networkName") {
        $script:networkName = $manifest.networkName
    }

    $script:tenantId = $(
        az account show `
            --query tenantId `
            --output tsv
    )

    $script:deployments = @{}
    $script:resourceGroups = @{}
    $resourceGroups = $manifest.resourceGroups
    foreach ($property in $resourceGroups.PSObject.Properties) {
        $script:deployments.Add($property.Name, "$($property.Value)-${timestamp}")
        $script:resourceGroups.Add($property.Name, $property.Value)
    }

    $script:externalResourceGroups = $null
    if ($manifest.PSobject.Properties.Name -contains "externalResourceGroups") {
        $externalResouceGroups = $manifest.externalResourceGroups
        $script:externalResourceGroups = @{}
        foreach ($property in $externalResouceGroups.PSObject.Properties) {
            $script:externalResourceGroups.Add($property.Name, $property.Value)
            $script:resourceGroups.Add($property.Name, $property.Value)
        }
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

    # Check if the external resource groups is not empty and contains an entry for DNS and skip DNS if so
    $script:useExternalDns = $false
    if ($script:externalResourceGroups -ne $null -and $script:externalResourceGroups.ContainsKey("dns")) {
        $script:useExternalDns = $true
    }

    $script:useExternalNetworking = $false
    if ($script:externalResourceGroups -ne $null -and $script:externalResourceGroups.ContainsKey("net")) {
        $script:useExternalNetworking = $true
    }

    Write-Host -ForegroundColor Blue "Configuration complete."
}
