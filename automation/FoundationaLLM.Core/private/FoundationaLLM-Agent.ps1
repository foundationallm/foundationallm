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

        $result = Invoke-ManagementAPI `
            -Method GET `
            -RelativeUri $uri `
            -BinaryOutput

        $contentDisposition = $result.Headers["Content-Disposition"]
        $extractedFileName = Get-FileNameFromContentDisposition -ContentDisposition $contentDisposition

        return @{
            ContentType = $result.Headers["Content-Type"]
            FileName = $extractedFileName
            Content = $result.Content
        }
    }

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri $uri
}

function Send-AgentFile {
    param (
        [string]$AgentName,
        [hashtable]$FileContent
    )

    $bytes = $FileContent.Content
    if ($bytes -is [string]) {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($bytes)
    }
    $stream = [System.IO.MemoryStream]::new($bytes)
    $streamContent = [System.Net.Http.StreamContent]::new($stream)
    $streamContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($FileContent.ContentType)

    $multipart = [System.Net.Http.MultipartFormDataContent]::new()
    $multipart.Add($streamContent, "file", $FileContent.FileName)

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/agents/$AgentName/agentFiles/$($FileContent.FileName)" `
        -MultipartContent $multipart
}

function Merge-AgentFileToolAssociations {
    param (
        [string]$AgentName,
        [string]$FileName,
        [hashtable]$ToolAssociations
    )

    $agentObjectId = (Get-ObjectId -Name $AgentName -Type "FoundationaLLM.Agent/agents")
    $agentFileObjectId = "$agentObjectId/agentFiles/$FileName"

    $qualifiedToolAssociations = @{
        "agent_file_tool_associations" = @{
            $agentFileObjectId = @{
                (Get-ObjectId -Name "Code" -Type "FoundationaLLM.Agent/tools") = $ToolAssociations["Code"]
                (Get-ObjectId -Name "Knowledge" -Type "FoundationaLLM.Agent/tools") = $ToolAssociations["Knowledge"]
            }
        }
    }

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/agents/$AgentName/agentFileToolAssociations/$FileName" `
        -Body $qualifiedToolAssociations
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

function Get-AgentTemplate {
    param (
        [string]$AgentTemplateName
    )

    return Invoke-ManagementAPI `
        -Method GET `
        -RelativeUri "providers/FoundationaLLM.Agent/agentTemplates/$AgentTemplateName"
}

function Merge-AgentTemplate {
    param (
        [hashtable]$AgentTemplate,
        [string]$AgentTemplateFileName,
        [byte[]]$AgentTemplateFileContent
    )

    $multipart = [System.Net.Http.MultipartFormDataContent]::new()

    # Add the agent template as a JSON payload
    $agentTemplateJson = $AgentTemplate | ConvertTo-Json -Depth 10
    $jsonContent = [System.Net.Http.StringContent]::new($agentTemplateJson, [System.Text.Encoding]::UTF8, "application/json")
    $multipart.Add($jsonContent, "resource")

    # Add the file content
    $stream = [System.IO.MemoryStream]::new($AgentTemplateFileContent)
    $streamContent = [System.Net.Http.StreamContent]::new($stream)
    $streamContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/octet-stream")
    $multipart.Add($streamContent, "file", $AgentTemplateFileName)

    return Invoke-ManagementAPI `
        -Method POST `
        -RelativeUri "providers/FoundationaLLM.Agent/agentTemplates/$($AgentTemplate.name)" `
        -MultipartContent $multipart
}
