function New-Completion {
    param (
        [string]$AgentName,
        [string]$ConversationId,
        [string]$UserPrompt,
        [hashtable]$Metadata = $null
    )

    $body = @{
        agent_name = $AgentName
        session_id = $ConversationId
        user_prompt = $UserPrompt
        metadata = $Metadata
    }

    return Invoke-CoreAPI `
        -Method POST `
        -RelativeUri "completions" `
        -Body $body
}