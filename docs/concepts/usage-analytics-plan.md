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
- **Sensitive PII**: Email addresses, full names, personal identifiers beyond username
- **Sensitive Metadata**: File names, attachment content, document identifiers
- **Full Prompts**: System prompts, prompt templates with actual content

### Data Inclusion (Safe to Analyze)
- **Username/UPN**: Displayed for operational and abuse detection purposes (required for security)
- **Aggregated Metrics**: Token counts, request counts, response times
- **Agent Metadata**: Agent names, agent types, workflow types
- **Resource Usage**: Model names, API endpoints, token consumption
- **Temporal Data**: Timestamps (hour/day level), time zones
- **Request Characteristics**: Request size categories, operation types, error codes
- **Feature Usage**: Tool names, attachment counts, file types (not content)
- **Behavioral Patterns**: Usage patterns, activity timelines (without content)

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

2. **Management Portal Dashboard**
   - Custom Vue.js components
   - PrimeVue Charts for visualizations
   - Real-time data fetching from API
   - Responsive design

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

The analytics dashboard will be built as a custom component within the FoundationaLLM Management Portal, integrated into the existing Nuxt.js application structure.

### Management Portal Integration

#### Navigation Structure
- **New Sidebar Section**: "Analytics" section added to sidebar
  - Icon: `pi pi-chart-line`
  - Main Analytics page: `/analytics`
  - Agent Analytics: `/analytics/agents`
  - Tool Analytics: `/analytics/tools`
  - Model Analytics: `/analytics/models`

#### Dashboard Pages

**Main Analytics Page** (`/analytics/index.vue`)
- Overview cards with key metrics
- Quick links to detailed analytics
- Recent activity summary

**Agent Analytics Dashboard** (`/analytics/agents/index.vue`)

*Overview Section*
- Metric cards:
  - Total agents
  - Total conversations
  - Total tokens consumed
  - Average response time
  - Active users (anonymized count)

*Agent Performance Table*
- Sortable, filterable DataTable component
- Columns:
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
- Row actions: Click to view agent detail

*Agent Usage Trends*
- Line chart: Conversations over time by agent (PrimeVue Chart)
- Line chart: Token consumption over time
- Line chart: Response time trends
- Bar chart: Top agents by usage

*Agent Deep Dive* (`/analytics/agents/[agentName].vue`)
- Agent selector dropdown
- Conversation length distribution (histogram)
- Tool usage frequency (pie/bar chart)
- File type distribution (pie chart)
- Response time distribution (histogram)
- Error analysis (table)
- Tool combinations visualization

**Tool Analytics Dashboard** (`/analytics/tools/index.vue`)

*Tool Overview*
- Metric cards:
  - Total unique tools
  - Most popular tools count
  - Tools by agent count
- Tool usage trends (line chart)

*Tool Performance Table*
- Tool name
- Usage frequency
- Average execution time
- Success rate
- Error count
- Agents using tool

**Model Analytics Dashboard** (`/analytics/models/index.vue`)

*Model Usage*
- Model selection distribution (pie chart)
- Model usage by agent (stacked bar chart)
- Model switching patterns (line chart)

*Model Performance Table*
- Model name
- Usage count
- Average response time
- Token efficiency
- Cost per request
- Error rate

### Frontend Components

#### Chart Components
- **UsageChart.vue**: Reusable line/bar chart component
- **DistributionChart.vue**: Histogram/pie chart component
- **MetricCard.vue**: Metric display card component
- **AnalyticsTable.vue**: Enhanced table with sorting/filtering

#### Data Fetching
- **Analytics API Client**: Extend `/js/api.ts` with analytics methods
- **Server-side Proxy**: Use Nuxt server API routes if needed for CORS
- **Real-time Updates**: Optional WebSocket or polling for live data

### Backend API Endpoints

#### Management API Controller

**AnalyticsController.cs** (`/src/dotnet/ManagementAPI/Controllers/AnalyticsController.cs`)

Endpoints:
- `GET /instances/{instanceId}/analytics/overview`
  - Returns overview metrics
- `GET /instances/{instanceId}/analytics/agents`
  - Returns agent analytics summary
- `GET /instances/{instanceId}/analytics/agents/{agentName}`
  - Returns detailed agent analytics
- `GET /instances/{instanceId}/analytics/tools`
  - Returns tool analytics
- `GET /instances/{instanceId}/analytics/models`
  - Returns model analytics
- `GET /instances/{instanceId}/analytics/agents/{agentName}/tools`
  - Returns tool combinations for agent
- `GET /instances/{instanceId}/analytics/agents/{agentName}/files`
  - Returns file analytics for agent
- `GET /instances/{instanceId}/analytics/users/top`
  - Returns top users with metrics
- `GET /instances/{instanceId}/analytics/users/{username}`
  - Returns detailed user analytics
- `GET /instances/{instanceId}/analytics/users/{username}/abuse-indicators`
  - Returns abuse detection indicators
- `GET /instances/{instanceId}/analytics/users/anomalies`
  - Returns detected user anomalies

#### Analytics Service

**AnalyticsService.cs** (`/src/dotnet/Common/Services/Analytics/`)

Service layer that:
- Executes Cosmos DB queries
- Executes Application Insights KQL queries
- Performs anonymization
- Aggregates data
- Returns structured analytics results

## User Analytics and Abuse Detection

### Privacy Considerations for User Analytics

**Safe to Display:**
- **Username/UPN**: Displayed for operational and abuse detection purposes
- **Aggregated Metrics**: Token counts, request counts, session counts
- **Temporal Patterns**: Usage times, frequency patterns
- **Behavioral Patterns**: Agent usage, tool usage, file upload patterns

**Never Displayed:**
- **Message Content**: User prompts, completions, conversation text
- **File Content**: Actual file contents or sensitive file names
- **Personal Information**: Beyond username/UPN (no email addresses, full names, etc.)
- **Session Details**: Individual conversation content

### Top Users Analytics

#### User Usage Metrics

**User Activity Summary**
- **Total Requests**: Number of completion requests per user
- **Total Tokens**: Token consumption per user
- **Total Sessions**: Number of conversations per user
- **Active Days**: Number of days with activity
- **Average Session Length**: Average messages per conversation
- **Average Response Time**: User's average wait time for responses

**User Engagement Patterns**
- **Peak Usage Times**: When user is most active (hour of day, day of week)
- **Usage Frequency**: Requests per hour/day/week
- **Session Patterns**: Average session duration, sessions per day
- **Agent Diversity**: Number of different agents used
- **Tool Usage**: Tools accessed by user

#### Abuse Detection Perspectives

**1. Volume-Based Abuse Indicators**
- **Request Rate Anomalies**: Users with unusually high request rates
  - Requests per minute/hour exceeding thresholds
  - Sudden spikes in request volume
  - Sustained high-volume usage
- **Token Consumption Anomalies**: Unusual token usage patterns
  - Extremely high token consumption per request
  - Total token consumption exceeding quotas significantly
  - Rapid token accumulation

**2. Temporal Pattern Anomalies**
- **Off-Hours Activity**: Unusual activity during non-business hours
- **Rapid-Fire Requests**: Multiple requests within very short timeframes
- **Continuous Usage**: Users with activity spanning 24+ hours
- **Irregular Patterns**: Users deviating from normal usage patterns

**3. Behavioral Anomalies**
- **Agent Hopping**: Rapid switching between multiple agents
- **Tool Exploitation**: Excessive tool usage, unusual tool combinations
- **File Upload Abuse**: 
  - Excessive file uploads
  - Unusually large files
  - Rapid file upload sequences
  - Suspicious file types
- **Error Rate Patterns**: High error rates that might indicate probing
- **Session Patterns**: 
  - Extremely short sessions (potential automation)
  - Extremely long sessions (potential resource exhaustion)
  - Abandoned sessions (high start/stop rate)

**4. Resource Exhaustion Indicators**
- **Quota Violations**: Users repeatedly hitting quota limits
- **Concurrent Session Abuse**: Multiple simultaneous sessions
- **Model Switching**: Rapid model switching (potential cost abuse)
- **Cache Bypass Patterns**: Users consistently bypassing semantic cache

**5. Security Risk Indicators**
- **Failed Authentication Attempts**: Users with authentication failures
- **Permission Boundary Testing**: Users attempting to access unauthorized agents
- **API Key Rotation Patterns**: Frequent key changes (potential key sharing)
- **Geographic Anomalies**: Unusual access locations (if available)

### User Analytics Dashboard

#### Top Users Page (`/analytics/users/index.vue`)

**Overview Section**
- Metric cards:
  - Total active users
  - Users with high activity (top 10%)
  - Users flagged for review
  - Total user requests today

**Top Users Table**
- Sortable, filterable DataTable
- Columns:
  - Username/UPN
  - Total Requests
  - Total Tokens
  - Active Sessions
  - Average Response Time
  - Error Rate
  - Agents Used
  - Last Activity
  - Abuse Risk Score (calculated)
- Row actions: Click to view user detail

**Abuse Detection Panel**
- **High-Risk Users**: Users flagged with multiple abuse indicators
- **Anomaly Alerts**: Recent anomalies detected
- **Volume Alerts**: Users exceeding volume thresholds
- **Pattern Alerts**: Users with unusual behavioral patterns

**User Activity Trends**
- Line chart: Top users' request volume over time
- Bar chart: Token consumption by top users
- Heatmap: User activity by hour/day

#### User Detail Page (`/analytics/users/[username].vue`)

**User Summary**
- Username/UPN display
- Account status
- Total activity metrics
- Risk indicators

**Activity Timeline**
- Timeline view of user activity
- Request patterns over time
- Session activity visualization

**Abuse Indicators Dashboard**
- **Volume Indicators**:
  - Requests per hour/day chart
  - Token consumption trends
  - Peak usage times
- **Behavioral Indicators**:
  - Agent usage distribution
  - Tool usage patterns
  - File upload patterns
  - Error rate trends
- **Temporal Indicators**:
  - Activity heatmap (hour/day)
  - Session duration distribution
  - Request frequency analysis

**Detailed Metrics**
- Request history (without content)
- Session summary
- Agent usage breakdown
- Tool usage breakdown
- File upload summary
- Error analysis

**Actions**
- Flag user for review
- View user's agent access
- Export user activity report
- Block user (if authorized)

### Abuse Detection Algorithms

#### Risk Scoring

**Risk Score Calculation**
- Base score: 0-100
- Factors:
  - Request volume (weight: 25%)
  - Token consumption (weight: 20%)
  - Error rate (weight: 15%)
  - Temporal anomalies (weight: 15%)
  - Behavioral anomalies (weight: 15%)
  - Resource exhaustion (weight: 10%)

**Thresholds**
- **Low Risk**: 0-30
- **Medium Risk**: 31-60
- **High Risk**: 61-80
- **Critical Risk**: 81-100

#### Anomaly Detection

**Statistical Anomaly Detection**
- Z-score analysis for request volumes
- Percentile-based thresholds (p95, p99)
- Moving average comparisons
- Standard deviation analysis

**Pattern Recognition**
- Machine learning-based pattern detection (future enhancement)
- Rule-based pattern matching
- Time-series analysis for trends

## Implementation Plan

### Backend Implementation

#### Analytics Service - User Analytics

**User Analytics Methods**
```csharp
Task<UserAnalyticsSummary> GetUserAnalyticsSummary(string instanceId, string username, DateTime? startDate, DateTime? endDate);
Task<List<TopUserSummary>> GetTopUsers(string instanceId, int topCount, UserSortBy sortBy, DateTime? startDate, DateTime? endDate);
Task<UserAbuseIndicators> GetUserAbuseIndicators(string instanceId, string username, DateTime? startDate, DateTime? endDate);
Task<List<AnomalyAlert>> GetAnomalyAlerts(string instanceId, DateTime? startDate, DateTime? endDate);
Task<UserActivityTimeline> GetUserActivityTimeline(string instanceId, string username, DateTime? startDate, DateTime? endDate);
```

**Abuse Detection Service**
```csharp
Task<AbuseRiskScore> CalculateAbuseRiskScore(string instanceId, string username);
Task<List<AbuseIndicator>> DetectAbuseIndicators(string instanceId, string username);
Task<List<UserAnomaly>> DetectUserAnomalies(string instanceId, DateTime? startDate, DateTime? endDate);
```

#### Management API Endpoints

**User Analytics Endpoints**
- `GET /instances/{instanceId}/analytics/users/top`
  - Query parameters: `top`, `sortBy`, `startDate`, `endDate`
  - Returns: List of top users with metrics
- `GET /instances/{instanceId}/analytics/users/{username}`
  - Returns: Detailed user analytics
- `GET /instances/{instanceId}/analytics/users/{username}/abuse-indicators`
  - Returns: Abuse detection indicators for user
- `GET /instances/{instanceId}/analytics/users/{username}/activity-timeline`
  - Query parameters: `startDate`, `endDate`
  - Returns: User activity timeline
- `GET /instances/{instanceId}/analytics/users/anomalies`
  - Query parameters: `startDate`, `endDate`, `severity`
  - Returns: List of user anomalies detected

#### Data Models

**User Analytics Models**
```csharp
public class TopUserSummary
{
    public string Username { get; set; }
    public string UPN { get; set; }
    public int TotalRequests { get; set; }
    public long TotalTokens { get; set; }
    public int ActiveSessions { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public double ErrorRate { get; set; }
    public int AgentsUsed { get; set; }
    public DateTime LastActivity { get; set; }
    public int AbuseRiskScore { get; set; }
    public List<string> AbuseIndicators { get; set; }
}

public class UserAbuseIndicators
{
    public string Username { get; set; }
    public int RiskScore { get; set; }
    public List<AbuseIndicator> VolumeIndicators { get; set; }
    public List<AbuseIndicator> TemporalIndicators { get; set; }
    public List<AbuseIndicator> BehavioralIndicators { get; set; }
    public List<AbuseIndicator> ResourceIndicators { get; set; }
}

public class AbuseIndicator
{
    public string Type { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> Metrics { get; set; }
}

public class UserAnomaly
{
    public string Username { get; set; }
    public string AnomalyType { get; set; }
    public string Description { get; set; }
    public string Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> AnomalyData { get; set; }
}
```

### Frontend Implementation

#### User Analytics Pages

**Top Users Page** (`/analytics/users/index.vue`)
- Top users table with sorting/filtering
- Abuse detection panel
- Activity trends charts
- Anomaly alerts list

**User Detail Page** (`/analytics/users/[username].vue`)
- User summary section
- Activity timeline visualization
- Abuse indicators dashboard
- Detailed metrics tables
- Action buttons (flag, export, block)

#### User Analytics Components

**UserMetricsCard.vue**
- Display user metrics in card format
- Show risk indicators
- Highlight anomalies

**AbuseIndicatorsPanel.vue**
- Display abuse indicators grouped by type
- Visual indicators (icons, colors) for severity
- Expandable details

**ActivityTimeline.vue**
- Timeline visualization of user activity
- Interactive chart showing request patterns
- Session markers

**AnomalyAlertList.vue**
- List of detected anomalies
- Filterable by severity, type, date
- Action buttons for each anomaly

#### API Integration

**Extend api.ts**
```typescript
async getTopUsers(top: number, sortBy: string, startDate?: string, endDate?: string): Promise<TopUserSummary[]>
async getUserAnalytics(username: string, startDate?: string, endDate?: string): Promise<UserAnalyticsSummary>
async getUserAbuseIndicators(username: string, startDate?: string, endDate?: string): Promise<UserAbuseIndicators>
async getUserActivityTimeline(username: string, startDate?: string, endDate?: string): Promise<UserActivityTimeline>
async getAnomalyAlerts(startDate?: string, endDate?: string, severity?: string): Promise<UserAnomaly[]>
```

### Query Implementation

#### Cosmos DB Queries for User Analytics

**Top Users Query**
```sql
SELECT 
    c.upn as Username,
    COUNT(DISTINCT c.id) as TotalSessions,
    SUM(c.tokensUsed) as TotalTokens,
    MAX(c._ts) as LastActivity
FROM c
WHERE c.type = "Session"
    AND c._ts >= @startTimestamp
    AND c._ts < @endTimestamp
GROUP BY c.upn
ORDER BY TotalTokens DESC
```

**User Request Count Query**
```sql
SELECT 
    m.upn as Username,
    COUNT(*) as RequestCount,
    COUNT(DISTINCT m.sessionId) as SessionCount
FROM m
WHERE m.type = "Message"
    AND m.sender = "User"
    AND m._ts >= @startTimestamp
    AND m._ts < @endTimestamp
GROUP BY m.upn
```

**User Token Consumption Query**
```sql
SELECT 
    c.upn as Username,
    SUM(c.tokensUsed) as TotalTokens,
    AVG(c.tokensUsed) as AvgTokensPerSession,
    MAX(c.tokensUsed) as MaxTokensInSession
FROM c
WHERE c.type = "Session"
    AND c._ts >= @startTimestamp
    AND c._ts < @endTimestamp
GROUP BY c.upn
```

**User Activity Timeline Query**
```sql
SELECT 
    m.upn as Username,
    m._ts as Timestamp,
    m.sessionId,
    m.tokens,
    m.operationId
FROM m
WHERE m.type = "Message"
    AND m.upn = @username
    AND m._ts >= @startTimestamp
    AND m._ts < @endTimestamp
ORDER BY m._ts ASC
```

**User Agent Usage Query**
```sql
SELECT 
    m.upn as Username,
    r.agentName,
    COUNT(*) as UsageCount
FROM m
JOIN r IN m.contentArtifacts
WHERE m.type = "Message"
    AND m.upn = @username
    AND r.agentName IS NOT NULL
    AND m._ts >= @startTimestamp
GROUP BY m.upn, r.agentName
```

#### Application Insights Queries for User Analytics

**User Request Rate Query**
```kusto
let startTime = ago(7d);
let endTime = now();
traces
| where timestamp between (startTime .. endTime)
| where name == "Completions_GetCompletion"
| extend Username = tostring(customDimensions["FoundationaLLM-UPN"])
| extend AgentName = tostring(customDimensions["FoundationaLLM-AgentName"])
| summarize 
    RequestCount = count(),
    AvgResponseTime = avg(duration),
    ErrorCount = countif(success == false)
    by Username, bin(timestamp, 1h)
| order by RequestCount desc
```

**User Anomaly Detection Query**
```kusto
let startTime = ago(1d);
traces
| where timestamp >= startTime
| where name == "Completions_GetCompletion"
| extend Username = tostring(customDimensions["FoundationaLLM-UPN"])
| summarize 
    RequestCount = count(),
    AvgResponseTime = avg(duration),
    P95ResponseTime = percentile(duration, 95),
    ErrorRate = countif(success == false) * 100.0 / count()
    by Username, bin(timestamp, 1h)
| extend ZScore = (RequestCount - avg(RequestCount)) / stdev(RequestCount)
| where abs(ZScore) > 2.0  // Statistical anomaly
| order by RequestCount desc
```

**User Rapid Request Detection**
```kusto
let startTime = ago(1d);
traces
| where timestamp >= startTime
| where name == "Completions_GetCompletion"
| extend Username = tostring(customDimensions["FoundationaLLM-UPN"])
| extend RequestTime = timestamp
| extend NextRequestTime = next(RequestTime)
| extend TimeBetweenRequests = NextRequestTime - RequestTime
| where TimeBetweenRequests < 1s  // Requests within 1 second
| summarize 
    RapidRequestCount = count(),
    MinTimeBetween = min(TimeBetweenRequests)
    by Username
| where RapidRequestCount > 10
| order by RapidRequestCount desc
```

### Abuse Detection Rules

#### Volume-Based Rules

**High Request Rate**
- Threshold: >100 requests per hour
- Severity: Medium
- Action: Flag for review

**Extreme Request Rate**
- Threshold: >500 requests per hour
- Severity: High
- Action: Immediate alert

**Token Consumption Spike**
- Threshold: >10x average user consumption
- Severity: Medium
- Action: Flag for review

#### Temporal Pattern Rules

**Off-Hours Activity**
- Threshold: >50% of requests outside business hours (9 AM - 5 PM)
- Severity: Low
- Action: Note in user profile

**Rapid-Fire Requests**
- Threshold: >20 requests within 1 minute
- Severity: High
- Action: Alert and flag

**Continuous Usage**
- Threshold: Activity spanning >20 hours continuously
- Severity: Medium
- Action: Flag for review

#### Behavioral Pattern Rules

**Agent Hopping**
- Threshold: >10 different agents accessed within 1 hour
- Severity: Medium
- Action: Flag for review

**Excessive Tool Usage**
- Threshold: >100 tool invocations per hour
- Severity: Medium
- Action: Flag for review

**File Upload Abuse**
- Threshold: >50 files uploaded per hour OR >1GB total per hour
- Severity: High
- Action: Alert and flag

**High Error Rate**
- Threshold: >30% error rate
- Severity: Medium
- Action: Flag for review (may indicate probing)

#### Resource Exhaustion Rules

**Quota Violations**
- Threshold: >5 quota violations per day
- Severity: High
- Action: Alert and review

**Concurrent Session Abuse**
- Threshold: >10 simultaneous active sessions
- Severity: Medium
- Action: Flag for review

### Dashboard Updates

#### Sidebar Navigation Update

Add to sidebar:
```vue
<!-- Analytics -->
<h3 class="sidebar__section-header">
    <span class="pi pi-chart-line" aria-hidden="true"></span>
    <span>Analytics</span>
</h3>
<ul>
    <li><NuxtLink to="/analytics" class="sidebar__item">Overview</NuxtLink></li>
    <li><NuxtLink to="/analytics/agents" class="sidebar__item">Agents</NuxtLink></li>
    <li><NuxtLink to="/analytics/tools" class="sidebar__item">Tools</NuxtLink></li>
    <li><NuxtLink to="/analytics/models" class="sidebar__item">Models</NuxtLink></li>
    <li><NuxtLink to="/analytics/users" class="sidebar__item">Top Users</NuxtLink></li>
</ul>
```

#### Overview Page Updates

Add user analytics section to main analytics overview:
- Top 5 users by requests
- Recent anomalies detected
- High-risk users alert
- User activity summary

## Implementation Details

### Backend Architecture

#### Analytics Service Structure
```
/src/dotnet/Common/Services/Analytics/
  - IAnalyticsService.cs (interface)
  - AnalyticsService.cs (implementation)
  - Models/
    - AgentAnalyticsSummary.cs
    - ToolAnalyticsSummary.cs
    - ModelAnalyticsSummary.cs
    - AnalyticsOverview.cs
  - Queries/
    - CosmosDBQueries.cs (query builders)
    - ApplicationInsightsQueries.cs (KQL query builders)
  - Anonymization/
    - IAnonymizationService.cs
    - AnonymizationService.cs
```

#### Management API Controller
```
/src/dotnet/ManagementAPI/Controllers/
  - AnalyticsController.cs
    - GET /instances/{instanceId}/analytics/overview
    - GET /instances/{instanceId}/analytics/agents
    - GET /instances/{instanceId}/analytics/agents/{agentName}
    - GET /instances/{instanceId}/analytics/tools
    - GET /instances/{instanceId}/analytics/models
```

### Frontend Architecture

#### Management Portal Structure
```
/src/ui/ManagementPortal/
  - pages/
    - analytics/
      - index.vue (overview)
      - agents/
        - index.vue (agent list)
        - [agentName].vue (agent detail)
      - tools/
        - index.vue (tool analytics)
      - models/
        - index.vue (model analytics)
  - components/
    - analytics/
      - UsageChart.vue
      - DistributionChart.vue
      - MetricCard.vue
      - AnalyticsTable.vue
  - js/
    - api.ts (extend with analytics methods)
```

### Charting Library

**Recommended**: PrimeVue Charts (already included in Management Portal)
- Line charts for trends
- Bar charts for comparisons
- Pie charts for distributions
- DataTable for tabular data

**Alternative**: Chart.js with vue-chartjs wrapper

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
    "CacheDurationMinutes": 5,
    "RetentionDays": 90,
    "EnableRealTimeUpdates": false
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
2. ✅ Username/UPN displayed only for operational abuse detection purposes
3. ✅ All analytics derived from existing data sources
4. ✅ Comprehensive agent, tool, model, and user analytics
5. ✅ Abuse detection capabilities operational
6. ✅ Real-time and historical dashboards operational
7. ✅ Privacy compliance validated

## Key Metrics Summary

### Agent Metrics
- Unique users per agent
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

### User Metrics
- Total requests per user
- Total tokens per user
- Active sessions per user
- Average response time per user
- Error rate per user
- Agents used per user
- Abuse risk score
- Abuse indicators detected

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
