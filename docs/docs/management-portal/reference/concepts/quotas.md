# Quotas

This document provides reference information for quotas in FoundationaLLM.

## Overview

A FoundationaLLM quota is a set of rules that define the limits on the usage of resources by a client. Quota definitions are used to enforce limits on the usage of resources by clients and to prevent abuse of the FoundationaLLM platform.

Each quota is enforced on a specific metric:
- **API requests**: Quota enforces an API request rate limit
- **Agent completion requests**: Quota enforces an agent completion request rate limit

Quota metric limits can be enforced globally or using specific partitioning mechanisms:
- **None**: No partitioning, limit is enforced globally
- **User Identifier**: Limit is enforced per user unique identifier
- **User Principal Name**: Limit is enforced per user principal name

## Quota Definition

Quota definitions are stored in the main FoundationaLLM storage account, in the `quota` container in a file named `quota-store.json`.

### Structure

```json
{
    "name": "CoreAPICompletionsUPNRawRequestRateLimit",
    "description": "Defines a per UPN raw request rate limit on the Core API Completions controller.",
    "context": "CoreAPI:Completions",
    "type": "RawRequestRateLimit",
    "metric_partition": "UserPrincipalName",
    "metric_limit": 120,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": false
}
```

### Properties

| Property | Description |
|----------|-------------|
| `name` | The name of the quota definition |
| `description` | A description of the quota definition |
| `context` | The context (format: `<service>:<controller>` or `<service>:<controller>:<agent>`) |
| `type` | `RawRequestRateLimit` or `AgentRequestRateLimit` |
| `metric_partition` | `None`, `UserPrincipalName`, or `UserIdentifier` |
| `metric_limit` | Maximum number of requests in the time window |
| `metric_window_seconds` | Time window in seconds |
| `lockout_duration_seconds` | Lockout duration after exceeding quota |
| `distributed_enforcement` | Whether to enforce across multiple API instances |

### Smoothing Time Window

FoundationaLLM uses a smoothing time window of 20 seconds for quota enforcement. Recommendations:
- Set `metric_window_seconds` to a multiple of 20 seconds (60 seconds is standard)
- Set `metric_limit` to a multiple of `metric_window_seconds / 20`

---

## API Raw Request Rate

The API raw request rate quota limits the number of raw requests to API controllers.

### Supported Controllers

| Controller | Context |
|------------|---------|
| `Completions` | `CoreAPI:Completions` |
| `CompletionsStatus` | `CoreAPI:CompletionsStatus` |
| `Branding` | `CoreAPI:Branding` |
| `Configuration` | `CoreAPI:Configuration` |
| `Files` | `CoreAPI:Files` |
| `OneDriveWorkSchool` | `CoreAPI:OneDriveWorkSchool` |
| `Sessions` | `CoreAPI:Sessions` |
| `UserProfiles` | `CoreAPI:UserProfiles` |
| `Status` | `CoreAPI:Status` |

### Example: 100 API requests per minute per user

```json
{
    "name": "CoreAPICompletionsRateLimit",
    "description": "100 requests per minute per user",
    "context": "CoreAPI:Completions",
    "type": "RawRequestRateLimit",
    "metric_partition": "UserPrincipalName",
    "metric_limit": 100,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": false
}
```

### CompletionsStatus Controller Note

Starting with FoundationaLLM `v0.9.7-rc158`, the `CompletionsStatus` controller handles status checks. Client applications (especially the User Portal) poll this endpoint frequently. Consider setting higher limits for `CompletionsStatus` than for `Completions`.

---

## Agent Request Rate

The agent request rate quota limits completion requests to specific agents.

### Context Format

```
CoreAPI:Completions:<agent_name>
```

### Example: 50 completions per minute per user for a specific agent

```json
{
    "name": "MyAgentRateLimit",
    "description": "50 completions per minute per user",
    "context": "CoreAPI:Completions:my-agent-name",
    "type": "AgentRequestRateLimit",
    "metric_partition": "UserPrincipalName",
    "metric_limit": 50,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": false
}
```

---

## Metric Partition Guidance

| Scenario | Recommended Partition |
|----------|----------------------|
| Standard user access | `UserPrincipalName` |
| Service-to-service calls (managed identity) | `UserIdentifier` |
| Global limit for all users | `None` |

---

## Examples

### Global limit of 1000 requests per minute

```json
{
    "name": "GlobalAPILimit",
    "context": "CoreAPI:Completions",
    "type": "RawRequestRateLimit",
    "metric_partition": "None",
    "metric_limit": 1000,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": true
}
```

### Per-user limit with distributed enforcement

```json
{
    "name": "DistributedUserLimit",
    "context": "CoreAPI:Completions",
    "type": "RawRequestRateLimit",
    "metric_partition": "UserPrincipalName",
    "metric_limit": 120,
    "metric_window_seconds": 60,
    "lockout_duration_seconds": 60,
    "distributed_enforcement": true
}
```

## Related Topics

- [Configuring Quotas](../../how-to-guides/configuring-quotas.md)
- [Monitoring Token Consumption](../../../chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md)
