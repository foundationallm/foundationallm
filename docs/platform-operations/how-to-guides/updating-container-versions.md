# Updating Container Versions

This guide covers updating FoundationaLLM container images in Standard (AKS) deployments.

## Overview

Container updates are required to:
- Apply bug fixes
- Access new features
- Address security vulnerabilities
- Maintain platform compatibility

## Prerequisites

| Requirement | Description |
|-------------|-------------|
| **Azure CLI** | Configured for correct tenant/subscription |
| **AKS Credentials** | In `.kube/config` directory |
| **Helm CLI** | Installed and configured |
| **kubectl** | Installed and configured |

### Getting AKS Credentials

```powershell
az aks get-credentials `
    --name "<aks-cluster-name>" `
    --resource-group "<resource-group>"
```

## Update Process

### Step 1: Check Current Versions

```powershell
# List all Helm releases
helm list -A

# Check specific deployment
kubectl get deployment -n fllm -o wide
```

### Step 2: Get Current Values

Export the current Helm values:

```powershell
# For backend services
helm get values "foundationallm" --all > gvalues.yaml

# For frontend services
helm get values "foundationallm-web" --all > gvalues-web.yaml
```

### Step 3: Identify Target Version

Check available versions:
- [GitHub Releases](https://github.com/foundationallm/foundationallm/releases)
- Container registry tags

### Step 4: Update Images

Use the deployment script:

```powershell
cd deploy/scripts

.\Deploy-Images-Aks.ps1 `
    -aksName "<aks-name>" `
    -resourceGroup "<resource-group>" `
    -tag "<version-tag>" `
    -charts "*" `
    -namespace "fllm"
```

## Script Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `-name` | `foundationallm` | Helm release name |
| `-aksName` | (Required) | AKS cluster name |
| `-resourceGroup` | (Required) | Azure resource group |
| `-tag` | `latest` | Image version tag |
| `-charts` | `*` | Charts to deploy (`*` for all) |
| `-valuesFile` | `gvalues.yaml` | Helm values file path |
| `-namespace` | (Current) | Kubernetes namespace |
| `-tlsEnv` | `prod` | TLS environment (`prod`, `staging`, `none`, `custom`) |
| `-tlsHost` | - | Custom TLS hostname |
| `-tlsSecretName` | - | Custom TLS secret name |
| `-autoscale` | `$false` | Enable autoscaling |

## Selective Updates

### Update Specific Charts

```powershell
# Update only API services
.\Deploy-Images-Aks.ps1 `
    -aksName "<aks-name>" `
    -resourceGroup "<resource-group>" `
    -tag "0.9.0" `
    -charts "core-api,management-api"
```

### Available Charts

| Chart | Description |
|-------|-------------|
| `core-api` | Core API service |
| `management-api` | Management API service |
| `orchestration-api` | Orchestration service |
| `gatekeeper-api` | Content safety service |
| `chat-ui` | Chat portal |
| `management-ui` | Management portal |

### Update Frontend Only

```powershell
.\Deploy-Images-Aks.ps1 `
    -aksName "<frontend-aks-name>" `
    -resourceGroup "<resource-group>" `
    -tag "0.9.0" `
    -charts "chat-ui,management-ui"
```

## Rolling Updates

Kubernetes performs rolling updates by default:
- Old pods remain until new pods are healthy
- No downtime during updates
- Automatic rollback on failure

### Monitor Update Progress

```powershell
# Watch deployment status
kubectl rollout status deployment/core-api -n fllm

# View pod status
kubectl get pods -n fllm -w
```

### Rollback if Needed

```powershell
# Rollback to previous version
kubectl rollout undo deployment/core-api -n fllm

# Rollback Helm release
helm rollback foundationallm 1 -n fllm
```

## Custom Values

### Modifying Configuration

Edit `gvalues.yaml` to customize:

```yaml
# Example: Increase replica count
coreApi:
  replicaCount: 3
  resources:
    requests:
      cpu: "500m"
      memory: "512Mi"
    limits:
      cpu: "1000m"
      memory: "1Gi"
```

### Apply Custom Values

```powershell
.\Deploy-Images-Aks.ps1 `
    -aksName "<aks-name>" `
    -resourceGroup "<resource-group>" `
    -tag "0.9.0" `
    -valuesFile "custom-values.yaml"
```

## Verifying Updates

### Check Image Versions

```powershell
kubectl get pods -n fllm -o jsonpath="{.items[*].spec.containers[*].image}"
```

### Check Service Health

```powershell
# Get pod status
kubectl get pods -n fllm

# Check logs
kubectl logs -n fllm deployment/core-api --tail=100
```

### Test Functionality

1. Access Chat Portal
2. Send a test message
3. Verify agent response
4. Check Management Portal

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Pods not starting | Check `kubectl describe pod <pod-name> -n fllm` |
| Image pull errors | Verify registry access and image tag |
| Service unavailable | Check `kubectl get svc -n fllm` |
| Configuration issues | Compare `gvalues.yaml` with defaults |

### Common Errors

**ImagePullBackOff:**
```powershell
# Check image details
kubectl describe pod <pod-name> -n fllm | grep -A 5 "Image:"

# Verify registry credentials
kubectl get secrets -n fllm
```

**CrashLoopBackOff:**
```powershell
# Check pod logs
kubectl logs <pod-name> -n fllm --previous

# Check events
kubectl get events -n fllm --sort-by='.lastTimestamp'
```

## Maintenance Windows

### Best Practices

1. **Schedule Updates** during low-traffic periods
2. **Test in Staging** before production
3. **Review Release Notes** for breaking changes
4. **Backup** before major updates
5. **Monitor** closely after updates

### Update Checklist

- [ ] Review release notes
- [ ] Backup current configuration
- [ ] Update staging environment
- [ ] Test functionality in staging
- [ ] Schedule production maintenance window
- [ ] Update production
- [ ] Verify all services
- [ ] Monitor for issues

## Related Topics

- [Standard Deployment](../deployment/deployment-standard.md)
- [Creating Release Notes](creating-release-notes.md)
- [Troubleshooting](../monitoring-troubleshooting/troubleshooting.md)
- [GitHub Releases](https://github.com/foundationallm/foundationallm/releases)
