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