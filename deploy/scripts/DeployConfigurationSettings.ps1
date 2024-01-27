Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$keyvaultName,
    [parameter(Mandatory = $false)][string]$configurationFile="appconfig.json"
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

$config = Get-Content -Raw -Path $configurationFile | ConvertFrom-Json

for ( $idx = 0; $idx -lt $config.count; $idx++ )
{
    Write-Host $config[$idx].key
    if ($config[$idx].keyVault)
    {
        $secretName = $config[$idx].value
        Write-Host "az appconfig kv set-keyvault -n $name --key $($config[$idx].key) --secret-identifier https://$($keyvaultName).vault.azure.net/Secrets/$($secretName)/ -y"
        az appconfig kv set-keyvault -n $name --key $config[$idx].key --secret-identifier https://$($keyvaultName).vault.azure.net/Secrets/$($secretName)/ -y
    }
    elseif ($config[$idx].featureFlag)
    {   
        Write-Host "az appconfig feature set -n $name --feature $($config[$idx].value) --key $($config[$idx].key) -y"
        az appconfig feature set -n $name --feature $config[$idx].value --key $config[$idx].key -y
    }
    else
    {
        Write-Host "az appconfig kv set -n $name --key $($config[$idx].key) --value \"$($config[$idx].value)\" -y"
        az appconfig kv set -n $name --key $config[$idx].key --value "$($config[$idx].value)" -y
    }
}

Pop-Location
