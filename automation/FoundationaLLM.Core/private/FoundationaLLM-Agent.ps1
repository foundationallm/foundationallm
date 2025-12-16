
function Get-AllAgents {
    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agents"
}

function Get-Agent {
    param (
        [string]$AgentName
    )

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agents/$AgentName"
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

function Get-AgentFiles {
    param (
        [string]$AgentName
    )

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agents/$AgentName/agentFiles"
}

function Get-AgentFile {
    param (
        [string]$AgentName,
        [string]$FileName,
        [switch]$LoadContent
    )

    $uri = "providers/FoundationaLLM.Agent/agents/$AgentName/agentFiles/$FileName"
    if ($LoadContent) {
        $uri += "?loadContent=true"

        return Invoke-ManagementAPI `
            -Method GET `
            -RelativeUri $uri `
            -BinaryOutput
    }

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri $uri
}

function Merge-ToolType {
    param (
        [hashtable]$ToolType
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/tools/$($ToolType.name)" `
        -Body $ToolType
}

function Merge-WorkflowType {
    param (
        [hashtable]$WorkflowType
    )

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/workflows/$($WorkflowType.name)" `
        -Body $WorkflowType
}