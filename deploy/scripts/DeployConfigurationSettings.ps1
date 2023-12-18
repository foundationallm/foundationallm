Param (
    [parameter(Mandatory = $true)][string]$resourceGroup,
    [parameter(Mandatory = $true)][string]$location,
    [parameter(Mandatory = $true)][string]$name,
    [parameter(Mandatory = $true)][string]$keyvaultName,
    [parameter(Mandatory = $true)][string]$configurationFile="appconfig.json"
)

Push-Location $($MyInvocation.InvocationName | Split-Path)

$config = Get-Content $configurationFile -Raw | ConvertFrom-Json

for ( $idx = 0; $idx -lt $config.count; $idx++ )
{
    if ($config[$idx].keyVault)
    {
        az appconfig kv set-keyvault -n $name --key $config[$idx].key --secret-identifier https://$($keyvaultName).vault.azure.net/Secrets/$($config.[idx].value)
    }
    else if ($config[$idx].featureFlag)
    {
        az appconfig feature set -n $name --key $config[$idx].key --value $config[$idx].value
    }
    else
    {
        az appconfig kv set -n $name --key $config[$idx].key --value $config[$idx].value
    }
}

Pop-Location
