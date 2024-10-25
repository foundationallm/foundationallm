#!/usr/bin/env pwsh

Push-Location $($MyInvocation.InvocationName | Split-Path)
try {
    . ./utility/Get-AbsolutePath.ps1
}
finally {
    Pop-Location
}
