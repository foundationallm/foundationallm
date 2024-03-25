#!/bin/pwsh
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

foreach ($servicePort in 5000..5010) {
    Write-Host "Testing Port #$servicePort..."
    curl "http://localhost:$servicePort/status"
}