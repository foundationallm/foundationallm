

function Get-AllPrompts {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Prompt/prompts"
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
