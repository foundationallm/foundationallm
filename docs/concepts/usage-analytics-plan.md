# FoundationaLLM Usage Analytics Plan

## Executive Summary

This document outlines a comprehensive plan for implementing usage analytics ("proprietary exhaust") for FoundationaLLM using **existing data sources only**. The analytics system will analyze data already being collected in Azure Cosmos DB, Azure Application Insights, Azure Log Analytics, and Azure Data Lake without requiring any new data collection mechanisms. The system strictly protects sensitive message content and personally identifiable information (PII) through anonymization and aggregation.

## Objectives

1. **Analyze Existing Data**: Leverage data already stored in Cosmos DB, App Insights, Log Analytics, and Data Lake
2. **Provide Agentic Insights**: Understand usage patterns across Agents, Tools, and Models
3. **Monitor Performance**: Analyze agent effectiveness, response times, and resource consumption
4. **Protect Privacy**: Ensure no message content or PII is exposed in analytics through anonymization
5. **Enable Business Intelligence**: Provide aggregated insights for platform improvement and capacity planning
6. **Detect Abuse**: Identify unusual usage patterns and potential system abuse

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

## Configuration and Setup

### Prerequisites

Before enabling analytics, ensure the following are configured:

1. **Azure Cosmos DB**: Must be configured and accessible
   - Database and containers must exist (Sessions, Attachments, State, ExternalResources)
   - Connection string must be available

2. **Azure Application Insights**: Must be configured for telemetry collection
   - Application Insights resource created
   - Connection string configured in all services
   - OpenTelemetry instrumentation enabled

3. **Azure Log Analytics** (Optional but recommended):
   - Log Analytics workspace created
   - Workspace ID available

4. **Azure Key Vault** (Required for anonymization):
   - Key Vault resource created
   - Access policies configured for Management API service identity
   - Secret for anonymization salt created

5. **Management Portal Access**:
   - User must have access to Management Portal
   - User must have appropriate RBAC permissions for analytics

### Required Configuration Settings

#### Azure App Configuration Keys

Add the following configuration keys to Azure App Configuration:

```json
{
  "FoundationaLLM:Analytics": {
    "Enabled": true,
    "AnonymizationSalt": "@Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/AnalyticsAnonymizationSalt/)",
    "CosmosDBConnectionString": "@Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/CosmosDBConnectionString/)",
    "ApplicationInsightsConnectionString": "@Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/ApplicationInsightsConnectionString/)",
    "LogAnalyticsWorkspaceId": "{workspace-id}",
    "LogAnalyticsSharedKey": "@Microsoft.KeyVault(SecretUri=https://{keyvault-name}.vault.azure.net/secrets/LogAnalyticsSharedKey/)",
    "CacheDurationMinutes": 5,
    "RetentionDays": 90,
    "EnableRealTimeUpdates": false,
    "AbuseDetection": {
      "Enabled": true,
      "HighRequestRateThreshold": 100,
      "ExtremeRequestRateThreshold": 500,
      "RapidFireThreshold": 20,
      "RapidFireWindowMinutes": 1,
      "ContinuousUsageHours": 20,
      "AgentHoppingThreshold": 10,
      "AgentHoppingWindowMinutes": 60,
      "FileUploadAbuseCount": 50,
      "FileUploadAbuseSizeGB": 1,
      "HighErrorRateThreshold": 30
    }
  }
}
```

#### Key Vault Secrets

Create the following secrets in Azure Key Vault:

1. **AnalyticsAnonymizationSalt**
   - Type: Secret
   - Value: Generate a random 32-character string
   - Example: `Generate-Random-32-Char-String-Here`

2. **CosmosDBConnectionString**
   - Type: Secret
   - Value: Cosmos DB connection string
   - Format: `AccountEndpoint=https://{account}.documents.azure.com:443/;AccountKey={key};`

3. **ApplicationInsightsConnectionString**
   - Type: Secret
   - Value: Application Insights connection string
   - Format: `InstrumentationKey={key};IngestionEndpoint=https://{region}.in.applicationinsights.azure.com/`

4. **LogAnalyticsSharedKey** (Optional)
   - Type: Secret
   - Value: Log Analytics workspace shared key

#### Management API Configuration

Ensure Management API has access to:
- Azure Cosmos DB (read access)
- Azure Application Insights (read access)
- Azure Log Analytics (read access)
- Azure Key Vault (read access for secrets)

#### Management Portal Configuration

No additional configuration required for Management Portal. Analytics will be available once backend is configured.

### Setup Instructions

#### Step 1: Configure Azure Resources

1. **Create/Verify Application Insights Resource**
   ```bash
   az monitor app-insights component create \
     --app {app-name} \
     --location {location} \
     --resource-group {resource-group}
   ```

2. **Create/Verify Key Vault**
   ```bash
   az keyvault create \
     --name {keyvault-name} \
     --resource-group {resource-group} \
     --location {location}
   ```

3. **Add Secrets to Key Vault**
   ```bash
   # Anonymization Salt
   az keyvault secret set \
     --vault-name {keyvault-name} \
     --name AnalyticsAnonymizationSalt \
     --value "{random-32-char-string}"
   
   # Cosmos DB Connection String
   az keyvault secret set \
     --vault-name {keyvault-name} \
     --name CosmosDBConnectionString \
     --value "{cosmos-db-connection-string}"
   
   # Application Insights Connection String
   az keyvault secret set \
     --vault-name {keyvault-name} \
     --name ApplicationInsightsConnectionString \
     --value "{app-insights-connection-string}"
   ```

#### Step 2: Configure App Configuration

1. **Add Configuration Keys**
   - Navigate to Azure App Configuration
   - Add the configuration keys listed above
   - Use Key Vault references for secrets

2. **Verify Configuration Access**
   - Ensure Management API service identity has access to App Configuration
   - Verify Key Vault access policies are configured

#### Step 3: Deploy Analytics Components

1. **Deploy Management API** (with analytics endpoints)
   - Analytics endpoints are included in Management API
   - No separate deployment required

2. **Deploy Management Portal** (with analytics pages)
   - Analytics pages are included in Management Portal
   - No separate deployment required

#### Step 4: Verify Setup

1. **Check Configuration**
   - Verify all configuration keys are set
   - Verify Key Vault secrets are accessible
   - Verify service identities have required permissions

2. **Test Analytics Endpoints**
   - Use Management API status endpoint
   - Verify analytics endpoints are accessible
   - Check for configuration errors in logs

3. **Access Analytics Dashboard**
   - Log into Management Portal
   - Navigate to Analytics section
   - Verify dashboard loads without errors

### Getting Started Guide

#### For Administrators

1. **Enable Analytics**
   - Set `FoundationaLLM:Analytics:Enabled` to `true` in App Configuration
   - Restart Management API service
   - Analytics will begin collecting data immediately

2. **Access Analytics Dashboard**
   - Log into Management Portal
   - Click "Analytics" in the sidebar
   - View overview metrics

3. **Configure Abuse Detection** (Optional)
   - Adjust thresholds in `FoundationaLLM:Analytics:AbuseDetection` settings
   - Customize risk scoring weights if needed
   - Enable/disable specific detection rules

#### For End Users

1. **View Agent Analytics**
   - Navigate to Analytics > Agents
   - View agent performance metrics
   - Click on an agent to see detailed analytics

2. **View User Analytics**
   - Navigate to Analytics > Top Users
   - View top users by various metrics
   - Click on a user to see detailed activity

3. **View Tool Analytics**
   - Navigate to Analytics > Tools
   - View tool usage patterns
   - Analyze tool performance

4. **View Model Analytics**
   - Navigate to Analytics > Models
   - View model usage and performance
   - Compare model metrics

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
- **Unique Users per Agent**: Count of distinct users using each agent over time periods
  - Query: `SELECT DISTINCT upn FROM Sessions WHERE agentName = @agent AND _ts >= @startTime AND _ts < @endTime`
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
  - Top Users: `/analytics/users`

#### Dashboard Pages

**Main Analytics Page** (`/analytics/index.vue`)
- Overview cards with key metrics
- Quick links to detailed analytics
- Recent activity summary
- Top 5 users by requests
- Recent anomalies detected
- High-risk users alert

**Agent Analytics Dashboard** (`/analytics/agents/index.vue`)

*Overview Section*
- Metric cards:
  - Total agents
  - Total conversations
  - Total tokens consumed
  - Average response time
  - Active users count

*Agent Performance Table*
- Sortable, filterable DataTable component
- Columns:
  - Agent Name
  - Unique Users
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

**User Analytics Dashboard** (`/analytics/users/index.vue`)

*Overview Section*
- Metric cards:
  - Total active users
  - Users with high activity (top 10%)
  - Users flagged for review
  - Total user requests today

*Top Users Table*
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

*Abuse Detection Panel*
- **High-Risk Users**: Users flagged with multiple abuse indicators
- **Anomaly Alerts**: Recent anomalies detected
- **Volume Alerts**: Users exceeding volume thresholds
- **Pattern Alerts**: Users with unusual behavioral patterns

*User Activity Trends*
- Line chart: Top users' request volume over time
- Bar chart: Token consumption by top users
- Heatmap: User activity by hour/day

**User Detail Page** (`/analytics/users/[username].vue`)

*User Summary*
- Username/UPN display
- Account status
- Total activity metrics
- Risk indicators

*Activity Timeline*
- Timeline view of user activity
- Request patterns over time
- Session activity visualization

*Abuse Indicators Dashboard*
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

*Detailed Metrics*
- Request history (without content)
- Session summary
- Agent usage breakdown
- Tool usage breakdown
- File upload summary
- Error analysis

*Actions*
- Flag user for review
- View user's agent access
- Export user activity report
- Block user (if authorized)

## User Documentation

### Overview

The FoundationaLLM Analytics Dashboard provides comprehensive insights into platform usage, agent performance, tool utilization, model selection, and user activity. All analytics are derived from existing data sources without requiring additional data collection.

### Accessing Analytics

1. **Log into Management Portal**
   - Navigate to the FoundationaLLM Management Portal
   - Authenticate with your credentials

2. **Navigate to Analytics**
   - Click "Analytics" in the sidebar navigation
   - You'll see the Analytics Overview page

3. **Select Analytics Category**
   - Click on "Agents", "Tools", "Models", or "Top Users" to view specific analytics

### Using Agent Analytics

#### Viewing Agent Performance

1. **Navigate to Agent Analytics**
   - Click "Analytics" > "Agents" in the sidebar

2. **Review Overview Metrics**
   - View key metrics at the top: total agents, conversations, tokens, response time
   - These provide a high-level view of platform usage

3. **Analyze Agent Performance Table**
   - Sort by any column (click column header)
   - Filter agents using the search box
   - Click on an agent name to view detailed analytics

4. **View Usage Trends**
   - Scroll down to see trend charts
   - Use date range selector to adjust time period
   - Hover over chart points to see exact values

#### Agent Detail View

1. **Access Agent Details**
   - Click on an agent name in the performance table
   - Or navigate directly to `/analytics/agents/{agentName}`

2. **Review Agent Metrics**
   - View conversation length distribution
   - Analyze tool usage frequency
   - Review file type distribution
   - Examine response time distribution
   - Review error analysis

3. **Analyze Tool Combinations**
   - View which tools are commonly used together
   - Identify patterns in tool usage

### Using Tool Analytics

1. **Navigate to Tool Analytics**
   - Click "Analytics" > "Tools" in the sidebar

2. **View Tool Overview**
   - See total unique tools
   - View most popular tools
   - Review tool usage trends

3. **Analyze Tool Performance**
   - Review tool performance table
   - Sort by usage frequency, execution time, or success rate
   - Identify tools with high error rates

4. **Identify Tool Patterns**
   - See which agents use which tools
   - Identify tool specialization patterns

### Using Model Analytics

1. **Navigate to Model Analytics**
   - Click "Analytics" > "Models" in the sidebar

2. **View Model Usage**
   - See model selection distribution (pie chart)
   - Review model usage by agent (stacked bar chart)
   - Analyze model switching patterns

3. **Compare Model Performance**
   - Review model performance table
   - Compare response times across models
   - Analyze token efficiency
   - Review cost per request

4. **Optimize Model Selection**
   - Use performance data to inform model selection
   - Identify cost optimization opportunities

### Using User Analytics

#### Viewing Top Users

1. **Navigate to User Analytics**
   - Click "Analytics" > "Top Users" in the sidebar

2. **Review Top Users Table**
   - Sort by requests, tokens, sessions, or risk score
   - Filter users using the search box
   - Click on a username to view detailed analytics

3. **Monitor Abuse Detection**
   - Review abuse detection panel for alerts
   - Check anomaly alerts for recent issues
   - Review high-risk users list

4. **View Activity Trends**
   - Analyze user activity trends over time
   - Review token consumption patterns
   - Examine activity heatmap

#### User Detail View

1. **Access User Details**
   - Click on a username in the top users table
   - Or navigate directly to `/analytics/users/{username}`

2. **Review User Summary**
   - View account status
   - Review total activity metrics
   - Check risk indicators

3. **Analyze Activity Timeline**
   - View timeline of user activity
   - Identify usage patterns
   - Review session activity

4. **Review Abuse Indicators**
   - **Volume Indicators**: Check for unusual request volumes
   - **Behavioral Indicators**: Review agent/tool usage patterns
   - **Temporal Indicators**: Analyze activity timing patterns

5. **Take Actions**
   - Flag user for review if suspicious activity detected
   - Export user activity report for investigation
   - Block user if authorized (requires appropriate permissions)

### Understanding Metrics

#### Agent Metrics Explained

- **Unique Users**: Number of distinct users who have used the agent
- **Total Conversations**: Total number of conversation sessions
- **Total Tokens**: Sum of all tokens consumed across all conversations
- **Avg Tokens per Session**: Average token consumption per conversation
- **Avg Response Time**: Mean time for agent to respond (milliseconds)
- **P95 Response Time**: 95th percentile response time (95% of requests faster than this)
- **Error Rate**: Percentage of requests that resulted in errors
- **Most Used Tools**: List of tools most frequently used with this agent
- **Avg Files per Conversation**: Average number of files uploaded per conversation

#### User Metrics Explained

- **Total Requests**: Total number of completion requests made by user
- **Total Tokens**: Total tokens consumed by user across all requests
- **Active Sessions**: Number of active conversation sessions
- **Average Response Time**: User's average wait time for responses
- **Error Rate**: Percentage of user's requests that resulted in errors
- **Agents Used**: Number of different agents accessed by user
- **Abuse Risk Score**: Calculated risk score (0-100) based on abuse indicators
- **Last Activity**: Timestamp of user's most recent activity

#### Abuse Risk Score

The abuse risk score is calculated based on multiple factors:

- **0-30 (Low Risk)**: Normal usage patterns
- **31-60 (Medium Risk)**: Some unusual patterns detected, review recommended
- **61-80 (High Risk)**: Multiple abuse indicators present, immediate review required
- **81-100 (Critical Risk)**: Severe abuse indicators, take immediate action

### Filtering and Sorting

#### Date Range Selection

Most analytics pages include a date range selector:
- Select predefined ranges (Last 7 days, Last 30 days, Last 90 days)
- Or select custom date range using date picker
- Changes apply immediately to all charts and tables

#### Table Sorting

- Click any column header to sort by that column
- Click again to reverse sort order
- Sort indicator shows current sort column and direction

#### Table Filtering

- Use search box to filter table rows
- Filter applies to all visible columns
- Clear filter to show all rows

### Exporting Data

#### Export Options

1. **Export Table Data**
   - Click "Export" button on any table
   - Choose format: CSV or JSON
   - Download file with current filtered/sorted data

2. **Export User Activity Report**
   - From user detail page, click "Export Report"
   - Includes user metrics, activity timeline, and abuse indicators
   - Format: PDF or JSON

### Best Practices

1. **Regular Monitoring**
   - Review analytics dashboard weekly
   - Check abuse detection alerts daily
   - Monitor high-risk users regularly

2. **Performance Optimization**
   - Use agent analytics to identify underperforming agents
   - Use model analytics to optimize model selection
   - Use tool analytics to identify inefficient tool usage

3. **Cost Management**
   - Monitor token consumption trends
   - Review model cost analysis
   - Identify cost optimization opportunities

4. **Security Monitoring**
   - Regularly review abuse detection alerts
   - Investigate high-risk users promptly
   - Monitor for unusual patterns

5. **Capacity Planning**
   - Use usage trends to forecast capacity needs
   - Monitor growth patterns
   - Plan for peak usage times

## Implementation Plan

### Backend Implementation

#### Analytics Service Structure

```
/src/dotnet/Common/Services/Analytics/
  - IAnalyticsService.cs (interface)
  - AnalyticsService.cs (implementation)
  - Models/
    - AgentAnalyticsSummary.cs
    - ToolAnalyticsSummary.cs
    - ModelAnalyticsSummary.cs
    - UserAnalyticsSummary.cs
    - AnalyticsOverview.cs
    - TopUserSummary.cs
    - UserAbuseIndicators.cs
    - AbuseIndicator.cs
    - UserAnomaly.cs
  - Queries/
    - CosmosDBQueries.cs (query builders)
    - ApplicationInsightsQueries.cs (KQL query builders)
  - Anonymization/
    - IAnonymizationService.cs
    - AnonymizationService.cs
  - AbuseDetection/
    - IAbuseDetectionService.cs
    - AbuseDetectionService.cs
    - RiskScoringService.cs
```

#### Analytics Service Methods

**Agent Analytics**
```csharp
Task<AgentAnalyticsSummary> GetAgentAnalyticsSummary(string instanceId, string agentName, DateTime? startDate, DateTime? endDate);
Task<List<AgentAnalyticsSummary>> GetAllAgentsAnalytics(string instanceId, DateTime? startDate, DateTime? endDate);
Task<Dictionary<string, int>> GetAgentToolCombinations(string instanceId, string agentName, DateTime? startDate, DateTime? endDate);
Task<FileAnalyticsSummary> GetAgentFileAnalytics(string instanceId, string agentName, DateTime? startDate, DateTime? endDate);
```

**Tool Analytics**
```csharp
Task<List<ToolAnalyticsSummary>> GetToolAnalytics(string instanceId, DateTime? startDate, DateTime? endDate);
Task<Dictionary<string, int>> GetToolUsageByAgent(string instanceId, string toolName, DateTime? startDate, DateTime? endDate);
```

**Model Analytics**
```csharp
Task<List<ModelAnalyticsSummary>> GetModelAnalytics(string instanceId, DateTime? startDate, DateTime? endDate);
Task<Dictionary<string, ModelUsageStats>> GetModelUsageByAgent(string instanceId, DateTime? startDate, DateTime? endDate);
```

**User Analytics**
```csharp
Task<UserAnalyticsSummary> GetUserAnalyticsSummary(string instanceId, string username, DateTime? startDate, DateTime? endDate);
Task<List<TopUserSummary>> GetTopUsers(string instanceId, int topCount, UserSortBy sortBy, DateTime? startDate, DateTime? endDate);
Task<UserAbuseIndicators> GetUserAbuseIndicators(string instanceId, string username, DateTime? startDate, DateTime? endDate);
Task<List<AnomalyAlert>> GetAnomalyAlerts(string instanceId, DateTime? startDate, DateTime? endDate);
Task<UserActivityTimeline> GetUserActivityTimeline(string instanceId, string username, DateTime? startDate, DateTime? endDate);
```

**Abuse Detection**
```csharp
Task<AbuseRiskScore> CalculateAbuseRiskScore(string instanceId, string username);
Task<List<AbuseIndicator>> DetectAbuseIndicators(string instanceId, string username);
Task<List<UserAnomaly>> DetectUserAnomalies(string instanceId, DateTime? startDate, DateTime? endDate);
```

#### Management API Controller

**AnalyticsController.cs** (`/src/dotnet/ManagementAPI/Controllers/AnalyticsController.cs`)

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
[ApiController]
[Route("instances/{instanceId}/analytics")]
public class AnalyticsController : ControllerBase
{
    // Overview
    [HttpGet("overview")]
    public async Task<AnalyticsOverview> GetOverview(string instanceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    // Agents
    [HttpGet("agents")]
    public async Task<List<AgentAnalyticsSummary>> GetAgents(string instanceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("agents/{agentName}")]
    public async Task<AgentAnalyticsSummary> GetAgent(string instanceId, string agentName, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("agents/{agentName}/tools")]
    public async Task<Dictionary<string, int>> GetAgentTools(string instanceId, string agentName, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("agents/{agentName}/files")]
    public async Task<FileAnalyticsSummary> GetAgentFiles(string instanceId, string agentName, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    // Tools
    [HttpGet("tools")]
    public async Task<List<ToolAnalyticsSummary>> GetTools(string instanceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    // Models
    [HttpGet("models")]
    public async Task<List<ModelAnalyticsSummary>> GetModels(string instanceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    // Users
    [HttpGet("users/top")]
    public async Task<List<TopUserSummary>> GetTopUsers(string instanceId, [FromQuery] int top = 100, [FromQuery] string sortBy = "requests", [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("users/{username}")]
    public async Task<UserAnalyticsSummary> GetUser(string instanceId, string username, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("users/{username}/abuse-indicators")]
    public async Task<UserAbuseIndicators> GetUserAbuseIndicators(string instanceId, string username, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("users/{username}/activity-timeline")]
    public async Task<UserActivityTimeline> GetUserActivityTimeline(string instanceId, string username, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate);
    
    [HttpGet("users/anomalies")]
    public async Task<List<UserAnomaly>> GetAnomalies(string instanceId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string severity);
}
```

### Frontend Implementation

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
      - users/
        - index.vue (top users)
        - [username].vue (user detail)
  - components/
    - analytics/
      - UsageChart.vue
      - DistributionChart.vue
      - MetricCard.vue
      - AnalyticsTable.vue
      - UserMetricsCard.vue
      - AbuseIndicatorsPanel.vue
      - ActivityTimeline.vue
      - AnomalyAlertList.vue
  - js/
    - api.ts (extend with analytics methods)
```

#### API Integration

**Extend api.ts**
```typescript
// Analytics API methods
async getAnalyticsOverview(startDate?: string, endDate?: string): Promise<AnalyticsOverview>
async getAgentsAnalytics(startDate?: string, endDate?: string): Promise<AgentAnalyticsSummary[]>
async getAgentAnalytics(agentName: string, startDate?: string, endDate?: string): Promise<AgentAnalyticsSummary>
async getAgentTools(agentName: string, startDate?: string, endDate?: string): Promise<Dictionary<string, int>>
async getAgentFiles(agentName: string, startDate?: string, endDate?: string): Promise<FileAnalyticsSummary>
async getToolsAnalytics(startDate?: string, endDate?: string): Promise<ToolAnalyticsSummary[]>
async getModelsAnalytics(startDate?: string, endDate?: string): Promise<ModelAnalyticsSummary[]>
async getTopUsers(top: number, sortBy: string, startDate?: string, endDate?: string): Promise<TopUserSummary[]>
async getUserAnalytics(username: string, startDate?: string, endDate?: string): Promise<UserAnalyticsSummary>
async getUserAbuseIndicators(username: string, startDate?: string, endDate?: string): Promise<UserAbuseIndicators>
async getUserActivityTimeline(username: string, startDate?: string, endDate?: string): Promise<UserActivityTimeline>
async getAnomalyAlerts(startDate?: string, endDate?: string, severity?: string): Promise<UserAnomaly[]>
```

### Query Implementation

#### Cosmos DB Queries

See "Analytics Queries" section for detailed query examples.

#### Application Insights KQL Queries

See "Analytics Queries" section for detailed KQL query examples.

## Testing Strategy

### Unit Tests

#### Backend Tests

**Analytics Service Tests** (`/tests/dotnet/Analytics.Tests/Services/`)

```csharp
public class AnalyticsServiceTests
{
    [Fact]
    public async Task GetAgentAnalyticsSummary_ValidAgent_ReturnsSummary()
    {
        // Arrange
        var instanceId = "test-instance";
        var agentName = "test-agent";
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        
        // Act
        var result = await _analyticsService.GetAgentAnalyticsSummary(instanceId, agentName, startDate, endDate);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(agentName, result.AgentName);
        Assert.True(result.TotalConversations >= 0);
        Assert.True(result.TotalTokens >= 0);
    }
    
    [Fact]
    public async Task GetTopUsers_ValidParameters_ReturnsUsers()
    {
        // Arrange
        var instanceId = "test-instance";
        var topCount = 10;
        var sortBy = UserSortBy.Requests;
        
        // Act
        var result = await _analyticsService.GetTopUsers(instanceId, topCount, sortBy, null, null);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= topCount);
        Assert.All(result, user => Assert.NotNull(user.Username));
    }
    
    [Fact]
    public async Task CalculateAbuseRiskScore_ValidUser_ReturnsScore()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        
        // Act
        var result = await _abuseDetectionService.CalculateAbuseRiskScore(instanceId, username);
        
        // Assert
        Assert.NotNull(result);
        Assert.InRange(result.Score, 0, 100);
        Assert.NotNull(result.Factors);
    }
    
    [Fact]
    public async Task DetectAbuseIndicators_ValidUser_ReturnsIndicators()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        
        // Act
        var result = await _abuseDetectionService.DetectAbuseIndicators(instanceId, username);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.VolumeIndicators);
        Assert.NotNull(result.TemporalIndicators);
        Assert.NotNull(result.BehavioralIndicators);
    }
}
```

**Anonymization Service Tests** (`/tests/dotnet/Analytics.Tests/Services/Anonymization/`)

```csharp
public class AnonymizationServiceTests
{
    [Fact]
    public void AnonymizeUPN_ValidUPN_ReturnsHash()
    {
        // Arrange
        var upn = "user@example.com";
        var salt = "test-salt-12345";
        
        // Act
        var result = _anonymizationService.AnonymizeUPN(upn, salt);
        
        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(upn, result);
        Assert.NotEmpty(result);
    }
    
    [Fact]
    public void AnonymizeUPN_SameUPN_SameHash()
    {
        // Arrange
        var upn = "user@example.com";
        var salt = "test-salt-12345";
        
        // Act
        var result1 = _anonymizationService.AnonymizeUPN(upn, salt);
        var result2 = _anonymizationService.AnonymizeUPN(upn, salt);
        
        // Assert
        Assert.Equal(result1, result2);
    }
    
    [Fact]
    public void AnonymizeUPN_NullUPN_ReturnsAnonymous()
    {
        // Arrange
        string upn = null;
        var salt = "test-salt-12345";
        
        // Act
        var result = _anonymizationService.AnonymizeUPN(upn, salt);
        
        // Assert
        Assert.Equal("anonymous", result);
    }
}
```

**Abuse Detection Service Tests** (`/tests/dotnet/Analytics.Tests/Services/AbuseDetection/`)

```csharp
public class AbuseDetectionServiceTests
{
    [Fact]
    public async Task DetectHighRequestRate_ExceedsThreshold_ReturnsIndicator()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        var requestCount = 150; // Exceeds threshold of 100
        
        // Act
        var result = await _abuseDetectionService.DetectAbuseIndicators(instanceId, username);
        
        // Assert
        Assert.Contains(result.VolumeIndicators, i => 
            i.Type == "HighRequestRate" && 
            i.Severity == "Medium");
    }
    
    [Fact]
    public async Task DetectRapidFireRequests_ExceedsThreshold_ReturnsIndicator()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        // Simulate rapid-fire requests
        
        // Act
        var result = await _abuseDetectionService.DetectAbuseIndicators(instanceId, username);
        
        // Assert
        Assert.Contains(result.TemporalIndicators, i => 
            i.Type == "RapidFireRequests" && 
            i.Severity == "High");
    }
    
    [Fact]
    public async Task DetectAgentHopping_ExceedsThreshold_ReturnsIndicator()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        // Simulate agent hopping
        
        // Act
        var result = await _abuseDetectionService.DetectAbuseIndicators(instanceId, username);
        
        // Assert
        Assert.Contains(result.BehavioralIndicators, i => 
            i.Type == "AgentHopping" && 
            i.Severity == "Medium");
    }
}
```

#### Frontend Tests

**Analytics Component Tests** (`/src/ui/ManagementPortal/components/analytics/__tests__/`)

```typescript
describe('UsageChart.vue', () => {
  it('renders chart with data', async () => {
    const wrapper = mount(UsageChart, {
      props: {
        data: [
          { date: '2024-01-01', value: 100 },
          { date: '2024-01-02', value: 150 }
        ],
        type: 'line'
      }
    })
    
    expect(wrapper.find('canvas').exists()).toBe(true)
  })
  
  it('handles empty data', async () => {
    const wrapper = mount(UsageChart, {
      props: {
        data: [],
        type: 'line'
      }
    })
    
    expect(wrapper.find('.empty-state').exists()).toBe(true)
  })
})

describe('MetricCard.vue', () => {
  it('displays metric value', () => {
    const wrapper = mount(MetricCard, {
      props: {
        title: 'Total Requests',
        value: 1234,
        format: 'number'
      }
    })
    
    expect(wrapper.text()).toContain('Total Requests')
    expect(wrapper.text()).toContain('1,234')
  })
  
  it('formats large numbers', () => {
    const wrapper = mount(MetricCard, {
      props: {
        title: 'Total Tokens',
        value: 1234567,
        format: 'number'
      }
    })
    
    expect(wrapper.text()).toContain('1,234,567')
  })
})

describe('AnalyticsTable.vue', () => {
  it('sorts by column', async () => {
    const wrapper = mount(AnalyticsTable, {
      props: {
        data: [
          { name: 'Agent A', requests: 100 },
          { name: 'Agent B', requests: 200 }
        ],
        columns: ['name', 'requests']
      }
    })
    
    await wrapper.find('[data-column="requests"]').trigger('click')
    const rows = wrapper.findAll('tbody tr')
    expect(rows[0].text()).toContain('Agent B')
  })
  
  it('filters data', async () => {
    const wrapper = mount(AnalyticsTable, {
      props: {
        data: [
          { name: 'Agent A', requests: 100 },
          { name: 'Agent B', requests: 200 }
        ],
        columns: ['name', 'requests']
      }
    })
    
    await wrapper.find('input[type="search"]').setValue('Agent A')
    const rows = wrapper.findAll('tbody tr')
    expect(rows.length).toBe(1)
    expect(rows[0].text()).toContain('Agent A')
  })
})
```

**Analytics Page Tests** (`/src/ui/ManagementPortal/pages/analytics/__tests__/`)

```typescript
describe('Agent Analytics Page', () => {
  it('loads and displays agent data', async () => {
    const mockAgents = [
      { agentName: 'Agent A', totalConversations: 100, totalTokens: 50000 },
      { agentName: 'Agent B', totalConversations: 200, totalTokens: 100000 }
    ]
    
    vi.spyOn(api, 'getAgentsAnalytics').mockResolvedValue(mockAgents)
    
    const wrapper = mount(AgentAnalyticsPage)
    await flushPromises()
    
    expect(wrapper.text()).toContain('Agent A')
    expect(wrapper.text()).toContain('Agent B')
  })
  
  it('handles API errors', async () => {
    vi.spyOn(api, 'getAgentsAnalytics').mockRejectedValue(new Error('API Error'))
    
    const wrapper = mount(AgentAnalyticsPage)
    await flushPromises()
    
    expect(wrapper.text()).toContain('Error loading analytics')
  })
  
  it('filters by date range', async () => {
    const wrapper = mount(AgentAnalyticsPage)
    
    await wrapper.find('[data-testid="date-range-start"]').setValue('2024-01-01')
    await wrapper.find('[data-testid="date-range-end"]').setValue('2024-01-31')
    await wrapper.find('[data-testid="apply-filter"]').trigger('click')
    
    expect(api.getAgentsAnalytics).toHaveBeenCalledWith(
      expect.any(String),
      expect.any(String)
    )
  })
})

describe('User Analytics Page', () => {
  it('displays top users', async () => {
    const mockUsers = [
      { username: 'user1@example.com', totalRequests: 1000, abuseRiskScore: 45 },
      { username: 'user2@example.com', totalRequests: 500, abuseRiskScore: 75 }
    ]
    
    vi.spyOn(api, 'getTopUsers').mockResolvedValue(mockUsers)
    
    const wrapper = mount(UserAnalyticsPage)
    await flushPromises()
    
    expect(wrapper.text()).toContain('user1@example.com')
    expect(wrapper.text()).toContain('user2@example.com')
  })
  
  it('highlights high-risk users', async () => {
    const mockUsers = [
      { username: 'user1@example.com', abuseRiskScore: 85 }
    ]
    
    vi.spyOn(api, 'getTopUsers').mockResolvedValue(mockUsers)
    
    const wrapper = mount(UserAnalyticsPage)
    await flushPromises()
    
    const riskBadge = wrapper.find('[data-testid="risk-badge"]')
    expect(riskBadge.classes()).toContain('high-risk')
  })
  
  it('displays abuse indicators', async () => {
    const mockIndicators = {
      volumeIndicators: [
        { type: 'HighRequestRate', severity: 'Medium', description: 'High request rate detected' }
      ]
    }
    
    vi.spyOn(api, 'getUserAbuseIndicators').mockResolvedValue(mockIndicators)
    
    const wrapper = mount(UserDetailPage, {
      route: { params: { username: 'test-user@example.com' } }
    })
    await flushPromises()
    
    expect(wrapper.text()).toContain('High request rate detected')
  })
})
```

### Integration Tests

#### API Integration Tests

**Analytics API Integration Tests** (`/tests/dotnet/Analytics.Tests/Integration/`)

```csharp
public class AnalyticsApiIntegrationTests : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetAnalyticsOverview_ReturnsOverview()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var instanceId = _fixture.InstanceId;
        
        // Act
        var response = await client.GetAsync($"/instances/{instanceId}/analytics/overview");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var overview = await response.Content.ReadFromJsonAsync<AnalyticsOverview>();
        Assert.NotNull(overview);
        Assert.True(overview.TotalAgents >= 0);
        Assert.True(overview.TotalConversations >= 0);
    }
    
    [Fact]
    public async Task GetAgentsAnalytics_ReturnsAgents()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var instanceId = _fixture.InstanceId;
        
        // Act
        var response = await client.GetAsync($"/instances/{instanceId}/analytics/agents");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var agents = await response.Content.ReadFromJsonAsync<List<AgentAnalyticsSummary>>();
        Assert.NotNull(agents);
    }
    
    [Fact]
    public async Task GetTopUsers_ReturnsUsers()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var instanceId = _fixture.InstanceId;
        
        // Act
        var response = await client.GetAsync($"/instances/{instanceId}/analytics/users/top?top=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<List<TopUserSummary>>();
        Assert.NotNull(users);
        Assert.True(users.Count <= 10);
    }
    
    [Fact]
    public async Task GetUserAbuseIndicators_ReturnsIndicators()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var instanceId = _fixture.InstanceId;
        var username = "test-user@example.com";
        
        // Act
        var response = await client.GetAsync($"/instances/{instanceId}/analytics/users/{username}/abuse-indicators");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var indicators = await response.Content.ReadFromJsonAsync<UserAbuseIndicators>();
        Assert.NotNull(indicators);
        Assert.Equal(username, indicators.Username);
    }
}
```

### End-to-End Tests

**Analytics E2E Tests** (`/tests/dotnet/Analytics.Tests/E2E/`)

```csharp
public class AnalyticsE2ETests : IClassFixture<TestFixture>
{
    [Fact]
    public async Task AnalyticsWorkflow_CompleteFlow_Succeeds()
    {
        // 1. Create test data (sessions, messages, etc.)
        await CreateTestData();
        
        // 2. Wait for data to be available
        await Task.Delay(5000);
        
        // 3. Query analytics overview
        var overview = await _analyticsService.GetAnalyticsOverview(_instanceId, null, null);
        Assert.NotNull(overview);
        
        // 4. Query agent analytics
        var agents = await _analyticsService.GetAllAgentsAnalytics(_instanceId, null, null);
        Assert.NotEmpty(agents);
        
        // 5. Query user analytics
        var users = await _analyticsService.GetTopUsers(_instanceId, 10, UserSortBy.Requests, null, null);
        Assert.NotEmpty(users);
        
        // 6. Verify abuse detection
        var abuseIndicators = await _abuseDetectionService.DetectAbuseIndicators(_instanceId, users.First().Username);
        Assert.NotNull(abuseIndicators);
    }
    
    [Fact]
    public async Task AbuseDetection_DetectsHighRequestRate_FlagsUser()
    {
        // Arrange: Create user with high request rate
        var username = "high-volume-user@example.com";
        await CreateHighVolumeUser(username, requestsPerHour: 150);
        
        // Act
        var indicators = await _abuseDetectionService.DetectAbuseIndicators(_instanceId, username);
        
        // Assert
        Assert.Contains(indicators.VolumeIndicators, i => i.Type == "HighRequestRate");
        var riskScore = await _abuseDetectionService.CalculateAbuseRiskScore(_instanceId, username);
        Assert.True(riskScore.Score > 30); // Should be medium or higher risk
    }
}
```

### Performance Tests

**Analytics Performance Tests** (`/tests/dotnet/Analytics.Tests/Performance/`)

```csharp
public class AnalyticsPerformanceTests
{
    [Fact]
    public async Task GetAgentsAnalytics_LargeDataset_CompletesWithinTimeout()
    {
        // Arrange
        var instanceId = "test-instance";
        var startDate = DateTime.UtcNow.AddDays(-90);
        var endDate = DateTime.UtcNow;
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _analyticsService.GetAllAgentsAnalytics(instanceId, startDate, endDate);
        stopwatch.Stop();
        
        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, "Query should complete within 5 seconds");
    }
    
    [Fact]
    public async Task GetTopUsers_LargeDataset_CompletesWithinTimeout()
    {
        // Arrange
        var instanceId = "test-instance";
        var topCount = 100;
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _analyticsService.GetTopUsers(instanceId, topCount, UserSortBy.Requests, null, null);
        stopwatch.Stop();
        
        // Assert
        Assert.NotNull(result);
        Assert.True(stopwatch.ElapsedMilliseconds < 3000, "Query should complete within 3 seconds");
    }
}
```

### Privacy Tests

**Privacy Validation Tests** (`/tests/dotnet/Analytics.Tests/Privacy/`)

```csharp
public class PrivacyValidationTests
{
    [Fact]
    public async Task GetAgentAnalytics_NoMessageContent_ReturnsCleanData()
    {
        // Arrange
        var instanceId = "test-instance";
        var agentName = "test-agent";
        
        // Act
        var result = await _analyticsService.GetAgentAnalyticsSummary(instanceId, agentName, null, null);
        var json = JsonSerializer.Serialize(result);
        
        // Assert
        Assert.DoesNotContain("user_prompt", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("completion", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("message_text", json, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task GetUserAnalytics_NoPII_ReturnsCleanData()
    {
        // Arrange
        var instanceId = "test-instance";
        var username = "test-user@example.com";
        
        // Act
        var result = await _analyticsService.GetUserAnalyticsSummary(instanceId, username, null, null);
        var json = JsonSerializer.Serialize(result);
        
        // Assert
        // Should only contain username/UPN, not email addresses or full names
        Assert.DoesNotContain("@example.com", json); // Except in username field which is allowed
        // Verify no sensitive PII patterns
    }
    
    [Fact]
    public void AnonymizeUPN_Reversible_ReturnsDifferentHash()
    {
        // Arrange
        var upn = "user@example.com";
        var salt = "test-salt";
        
        // Act
        var hash1 = _anonymizationService.AnonymizeUPN(upn, salt);
        var hash2 = _anonymizationService.AnonymizeUPN(upn + "x", salt);
        
        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(upn, hash1);
    }
}
```

### Test Data Setup

**Test Data Factory** (`/tests/dotnet/Analytics.Tests/TestData/`)

```csharp
public class AnalyticsTestDataFactory
{
    public static async Task CreateTestSessions(CosmosClient client, string database, string container, int count)
    {
        var container = client.GetContainer(database, container);
        var sessions = Enumerable.Range(0, count).Select(i => new
        {
            id = Guid.NewGuid().ToString(),
            type = "Session",
            sessionId = Guid.NewGuid().ToString(),
            upn = $"user{i}@example.com",
            tokensUsed = Random.Shared.Next(100, 10000),
            _ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Random.Shared.Next(0, 86400 * 7)
        });
        
        foreach (var session in sessions)
        {
            await container.CreateItemAsync(session);
        }
    }
    
    public static async Task CreateTestMessages(CosmosClient client, string database, string container, string sessionId, int count)
    {
        var container = client.GetContainer(database, container);
        var messages = Enumerable.Range(0, count).Select(i => new
        {
            id = Guid.NewGuid().ToString(),
            type = "Message",
            sessionId = sessionId,
            sender = i % 2 == 0 ? "User" : "Agent",
            tokens = Random.Shared.Next(10, 1000),
            _ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Random.Shared.Next(0, 86400)
        });
        
        foreach (var message in messages)
        {
            await container.CreateItemAsync(message);
        }
    }
}
```

## Analytics Queries

### Agent Usage Query Example

```kusto
// Application Insights: Agent usage
let startTime = ago(30d);
let endTime = now();
traces
| where timestamp between (startTime .. endTime)
| where name == "Completions_GetCompletion"
| extend AgentName = tostring(customDimensions["FoundationaLLM-AgentName"])
| extend Username = tostring(customDimensions["FoundationaLLM-UPN"])
| extend ResponseTime = duration
| summarize 
    RequestCount = count(),
    UniqueUsers = dcount(Username),
    AvgResponseTime = avg(ResponseTime),
    P95ResponseTime = percentile(ResponseTime, 95)
    by AgentName, bin(timestamp, 1d)
| order by timestamp desc, RequestCount desc
```

### Cosmos DB Query Example

```sql
-- Agent token consumption
SELECT 
    c.agentName,
    COUNT(DISTINCT c.upn) as uniqueUsers,
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
    - UserAnalyticsSummary.cs
    - AnalyticsOverview.cs
    - TopUserSummary.cs
    - UserAbuseIndicators.cs
    - AbuseIndicator.cs
    - UserAnomaly.cs
  - Queries/
    - CosmosDBQueries.cs (query builders)
    - ApplicationInsightsQueries.cs (KQL query builders)
  - Anonymization/
    - IAnonymizationService.cs
    - AnonymizationService.cs
  - AbuseDetection/
    - IAbuseDetectionService.cs
    - AbuseDetectionService.cs
    - RiskScoringService.cs
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
    - GET /instances/{instanceId}/analytics/users/top
    - GET /instances/{instanceId}/analytics/users/{username}
    - GET /instances/{instanceId}/analytics/users/{username}/abuse-indicators
    - GET /instances/{instanceId}/analytics/users/anomalies
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
      - users/
        - index.vue (top users)
        - [username].vue (user detail)
  - components/
    - analytics/
      - UsageChart.vue
      - DistributionChart.vue
      - MetricCard.vue
      - AnalyticsTable.vue
      - UserMetricsCard.vue
      - AbuseIndicatorsPanel.vue
      - ActivityTimeline.vue
      - AnomalyAlertList.vue
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

## Security and Compliance

### Data Protection
1. **Access Control**: Role-based access to analytics data
2. **Encryption**: All data encrypted at rest and in transit
3. **Audit Logging**: All analytics access logged
4. **Data Retention**: Configurable retention policies

### Privacy Safeguards
1. **Anonymization**: User identifiers hashed where appropriate (username/UPN displayed only for abuse detection)
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
8. ✅ All tests passing
9. ✅ Performance requirements met
10. ✅ Documentation complete

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
2. **Advanced Anomaly Detection**: Machine learning-based pattern detection
3. **Recommendation Engine**: Agent/tool/model recommendations
4. **Cost Optimization**: ML-based cost optimization
5. **Quality Metrics**: Automated quality scoring
6. **Real-time Alerts**: WebSocket-based real-time anomaly alerts
7. **Custom Dashboards**: User-configurable dashboard layouts

## Appendix

### Data Source Mapping

| Analytics Need | Data Source | Query Type |
|----------------|-------------|------------|
| Agent usage | Cosmos DB (Sessions) | SQL |
| User count | Cosmos DB (Sessions) | SQL |
| Token consumption | Cosmos DB (Messages/Sessions) | SQL |
| Tool usage | Cosmos DB (Messages.analysisResults) | SQL |
| File statistics | Cosmos DB (Attachments) | SQL |
| Response times | Application Insights | KQL |
| Error rates | Application Insights + Log Analytics | KQL |
| Model usage | Application Insights | KQL |
| Historical trends | Data Lake | SQL/KQL |

### Example Anonymization

```csharp
// Pseudo-code for UPN anonymization (when needed for non-abuse analytics)
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
