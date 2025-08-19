function New-Completion {
    param (
        [string]$AgentName,
        [string]$ConversationId,
        [string]$UserPrompt,
        [string[]]$Attachments = $null,
        [hashtable]$Metadata = $null
    )

    $body = @{
        agent_name = $AgentName
        session_id = $ConversationId
        user_prompt = $UserPrompt
        attachments = $Attachments
        metadata = $Metadata
    }

    return Invoke-CoreAPI `
        -Method POST `
        -RelativeUri "completions" `
        -Body $body
}