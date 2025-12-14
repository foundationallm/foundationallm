# Deployment Information

Learn how to view deployment details and monitor API health for your FoundationaLLM instance.

## Overview

The Deployment Information page provides visibility into your FoundationaLLM deployment, including instance identification and API health status.

## Accessing Deployment Information

1. In the Management Portal sidebar, click **Deployment Information** under the **FLLM Platform** section
2. The deployment information page displays

## Instance Information

### Instance ID

The page displays your unique **Instance ID**:

- Used to identify your specific FoundationaLLM deployment
- Required when contacting support or troubleshooting
- Used internally for API routing and configuration

## API Status

The page displays status cards for core APIs:

### Management API

| Field | Description |
|-------|-------------|
| **Name** | Management API |
| **Description** | Used by the Management Portal to manage the FoundationaLLM platform |
| **Status Endpoint** | `/{instanceId}/status` |

### Authorization API

| Field | Description |
|-------|-------------|
| **Name** | Authorization API |
| **Description** | Manages role-based access control (RBAC) and auth-related functions |
| **Status Endpoint** | `/status` |

## API Status Indicators

Each API card shows:

| Indicator | Meaning |
|-----------|---------|
| **Green/Online** | API is responding normally |
| **Red/Offline** | API is not responding |
| **Yellow/Degraded** | API is responding but with issues |

## Monitoring API Health

### Real-Time Status

The API status cards refresh automatically or can be manually refreshed:

1. View the current status on each card
2. Check the timestamp of the last check
3. Refresh the page to get updated status

### Status Endpoints

You can also check API status directly:

**Management API:**
```
GET {ManagementAPIUrl}/instances/{instanceId}/status
```

**Authorization API:**
```
GET {AuthorizationAPIUrl}/status
```

## Use Cases

### Troubleshooting

When experiencing issues:

1. Check API status on this page
2. Note which APIs are offline or degraded
3. Provide this information to support

### Monitoring

For ongoing health monitoring:

1. Periodically check this page
2. Set up external monitoring to ping status endpoints
3. Configure alerts for API unavailability

### Documentation

When documenting your deployment:

1. Record the Instance ID
2. Note API endpoints for reference
3. Document expected normal status

## Additional APIs

> **TODO**: Document additional API status cards if they become available, such as:
> - Core API
> - Orchestration API
> - Gateway API

## Troubleshooting

### API Shows Offline

1. Check network connectivity
2. Verify the API service is running in Azure
3. Review Azure service health
4. Check for deployment/update in progress

### Status Not Updating

1. Refresh the browser page
2. Check browser console for errors
3. Verify authentication is still valid

### Cannot Access Page

1. Verify you have Management Portal access
2. Check your role assignments
3. Try signing out and back in

## Related Topics

- [Configuration](configuration.md)
- [APIs & SDKs Overview](../../../apis-sdks/index.md)
