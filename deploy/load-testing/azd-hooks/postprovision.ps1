#!/usr/bin/env pwsh

Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# Load utility functions
Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Load-Utility-Functions.ps1
}
finally {
    Pop-Location
}
