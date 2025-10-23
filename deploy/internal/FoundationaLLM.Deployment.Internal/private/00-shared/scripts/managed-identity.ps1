function Initialize-ManagedIdentities {
    param (
        [string[]]$ManagedIdentityNames,
        [string]$ResourceGroupName,
        [string]$Location
    )

    foreach ($miName in $ManagedIdentityNames) {
        if ((az identity list `
            -g $ResourceGroupName `
            --query "[?name=='$($miName)']" -o tsv).Count -eq 0) {

            Write-Host "Creating managed identity $miName in resource group $ResourceGroupName..."
            az identity create `
                --name $miName `
                --resource-group $ResourceGroupName `
                --location $Location | Out-Null
            Write-Host "Managed identity $miName created."
        } else {
            Write-Host "Managed identity $miName already exists."
        }
    }
}