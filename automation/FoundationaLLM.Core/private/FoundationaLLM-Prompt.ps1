

function Get-AllPrompts {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts"
}

function Get-Prompt {
    param (
        [string]$PromptName
    )

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts/$PromptName"
}

function Merge-Prompt {
    param (
        [hashtable]$Prompt
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts/$($Prompt.name)" `
        -Body $Prompt
}
