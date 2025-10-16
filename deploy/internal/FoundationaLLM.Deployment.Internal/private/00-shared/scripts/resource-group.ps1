function Get-ResourceGroupNamesList {
    param (
        [Parameter(Mandatory = $true)]
        [string[]]$UniqueName
    )

    $resourceGroupNames = @(
        "core",
        "auth",
        "context",
        "data",
        "ai-foundry"
    )

    return $resourceGroupNames | ForEach-Object { "$($UniqueName)-$($_)" }
}

function Initialize-ResourceGroup {
    param (
        [Parameter(Mandatory = $true)]
        [string]$SubscriptionId,

        [Parameter(Mandatory = $true)]
        [string[]]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Location
    )

    if ((az group exists -n $Name) -ne $true) {
        Write-Host "Creating resource group '$Name' in location '$Location'..."
        az group create --name $Name --location $Location --subscription $SubscriptionId | Out-Null
        Write-Host "Resource group '$Name' created."
    }
}