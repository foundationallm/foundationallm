Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$PSAKE_VERSION = "4.9.0"

try {
    $url = "https://github.com/psake/psake/archive/refs/tags/v${PSAKE_VERSION}.zip"
    $outputPath = "./tools/psake.zip"

    if (Test-Path -Path "./tools/psake-${PSAKE_VERSION}") {
        Write-Host "psake-${PSAKE_VERSION} already exists."
    } else {
        Invoke-WebRequest -Uri $url -OutFile $outputPath
        Expand-Archive -Path $outputPath -DestinationPath ./tools
    }

    Write-Host "Importing psake.psm1"
    Push-Location -Path ./tools/psake-${PSAKE_VERSION}/src
    Import-Module -Name ./psake.psm1 -Scope Global -Force
}
catch {
    Write-Error -Message "Unable to import psake.psm1"
}
finally {
    Pop-Location
}
