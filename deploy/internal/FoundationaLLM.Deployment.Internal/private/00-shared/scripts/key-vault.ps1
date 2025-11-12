function Set-KeyVaultSecrets {
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory = $true)]
        [hashtable]$Secrets
    )

    foreach ($secretName in $Secrets.Keys) {
        $secretValue = $Secrets[$secretName]

        if ((az keyvault secret list `
            --vault-name $KeyVaultName `
            --query "[?name=='$($secretName)']" -o tsv).Count -eq 0) {

            Write-Host "Setting secret $secretName in Key Vault $KeyVaultName..."
            az keyvault secret set `
                --vault-name $KeyVaultName `
                --name $secretName `
                --value $secretValue | Out-Null
            Write-Host "Secret $secretName set."
        } else {
            Write-Host "Secret $secretName already exists in Key Vault $KeyVaultName."
        }
    }
}

function Set-KeyVaultSecret {
    param (
        [Parameter(Mandatory = $true)]
        [string]$KeyVaultName,
        [Parameter(Mandatory = $true)]
        [string]$SecretName,
        [Parameter(Mandatory = $true)]
        [string]$SecretValue,
        [bool]$Force = $false
    )

    if ($Force -or (az keyvault secret list `
            --vault-name $KeyVaultName `
            --query "[?name=='$($SecretName)']" -o tsv).Count -eq 0) {
        Write-Host "Setting secret $SecretName in Key Vault $KeyVaultName..."
        az keyvault secret set `
            --vault-name $KeyVaultName `
            --name $SecretName `
            --value $SecretValue | Out-Null
        Write-Host "Secret $SecretName set."
    } else {
        Write-Host "Secret $SecretName already exists in Key Vault $KeyVaultName."
    }
}