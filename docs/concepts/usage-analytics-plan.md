# FoundationaLLM Usage Analytics Plan

## Executive Summary

This document outlines a comprehensive plan for implementing usage analytics ("proprietary exhaust") for FoundationaLLM while strictly protecting sensitive message content and personally identifiable information (PII). The analytics system will provide valuable insights into platform usage patterns, agent performance, and resource utilization without exposing user conversations or identifying information.

## Objectives

1. **Track Usage Patterns**: Understand how users interact with FoundationaLLM agents
2. **Monitor Performance**: Analyze agent effectiveness, response times, and resource consumption
3. **Optimize Resources**: Identify popular agents, peak usage times, and resource bottlenecks
4. **Protect Privacy**: Ensure no message content or PII is stored or exposed in analytics
5. **Enable Business Intelligence**: Provide aggregated insights for platform improvement and capacity planning

## Privacy and Security Principles

### Data Exclusion (Never Collected)
- **Message Content**: User prompts, completions, and conversation history
- **PII**: User names, email addresses, UPNs, user IDs (in raw form)
- **Sensitive Metadata**: File names, attachment content, document identifiers
- **Full Prompts**: System prompts, prompt templates with actual content
- **Embeddings**: Vector embeddings derived from user content

### Data Inclusion (Safe to Collect)
- **Aggregated Metrics**: Token counts, request counts, response times
- **Anonymized Identifiers**: Hashed user identifiers for session correlation only
- **Agent Metadata**: Agent names, agent types, workflow types
- **Resource Usage**: Model names, API endpoints, token consumption
- **Temporal Data**: Timestamps (hour/day level, not precise), time zones
- **Request Characteristics**: Request size categories, operation types, error codes
- **Feature Usage**: Gatekeeper options used, attachments count (not content)

## Architecture Overview

### Data Collection Layer

#### Collection Points
1. **CoreAPI CompletionsController** (`/workspace/src/dotnet/CoreAPI/Controllers/CompletionsController.cs`)
   - Primary entry point for user-facing completions
   - Collects: agent usage, request patterns, user session data (anonymized)

2. **GatekeeperAPI CompletionsController** (`/workspace/src/dotnet/GatekeeperAPI/Controllers/CompletionsController.cs`)
   - Security and content filtering layer
   - Collects: gatekeeper feature usage, content safety metrics

3. **OrchestrationAPI CompletionsController** (`/workspace/src/dotnet/OrchestrationAPI/Controllers/CompletionsController.cs`)
   - Orchestration layer
   - Collects: model usage, token consumption, orchestration patterns

4. **GatewayAPI CompletionsController** (`/workspace/src/dotnet/GatewayAPI/Controllers/CompletionsController.cs`)
   - Gateway layer for LLM routing
   - Collects: model selection patterns, load balancing metrics

#### Collection Mechanism
- **Middleware/Interceptors**: Add analytics collection as middleware in the request pipeline
- **Event-Driven**: Emit analytics events asynchronously to avoid impacting request latency
- **Structured Logging**: Extend existing OpenTelemetry infrastructure for structured events

### Data Processing Layer

#### Anonymization Service
- **User Identity Hashing**: One-way hash (SHA-256) of UserId/UPN with salt
  - Salt stored securely in Azure Key Vault
  - Hash used only for session correlation, not user identification
- **Content Filtering**: Strip all message content before processing
- **Metadata Sanitization**: Remove or generalize sensitive metadata

#### Aggregation Service
- **Real-time Aggregation**: Stream processing for near-real-time dashboards
- **Batch Aggregation**: Daily/hourly aggregation for historical analysis
- **Dimensional Aggregation**: Pre-aggregate by common dimensions (agent, time, model)

### Data Storage Layer

#### Storage Options
1. **Azure Application Insights** (Primary)
   - Extend existing telemetry infrastructure
   - Custom events and metrics
   - Built-in retention and query capabilities

2. **Azure Data Explorer (Kusto)** (Analytics)
   - Long-term storage for historical analysis
   - Advanced query capabilities
   - Integration with Power BI

3. **Azure Cosmos DB** (Optional - Real-time)
   - For real-time dashboards requiring sub-second latency
   - Time-series optimized collections

#### Data Schema

```json
{
  "eventId": "guid",
  "timestamp": "2024-01-15T10:30:00Z",
  "instanceId": "foundationallm-instance-001",
  "eventType": "completion_request|completion_response|error",
  
  // Anonymized user context
  "userHash": "sha256-hash-of-userid-with-salt",
  "sessionHash": "sha256-hash-of-sessionid",
  
  // Agent context
  "agentName": "customer-support-agent",
  "agentType": "generic|workflow|assistant",
  "workflowType": "langchain|semantic-kernel|azure-openai-assistants",
  
  // Request characteristics (NO CONTENT)
  "requestSizeCategory": "small|medium|large|xl", // Based on token count
  "hasAttachments": true,
  "attachmentCount": 2,
  "hasMessageHistory": true,
  "messageHistoryLength": 5,
  "gatekeeperOptions": ["content_safety", "pii_detection"],
  
  // Model and resource usage
  "modelName": "gpt-4",
  "modelProvider": "azure-openai",
  "promptTokens": 150,
  "completionTokens": 200,
  "totalTokens": 350,
  "estimatedCost": 0.0025,
  
  // Performance metrics
  "responseTimeMs": 1250,
  "orchestrationTimeMs": 1100,
  "modelTimeMs": 1000,
  
  // Outcome
  "statusCode": 200,
  "hasErrors": false,
  "errorCategory": null, // "quota_exceeded|content_filtered|model_error"
  
  // Feature usage
  "featuresUsed": ["rag", "semantic_cache", "prompt_rewrite"],
  "contentArtifactsCount": 3,
  
  // Temporal (generalized)
  "hourOfDay": 10,
  "dayOfWeek": "Monday",
  "isBusinessHours": true
}
```

## Analytics Capabilities

### 1. Usage Analytics

#### Agent Usage
- Most popular agents by request volume
- Agent usage trends over time
- Agent adoption rates
- Agent usage by user segment (anonymized)

#### User Engagement
- Active user counts (anonymized, aggregated)
- Session duration patterns
- Requests per session
- User retention metrics

#### Temporal Patterns
- Peak usage hours/days
- Usage patterns by time zone
- Seasonal trends
- Growth trends

### 2. Performance Analytics

#### Response Times
- Average response times by agent
- Response time percentiles (p50, p95, p99)
- Response time trends
- Orchestration vs model time breakdown

#### Resource Utilization
- Token consumption by agent/model
- Cost analysis by agent/model
- Model selection patterns
- Load distribution across models

#### Error Analysis
- Error rates by agent/model
- Error category distribution
- Error trends over time
- Quota utilization

### 3. Feature Analytics

#### Feature Adoption
- Gatekeeper feature usage
- RAG usage patterns
- Semantic cache hit rates
- Prompt rewrite usage

#### Workflow Analytics
- Workflow type distribution
- Workflow performance comparison
- Custom workflow usage

### 4. Business Intelligence

#### Capacity Planning
- Request volume projections
- Resource consumption forecasts
- Scaling recommendations

#### Cost Analysis
- Cost per agent
- Cost trends
- Cost optimization opportunities

#### Quality Metrics
- Content safety filter rates
- PII detection rates
- User satisfaction indicators (if collected separately)

## Implementation Plan

### Phase 1: Foundation (Weeks 1-2)

#### 1.1 Create Analytics Service Interface
- **Location**: `/workspace/src/dotnet/Common/Services/Analytics/`
- **Components**:
  - `IAnalyticsService` interface
  - `AnalyticsEvent` model (schema defined above)
  - `AnalyticsService` implementation
  - Dependency injection setup

#### 1.2 Implement Anonymization Service
- **Location**: `/workspace/src/dotnet/Common/Services/Analytics/`
- **Components**:
  - `IAnonymizationService` interface
  - `AnonymizationService` implementation
  - Secure salt management via Azure Key Vault
  - User identity hashing
  - Session hashing

#### 1.3 Extend Telemetry Infrastructure
- **Location**: `/workspace/src/dotnet/Common/Telemetry/`
- **Components**:
  - Extend `TelemetryActivityTagNames` with analytics tags
  - Add analytics event emission to existing telemetry
  - Configure Application Insights custom events

### Phase 2: Data Collection (Weeks 3-4)

#### 2.1 CoreAPI Integration
- **File**: `/workspace/src/dotnet/CoreAPI/Controllers/CompletionsController.cs`
- **Changes**:
  - Inject `IAnalyticsService` into controller
  - Emit analytics events in `GetCompletion` and `StartCompletionOperation`
  - Collect completion response data
  - Ensure no content is included

#### 2.2 GatekeeperAPI Integration
- **File**: `/workspace/src/dotnet/GatekeeperAPI/Controllers/CompletionsController.cs`
- **Changes**:
  - Track gatekeeper feature usage
  - Record content safety metrics (counts only, no content)
  - Track PII detection events (counts only)

#### 2.3 OrchestrationAPI Integration
- **File**: `/workspace/src/dotnet/OrchestrationAPI/Controllers/CompletionsController.cs`
- **Changes**:
  - Track model selection
  - Record token usage
  - Track orchestration performance metrics

#### 2.4 GatewayAPI Integration
- **File**: `/workspace/src/dotnet/GatewayAPI/Controllers/CompletionsController.cs`
- **Changes**:
  - Track model routing decisions
  - Record load balancing metrics

### Phase 3: Data Processing (Weeks 5-6)

#### 3.1 Event Processing Pipeline
- **Components**:
  - Azure Event Grid or Service Bus for event queuing
  - Azure Functions for event processing
  - Anonymization pipeline
  - Aggregation logic

#### 3.2 Storage Implementation
- **Components**:
  - Application Insights custom events
  - Azure Data Explorer tables
  - Data retention policies
  - Data export to analytics warehouse

### Phase 4: Analytics Dashboard (Weeks 7-8)

#### 4.1 Power BI Dashboard
- **Components**:
  - Usage overview dashboard
  - Performance metrics dashboard
  - Cost analysis dashboard
  - Agent analytics dashboard

#### 4.2 Application Insights Workbooks
- **Components**:
  - Real-time usage monitoring
  - Error tracking
  - Performance monitoring

### Phase 5: Testing and Validation (Week 9)

#### 5.1 Privacy Validation
- Verify no PII in analytics data
- Verify no message content in analytics data
- Test anonymization effectiveness
- Security review

#### 5.2 Performance Testing
- Ensure analytics collection doesn't impact request latency
- Load testing with analytics enabled
- Verify async processing doesn't block requests

#### 5.3 Data Quality Testing
- Verify data accuracy
- Test aggregation logic
- Validate metrics calculations

## Security and Compliance

### Data Protection
1. **Encryption**: All analytics data encrypted at rest and in transit
2. **Access Control**: Role-based access to analytics data (separate from production data)
3. **Audit Logging**: All access to analytics data logged
4. **Data Retention**: Configurable retention policies (default: 90 days aggregated, 1 year summary)

### Compliance Considerations
1. **GDPR**: Anonymized data, right to deletion (remove from analytics)
2. **SOC 2**: Access controls, audit trails
3. **HIPAA**: If applicable, ensure no PHI in analytics

### Privacy Safeguards
1. **Content Filtering**: Automated checks to prevent content leakage
2. **PII Detection**: Scan analytics events for potential PII (false positive handling)
3. **Regular Audits**: Quarterly reviews of analytics data for privacy compliance

## Configuration

### Application Settings
```json
{
  "FoundationaLLM:Analytics": {
    "Enabled": true,
    "CollectionLevel": "standard", // minimal|standard|detailed
    "AnonymizationSalt": "@Microsoft.KeyVault(SecretUri=...)",
    "ApplicationInsightsConnectionString": "...",
    "DataExplorerConnectionString": "...",
    "RetentionDays": 90,
    "AggregationIntervalMinutes": 60,
    "EnableRealTimeDashboard": true
  }
}
```

### Feature Flags
- Enable/disable analytics per instance
- Control collection granularity
- Toggle specific analytics features

## Monitoring and Alerting

### Key Metrics to Monitor
1. Analytics event emission rate
2. Analytics processing latency
3. Storage consumption
4. Anonymization service health
5. Data quality metrics

### Alerts
1. Analytics collection failures
2. Unusual data patterns (potential content leakage)
3. Storage capacity warnings
4. Processing delays

## Success Criteria

1. ✅ Zero message content in analytics data
2. ✅ Zero PII in analytics data
3. ✅ <10ms latency impact from analytics collection
4. ✅ 99.9% analytics event delivery rate
5. ✅ Comprehensive dashboards operational
6. ✅ Privacy compliance validated

## Future Enhancements

1. **Machine Learning Insights**: Anomaly detection, usage prediction
2. **A/B Testing Framework**: Agent comparison, feature testing
3. **User Segmentation**: Anonymous user cohorts for analysis
4. **Advanced Cost Optimization**: ML-based cost prediction and optimization
5. **Quality Metrics**: Automated quality scoring (without content)

## Appendix

### Data Dictionary

| Field | Type | Description | Privacy Level |
|-------|------|-------------|---------------|
| eventId | Guid | Unique event identifier | Safe |
| timestamp | DateTime | Event timestamp (UTC) | Safe |
| instanceId | String | FoundationaLLM instance ID | Safe |
| userHash | String | SHA-256 hash of UserId | Anonymized |
| sessionHash | String | SHA-256 hash of SessionId | Anonymized |
| agentName | String | Agent identifier | Safe |
| promptTokens | Int | Token count (no content) | Safe |
| completionTokens | Int | Token count (no content) | Safe |
| responseTimeMs | Int | Response time in milliseconds | Safe |

### Example Queries

#### Most Popular Agents
```kusto
UsageEvents
| where timestamp > ago(7d)
| summarize RequestCount = count() by agentName
| order by RequestCount desc
| take 10
```

#### Average Response Time by Agent
```kusto
UsageEvents
| where timestamp > ago(7d) and eventType == "completion_response"
| summarize AvgResponseTime = avg(responseTimeMs) by agentName
| order by AvgResponseTime asc
```

#### Token Consumption Trends
```kusto
UsageEvents
| where timestamp > ago(30d)
| summarize TotalTokens = sum(totalTokens) by bin(timestamp, 1d), agentName
| render timechart
```

## References

- [Azure Application Insights Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Azure Data Explorer Documentation](https://learn.microsoft.com/en-us/azure/data-explorer/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/specs/)
- FoundationaLLM Telemetry Implementation: `/workspace/src/dotnet/Common/Telemetry/`
- FoundationaLLM Authentication: `/workspace/src/dotnet/Common/Models/Authentication/`
