# Configuring Quotas

Learn how to configure and manage quotas in FoundationaLLM.

## Overview

Quotas define limits on resource usage to prevent abuse and manage costs.

## Quota Types

### API Raw Request Rate
Limits on API calls per time period.

### Agent Request Rate
Limits on agent completion calls per time period.

## Configuring Quotas

### Via App Configuration
Quota definitions are stored in the main storage account.

### Quota Definition Structure
```json
{
  "metric": "api_requests",
  "limit": 100,
  "period": "minute",
  "partitioning": "user_principal_name"
}
```

## Partitioning Options

- **None**: Global limit for all users
- **User Identifier**: Per-user limits by ID
- **User Principal Name**: Per-user limits by UPN

## Examples

### 100 API requests per minute globally
```json
{
  "metric": "api_requests",
  "limit": 100,
  "period": "minute"
}
```

### 50 requests per user per minute
```json
{
  "metric": "api_requests",
  "limit": 50,
  "period": "minute",
  "partitioning": "user_principal_name"
}
```

## Monitoring Quota Usage

- Check user token consumption in the portal
- Review API metrics in Azure Monitor

## Related Topics

- [Quotas Reference](reference/concepts/quotas.md)
- [Monitoring Token Consumption](../chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md)
