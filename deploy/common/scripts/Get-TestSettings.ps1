#! /usr/bin/pwsh

Param(
    [parameter(Mandatory = $true)][string]$storageAccountName
)

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

function Invoke-CLICommand {
    <#
    .SYNOPSIS
    Invoke a CLI Command and allow all output to print to the terminal.  Does not check for return values or pass the output to the caller.
    #>
    param (
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Message,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock]$ScriptBlock
    )

    Write-Host "${message}..." -ForegroundColor Blue
    & $ScriptBlock

    if ($LASTEXITCODE -ne 0) {
        throw "Failed ${message} (code: ${LASTEXITCODE})"
    }
}

function Get-AbsolutePath {
    <#
    .SYNOPSIS
    Get the absolute path of a file or directory. Relative path does not need to exist.
    #>
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true, Position = 0)]
        [string]$RelatviePath
    )

    return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($RelatviePath)
}

$testSettingsPath = "../../../tests/dotnet/Core.Examples/testsettings.e2e.json" | Get-AbsolutePath
Write-Host "Writing testsettings.e2e.json data to: $testSettingsPath" -ForegroundColor Yellow
Invoke-CLICommand "Copy testsettings.e2e.json data from the storage account: ${storageAccountName}" {
    azcopy copy "https://$($storageAccountName).blob.core.windows.net/e2e/testsettings.e2e.json" $testSettingsPath
}