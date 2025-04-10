Import-Module "./package/common/FoundationaLLM-Core.psm1" -Force -NoClobber

function Get-AllAgents {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agents"
}

function Merge-Agent {
    param (
        [hashtable]$Agent
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/agents/$($Agent.name)" `
        -Body $Agent
}