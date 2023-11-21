#!/usr/bin/env pwsh

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$environment = "demo"
$location = "eastus"
$project = "fllm"
$subscription = "4dae7dc4-ef9c-4591-b247-8eacb27f3c9e"

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

task default -depends OpenAI

task ResourceGroups {
    Write-Host "Ensuring resource groups exist..."

    foreach ($resourceGroup in $resourceGroups.values) {
        if (-Not ($(az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup)) {
            Write-Host "The resource group $resourceGroup was not found, creating it..."
            az group create -g $resourceGroup -l $location --subscription $subscription

            if (-Not (az group list --query '[].name' -o json | ConvertFrom-Json) -Contains $resourceGroup) {
                Write-Error("The resource group $resourceGroup was not found, and could not be created.")
                exit 1
            } 
        }
        else {
            Write-Host -ForegroundColor Blue "The resource group $resourceGroup was found."
        }
    }
}

task OpenAI -depends ResourceGroups {
    Write-Host "Ensuring OpenAI account exists..."

    $name = "${environment}-${location}-oai-${project}"

    if (-Not (az cognitiveservices account list --resource-group $resourceGroups["oai"] --query '[].name' -o json | ConvertFrom-Json) -Contains "oai-${name}-0") {
        Write-Host "The Azure OpenAI account oai-${name}-0 was not found, creating it..."
        az deployment group create `
            --resource-group $resourceGroups["oai"] `
            --template-file ./openai-rg/template.bicep `
            --parameters environment=$environment location=$location project=$project

        if (-Not (az cognitiveservices account list --resource-group $resourceGroups["oai"] --query '[].name' -o json | ConvertFrom-Json) -Contains "oai-${name}-0") {
            throw "The Azure OpenAI account oai-${name}-0 was not found, and could not be created."
        }
    }
    else {
        Write-Host -ForegroundColor Blue "The Azure OpenAI account oai-${name}-0 was found."
    }
}
