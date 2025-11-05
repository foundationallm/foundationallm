function Initialize-ContextServices {
    param (
        [string]$UniqueName,
        [string]$Location
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName
    $resourceGroupNames = Get-ResourceGroupNames -UniqueName $UniqueName

    Write-Host "Ensuring AI Foundry $($resourceNames.AIFoundry) exists in resource group $($resourceGroupNames.AIFoundry)..."
    if ((az cognitiveservices account list `
        -g $resourceGroupNames.AIFoundry `
        --query "[?name=='$($resourceNames.AIFoundry)']" -o tsv).Count -eq 0) {

        Write-Host "Creating AI Foundry $($resourceNames.AIFoundry) in resource group $($resourceGroupNames.AIFoundry)..."
        az cognitiveservices account create `
            --kind "AIServices" `
            --name $resourceNames.AIFoundry `
            --resource-group $resourceGroupNames.AIFoundry `
            --location $Location `
            --sku S0 `
            --yes | Out-Null
        Write-Host "AI Foundry '$($resourceNames.AIFoundry)' created."
    } else {
        Write-Host "AI Foundry $($resourceNames.AIFoundry) already exists in resource group $($resourceGroupNames.AIFoundry)."
    }
}