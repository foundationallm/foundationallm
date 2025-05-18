#!/usr/bin/env pwsh

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Format-EnvironmentVariables.ps1
    . ./utility/Format-Json.ps1
    . ./utility/Get-AbsolutePath.ps1
    . ./utility/Get-Resource-Suffix.ps1
    . ./utility/Invoke-AndRequireSuccess.ps1
    . ./utility/Credential-Helpers.ps1
    . ./utility/Dns-Helpers.ps1
    . ./utility/Network-Helpers.ps1
    . ./utility/Escrow-Helpers.ps1
    . ./utility/Entra-Helpers.ps1
    . ./utility/FoundationaLLM-Helpers.ps1
}
finally {
    Pop-Location
}
