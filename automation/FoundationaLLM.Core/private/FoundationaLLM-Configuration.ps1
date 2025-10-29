function Get-AppConfiguration {
    param (
        [string]$Name
    )

    $result = Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Configuration/appConfigurations/$Name"

    return $result
}

function Merge-AppConfiguration {
    param (
        [hashtable]$Configuration
    )

    if ($Configuration.ContainsKey('key_vault_secret_name') -and $Configuration['key_vault_secret_name']) {
        $Configuration['key_vault_secret_name'] = $Configuration['key_vault_secret_name'].ToLower()
    }

    $result = Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Configuration/appConfigurations/$($Configuration['name'])" `
        -Body $Configuration

    return $result
}

function Merge-APIEndpointConfiguration {
    param (
        [hashtable]$Configuration
    )

    $result = Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Configuration/apiEndpointConfigurations/$($Configuration['name'])" `
        -Body $Configuration

    return $result
}