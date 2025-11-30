# FoundationaLLM Usage Analytics - Executive Summary

## Overview

This document provides a high-level summary of the FoundationaLLM Usage Analytics plan. The full detailed plan is available in [Usage Analytics Plan](./usage-analytics-plan.md).

## Purpose

Enable comprehensive analytics on FoundationaLLM platform usage ("proprietary exhaust") using **existing data sources only** while maintaining strict privacy protections for message content and user identities.

## Key Principle: No-Touch Data Collection

**All analytics are derived from existing data sources** - no new data collection mechanisms are required. The system analyzes data already being stored in:

- **Azure Cosmos DB**: Sessions, Messages, Attachments, State
- **Azure Application Insights**: Telemetry traces, custom events, dependencies
- **Azure Log Analytics**: Application logs, performance counters
- **Azure Data Lake**: Exported telemetry and batch-processed data

## Analytics Hierarchy

The analytics system is organized in a thoughtful hierarchical structure:

### Level 1: Agents (Top Level)
Primary focus on agent usage, performance, and patterns

### Level 2: Tools
Analysis of tool usage within agent contexts

### Level 3: Models
Analysis of model usage and performance

## Agent Analytics (Primary Focus)

### Usage Metrics
- **Unique Users**: Count of distinct users (anonymized) using each agent over time periods
- **Token Consumption**: Total and average tokens consumed per agent
- **Conversation Patterns**: Average rounds of interaction per conversation
- **Session Metrics**: Total sessions, session duration, conversation length distribution

### Tool Analysis
- **Tool Combinations**: Most common tool combinations used together in conversations
- **Tool Frequency**: Most frequently used tools per agent
- **Tool Usage Rate**: Percentage of conversations using each tool
- **Tool Sequences**: Common sequences of tool invocations

### File Analytics
- **Average Files per Conversation**: Mean number of files uploaded per agent conversation
- **File Sizes**: Average, median, and distribution of file sizes
- **File Types**: Most common file types uploaded (by MIME type)
- **File Type Distribution**: File type breakdown per agent

### Performance Metrics
- **Average Response Time**: Mean latency for agent completions
- **Response Time Distribution**: Percentiles (p50, p75, p90, p95, p99)
- **Response Time Trends**: Latency over time
- **Performance Breakdown**: Orchestration time, model time, tool execution time

## Key Principles

### ✅ What We Analyze
- **Aggregated Metrics**: Token counts, request volumes, response times
- **Agent Metadata**: Agent names, agent types, workflow types
- **Resource Usage**: Model names, token consumption, feature usage
- **Username/UPN**: Displayed for operational abuse detection (required for security)
- **Temporal Patterns**: Usage trends, peak times, growth patterns
- **User Behavior**: Usage patterns, activity timelines (without content)

### ❌ What We Never Analyze
- **Message Content**: User prompts, completions, conversation text
- **Sensitive PII**: Email addresses, full names (beyond username/UPN)
- **Sensitive Metadata**: File names, attachment content, document identifiers
- **Full Prompts**: System prompts or prompt templates with actual content

## Architecture Highlights

### Data Sources (Existing)
- **Cosmos DB Containers**: Sessions, Messages, Attachments, State, ExternalResources
- **Application Insights**: OpenTelemetry traces with agent names, operation IDs, response times
- **Log Analytics**: Performance and error logs
- **Data Lake**: Historical aggregations

### Data Processing
- **Anonymization**: One-way hashing of user identities with secure salt
- **Aggregation**: Temporal and dimensional aggregation of existing data
- **Analysis**: Query-based analytics on existing data stores
- **API Layer**: Management API endpoints serving analytics data
- **Dashboard**: Custom Vue.js dashboard in Management Portal

### Analytics Capabilities
1. **Agent Analytics**: Usage, performance, tool combinations, file patterns
2. **Tool Analytics**: Tool usage across agents, tool performance
3. **Model Analytics**: Model selection, performance, cost analysis
4. **User Analytics**: Top users, usage patterns, abuse detection
5. **Business Intelligence**: Capacity planning, cost analysis, quality metrics
6. **Abuse Detection**: Anomaly detection, risk scoring, security monitoring

### Dashboard Integration
- **Custom Dashboard**: Built within Management Portal (Nuxt.js/Vue.js)
- **Chart Library**: PrimeVue Charts for visualizations
- **Real-time Updates**: API-driven data fetching with refresh capability
- **Responsive Design**: Works on desktop and tablet devices

## Implementation Approach

### Backend Components
- **Analytics Service**: Query execution, data aggregation, anonymization
- **Abuse Detection Service**: Risk scoring, anomaly detection, pattern recognition
- **Management API Controller**: REST endpoints for all analytics data
- **Data Models**: Structured response models for frontend consumption

### Frontend Components
- **Dashboard Pages**: Overview, Agents, Tools, Models, Users
- **Chart Components**: Reusable visualization components
- **Abuse Detection UI**: Anomaly alerts, risk indicators, user flags
- **API Integration**: Data fetching and state management

### Key Implementation Areas
1. **Query Development**: Cosmos DB and Application Insights queries
2. **Anomaly Detection**: Statistical analysis and rule-based detection
3. **Risk Scoring**: Multi-factor abuse risk calculation
4. **Dashboard UI**: Interactive charts, tables, and visualizations
5. **Privacy Safeguards**: Content filtering, username-only display

## Privacy & Security

### Privacy Safeguards
- Automated content filtering (queries exclude message content)
- PII anonymization via one-way hashing
- Quarterly privacy compliance audits
- Configurable data retention (default: 90 days aggregated)

### Security Measures
- Encryption at rest and in transit
- Role-based access control (separate from production)
- Audit logging for all analytics access
- Secure salt management via Azure Key Vault

### Compliance
- GDPR: Anonymized data, right to deletion support
- SOC 2: Access controls, audit trails
- HIPAA: No PHI in analytics (if applicable)

## Success Criteria

1. ✅ Zero message content in analytics data
2. ✅ Zero PII in analytics data
3. ✅ All analytics derived from existing data sources (no new collection)
4. ✅ Comprehensive agent, tool, and model analytics
5. ✅ Real-time and historical dashboards operational
6. ✅ Privacy compliance validated

## Business Value

### For Platform Operators
- **Capacity Planning**: Understand usage patterns to optimize resource allocation
- **Cost Management**: Track and optimize costs by agent, model, and feature
- **Performance Optimization**: Identify bottlenecks and optimize response times
- **Quality Assurance**: Monitor error rates and system health

### For Business Stakeholders
- **Adoption Insights**: Understand which agents and features drive value
- **ROI Analysis**: Measure platform utilization and effectiveness
- **Growth Planning**: Forecast capacity needs based on usage trends
- **Risk Management**: Monitor system health and compliance metrics

## Key Metrics Tracked

| Category | Metrics |
|----------|---------|
| **Agent Usage** | Unique users (anonymized), conversations, token consumption, tool combinations, rounds of interaction |
| **File Analytics** | Files per conversation, file sizes, file types, file type distribution |
| **Performance** | Response times (p50/p95/p99), latency trends, performance breakdown |
| **Tools** | Tool frequency, tool combinations, tool usage rate, tool sequences |
| **Models** | Model selection, model performance, model cost, model comparison |
| **Temporal** | Peak usage times, growth trends, seasonal patterns |

## Example Analytics Queries

### Agent Usage with Anonymized Users
```kusto
// Application Insights query
traces
| where name == "Completions_GetCompletion"
| extend AgentName = customDimensions["FoundationaLLM-AgentName"]
| extend UserHash = hash(customDimensions["FoundationaLLM-UPN"], 12345)
| summarize 
    RequestCount = count(),
    UniqueUsers = dcount(UserHash),
    AvgResponseTime = avg(duration)
    by AgentName, bin(timestamp, 1d)
```

### Tool Combinations from Cosmos DB
```sql
-- Extract tool combinations from messages
SELECT 
    ARRAY(SELECT VALUE r.toolName FROM r IN c.analysisResults) as tools
FROM c
WHERE c.type = "Message" AND IS_ARRAY(c.analysisResults)
```

## Next Steps

1. **Review & Approval**: Stakeholder review of detailed plan
2. **Resource Allocation**: Assign analytics and data engineering team members
3. **Phase 1 Kickoff**: Begin data exploration and schema documentation
4. **Privacy Review**: Engage privacy/compliance team for validation
5. **Pilot Implementation**: Test queries and anonymization with sample data

## Questions & Contact

For detailed technical information, see [Usage Analytics Plan](./usage-analytics-plan.md).

For questions or clarifications, contact the FoundationaLLM development team.
