# System Logs & Audit Trails

This guide covers accessing and analyzing logs in FoundationaLLM deployments.

## Overview

FoundationaLLM centralizes logs in Azure Log Analytics Workspace, providing:
- Unified view of all platform components
- Correlation across services
- Advanced query capabilities
- Integration with Azure Monitor

## Log Types

| Log Type | Source | Purpose |
|----------|--------|---------|
| **Application Logs** | Container apps/pods | Application errors, info, debug |
| **Security Logs** | Entra ID, Key Vault | Authentication, authorization |
| **System Logs** | AKS, ACA infrastructure | Platform health, scaling |
| **Audit Logs** | Key Vault, Cosmos DB | Resource access tracking |

## Log Location

### Standard Deployment

All logs flow to the Log Analytics Workspace created during deployment:
- Located in the Operations (`ops`) resource group
- Named: `log-{project}-{env}-{region}`

### Quick Start Deployment

Logs are available in:
- Container Apps environment logs
- Log Analytics Workspace

## Accessing Logs

### Azure Portal

1. Navigate to **Log Analytics workspace**
2. Select **Logs** in the left menu
3. Use Kusto Query Language (KQL) to query

### Azure CLI

```bash
# Query logs
az monitor log-analytics query \
  --workspace <workspace-id> \
  --analytics-query "ContainerAppConsoleLogs | take 100"
```

## Common Queries

### Application Errors

```kql
// Last 24 hours of errors across all services
ContainerAppConsoleLogs
| where TimeGenerated > ago(24h)
| where Log contains "error" or Log contains "Error" or Log contains "ERROR"
| project TimeGenerated, ContainerAppName, Log
| order by TimeGenerated desc
```

### Specific Service Logs

```kql
// Core API logs
ContainerAppConsoleLogs
| where ContainerAppName contains "core-api"
| where TimeGenerated > ago(1h)
| project TimeGenerated, Log
| order by TimeGenerated desc
```

### Authentication Failures

```kql
// Failed authentication attempts
AADSignInLogs
| where TimeGenerated > ago(7d)
| where ResultType != 0
| project TimeGenerated, UserPrincipalName, ResultType, ResultDescription
| order by TimeGenerated desc
```

### Key Vault Access

```kql
// Key Vault operations
AzureDiagnostics
| where ResourceProvider == "MICROSOFT.KEYVAULT"
| where TimeGenerated > ago(24h)
| project TimeGenerated, OperationName, ResultType, CallerIPAddress
| order by TimeGenerated desc
```

### Request Performance

```kql
// API request duration
AppRequests
| where TimeGenerated > ago(1h)
| summarize avg(DurationMs), percentile(DurationMs, 95), count() by Name
| order by avg_DurationMs desc
```

### Container Restarts

```kql
// Container restart events
ContainerAppSystemLogs
| where TimeGenerated > ago(24h)
| where Reason == "Restarted" or Reason == "BackOff"
| project TimeGenerated, ContainerAppName, Reason, Log
| order by TimeGenerated desc
```

## Setting Up Alerts

### Create Alert Rule

1. Navigate to **Monitor** > **Alerts**
2. Click **+ Create** > **Alert rule**
3. Select your Log Analytics workspace
4. Configure condition (e.g., error count > threshold)
5. Configure action group (email, webhook, etc.)
6. Create rule

### Example: Error Spike Alert

```kql
// Alert condition query
ContainerAppConsoleLogs
| where TimeGenerated > ago(5m)
| where Log contains "error" or Log contains "ERROR"
| summarize ErrorCount = count() by bin(TimeGenerated, 5m)
| where ErrorCount > 10
```

## Log Retention

### Default Settings

| Log Type | Default Retention |
|----------|-------------------|
| Application Logs | 30 days |
| Security Logs | 90 days |
| System Logs | 30 days |

### Changing Retention

**Via Azure Portal:**
1. Navigate to Log Analytics workspace
2. Select **Usage and estimated costs**
3. Select **Data Retention**
4. Adjust retention period

**Via Azure CLI:**
```bash
az monitor log-analytics workspace update \
  --resource-group <resource-group> \
  --workspace-name <workspace-name> \
  --retention-time 90
```

## Long-Term Archival

For retention beyond 730 days:

### Export to Storage Account

1. Navigate to Log Analytics workspace
2. Select **Export** under Settings
3. Configure export to Storage Account
4. Select tables and destination

### Archive to Data Lake

Configure continuous export:
```bash
az monitor log-analytics workspace data-export create \
  --resource-group <resource-group> \
  --workspace-name <workspace-name> \
  --name "archive-export" \
  --destination <storage-account-resource-id> \
  --enable true \
  --tables ContainerAppConsoleLogs
```

## Access Control

### Required Permissions

| Role | Access |
|------|--------|
| **Log Analytics Reader** | Read logs, run queries |
| **Log Analytics Contributor** | Read/write, manage queries |
| **Monitoring Contributor** | Full access to monitoring |

### Restrict Access

Use Azure RBAC to limit log access:
```bash
az role assignment create \
  --assignee <user-or-group-id> \
  --role "Log Analytics Reader" \
  --scope <workspace-resource-id>
```

## Integration with Azure Monitor

### Application Insights

FoundationaLLM APIs integrate with Application Insights for:
- Request tracing
- Dependency tracking
- Performance metrics
- Custom telemetry

### Dashboards

Create custom dashboards:
1. Navigate to **Dashboard** in Azure Portal
2. Click **+ Add tile**
3. Select **Logs** and add your query
4. Configure visualization

### Workbooks

Use Azure Monitor Workbooks for interactive reports:
1. Navigate to **Monitor** > **Workbooks**
2. Create or use existing templates
3. Add queries and visualizations

## Azure Sentinel Integration

For advanced security monitoring:

1. **Enable Azure Sentinel** on the Log Analytics workspace
2. **Configure data connectors** for Entra ID, Key Vault
3. **Create analytics rules** for threat detection
4. **Set up playbooks** for automated response

> **Note:** Azure Sentinel is not configured by default in Standard deployment.

## Best Practices

| Practice | Description |
|----------|-------------|
| **Centralize** | Send all logs to single workspace |
| **Retain** | Set appropriate retention policies |
| **Alert** | Configure alerts for critical issues |
| **Review** | Regularly review security logs |
| **Archive** | Export for long-term compliance |

## Related Topics

- [Troubleshooting](troubleshooting.md)
- [Platform Security](../security-permissions/platform-security.md)
- [Backups & Data Resiliency](../how-to-guides/backups.md)
