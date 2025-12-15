# Troubleshooting Guide

This guide provides structured approaches to diagnosing and resolving common issues in FoundationaLLM deployments.

## Quick Diagnostics

### Health Check Commands

```powershell
# AKS deployment - check all pods
kubectl get pods -n fllm

# AKS deployment - check services
kubectl get svc -n fllm

# Quick Start - check container apps
az containerapp list -g <resource-group> -o table
```

### Expected Healthy State

| Component | Status |
|-----------|--------|
| All pods | `Running` |
| All services | `Active` |
| All containers | `Running` |

## Common Issues

### 1. Authentication Failures

#### Symptoms
- Unable to log in to portals
- "Invalid token" errors
- Redirect loops after login

#### Diagnosis

```kql
// Check Entra ID sign-in logs
AADSignInLogs
| where TimeGenerated > ago(1h)
| where ResultType != 0
| project TimeGenerated, UserPrincipalName, AppDisplayName, ResultType, ResultDescription
```

#### Solutions

| Issue | Solution |
|-------|----------|
| **Invalid redirect URI** | Update redirect URIs in App Registration |
| **Missing scopes** | Configure API permissions |
| **Client secret expired** | Generate new secret, update Key Vault |
| **Wrong tenant** | Verify tenant ID in App Configuration |

**Verify App Registration:**
1. Open Azure Portal > **Microsoft Entra ID** > **App registrations**
2. Select the application
3. Check **Authentication** > **Redirect URIs**
4. Verify the URI matches your deployment URL + `/signin-oidc`

See [Authentication Setup](../security-permissions/authentication-authorization/index.md) for detailed configuration.

### 2. Missing App Configuration Values

#### Symptoms
- Services fail to start
- Configuration-related errors in logs
- "Key not found" errors

#### Diagnosis

```powershell
# Check App Configuration
az appconfig kv list --name <app-config-name> -o table

# Check specific key
az appconfig kv show --name <app-config-name> --key "FoundationaLLM:Instance:Id"
```

#### Solutions

1. **Verify Key Vault References**
   ```powershell
   # Check Key Vault secret exists
   az keyvault secret show --vault-name <vault-name> --name <secret-name>
   ```

2. **Check Managed Identity Permissions**
   - App Configuration Reader on App Config
   - Key Vault Secrets User on Key Vault

3. **Re-run Configuration Script**
   ```powershell
   # Quick Start
   cd deploy/quick-start
   ../common/scripts/Set-AzdEnvEntra.ps1
   ```

### 3. Container Crashes

#### Symptoms
- Pods in `CrashLoopBackOff` state
- Services intermittently unavailable
- Container restarts

#### Diagnosis

```powershell
# Get pod status
kubectl get pods -n fllm

# Describe failing pod
kubectl describe pod <pod-name> -n fllm

# Get logs from crashed container
kubectl logs <pod-name> -n fllm --previous
```

```kql
// Query for container crashes
ContainerAppConsoleLogs
| where TimeGenerated > ago(24h)
| where Log contains "exception" or Log contains "fatal" or Log contains "crash"
| project TimeGenerated, ContainerAppName, Log
| order by TimeGenerated desc
```

#### Solutions

| Issue | Solution |
|-------|----------|
| **Out of memory** | Increase memory limits in Helm values |
| **Missing config** | Check environment variables and App Config |
| **Dependency unavailable** | Verify dependent services are running |
| **Image pull error** | Check registry access and image tag |

**Check Resource Usage:**
```powershell
kubectl top pods -n fllm
```

**Update Resource Limits:**
```yaml
# In Helm values
resources:
  limits:
    memory: "2Gi"
    cpu: "1000m"
  requests:
    memory: "1Gi"
    cpu: "500m"
```

### 4. API Errors

#### Symptoms
- 500 errors from APIs
- Timeout errors
- Incomplete responses

#### Diagnosis

```kql
// API error analysis
AppRequests
| where TimeGenerated > ago(1h)
| where Success == false
| summarize count() by Name, ResultCode
| order by count_ desc
```

```powershell
# Check API pod logs
kubectl logs -n fllm deployment/core-api --tail=200
```

#### Solutions

| Error Code | Likely Cause | Solution |
|------------|--------------|----------|
| **401** | Authentication | Check token and permissions |
| **403** | Authorization | Verify RBAC roles |
| **500** | Server error | Check logs for details |
| **502** | Bad gateway | Check pod health |
| **503** | Service unavailable | Check service endpoints |
| **504** | Timeout | Check dependencies, increase timeout |

### 5. Azure OpenAI Errors

#### Symptoms
- "Model deployment not found"
- Quota exceeded errors
- Timeout on completions

#### Diagnosis

```powershell
# Check OpenAI deployment
az cognitiveservices account deployment list \
  --name <openai-account> \
  --resource-group <resource-group>

# Check quota
az cognitiveservices account show \
  --name <openai-account> \
  --resource-group <resource-group>
```

#### Solutions

| Issue | Solution |
|-------|----------|
| **Deployment not found** | Create model deployment in Azure Portal |
| **Quota exceeded** | Request quota increase or use different region |
| **Wrong deployment name** | Update App Configuration with correct name |
| **Region unavailable** | Check model availability by region |

### 6. Vector Search Issues

#### Symptoms
- No results from knowledge queries
- "Index not found" errors
- Slow search responses

#### Diagnosis

```powershell
# Check AI Search service
az search service show --name <search-service> --resource-group <resource-group>

# List indexes
az search index list --service-name <search-service> --resource-group <resource-group>
```

#### Solutions

1. **Verify Index Exists** - Check in Azure Portal
2. **Check Permissions** - Managed Identity needs Search Index Data Reader
3. **Re-run Indexing** - Trigger data pipeline

### 7. Network Connectivity

#### Symptoms
- Services can't communicate
- DNS resolution failures
- Timeout between services

#### Diagnosis (AKS)

```powershell
# Check service endpoints
kubectl get endpoints -n fllm

# Test DNS resolution
kubectl run dns-test --image=busybox --rm -it --restart=Never -- nslookup core-api.fllm.svc.cluster.local

# Check network policies
kubectl get networkpolicies -n fllm
```

#### Solutions

| Issue | Solution |
|-------|----------|
| **No endpoints** | Check pod selector labels |
| **DNS failure** | Restart CoreDNS pods |
| **Network policy blocking** | Review network policy rules |
| **Private endpoint issues** | Check NSG and private DNS zones |

## Diagnostic Tools

### Log Analytics Queries

Save these queries for quick access:

```kql
// Error summary by service
ContainerAppConsoleLogs
| where TimeGenerated > ago(24h)
| where Log contains "error" or Log contains "Error"
| summarize ErrorCount = count() by ContainerAppName
| order by ErrorCount desc

// Request latency percentiles
AppRequests
| where TimeGenerated > ago(1h)
| summarize 
    p50 = percentile(DurationMs, 50),
    p95 = percentile(DurationMs, 95),
    p99 = percentile(DurationMs, 99)
    by Name
```

### Health Check Script

```powershell
# Quick health check script
$namespace = "fllm"

Write-Host "=== Pod Status ===" -ForegroundColor Cyan
kubectl get pods -n $namespace

Write-Host "`n=== Service Status ===" -ForegroundColor Cyan
kubectl get svc -n $namespace

Write-Host "`n=== Recent Events ===" -ForegroundColor Cyan
kubectl get events -n $namespace --sort-by='.lastTimestamp' | Select-Object -Last 10

Write-Host "`n=== Resource Usage ===" -ForegroundColor Cyan
kubectl top pods -n $namespace
```

## Reporting Issues

If issues persist after troubleshooting:

### 1. Check Existing Issues

Search [GitHub Issues](https://github.com/foundationallm/foundationallm/issues) for similar problems.

### 2. Gather Information

Collect before opening an issue:
- Deployment type (Quick Start/Standard)
- Version/release tag
- Error messages and logs
- Steps to reproduce
- Configuration (sanitized)

### 3. Open GitHub Issue

1. Navigate to [GitHub Issues](https://github.com/foundationallm/foundationallm/issues)
2. Click **New Issue**
3. Select appropriate template
4. Provide detailed information
5. Add relevant labels

### Issue Template

```markdown
## Description
Brief description of the issue

## Environment
- Deployment Type: [Quick Start / Standard]
- Version: [e.g., 0.9.0]
- Azure Region: [e.g., eastus2]

## Steps to Reproduce
1. Step 1
2. Step 2
3. Step 3

## Expected Behavior
What should happen

## Actual Behavior
What actually happens

## Logs/Screenshots
[Attach relevant logs or screenshots]

## Additional Context
Any other relevant information
```

## Related Topics

- [System Logs](logs.md)
- [Authentication Setup](../security-permissions/authentication-authorization/index.md)
- [App Configuration Values](../deployment/app-configuration-values.md)
- [Platform Security](../security-permissions/platform-security.md)
