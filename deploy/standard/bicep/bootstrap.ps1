Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

try {
    Push-Location -Path ./tools/psake-4.9.0/src
    Import-Module -Name ./psake.psm1 -Scope Global -Force
}
catch {
    Write-Error -Message "Unable to import psake.psm1"
}
finally {
    Pop-Location
}
