#!/usr/bin/env pwsh

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Format-EnvironmentVariables.ps1
    . ./utility/Format-Json.ps1
    . ./utility/Get-AbsolutePath.ps1
    . ./utility/Get-Resource-Suffix.ps1
    . ./utility/Invoke-AndRequireSuccess.ps1
}
finally {
    Pop-Location
}
