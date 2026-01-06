# OpenAI Model Endpoint Facades

> [!NOTE]
> This feature is currently under development. Documentation will be updated as the feature becomes available.

## Overview

OpenAI Model Endpoint Facades provide an abstraction layer over OpenAI-compatible model endpoints, enabling:

- **Unified interface**: Single API surface for multiple model deployments
- **Load balancing**: Distribute requests across endpoints
- **Failover**: Automatic routing on endpoint failures
- **Quota management**: Centralized token and rate limit enforcement

## Concept

```
Client Applications
        |
        v
 ┌──────────────────┐
 │  Endpoint Facade │
 │                  │
 │  - Routing       │
 │  - Load Balance  │
 │  - Failover      │
 │  - Quota         │
 └──────────────────┘
        |
   ┌────┴────┬───────────┐
   v         v           v
┌─────┐  ┌─────┐    ┌─────┐
│ EP1 │  │ EP2 │    │ EP3 │
│AOAI │  │AOAI │    │ OAI │
└─────┘  └─────┘    └─────┘
```

## Architecture

### Components

<!-- [TODO: Document architecture when implementation is complete] -->

| Component | Description |
|-----------|-------------|
| Facade Gateway | Entry point for client requests |
| Endpoint Registry | Catalog of available endpoints |
| Load Balancer | Request distribution logic |
| Health Monitor | Endpoint availability tracking |

### Supported Endpoints

| Provider | Endpoint Type | Status |
|----------|--------------|--------|
| Azure OpenAI | Chat Completions | <!-- [TODO] --> |
| Azure OpenAI | Embeddings | <!-- [TODO] --> |
| OpenAI | Chat Completions | <!-- [TODO] --> |
| OpenAI | Embeddings | <!-- [TODO] --> |

## Configuration

### Defining Facades

<!-- [TODO: Document facade configuration when available] -->

### Endpoint Registration

<!-- [TODO: Document endpoint registration] -->

### Load Balancing Policies

| Policy | Description |
|--------|-------------|
| Round Robin | Rotate through endpoints |
| Weighted | Priority-based distribution |
| Least Connections | Route to least busy |
| Latency-based | Route to fastest |

<!-- [TODO: Confirm available policies] -->

### Failover Configuration

| Setting | Description |
|---------|-------------|
| Retry count | Attempts before failover |
| Timeout | Max wait per endpoint |
| Circuit breaker | Threshold for endpoint exclusion |

## Use Cases

### High Availability

Deploy multiple Azure OpenAI instances across regions:

```
Facade: "high-availability-completions"
├── Endpoint: East US (primary)
├── Endpoint: West US (secondary)
└── Endpoint: Central US (tertiary)
```

### Cost Optimization

Route to cost-effective endpoints when possible:

```
Facade: "cost-optimized"
├── Endpoint: GPT-4o-mini (priority: high)
└── Endpoint: GPT-4o (fallback for complex tasks)
```

### Quota Distribution

Spread load across quota pools:

```
Facade: "distributed-quota"
├── Endpoint: Subscription A (quota: 60K TPM)
├── Endpoint: Subscription B (quota: 60K TPM)
└── Endpoint: Subscription C (quota: 60K TPM)
Total available: 180K TPM
```

## Monitoring

### Metrics

| Metric | Description |
|--------|-------------|
| Requests per endpoint | Distribution visualization |
| Latency per endpoint | Performance comparison |
| Error rates | Endpoint health indicators |
| Failover events | Routing anomalies |

### Logging

<!-- [TODO: Document logging configuration] -->

## API Reference

<!-- [TODO: Document API when available] -->

### Create Facade

```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/endpointFacades
Content-Type: application/json

{
  "name": "my-facade",
  "endpoints": [...],
  "loadBalancingPolicy": "round-robin"
}
```

### List Facades

```http
GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/endpointFacades
```

## Current Status

<!-- [TODO: Update status as development progresses] -->

| Feature | Status |
|---------|--------|
| Basic routing | In Development |
| Load balancing | Planned |
| Failover | Planned |
| Monitoring | Planned |

## Related Topics

- [API Quota and Rate Limits](quota/quota-definition.md)
- [Token Usage Limits](quota/api-raw-request-rate.md)
- [Deployment Configuration](../deployment/deployment-configuration.md)
