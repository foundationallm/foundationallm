# Configuring Quotas

Learn how to configure and manage quotas to control resource usage in FoundationaLLM.

## Overview

Quotas define limits on resource usage to:

- **Prevent abuse**: Stop runaway usage or malicious activity
- **Manage costs**: Control AI model token consumption
- **Ensure fairness**: Distribute resources among users
- **Maintain performance**: Prevent system overload

## Quota Types

### API Raw Request Rate

Limits the number of raw API calls made to FoundationaLLM services.

| Metric | Description |
|--------|-------------|
| `api_requests` | Total API calls |
| Period | Minute, Hour, Day |

### Agent Request Rate

Limits the number of agent completion requests (conversations with AI).

| Metric | Description |
|--------|-------------|
| `agent_requests` | Agent completion calls |
| Period | Minute, Hour, Day |

## Quota Configuration

> **TODO**: Document the UI for quota configuration in the Management Portal if available. Currently, quotas are configured via App Configuration.

### Via Azure App Configuration

Quota definitions are stored in Azure App Configuration and the storage account.

### Quota Definition Structure

```json
{
  "name": "api-rate-limit",
  "metric": "api_requests",
  "limit": 100,
  "period": "minute",
  "partitioning": "user_principal_name"
}
```

| Field | Description |
|-------|-------------|
| `name` | Unique quota identifier |
| `metric` | What to measure (`api_requests`, `agent_requests`) |
| `limit` | Maximum allowed within period |
| `period` | Time window (`minute`, `hour`, `day`) |
| `partitioning` | How to segment limits (optional) |

## Partitioning Options

Partitioning determines how quotas are applied:

| Partitioning | Description |
|--------------|-------------|
| **None** | Global limit shared by all users |
| `user_identifier` | Per-user by internal user ID |
| `user_principal_name` | Per-user by Azure AD UPN/email |

## Configuration Examples

### Global Rate Limit

100 API requests per minute for all users combined:

```json
{
  "name": "global-api-limit",
  "metric": "api_requests",
  "limit": 100,
  "period": "minute"
}
```

### Per-User Rate Limit

50 requests per user per minute:

```json
{
  "name": "user-api-limit",
  "metric": "api_requests",
  "limit": 50,
  "period": "minute",
  "partitioning": "user_principal_name"
}
```

### Daily Agent Request Limit

1000 agent requests per user per day:

```json
{
  "name": "daily-agent-limit",
  "metric": "agent_requests",
  "limit": 1000,
  "period": "day",
  "partitioning": "user_principal_name"
}
```

## Applying Quotas

### Instance-Level Quotas

Apply to the entire FoundationaLLM instance.

### Agent-Level Quotas

> **TODO**: Document agent-level quota configuration if available.

## Monitoring Quota Usage

### In the Chat User Portal

- Token consumption is displayed per message (if enabled)
- Users can see their current usage

### In Azure Monitor

- Review API metrics and logs
- Set up alerts for quota threshold warnings

### In the Management Portal

> **TODO**: Document quota monitoring dashboards in the Management Portal if available.

## Quota Exceeded Behavior

When a quota is exceeded:

1. **API Response**: Returns HTTP 429 (Too Many Requests)
2. **Error Message**: Indicates quota type and reset time
3. **User Experience**: Chat User Portal shows an appropriate error

## Best Practices

### Setting Appropriate Limits

| Use Case | Recommendation |
|----------|----------------|
| **Development** | Higher limits for testing |
| **Production** | Balanced limits for normal usage |
| **Public apps** | Stricter limits to prevent abuse |

### Monitoring and Adjustment

1. Start with conservative limits
2. Monitor actual usage patterns
3. Adjust limits based on real needs
4. Set up alerts before limits are hit

### Communication

- Document quota limits for users
- Provide clear error messages
- Offer escalation paths for legitimate high-usage scenarios

## Troubleshooting

### Users Hitting Limits Unexpectedly

- Review current quota configuration
- Check for automated tools consuming quota
- Consider if limits are appropriately set

### Quotas Not Enforcing

- Verify quota configuration is properly deployed
- Check the quota applies to the correct scope
- Review service logs for quota processing

### Performance Impact

- Quota checking adds minimal latency
- Use appropriate partitioning to distribute checks
- Consider caching for high-volume scenarios

## Related Topics

- [Quotas Reference](../reference/concepts/quotas.md)
- [Monitoring Token Consumption](../../chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md)
- [Configuration Reference](../reference/configuration-reference.md)
