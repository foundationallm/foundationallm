function Resolve-ConfigurationVariables {
    param(
        [string]$Value,
        [hashtable]$ConfigurationVariables,
        [bool]$StopOnError = $true
    )

    $variablePattern = '\$\{env:([A-Za-z_][A-Za-z0-9_]*)\}'

    [regex]::Matches($Value, $variablePattern) |
        ForEach-Object {
            if ($ConfigurationVariables.ContainsKey($_.Groups[1].Value)) {
                $variableValue = $ConfigurationVariables[$_.Groups[1].Value]
                $Value = $Value -replace [regex]::Escape($_.Value), $variableValue
            } else {
                Write-Host "No matching configuration variable found for '$($_.Groups[1].Value)' in value '$Value'." -ForegroundColor Red
                if ($StopOnError) {
                    throw "Configuration variable '$($_.Groups[1].Value)' not found."
                }
            }
        }

    return $Value
}

function Initialize-AppConfigurationKey {
    param(
        [string]$Key,
        [string]$Value,
        [string]$KeyVaultSecretName,
        [string]$KeyVaultSecretValue,
        [string]$ContentType,
        [hashtable]$ResourceNames
    )

    if ($KeyVaultSecretName) {
        Set-KeyVaultSecret `
            -KeyVaultName $ResourceNames.KeyVault `
            -SecretName $KeyVaultSecretName `
            -SecretValue $KeyVaultSecretValue
    }

    if ((az appconfig kv list --all -n $ResourceNames.AppConfig --query "[?key=='$Key']" -o tsv).Count -eq 0) {
        Write-Host "Setting App Configuration key $Key..."

        if ($KeyVaultSecretName) {
            az appconfig kv set-keyvault `
                --name $ResourceNames.AppConfig `
                --key $Key `
                --secret-identifier "https://$($ResourceNames.KeyVault).vault.azure.net/secrets/$KeyVaultSecretName" `
                --yes | Out-Null
        } elseif ($ContentType) {
            if ($ContentType -eq "application/json") {
                # Use a temporary file to handle JSON content
                $tmpFile = New-TemporaryFile
                $Value | Out-File -FilePath $tmpFile -Encoding utf8
                az appconfig kv set `
                    --name $ResourceNames.AppConfig `
                    --key $Key `
                    --value=@"$tmpFile" `
                    --content-type $ContentType `
                    --yes | Out-Null
                Remove-Item $tmpFile -Force
            } else {
                az appconfig kv set `
                    --name $ResourceNames.AppConfig `
                    --key $Key `
                    --value="$($Value)" `
                    --content-type $ContentType `
                    --yes | Out-Null
            }
        } else {
            az appconfig kv set `
                --name $ResourceNames.AppConfig `
                --key $Key `
                --value="$($Value)" `
                --yes | Out-Null
        }
        Write-Host "App Configuration key $Key set."
    } else {
        Write-Host "App Configuration key $Key already exists."
    }
}

function Initialize-AppConfigurationFeatureFlag {
    param(
        [string]$Name,
        [bool]$Enabled,
        [hashtable]$ResourceNames
    )

    if ((az appconfig feature list `
            -n $ResourceNames.AppConfig `
            --query "[?name=='$Name']" -o tsv).Count -eq 0) {
        
        Write-Host "Creating App Configuration feature flag $Name..."
        az appconfig feature set -n $ResourceNames.AppConfig --feature $Name --yes | Out-Null
        Write-Host "App Configuration feature flag $Name created."
    } else {
        Write-Host "App Configuration feature flag $Name already exists."
    }

    if ($Enabled) {
        Write-Host "Enabling App Configuration feature flag $Name..."
        az appconfig feature enable -n $ResourceNames.AppConfig --feature $Name --yes | Out-Null
        Write-Host "App Configuration feature flag $Name enabled."
    } else {
        Write-Host "Disabling App Configuration feature flag $Name..."
        az appconfig feature disable -n $ResourceNames.AppConfig --feature $Name --yes | Out-Null
        Write-Host "App Configuration feature flag $Name disabled."
    }
}

function Initialize-Configuration {
    param(
        [string]$TenantId,
        [string]$SubscriptionId,
        [string]$InstanceId,
        [string]$UniqueName,
        [string]$FoundationaLLMRepoPath,
        [string]$ConfigurationCategoryName,
        [bool]$StopOnError = $true
    )

    $resourceNames = Get-ResourceNames -UniqueName $UniqueName

    $configurationVariables = Get-ConfigurationVariables `
        -TenantId $TenantId `
        -SubscriptionId $SubscriptionId `
        -InstanceId $InstanceId `
        -UniqueName $UniqueName

    $configurationMappingPath = Join-Path `
        -Path $FoundationaLLMRepoPath `
        -ChildPath "src/dotnet/Common/Constants/Data/ConfigurationMapping.json"

    $appConfigurationTemplatePath = Join-Path `
        -Path $FoundationaLLMRepoPath `
        -ChildPath "src/dotnet/Common/Templates/appconfig.template.json"

    $configurationMappings = Get-Content -Path $configurationMappingPath -Raw | ConvertFrom-Json -AsHashtable
    $configurationOptions = $configurationMappings `
        | Where-Object { $_.name -eq $ConfigurationCategoryName } `
        | Select-Object -ExpandProperty configuration_options
    $appConfigurationTemplate = Get-Content -Path $appConfigurationTemplatePath -Raw | ConvertFrom-Json -AsHashtable

    $matchingAppConfigurationItems = @()

    foreach ($configurationOption in $configurationOptions) {
        $wildCardMatch = $configurationOption.EndsWith(":*")
        if ($configurationOption.StartsWith(".appconfig.featureflag/")) {
            $baseKey = $configurationOption
        } else {
            $baseKey = "FoundationaLLM:" + $configurationOption.TrimEnd("*")
        }

        foreach ($appConfigurationItem in $appConfigurationTemplate.items) {
            if ($wildCardMatch) {
                if ($appConfigurationItem.key.StartsWith($baseKey)) {
                    $matchingAppConfigurationItems += $appConfigurationItem
                }
            } else {
                if ($appConfigurationItem.key -eq $baseKey) {
                    $matchingAppConfigurationItems += $appConfigurationItem
                }
            }
        }
    }

    foreach ($appConfigurationItem in $matchingAppConfigurationItems) {

        if ($appConfigurationItem.key.StartsWith(".appconfig.featureflag/")) {
            $featureFlagName = $appConfigurationItem.key.Replace(".appconfig.featureflag/", "")
            $featureFlagEnabled = [bool]$appConfigurationItem.value
            Initialize-AppConfigurationFeatureFlag `
                -Name $featureFlagName `
                -Enabled $featureFlagEnabled `
                -ResourceNames $resourceNames
            continue
        }

        $appConfigurationValue = $appConfigurationItem.value
        $appConfigurationValue = Resolve-ConfigurationVariables `
            -Value $appConfigurationValue `
            -ConfigurationVariables $configurationVariables `
            -StopOnError $StopOnError

        $appConfigurationSecretValue = $appConfigurationItem.key_vault_secret_value
        $appConfigurationSecretValue = Resolve-ConfigurationVariables `
            -Value $appConfigurationSecretValue `
            -ConfigurationVariables $configurationVariables `
            -StopOnError $StopOnError

        Initialize-AppConfigurationKey `
            -Key $appConfigurationItem.key `
            -Value $appConfigurationValue `
            -KeyVaultSecretName $appConfigurationItem.key_vault_secret_name `
            -KeyVaultSecretValue $appConfigurationSecretValue `
            -ContentType $appConfigurationItem.content_type `
            -ResourceNames $resourceNames
    }
}