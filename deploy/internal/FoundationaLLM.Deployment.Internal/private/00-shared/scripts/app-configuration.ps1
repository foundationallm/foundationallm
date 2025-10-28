function Get-AppConfigurationItems {
    param (
        [string]$AppName
    )

    $configurationMappings = Get-Content -Raw -LiteralPath "$PSScriptRoot/../../../../../../src/dotnet/Common/Constants/Data/ConfigurationMapping.json" | ConvertFrom-Json

    $configurationOptions = $configurationMappings `
        | Where-Object { $_.name -eq $AppName } `
        | Select-Object -ExpandProperty configuration_options

    $appConfigurationTemplate = Get-Content -Raw -LiteralPath "$PSScriptRoot/../../../../../../src/dotnet/Common/Templates/appconfig.template.json" | ConvertFrom-Json

    $matchingAppConfigurationItems = @()

    foreach ($configurationOption in $configurationOptions) {
        $wildCardMatch = $configurationOption.EndsWith(":*")
        $baseKey = "FoundationaLLM:" + $configurationOption.TrimEnd("*")

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

    return $matchingAppConfigurationItems
}

# $appConfigurationItems = Get-AppConfigurationItems -AppName "ManagementAPI"
# $variableNames = $appConfigurationItems  `
#     | Where-Object { $_.value.StartsWith('${env:') } `
#     | Select-Object -ExpandProperty value `
#     | Sort-Object -Unique
