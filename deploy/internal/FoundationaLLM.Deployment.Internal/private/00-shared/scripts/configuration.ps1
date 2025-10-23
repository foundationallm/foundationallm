function Get-Configuration {
    param(
        [string]$FoundationaLLMRepoPath,
        [string]$ConfigurationCategoryName
    )

    $configurationMappingPath = Join-Path `
        -Path $FoundationaLLMRepoPath `
        -ChildPath "src/dotnet/Common/Constants/Data/ConfigurationMapping.json"

    $appConfigurationTemplatePath = Join-Path `
        -Path $FoundationaLLMRepoPath `
        -ChildPath "src/dotnet/Common/Templates/appconfig.template.json"

    $configurationMapping = Get-Content -Path $configurationMappingPath -Raw | ConvertFrom-Json -AsHashtable
    $appConfigurationTemplate = Get-Content -Path $appConfigurationTemplatePath -Raw | ConvertFrom-Json -AsHashtable
}

$configurationMapping = (Get-Content -Path "D:/Repos/FoundationaLLM/src/dotnet/Common/Constants/Data/ConfigurationMapping.json" -Raw | ConvertFrom-Json) `
    | Where-Object { $_.name -eq "ManagementAPI" }

foreach ($configurationSection in $configurationMapping.configuration_options) {
    Write-Host "Processing configuration section: $($configurationSection)"
}


$appConfigurationTemplate = Get-Content -Path "D:/Repos/FoundationaLLM/src/dotnet/Common/Templates/appconfig.template.json" -Raw | ConvertFrom-Json -AsHashtable