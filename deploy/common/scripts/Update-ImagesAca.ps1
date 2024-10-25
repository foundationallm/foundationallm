#! /usr/bin/pwsh
<#
.SYNOPSIS
    This script updates container apps image within a specified resource group in Azure by updating the image URI tag.

.DESCRIPTION
    The script retrieves the list of container apps in the provided resource group and subscription.
    It then iterates over each container app, extracting the current image URI, updating the tag with
    the value provided via the -tag parameter, and finally, applying the updated image URI to the container app.

.PARAMETER resourceGroupNameName
    The name of the resource group containing the container apps. This parameter is mandatory.

.PARAMETER subscriptionId
    The ID of the Azure subscription where the resource group resides. This parameter is mandatory.

.PARAMETER tag
    The new tag to be applied to the image URI of each container app. This parameter is mandatory.

.EXAMPLE
    ./Update-ImagesAca.ps1 -resourceGroupName "myresourceGroupName" -subscriptionId "12345678-1234-1234-1234-123456789012" -tag "latest"
    This example restarts all container apps in the "myresourceGroupName" by updating their image URIs to use the "latest" tag.

.NOTES
    Set the Azure CLI context to the appropriate subscription before running this script.
    You can set the subscription using the 'az account set --subscription' command.
#>

Param(
    # Mandatory parameters
    [parameter(Mandatory = $true)][string]$resourceGroupName,
    [parameter(Mandatory = $true)][string]$subscriptionId,
    [parameter(Mandatory = $true)][string]$tag
)

# Set Debugging and Error Handling
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Get the list of container apps in the resource group
$containerApps = (az containerapp list -g $resourceGroupName --query "[].name" --output tsv)
if ($null -eq $containerApps -or $containerApps.Count -eq 0) {
    Write-Host -ForegroundColor Yellow "No container apps found in the specified resource group."
    return
}

Write-Host -ForegroundColor Yellow "Container Apps to be updated $containerApps"
Write-Host -ForegroundColor Yellow "Updating container apps using these parameters:Resource group: ${resourceGroupName} Subscription: ${subscriptionId}"

foreach ($acaName in $containerApps.GetEnumerator()) {
    $imgUri = $(
        az containerapp revision list `
            --name $acaName `
            -g $resourceGroupName `
            --subscription $subscriptionId `
            --query "[0].properties.template.containers[0].image" `
            -o tsv
    )
    $tagDelimiterPos = $imgUri.IndexOf(":")
    $newImgUri = $imgUri.Substring(0, $tagDelimiterPos) + ":" + $tag

    Write-Host -ForegroundColor Yellow "Set Image URI for $acaName to $newImgUri"

    $updatedImage = az containerapp update `
        --name $acaName `
        -g $resourceGroupName `
        --subscription $subscriptionId `
        --image $newImgUri `
        --query "properties.template.containers[0].image" `
        --output tsv

    Write-Host -ForegroundColor Green "New Image URI for $acaName is $updatedImage"
}
