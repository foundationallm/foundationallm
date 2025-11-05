function Initialize-AIServices {
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

    Get-Content "$PSScriptRoot/../data/ai-foundry-model-deployments.json" -Raw `
        | ConvertFrom-Json `
        | ForEach-Object {
            Write-Host "Ensuring AI Foundry model deployment $($_.deployment_name) exists in resource group $($resourceGroupNames.AIFoundry)..."
            if ((az cognitiveservices account deployment list `
                -g $resourceGroupNames.AIFoundry `
                -n $resourceNames.AIFoundry `
                --query "[?name=='$($_.deployment_name)']" -o tsv).Count -eq 0) {

                Write-Host "Creating AI Foundry model deployment $($_.deployment_name) in resource group $($resourceGroupNames.AIFoundry)..."
                az cognitiveservices account deployment create `
                    -g $resourceGroupNames.AIFoundry `
                    -n $resourceNames.AIFoundry `
                    --model-format $_.model_format `
                    --model-name $_.model_name `
                    --model-version $_.model_version `
                    --deployment-name $_.deployment_name `
                    --sku $_.deployment_type `
                    --sku-capacity $_.capacity_units `
                    | Out-Null
                Write-Host "AI Foundry model deployment '$($_.deployment_name)' created."
            } else {
                Write-Host "AI Foundry model deployment $($_.deployment_name) already exists in resource group $($resourceGroupNames.AIFoundry)."
            }
        }
}