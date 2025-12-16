
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
        [switch]$LoadContent,
        [string]$OutFile
    )

    $uri = "providers/FoundationaLLM.Agent/agents/$AgentName/agentFiles/$FileName"
    if ($LoadContent) {
        $uri += "?loadContent=true"
    }

    Test-ManagementAPIAccessToken

    $headers = @{
        "Authorization" = "Bearer $($global:ManagementAPIAccessToken)"
    }

    $baseUri = Get-ManagementAPIBaseUri
    $fullUri = "$($baseUri.AbsoluteUri)/$uri"

    Write-Host "GET $fullUri" -ForegroundColor Green

    if ($OutFile) {
        # Download file directly to disk
        return Invoke-RestMethod `
            -Method GET `
            -Uri $fullUri `
            -Headers $headers `
            -OutFile $OutFile
    } else {
        # Return raw byte content
        return Invoke-WebRequest `
            -Method GET `
            -Uri $fullUri `
            -Headers $headers
    }
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