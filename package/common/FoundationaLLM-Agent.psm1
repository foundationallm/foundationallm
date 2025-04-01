Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber

function Get-AllAgents {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agents"
}