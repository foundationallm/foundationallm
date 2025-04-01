Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber

function Get-AllPrompts {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts"
}

function Merge-Prompt {
    param (
        [string]$Name,
        [string]$DisplayName,
        [string]$Description,
        [string]$Body,
        [string]$Category,
        [hashtable]$Properties = @{}
    )

    $requestBody = [ordered]@{
        type = "multipart"
        name = $Name
        display_name = $DisplayName
        description = $Description
        prefix = $Body
        category = $Category
        properties = $Properties
    }

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts/$($Name)" `
        -Body $requestBody
}

