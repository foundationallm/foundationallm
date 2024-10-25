#!/usr/bin/env pwsh

function Invoke-AndRequireSuccess {
    <#
    .SYNOPSIS
    Invokes a script block and requires it to execute successfully.

    .DESCRIPTION
    The Invoke-AndRequireSuccess function is used to invoke a script block and ensure that it executes successfully. It takes a message and a script block as parameters. The function will display the message in blue color, execute the script block, and check the exit code. If the exit code is non-zero, an exception will be thrown.

    .PARAMETER Message
    The message to be displayed before executing the script block.

    .PARAMETER ScriptBlock
    The script block to be executed.

    .EXAMPLE
    Invoke-AndRequireSuccess -Message "Running script" -ScriptBlock {
        # Your script code here
    }

    This example demonstrates how to use the Invoke-AndRequireSuccess function to run a script block and require it to execute successfully.

    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    $result = & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }

    return $result
}
