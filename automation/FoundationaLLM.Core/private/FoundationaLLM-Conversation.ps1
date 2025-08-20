function Get-Conversations {
    return Invoke-CoreAPI `
        -Method GET `
        -RelativeUri "sessions" 
}

function New-Conversation {
    param (
        [string]$Name
    )

    $body = @{
        name        = $Name
    }

    return Invoke-CoreAPI `
        -Method POST `
        -RelativeUri "sessions" `
        -Body $body
}

function Send-ConversationFile {
    param (
        [string]$AgentName,
        [string]$ConversationId,
        [string]$FilePath,
        [string]$FileContentType
    )
    return Invoke-CoreAPI `
        -Method POST `
        -RelativeUri "files/upload?sessionId=$ConversationId&agentName=$AgentName" `
        -FilePath $FilePath `
        -FileContentType $FileContentType
}