# FoundationaLLM Usage Analytics Plan

## Executive Summary

This document outlines a comprehensive plan for implementing usage analytics ("proprietary exhaust") for FoundationaLLM using **existing data sources only**. The analytics system will analyze data already being collected in Azure Cosmos DB, Azure Application Insights, Azure Log Analytics, and Azure Data Lake without requiring any new data collection mechanisms. The system strictly protects sensitive message content and personally identifiable information (PII) through anonymization and aggregation.

## Objectives

1. **Analyze Existing Data**: Leverage data already stored in Cosmos DB, App Insights, Log Analytics, and Data Lake
2. **Provide Agentic Insights**: Understand usage patterns across Agents, Tools, and Models
3. **Monitor Performance**: Analyze agent effectiveness, response times, and resource consumption
4. **Protect Privacy**: Ensure no message content or PII is exposed in analytics through anonymization
5. **Enable Business Intelligence**: Provide aggregated insights for platform improvement and capacity planning

## Privacy and Security Principles

### Data Exclusion (Never Exposed)
- **Message Content**: User prompts, completions, and conversation text (filtered out in queries)
- **PII**: User names, email addresses, UPNs (anonymized via hashing)
- **Sensitive Metadata**: File names, attachment content, document identifiers
- **Full Prompts**: System prompts, prompt templates with actual content

### Data Inclusion (Safe to Analyze)
- **Aggregated Metrics**: Token counts, request counts, response times
- **Anonymized Identifiers**: Hashed user identifiers for correlation only
- **Agent Metadata**: Agent names, agent types, workflow types
- **Resource Usage**: Model names, API endpoints, token consumption
- **Temporal Data**: Timestamps (hour/day level), time zones
- **Request Characteristics**: Request size categories, operation types, error codes
- **Feature Usage**: Tool names, attachment counts, file types (not content)

## Data Sources

### 1. Azure Cosmos DB

#### Containers Available

**Sessions Container**
- **Conversation Documents** (`type = "Session"`):
  - `id`: Session identifier
  - `sessionId`: Partition key
  - `upn`: User Principal Name (for anonymization)
  - `tokensUsed`: Total tokens in session
  - `metadata`: Session metadata
  - `_ts`: Timestamp

- **Message Documents** (`type = "Message"`):
  - `id`: Message identifier
  - `sessionId`: Partition key (links to conversation)
  - `operationId`: Operation identifier
  - `sender`: "User" or "Agent"
  - `tokens`: Token count for message
  - `attachments`: List of attachment object IDs
  - `contentArtifacts`: Array of content artifacts (sources used)
  - `analysisResults`: Array of tool analysis results (includes `toolName`)
  - `completionPromptId`: Links to completion prompt
  - `_ts`: Timestamp

**Attachments Container**
- **Attachment Documents**:
  - `id`: Attachment identifier
  - `fileObjectId`: File object identifier
  - `fileName`: File name (can be anonymized)
  - `fileSize`: File size in bytes
  - `contentType`: MIME type / file type
  - `uploadedBy`: UPN (for anonymization)
  - `_ts`: Upload timestamp

**State Container**
- **LongRunningOperation Documents**:
  - `operationId`: Operation identifier
  - `status`: Operation status
  - `upn`: User Principal Name
  - `_ts`: Timestamp

**ExternalResources Container**
- **Azure OpenAI Mappings**:
  - `type`: Resource type
  - `fileObjectId`: File identifier
  - `openAIFileId`: OpenAI file identifier
  - File metadata

### 2. Azure Application Insights

#### Telemetry Data Available

**Traces** (OpenTelemetry Activities):
- `FoundationaLLM-InstanceId`: Instance identifier
- `FoundationaLLM-OperationId`: Operation identifier
- `FoundationaLLM-ConversationId`: Session/conversation identifier
- `FoundationaLLM-UPN`: User Principal Name (for anonymization)
- `FoundationaLLM-UserId`: User identifier (for anonymization)
- `FoundationaLLM-AgentName`: Agent name
- Duration: Response time measurements
- Activity names: `Completions_GetCompletion`, `AsyncCompletions_StartCompletionOperation`

**Custom Events**:
- Operation completion events
- Error events
- Feature usage events

**Dependencies**:
- Downstream API calls
- Model API calls
- External service calls

### 3. Azure Log Analytics

#### Log Data Available
- Application logs from all services
- Performance counters
- Error logs
- Request/response logs (without content)

### 4. Azure Data Lake (if configured)

#### Data Available
- Exported telemetry data
- Batch-processed analytics data
- Historical aggregations

## Analytics Hierarchy

The analytics system is organized in a hierarchical structure focusing on three primary dimensions:

### Level 1: Agents
The top-level analysis focuses on agent usage and performance.

### Level 2: Tools
Analysis of tool usage within agent contexts.

### Level 3: Models
Analysis of model usage and performance.

## Agent Analytics

### Usage Metrics

#### User Engagement
- **Unique Users per Agent**: Count of distinct users (anonymized) using each agent over time periods
  - Query: `SELECT DISTINCT upn FROM Sessions WHERE agentName = @agent AND _ts >= @startTime AND _ts < @endTime`
  - Anonymize UPNs before counting
  - Time periods: Daily, Weekly, Monthly, Quarterly

- **Active Sessions**: Number of conversations per agent
  - Query: `SELECT COUNT(*) FROM Sessions WHERE type = "Session" AND agentName = @agent AND _ts >= @startTime`

- **Session Duration**: Average and median conversation length
  - Calculate from first message to last message timestamp per session

#### Token Consumption
- **Total Tokens per Agent**: Sum of tokens across all messages for an agent
  - Query: `SELECT SUM(tokens) FROM Messages WHERE sessionId IN (SELECT id FROM Sessions WHERE agentName = @agent)`
  - Also available from `Conversation.tokensUsed` field

- **Average Tokens per Session**: Mean token usage per conversation
- **Token Distribution**: Percentiles (p50, p75, p90, p95, p99) of token usage
- **Token Trends**: Token consumption over time (daily/weekly/monthly)

#### Conversation Patterns
- **Rounds of Interaction**: Average number of message exchanges per conversation
  - Query: Count messages per session, group by sender alternation
  - Formula: `COUNT(Messages) / 2` per session (assuming User-Agent pairs)

- **Conversation Length Distribution**: Histogram of conversation lengths
- **Multi-turn vs Single-turn**: Percentage of conversations with >1 round

### Tool Analytics

#### Tool Usage Patterns
- **Tool Combinations**: Most common tool combinations used together in conversations
  - Source: `Message.analysisResults[].toolName` array
  - Query: Extract all `toolName` values from messages in agent's sessions
  - Analyze co-occurrence patterns

- **Tool Frequency**: Most frequently used tools per agent
  - Query: `SELECT toolName, COUNT(*) FROM Messages WHERE analysisResults.toolName IS NOT NULL GROUP BY toolName`

- **Tool Usage Rate**: Percentage of conversations using each tool
- **Tool Sequences**: Common sequences of tool invocations

#### Tool Performance
- **Tool Execution Time**: Average time for tool execution (from telemetry)
- **Tool Success Rate**: Percentage of successful tool invocations
- **Tool Error Analysis**: Error patterns by tool type

### File Analytics

#### File Upload Patterns
- **Average Files per Conversation**: Mean number of files uploaded per agent conversation
  - Query: `SELECT AVG(ARRAY_LENGTH(attachments)) FROM Messages WHERE sender = "User" AND attachments IS NOT NULL AND sessionId IN (SELECT id FROM Sessions WHERE agentName = @agent)`

- **File Upload Distribution**: Histogram of file counts per conversation
- **Files per Agent**: Total files uploaded to each agent

#### File Characteristics
- **File Sizes**: Average, median, and distribution of file sizes
  - Query: Join Messages.attachments[] with Attachments container
  - Source: `Attachment.fileSize`
  - Metrics: Mean, median, percentiles, total storage

- **File Types**: Most common file types uploaded
  - Query: `SELECT contentType, COUNT(*) FROM Attachments WHERE id IN (SELECT VALUE attachment FROM Messages WHERE attachments IS NOT NULL) GROUP BY contentType`
  - Group by MIME type categories (e.g., "application/pdf", "image/*", "text/*")

- **File Type by Agent**: File type distribution per agent
- **Large File Analysis**: Files exceeding size thresholds

### Performance Analytics

#### Latency Metrics
- **Average Response Time**: Mean latency for agent completions
  - Source: Application Insights traces
  - Query: `traces | where name == "Completions_GetCompletion" and customDimensions["FoundationaLLM-AgentName"] == @agent | summarize avg(duration)`

- **Response Time Distribution**: Percentiles (p50, p75, p90, p95, p99)
- **Response Time Trends**: Latency over time
- **Response Time by Model**: Latency breakdown by underlying model

#### Performance Breakdown
- **Orchestration Time**: Time spent in orchestration layer
- **Model Time**: Time spent calling LLM models
- **Tool Execution Time**: Time spent executing tools
- **RAG Time**: Time spent in retrieval-augmented generation

#### Error Analysis
- **Error Rate**: Percentage of failed requests per agent
- **Error Types**: Distribution of error categories
- **Error Trends**: Error rate over time
- **Error Recovery**: Retry success rates

## Tool Analytics

### Tool Usage Across Agents
- **Tool Popularity**: Most used tools across all agents
- **Tool Adoption**: Number of agents using each tool
- **Tool Specialization**: Tools unique to specific agents

### Tool Performance
- **Tool Execution Metrics**: Average execution time, success rate
- **Tool Error Patterns**: Common failure modes
- **Tool Efficiency**: Tokens consumed vs. value provided

## Model Analytics

### Model Usage
- **Model Selection**: Which models are used most frequently
- **Model Distribution**: Model usage by agent
- **Model Switching**: Patterns of model selection changes

### Model Performance
- **Model Latency**: Response times by model
- **Model Token Efficiency**: Tokens consumed per completion
- **Model Cost Analysis**: Cost per model, cost trends
- **Model Error Rates**: Error rates by model

### Model Comparison
- **A/B Comparison**: Performance comparison between models
- **Model Recommendations**: Optimal model selection by use case

## Implementation Architecture

### Data Processing Pipeline

#### Phase 1: Data Extraction
1. **Cosmos DB Queries**
   - Scheduled queries to extract aggregated data
   - Use Cosmos DB change feed for incremental updates
   - Partition queries by time windows

2. **Application Insights Queries**
   - KQL queries to extract telemetry data
   - Time-range queries for historical analysis
   - Real-time queries for dashboards

3. **Log Analytics Queries**
   - Extract performance and error data
   - Correlate with Cosmos DB data using operation IDs

#### Phase 2: Data Anonymization
1. **User Identity Hashing**
   - Hash UPNs and UserIds using SHA-256 with salt
   - Salt stored in Azure Key Vault
   - One-way hashing (cannot reverse)

2. **Content Filtering**
   - Strip message text from queries
   - Remove file names from results
   - Generalize sensitive metadata

#### Phase 3: Data Aggregation
1. **Temporal Aggregation**
   - Aggregate by hour, day, week, month
   - Pre-compute common aggregations

2. **Dimensional Aggregation**
   - Aggregate by agent, tool, model
   - Cross-dimensional analysis

3. **Statistical Aggregation**
   - Calculate means, medians, percentiles
   - Compute distributions and histograms

#### Phase 4: Data Storage
1. **Analytics Data Lake**
   - Store aggregated analytics data
   - Partitioned by date and dimension
   - Optimized for query performance

2. **Power BI Dataset**
   - Import aggregated data
   - Create semantic model
   - Enable self-service analytics

## Analytics Queries

### Agent Usage Query Example

```kusto
// Application Insights: Agent usage with anonymized users
let startTime = ago(30d);
let endTime = now();
traces
| where timestamp between (startTime .. endTime)
| where name == "Completions_GetCompletion"
| extend AgentName = tostring(customDimensions["FoundationaLLM-AgentName"])
| extend UserHash = hash(tostring(customDimensions["FoundationaLLM-UPN"]), 12345) // Salted hash
| extend ResponseTime = duration
| summarize 
    RequestCount = count(),
    UniqueUsers = dcount(UserHash),
    AvgResponseTime = avg(ResponseTime),
    P95ResponseTime = percentile(ResponseTime, 95)
    by AgentName, bin(timestamp, 1d)
| order by timestamp desc, RequestCount desc
```

### Cosmos DB Query Example

```sql
-- Agent token consumption with anonymized users
SELECT 
    c.agentName,
    COUNT(DISTINCT c.upn) as uniqueUsers, -- Will be anonymized in processing
    SUM(c.tokensUsed) as totalTokens,
    AVG(c.tokensUsed) as avgTokensPerSession,
    COUNT(*) as sessionCount
FROM c
WHERE c.type = "Session"
    AND c._ts >= @startTimestamp
    AND c._ts < @endTimestamp
    AND c.agentName = @agentName
GROUP BY c.agentName
```

### Tool Combination Analysis Query

```sql
-- Extract tool combinations from messages
SELECT 
    c.sessionId,
    ARRAY(SELECT VALUE r.toolName FROM r IN c.analysisResults WHERE r.toolName IS NOT NULL) as tools
FROM c
WHERE c.type = "Message"
    AND IS_ARRAY(c.analysisResults)
    AND ARRAY_LENGTH(c.analysisResults) > 0
    AND c.sessionId IN (
        SELECT VALUE s.id FROM s WHERE s.type = "Session" AND s.agentName = @agentName
    )
```

### File Analytics Query

```sql
-- File upload statistics per agent
SELECT 
    a.contentType,
    COUNT(*) as fileCount,
    AVG(a.fileSize) as avgFileSize,
    SUM(a.fileSize) as totalSize,
    MIN(a.fileSize) as minSize,
    MAX(a.fileSize) as maxSize
FROM a IN Attachments
WHERE a.id IN (
    SELECT VALUE attachment FROM m IN Messages 
    WHERE m.type = "Message" 
        AND m.sender = "User"
        AND IS_ARRAY(m.attachments)
        AND m.sessionId IN (
            SELECT VALUE s.id FROM s WHERE s.type = "Session" AND s.agentName = @agentName
        )
)
GROUP BY a.contentType
ORDER BY fileCount DESC
```

## Dashboard Design

### Agent Analytics Dashboard

#### Overview Section
- Total agents
- Total conversations
- Total tokens consumed
- Average response time
- Active users (anonymized count)

#### Agent Performance Table
Columns:
- Agent Name
- Unique Users (anonymized)
- Total Conversations
- Total Tokens
- Avg Tokens per Session
- Avg Response Time (ms)
- P95 Response Time (ms)
- Error Rate (%)
- Most Used Tools
- Avg Files per Conversation

#### Agent Usage Trends
- Line chart: Conversations over time by agent
- Line chart: Token consumption over time
- Line chart: Response time trends
- Bar chart: Top agents by usage

#### Agent Deep Dive
- Agent selector dropdown
- Conversation length distribution
- Tool usage frequency
- File type distribution
- Response time distribution
- Error analysis

### Tool Analytics Dashboard

#### Tool Overview
- Total unique tools
- Most popular tools
- Tools by agent
- Tool usage trends

#### Tool Performance
- Average execution time
- Success rate
- Error analysis
- Tool efficiency metrics

### Model Analytics Dashboard

#### Model Usage
- Model selection distribution
- Model usage by agent
- Model switching patterns

#### Model Performance
- Response time by model
- Token efficiency
- Cost analysis
- Error rates

## Implementation Plan

### Phase 1: Data Exploration and Schema Design (Week 1)

#### 1.1 Data Source Analysis
- Document all available data sources
- Identify data schemas and structures
- Map relationships between data sources
- Document anonymization requirements

#### 1.2 Analytics Schema Design
- Design aggregated data schema
- Define anonymization procedures
- Design data partitioning strategy
- Create data dictionary

### Phase 2: Query Development (Weeks 2-3)

#### 2.1 Cosmos DB Queries
- Develop agent usage queries
- Develop tool analysis queries
- Develop file analytics queries
- Develop conversation pattern queries

#### 2.2 Application Insights Queries
- Develop performance queries
- Develop error analysis queries
- Develop user engagement queries
- Develop model usage queries

#### 2.3 Data Lake Queries
- Develop historical analysis queries
- Develop trend analysis queries
- Develop comparative analysis queries

### Phase 3: Anonymization Implementation (Week 4)

#### 3.1 Anonymization Service
- Implement user identity hashing
- Implement content filtering
- Implement metadata sanitization
- Test anonymization effectiveness

#### 3.2 Privacy Validation
- Verify no PII in results
- Verify no message content
- Security review
- Compliance validation

### Phase 4: Aggregation Pipeline (Weeks 5-6)

#### 4.1 Batch Processing
- Implement scheduled aggregation jobs
- Implement incremental updates
- Implement error handling
- Implement monitoring

#### 4.2 Real-time Processing
- Implement streaming aggregations
- Implement real-time dashboards
- Implement alerting

### Phase 5: Dashboard Development (Weeks 7-8)

#### 5.1 Power BI Dashboards
- Agent analytics dashboard
- Tool analytics dashboard
- Model analytics dashboard
- Executive summary dashboard

#### 5.2 Application Insights Workbooks
- Real-time monitoring
- Performance dashboards
- Error tracking

### Phase 6: Testing and Validation (Week 9)

#### 6.1 Data Quality Testing
- Verify query accuracy
- Validate aggregations
- Test edge cases
- Performance testing

#### 6.2 Privacy Testing
- Verify anonymization
- Test for content leakage
- Security audit
- Compliance validation

## Configuration

### Analytics Settings

```json
{
  "FoundationaLLM:Analytics": {
    "Enabled": true,
    "AnonymizationSalt": "@Microsoft.KeyVault(SecretUri=...)",
    "CosmosDBConnectionString": "...",
    "ApplicationInsightsConnectionString": "...",
    "LogAnalyticsWorkspaceId": "...",
    "DataLakeConnectionString": "...",
    "AggregationSchedule": "0 0 * * *", // Daily at midnight
    "RetentionDays": 90,
    "EnableRealTimeDashboard": true
  }
}
```

## Security and Compliance

### Data Protection
1. **Access Control**: Role-based access to analytics data
2. **Encryption**: All data encrypted at rest and in transit
3. **Audit Logging**: All analytics access logged
4. **Data Retention**: Configurable retention policies

### Privacy Safeguards
1. **Anonymization**: All user identifiers hashed
2. **Content Filtering**: Automated content exclusion
3. **Regular Audits**: Quarterly privacy compliance reviews
4. **Data Minimization**: Only aggregate necessary data

### Compliance
1. **GDPR**: Anonymized data, right to deletion support
2. **SOC 2**: Access controls, audit trails
3. **HIPAA**: No PHI in analytics (if applicable)

## Success Criteria

1. ✅ Zero message content in analytics data
2. ✅ Zero PII in analytics data
3. ✅ All analytics derived from existing data sources
4. ✅ Comprehensive agent, tool, and model analytics
5. ✅ Real-time and historical dashboards operational
6. ✅ Privacy compliance validated

## Key Metrics Summary

### Agent Metrics
- Unique users per agent (anonymized)
- Total tokens consumed
- Average tokens per session
- Tool combinations used
- Rounds of interaction per conversation
- Average files uploaded
- File sizes and types
- Average response time
- Response time percentiles

### Tool Metrics
- Tool usage frequency
- Tool combinations
- Tool execution time
- Tool success rate

### Model Metrics
- Model selection patterns
- Model performance (latency, tokens, cost)
- Model error rates
- Model comparison metrics

## Future Enhancements

1. **Predictive Analytics**: Usage forecasting, capacity planning
2. **Anomaly Detection**: Unusual usage patterns
3. **Recommendation Engine**: Agent/tool/model recommendations
4. **Cost Optimization**: ML-based cost optimization
5. **Quality Metrics**: Automated quality scoring

## Appendix

### Data Source Mapping

| Analytics Need | Data Source | Query Type |
|----------------|-------------|------------|
| Agent usage | Cosmos DB (Sessions) | SQL |
| User count | Cosmos DB (Sessions) + Anonymization | SQL + Processing |
| Token consumption | Cosmos DB (Messages/Sessions) | SQL |
| Tool usage | Cosmos DB (Messages.analysisResults) | SQL |
| File statistics | Cosmos DB (Attachments) | SQL |
| Response times | Application Insights | KQL |
| Error rates | Application Insights + Log Analytics | KQL |
| Model usage | Application Insights | KQL |
| Historical trends | Data Lake | SQL/KQL |

### Example Anonymization

```csharp
// Pseudo-code for UPN anonymization
public string AnonymizeUPN(string upn, string salt)
{
    if (string.IsNullOrEmpty(upn) || upn == "N/A")
        return "anonymous";
    
    using (var sha256 = SHA256.Create())
    {
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(upn + salt));
        return Convert.ToBase64String(hash).Substring(0, 16); // Truncate for readability
    }
}
```

## References

- [Azure Cosmos DB Query Documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/getting-started)
- [Azure Application Insights KQL Documentation](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)
- [Azure Log Analytics Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/log-analytics-overview)
- FoundationaLLM Cosmos DB Containers: `/workspace/src/dotnet/Common/Constants/AzureCosmosDBContainers.cs`
- FoundationaLLM Telemetry: `/workspace/src/dotnet/Common/Telemetry/`
- FoundationaLLM Models: `/workspace/src/dotnet/Common/Models/`
