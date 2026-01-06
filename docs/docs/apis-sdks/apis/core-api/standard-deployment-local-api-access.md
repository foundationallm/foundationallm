# Standard Deployment Local API Access

Standard (AKS) deployments expose backend services internally, preventing direct API access over the public internet. This guide explains how to use `kubectl` port forwarding to access FoundationaLLM APIs locally during development.

## Overview

In standard deployments:
- APIs are deployed within Kubernetes and not publicly exposed
- Access requires port forwarding through `kubectl`
- This approach is useful for development and testing

## Prerequisites

### Required Tools

| Tool | Description | Installation |
|------|-------------|--------------|
| `kubectl` | Kubernetes CLI | `az aks install-cli` |
| `kubelogin` | Azure authentication for kubectl | `az aks install-cli` |
| Azure CLI | For authentication | [Install Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) |

### Setup Steps

1. **Install kubectl and kubelogin:**

   ```bash
   az aks install-cli
   ```

   > Restart your terminal after installation to update `$PATH`.

2. **Authenticate with Azure:**

   ```bash
   az login
   ```

3. **Get Kubernetes credentials:**

   ```bash
   az aks get-credentials \
     --name <your-aks-cluster-name> \
     --resource-group <your-resource-group>
   ```

   This stores credentials in `$HOME/.kube/config`.

## Port Forwarding Script

The following PowerShell script forwards all FoundationaLLM API services to local ports:

### Service Port Mappings

| Service | Local Port |
|---------|------------|
| Orchestration API | 5000 |
| Gatekeeper API | 5001 |
| Agent Hub API | 5002 |
| **Core API** | **5003** |
| Data Source Hub API | 5004 |
| Gatekeeper Integration API | 5005 |
| LangChain API | 5006 |
| **Management API** | **5007** |
| Prompt Hub API | 5008 |
| Semantic Kernel API | 5009 |
| Vectorization API | 5010 |

### Forwarding Script

Save this script or run from `/deploy/standard/scripts/Kubectl-Proxy.ps1`:

```powershell
#!/bin/pwsh
Set-PSDebug -Trace 0
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
        $job = Start-Job -ScriptBlock ([scriptblock]::Create(
            "kubectl port-forward service/$($servicePortPairing.key) $($servicePortPairing.value):80"
        ))
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

### Usage

1. Ensure ports 5000-5010 are not in use
2. Run the script:

   ```powershell
   ./Kubectl-Proxy.ps1
   ```

3. The script runs until you press any key
4. Access APIs at `http://localhost:{port}`

## Verification Script

Verify all APIs are accessible:

```powershell
#!/bin/pwsh
Set-PSDebug -Trace 0
Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

foreach ($servicePort in 5000..5010) {
    Write-Host "Testing Port #$servicePort..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$servicePort/status" -TimeoutSec 5
        Write-Host "  Status: $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        Write-Host "  Failed: $_" -ForegroundColor Red
    }
}
```

## Calling Local APIs

Once forwarding is active:

### Core API

```bash
# Status check
curl http://localhost:5003/status

# List sessions (with auth)
curl http://localhost:5003/instances/{instanceId}/sessions \
  -H "Authorization: Bearer $TOKEN"
```

### Management API

```bash
# Status check
curl http://localhost:5007/status

# List agents (with auth)
curl http://localhost:5007/instances/{instanceId}/providers/FoundationaLLM.Agent/agents \
  -H "Authorization: Bearer $TOKEN"
```

## Troubleshooting

### "Unable to connect" Errors

- Verify kubectl is authenticated: `kubectl get pods`
- Check the service exists: `kubectl get services`
- Ensure port is not already in use

### Connection Drops

- The forwarding script requires an active terminal
- Rerun the script if connection drops
- Node restarts may require script restart

### Authentication Errors

- Run `az login` to refresh Azure credentials
- Run `az aks get-credentials` to refresh Kubernetes credentials
- Verify kubelogin is installed: `kubelogin --version`

## Alternative: Single Service Forwarding

To forward just one service:

```bash
# Forward Core API only
kubectl port-forward service/foundationallm-core-api 5003:80

# Forward Management API only
kubectl port-forward service/foundationallm-management-api 5007:80
```

## Related Topics

- [Core API Overview](index.md)
- [Finding Your Core API URL](finding-core-api-url.md)
- [Directly Calling Core API](directly-calling-core-api.md)
