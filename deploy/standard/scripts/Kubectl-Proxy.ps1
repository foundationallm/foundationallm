#!/bin/pwsh
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$services = @{
    "foundationallm-agent-factory-api" = 5000
    "foundationallm-gatekeeper-api" = 5001
    "foundationallm-agent-hub-api" = 5002
    "foundationallm-core-api" = 5003
    "foundationallm-data-source-hub-api" = 5004
    "foundationallm-gatekeeper-integration-api" = 5005
    "foundationallm-langchain-api" = 5006
    "foundationallm-management-api" = 5007
    "foundationallm-prompt-hub-api" = 5008
    "foundationallm-semantic-kernel-api" = 5009
    "foundationallm-vectorization-api" = 5010
}
$jobIds = @()

try {
    foreach ($servicePortPairing in $services.GetEnumerator()) {
        Write-Host "Starting Kubectl Tunnel for $($servicePortPairing.key)"
        $job = Start-Job -ScriptBlock ([scriptblock]::Create("kubectl port-forward service/$($servicePortPairing.key) $($servicePortPairing.value):80"))
        Write-Host "Job: $($job.Command)"
        $jobIds += $job.Id
    }

    Write-Host "Press any key to kill the Kubernetes tunnels..."
    $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
catch {}
finally {
    foreach ($jobId in $jobIds) {
        Write-Host "Killing $jobId"
        Stop-Job -Id $jobId
    }
}