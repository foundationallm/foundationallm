#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$environment = "demo"
$location = "eastus"
$project = "fllm"
$subscription = "4dae7dc4-ef9c-4591-b247-8eacb27f3c9e"
$timestamp = [int](Get-Date -UFormat %s -Millisecond 0)

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

task default -depends DNS

task DNS -depends ResourceGroups, Networking -description "Ensure DNS resources exist" {
    $vnetId = $(
        az deployment group show `
            --name $deployments["net"] `
            --output tsv `
            --query properties.outputs.vnetId.value `
            --resource-group $resourceGroups["net"] 
    )

    if ($LASTEXITCODE -ne 0) {
        throw "The VNet ID could not be retrieved."
    }

    az deployment group create `
        --name $deployments["dns"] `
        --parameters environmentName=$environment location=$location project=$project vnetId=$vnetId `
        --resource-group $resourceGroups["dns"] `
        --template-file ./dns-rg/template.bicep 

    if ($LASTEXITCODE -ne 0) {
        throw "The DNS deployment failed."
    }
}

task Networking -depends ResourceGroups -description "Ensure networking resources exist" {
    az deployment group create `
        --name $deployments["net"] `
        --parameters environmentName=$environment location=$location project=$project `
        --resource-group $resourceGroups["net"] `
        --template-file ./networking-rg/template.bicep 

    if ($LASTEXITCODE -ne 0) {
        throw "The networking deployment failed."
    }
}

task OpenAI -depends ResourceGroups -description "Ensure OpenAI accounts exist" {
    az deployment group create `
        --name $deployments["oai"] `
        --parameters environmentName=$environment location=$location project=$project `
        --resource-group $resourceGroups["oai"] `
        --template-file ./openai-rg/template.bicep 

    if ($LASTEXITCODE -ne 0) {
        throw "The OpenAI deployment failed."
    }
}

task ResourceGroups -description "Ensure resource groups exist" {
    foreach ($resourceGroup in $resourceGroups.values) {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup)) {
            Write-Host "The resource group $resourceGroup was not found, creating it..."
            az group create -g $resourceGroup -l $location --subscription $subscription

            if (-Not (az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup) {
                throw "The resource group $resourceGroup was not found, and could not be created."
            } 
        }
        else {
            Write-Host -ForegroundColor Blue "The resource group $resourceGroup was found."
        }
    }
}

