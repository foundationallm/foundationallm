# Standard Deployment Local API Access

Standard deployments expose backend services internally, preventing API access over the public internet. Using the `kubectl` CLI, however, it is possible to forward FoundationaLLM APIs deployed within Kubernetes for local consumption.

## kubectl Forwarding Script

### Prerequisites

- `kubectl` with the [`kubelogin`](https://azure.github.io/kubelogin/) extension
  - Both of these utilities can be installed by the Azure CLI: `az aks install-cli`. If you use this command, you will need to restart your terminal to reflect the changes to `$PATH`.
- Kubernetes credentials stored in `$HOME/.kube/config`

  Obtain these credentials using the Azure CLI.

  ```
  az login
  az aks get-credentials --name MyManagedCluster --resource-group MyResourceGroup
  ```

### Script

Navigate to `/deploy/standard/scripts/Kubectl-Proxy.ps1` or copy the following PowerShell script to your local environment. Before running it, ensure that no applications are running on ports 5000-5010. To stop the tunnels, press any key in the terminal context where you started the script.

```pwsh
#!/bin/pwsh
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

$services = @{
    "foundationallm-orchestration-api" = 5000
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
```

> [!NOTE]
> You will need to rerun the script if you restart any nodes while the script is running.

### Verification

Run the following script to ensure that all APIs are accessible through `kubectl` forwarding.

```pwsh
#!/bin/pwsh
Set-PSDebug -Trace 0 # Echo every command (0 to disable, 1 to enable, 2 to enable verbose)
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

foreach ($servicePort in 5000..5010) {
    Write-Host "Testing Port #$servicePort..."
    curl "http://localhost:$servicePort/status"
}
```
