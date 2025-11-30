using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for retrieving analytics data from existing data sources.
    /// </summary>
    /// <param name="cosmosDBService">The Cosmos DB service.</param>
    /// <param name="logger">The logger.</param>
    public class AnalyticsService(
        IAzureCosmosDBService cosmosDBService,
        ILogger<AnalyticsService> logger) : IAnalyticsService
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
        private readonly ILogger<AnalyticsService> _logger = logger;

        /// <inheritdoc/>
        public async Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string instanceId, DateTime? startDate, DateTime? endDate, string? agentName = null, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Build query for conversations - if agentName is provided, filter by sessions that have messages from that agent
                int totalConversations;
                if (!string.IsNullOrEmpty(agentName))
                {
                    // Get distinct sessions that have at least one message from the specified agent
                    // Cosmos DB doesn't support COUNT(DISTINCT) directly, so we'll get distinct sessionIds and count
                    var sessionsQuery = new QueryDefinition("SELECT DISTINCT c.sessionId FROM c WHERE c.type = @type AND c.sender = @sender AND c.senderDisplayName = @agentName AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.sessionId) AND c.sessionId != null")
                        .WithParameter("@type", "Message")
                        .WithParameter("@sender", "Agent")
                        .WithParameter("@agentName", agentName)
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                    var distinctSessions = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                        AzureCosmosDBContainers.Sessions,
                        sessionsQuery,
                        cancellationToken);
                    totalConversations = distinctSessions.Count;
                }
                else
                {
                    var sessionsQuery = new QueryDefinition("SELECT COUNT(1) AS count FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp")
                        .WithParameter("@type", "Session")
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                    totalConversations = await ExecuteCountQueryAsync(sessionsQuery, cancellationToken);
                }

                // Get total tokens - tokens are stored in Message documents, not Session documents
                // Sum all tokens from Message documents (both User and Agent messages)
                // If agentName is provided, only count tokens from that agent's messages
                QueryDefinition tokensQuery;
                if (!string.IsNullOrEmpty(agentName))
                {
                    tokensQuery = new QueryDefinition("SELECT SUM(c.tokens) AS sum FROM c WHERE c.type = @type AND c.sender = @sender AND c.senderDisplayName = @agentName AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.tokens)")
                        .WithParameter("@type", "Message")
                        .WithParameter("@sender", "Agent")
                        .WithParameter("@agentName", agentName)
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                }
                else
                {
                    tokensQuery = new QueryDefinition("SELECT SUM(c.tokens) AS sum FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.tokens)")
                        .WithParameter("@type", "Message")
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                }
                var totalTokens = await ExecuteSumQueryAsync(tokensQuery, cancellationToken);

                // Get unique users - Cosmos DB doesn't support COUNT(DISTINCT) directly, so we'll get distinct UPNs and count
                // If agentName is provided, only count users who interacted with that agent
                QueryDefinition usersQuery;
                if (!string.IsNullOrEmpty(agentName))
                {
                    usersQuery = new QueryDefinition("SELECT DISTINCT c.upn FROM c WHERE c.type = @type AND c.sender = @sender AND c.senderDisplayName = @agentName AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.upn) AND c.upn != null")
                        .WithParameter("@type", "Message")
                        .WithParameter("@sender", "Agent")
                        .WithParameter("@agentName", agentName)
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                }
                else
                {
                    usersQuery = new QueryDefinition("SELECT DISTINCT c.upn FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.upn) AND c.upn != null")
                        .WithParameter("@type", "Session")
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                }
                var distinctUsers = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    usersQuery,
                    cancellationToken);
                var activeUsers = distinctUsers.Count;

                // Get distinct agents - query Message documents for distinct senderDisplayName where sender = "Agent"
                // If agentName is provided, totalAgents will be 1 (just that agent) or 0
                int totalAgents;
                if (!string.IsNullOrEmpty(agentName))
                {
                    // Check if the agent exists in the date range
                    var agentCheckQuery = new QueryDefinition("SELECT TOP 1 c.senderDisplayName FROM c WHERE c.type = @type AND c.sender = @sender AND c.senderDisplayName = @agentName AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.senderDisplayName) AND c.senderDisplayName != null")
                        .WithParameter("@type", "Message")
                        .WithParameter("@sender", "Agent")
                        .WithParameter("@agentName", agentName)
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                    var agentCheck = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                        AzureCosmosDBContainers.Sessions,
                        agentCheckQuery,
                        cancellationToken);
                    totalAgents = agentCheck.Any() ? 1 : 0;
                }
                else
                {
                    var agentsQuery = new QueryDefinition("SELECT DISTINCT c.senderDisplayName FROM c WHERE c.type = @type AND c.sender = @sender AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND IS_DEFINED(c.senderDisplayName) AND c.senderDisplayName != null")
                        .WithParameter("@type", "Message")
                        .WithParameter("@sender", "Agent")
                        .WithParameter("@startTimestamp", startTimestamp)
                        .WithParameter("@endTimestamp", endTimestamp);
                    var distinctAgents = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                        AzureCosmosDBContainers.Sessions,
                        agentsQuery,
                        cancellationToken);
                    totalAgents = distinctAgents.Count;
                }

                return new AnalyticsOverview
                {
                    TotalConversations = totalConversations,
                    TotalTokens = totalTokens,
                    ActiveUsers = activeUsers,
                    TotalAgents = totalAgents,
                    AvgResponseTimeMs = 0, // Will be populated from Application Insights
                    TotalTools = 0, // Will be populated from tool analysis
                    TotalModels = 0 // Will be populated from Application Insights
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics overview");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<AgentAnalyticsSummary>> GetAllAgentsAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Get all agent messages with their details
                var messagesQuery = new QueryDefinition(@"
                    SELECT 
                        c.senderDisplayName as agentName,
                        c.upn,
                        c.sessionId,
                        c.tokens,
                        c.contentArtifacts
                    FROM c 
                    WHERE c.type = @type 
                        AND c.sender = @sender 
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.senderDisplayName) 
                        AND c.senderDisplayName != null")
                    .WithParameter("@type", "Message")
                    .WithParameter("@sender", "Agent")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var messages = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    messagesQuery,
                    cancellationToken);

                // Group by agent
                var agentData = new Dictionary<string, AgentMetrics>();

                foreach (var message in messages)
                {
                    if (message == null || !message.TryGetValue("agentName", out var agentNameObj) || agentNameObj is not string agentName || string.IsNullOrEmpty(agentName))
                        continue;

                    if (!agentData.ContainsKey(agentName))
                    {
                        agentData[agentName] = new AgentMetrics
                        {
                            AgentName = agentName,
                            UniqueUsers = new HashSet<string>(),
                            Sessions = new HashSet<string>(),
                            TotalTokens = 0,
                            TotalMessages = 0,
                            TotalToolsUsed = 0
                        };
                    }

                    var metrics = agentData[agentName];

                    // Track unique users
                    if (message.TryGetValue("upn", out var upnObj) && upnObj is string upn && !string.IsNullOrEmpty(upn))
                    {
                        metrics.UniqueUsers.Add(upn);
                    }

                    // Track unique sessions
                    if (message.TryGetValue("sessionId", out var sessionIdObj) && sessionIdObj is string sessionId && !string.IsNullOrEmpty(sessionId))
                    {
                        metrics.Sessions.Add(sessionId);
                    }

                    // Sum tokens
                    if (message.TryGetValue("tokens", out var tokensObj))
                    {
                        if (tokensObj is long tokensLong)
                        {
                            metrics.TotalTokens += tokensLong;
                        }
                        else if (tokensObj is int tokensInt)
                        {
                            metrics.TotalTokens += tokensInt;
                        }
                    }

                    // Count tools used (ToolExecution in contentArtifacts)
                    metrics.TotalMessages++;
                    int toolCount = 0;
                    
                    if (message.TryGetValue("contentArtifacts", out var artifactsObj) && artifactsObj != null)
                    {
                        try
                        {
                            // Handle Newtonsoft.Json JArray (most common case based on logs)
                            if (artifactsObj is JArray jArray)
                            {
                                foreach (var artifact in jArray)
                                {
                                    if (artifact is JObject artifactObj && artifactObj["type"] != null)
                                    {
                                        var typeValue = artifactObj["type"].ToString();
                                        if (typeValue == "ToolExecution")
                                        {
                                            toolCount++;
                                        }
                                    }
                                }
                            }
                            // Handle JsonElement array
                            else if (artifactsObj is System.Text.Json.JsonElement artifactsElement)
                            {
                                if (artifactsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                {
                                    foreach (var artifact in artifactsElement.EnumerateArray())
                                    {
                                        if (artifact.ValueKind == System.Text.Json.JsonValueKind.Object &&
                                            artifact.TryGetProperty("type", out var typeElement) && 
                                            typeElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                        {
                                            var typeValue = typeElement.GetString();
                                            if (typeValue == "ToolExecution")
                                            {
                                                toolCount++;
                                            }
                                        }
                                    }
                                }
                            }
                            // Handle List<Dictionary<string, object>> or similar
                            else if (artifactsObj is System.Collections.IEnumerable enumerable && !(artifactsObj is string))
                            {
                                foreach (var item in enumerable)
                                {
                                    if (item is Dictionary<string, object> artifactDict)
                                    {
                                        if (artifactDict.TryGetValue("type", out var typeObj) && typeObj is string typeStr && typeStr == "ToolExecution")
                                        {
                                            toolCount++;
                                        }
                                    }
                                    else if (item is JObject jObject && jObject["type"] != null)
                                    {
                                        var typeValue = jObject["type"].ToString();
                                        if (typeValue == "ToolExecution")
                                        {
                                            toolCount++;
                                        }
                                    }
                                    else if (item is System.Text.Json.JsonElement artifactElement && artifactElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                                    {
                                        if (artifactElement.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                        {
                                            var typeValue = typeElement.GetString();
                                            if (typeValue == "ToolExecution")
                                            {
                                                toolCount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing contentArtifacts for agent {AgentName}", agentName);
                        }
                    }
                    
                    metrics.TotalToolsUsed += toolCount;
                }

                // Convert to AgentAnalyticsSummary list
                var summaries = agentData.Values.Select(metrics => new AgentAnalyticsSummary
                {
                    AgentName = metrics.AgentName,
                    UniqueUsers = metrics.UniqueUsers.Count,
                    TotalConversations = metrics.Sessions.Count,
                    TotalTokens = metrics.TotalTokens,
                    AvgToolsUsed = metrics.TotalMessages > 0 ? (double)metrics.TotalToolsUsed / metrics.TotalMessages : 0.0
                }).OrderBy(a => a.AgentName).ToList();

                return summaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all agents analytics");
                throw;
            }
        }

        private class AgentMetrics
        {
            public string AgentName { get; set; } = string.Empty;
            public HashSet<string> UniqueUsers { get; set; } = new();
            public HashSet<string> Sessions { get; set; } = new();
            public long TotalTokens { get; set; }
            public int TotalMessages { get; set; }
            public int TotalToolsUsed { get; set; }
        }

        /// <inheritdoc/>
        public async Task<AgentAnalyticsSummary> GetAgentAnalyticsSummaryAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            // Note: Agent analytics requires Application Insights integration
            _logger.LogWarning("GetAgentAnalyticsSummaryAsync: Agent analytics requires Application Insights integration");
            return new AgentAnalyticsSummary { AgentName = agentName };
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, int>> GetAgentToolCombinationsAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query messages with analysis results to extract tool combinations
                // Note: This requires proper parsing of analysisResults which contains tool names
                // For now, return empty dictionary - full implementation would parse JSON structure
                var toolCombinations = new Dictionary<string, int>();
                _logger.LogWarning("GetAgentToolCombinationsAsync: Tool combination analysis requires proper JSON parsing of analysisResults");
                return toolCombinations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent tool combinations");
                return new Dictionary<string, int>();
            }
        }

        /// <inheritdoc/>
        public async Task<FileAnalyticsSummary> GetAgentFileAnalyticsAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query messages with attachments
                var messagesQuery = new QueryDefinition(@"
                    SELECT VALUE COUNT(1) 
                    FROM c 
                    WHERE c.type = ""Message"" 
                        AND c.sender = ""User""
                        AND IS_ARRAY(c.attachments) 
                        AND ARRAY_LENGTH(c.attachments) > 0
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var messagesWithAttachments = await ExecuteCountQueryAsync(messagesQuery, cancellationToken);

                // Query attachments container for file statistics
                var attachmentsQuery = new QueryDefinition(@"
                    SELECT 
                        COUNT(1) as fileCount,
                        AVG(c.fileSize) as avgSize,
                        SUM(c.fileSize) as totalSize
                    FROM c 
                    WHERE c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var attachmentStats = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Attachments,
                    attachmentsQuery,
                    cancellationToken);

                var fileCount = 0;
                var avgSize = 0.0;
                var totalSize = 0L;

                if (attachmentStats.Any())
                {
                    var stat = attachmentStats.First();
                    fileCount = stat.TryGetProperty("fileCount", out var fcProp) ? fcProp.GetInt32() : 0;
                    avgSize = stat.TryGetProperty("avgSize", out var avgProp) ? avgProp.GetDouble() : 0.0;
                    totalSize = stat.TryGetProperty("totalSize", out var totalProp) ? totalProp.GetInt64() : 0L;
                }

                return new FileAnalyticsSummary
                {
                    TotalFiles = fileCount > 0 ? fileCount : messagesWithAttachments,
                    AvgFilesPerConversation = 0, // Would need to calculate from conversation data
                    AvgFileSizeBytes = avgSize,
                    MedianFileSizeBytes = 0, // Would need percentile calculation
                    TotalStorageBytes = totalSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file analytics");
                return new FileAnalyticsSummary();
            }
        }

        /// <inheritdoc/>
        public async Task<List<ToolAnalyticsSummary>> GetToolAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query messages with analysis results to extract tool usage
                // Note: This requires proper parsing of analysisResults JSON structure
                // For now, return empty list - full implementation would parse and aggregate tool usage
                _logger.LogWarning("GetToolAnalyticsAsync: Tool analytics requires proper JSON parsing of analysisResults");
                return new List<ToolAnalyticsSummary>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tool analytics");
                return new List<ToolAnalyticsSummary>();
            }
        }

        /// <inheritdoc/>
        public async Task<List<ModelAnalyticsSummary>> GetModelAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            // Model analytics requires Application Insights integration
            _logger.LogWarning("GetModelAnalyticsAsync: Model analytics requires Application Insights integration");
            return new List<ModelAnalyticsSummary>();
        }

        /// <inheritdoc/>
        public async Task<List<TopUserSummary>> GetTopUsersAsync(string instanceId, int topCount, UserSortBy sortBy, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                var orderBy = sortBy switch
                {
                    UserSortBy.Requests => "totalRequests DESC",
                    UserSortBy.Tokens => "totalTokens DESC",
                    UserSortBy.Sessions => "totalSessions DESC",
                    UserSortBy.RiskScore => "abuseRiskScore DESC",
                    _ => "totalRequests DESC"
                };

                var query = new QueryDefinition($@"
                    SELECT 
                        c.upn as username,
                        COUNT(1) as totalSessions,
                        SUM(c.tokensUsed) as totalTokens,
                        MAX(c._ts) as lastActivity
                    FROM c
                    WHERE c.type = ""Session""
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.upn) AND c.upn != null
                    GROUP BY c.upn
                    ORDER BY {orderBy}
                    OFFSET 0 LIMIT @topCount")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp)
                    .WithParameter("@topCount", topCount);

                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                var topUsers = new List<TopUserSummary>();
                foreach (var result in results)
                {
                    var username = result.TryGetProperty("username", out var usernameProp) 
                        ? usernameProp.GetString() ?? "unknown" 
                        : "unknown";
                    var totalSessions = result.TryGetProperty("totalSessions", out var sessionsProp)
                        ? sessionsProp.GetInt32()
                        : 0;
                    var totalTokens = result.TryGetProperty("totalTokens", out var tokensProp)
                        ? tokensProp.GetInt64()
                        : 0L;
                    var lastActivityTs = result.TryGetProperty("lastActivity", out var activityProp)
                        ? activityProp.GetInt64()
                        : 0L;
                    var lastActivity = lastActivityTs > 0 
                        ? DateTimeOffset.FromUnixTimeSeconds(lastActivityTs).DateTime 
                        : DateTime.UtcNow;

                    // Get request count from messages
                    var requestCount = await GetUserRequestCountAsync(instanceId, username, startTimestamp, endTimestamp, cancellationToken);

                    topUsers.Add(new TopUserSummary
                    {
                        Username = username,
                        UPN = username,
                        TotalRequests = requestCount,
                        TotalTokens = totalTokens,
                        ActiveSessions = totalSessions,
                        LastActivity = lastActivity,
                        AbuseRiskScore = 0, // Will be calculated by abuse detection service
                        AgentsUsed = 0 // Will be calculated from Application Insights
                    });
                }

                return topUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top users");
                return new List<TopUserSummary>();
            }
        }

        /// <inheritdoc/>
        public async Task<UserAnalyticsSummary> GetUserAnalyticsSummaryAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Get user sessions
                var sessionsQuery = new QueryDefinition(@"
                    SELECT 
                        COUNT(1) as sessionCount,
                        SUM(c.tokensUsed) as totalTokens,
                        AVG(c.tokensUsed) as avgTokens
                    FROM c
                    WHERE c.type = ""Session""
                        AND c.upn = @username
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp")
                    .WithParameter("@username", username)
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var sessions = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    sessionsQuery,
                    cancellationToken);

                var sessionData = sessions.FirstOrDefault();
                var totalSessions = sessionData.TryGetProperty("sessionCount", out var sessionCountProp)
                    ? sessionCountProp.GetInt32()
                    : 0;
                var totalTokens = sessionData.TryGetProperty("totalTokens", out var totalTokensProp)
                    ? totalTokensProp.GetInt64()
                    : 0L;

                // Get request count
                var totalRequests = await GetUserRequestCountAsync(instanceId, username, startTimestamp, endTimestamp, cancellationToken);

                // Get active days - get distinct days from timestamps
                var activeDaysQuery = new QueryDefinition(@"
                    SELECT DISTINCT DateTimeAdd(""dd"", 0, DateTimeFromEpoch(c._ts)) as day
                    FROM c
                    WHERE c.type = ""Session""
                        AND c.upn = @username
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp")
                    .WithParameter("@username", username)
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var distinctDays = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    activeDaysQuery,
                    cancellationToken);
                var activeDays = distinctDays.Count;

                // Get average session length
                var avgSessionLength = await GetAverageSessionLengthAsync(username, startTimestamp, endTimestamp, cancellationToken);

                return new UserAnalyticsSummary
                {
                    Username = username,
                    TotalRequests = totalRequests,
                    TotalTokens = totalTokens,
                    TotalSessions = totalSessions,
                    ActiveDays = activeDays,
                    AvgSessionLength = avgSessionLength,
                    AgentsUsed = 0, // Will be populated from Application Insights
                    ErrorRate = 0 // Will be populated from Application Insights
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user analytics summary");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserActivityTimeline> GetUserActivityTimelineAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                var query = new QueryDefinition(@"
                    SELECT 
                        c._ts as timestamp,
                        c.sessionId,
                        c.operationId,
                        c.tokens
                    FROM c
                    WHERE c.type = ""Message""
                        AND c.upn = @username
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp
                    ORDER BY c._ts ASC")
                    .WithParameter("@username", username)
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                var entries = new List<UserActivityEntry>();
                foreach (var result in results)
                {
                    var timestampTs = result.TryGetProperty("timestamp", out var tsProp)
                        ? tsProp.GetInt64()
                        : 0L;
                    var timestamp = timestampTs > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(timestampTs).DateTime
                        : DateTime.UtcNow;

                    entries.Add(new UserActivityEntry
                    {
                        Timestamp = timestamp,
                        SessionId = result.TryGetProperty("sessionId", out var sessionIdProp)
                            ? sessionIdProp.GetString() ?? string.Empty
                            : string.Empty,
                        OperationId = result.TryGetProperty("operationId", out var opIdProp)
                            ? opIdProp.GetString()
                            : null,
                        Tokens = result.TryGetProperty("tokens", out var tokensProp)
                            ? tokensProp.GetInt32()
                            : 0
                    });
                }

                return new UserActivityTimeline
                {
                    Username = username,
                    Entries = entries
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activity timeline");
                return new UserActivityTimeline { Username = username };
            }
        }

        private async Task<int> GetUserRequestCountAsync(string instanceId, string username, long startTimestamp, long endTimestamp, CancellationToken cancellationToken)
        {
            try
            {
                var query = new QueryDefinition(@"
                    SELECT VALUE COUNT(1)
                    FROM c
                    WHERE c.type = @type
                        AND c.sender = @sender
                        AND c.upn = @username
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp")
                    .WithParameter("@type", "Message")
                    .WithParameter("@sender", "User")
                    .WithParameter("@username", username)
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                if (results.Any() && results.First().ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    return results.First().GetInt32();
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user request count");
                return 0;
            }
        }

        private async Task<double> GetAverageSessionLengthAsync(string username, long startTimestamp, long endTimestamp, CancellationToken cancellationToken)
        {
            try
            {
                // Get message count per session, then average
                var query = new QueryDefinition(@"
                    SELECT 
                        c.sessionId,
                        COUNT(1) as messageCount
                    FROM c
                    WHERE c.type = ""Message""
                        AND c.upn = @username
                        AND c._ts >= @startTimestamp
                        AND c._ts < @endTimestamp
                    GROUP BY c.sessionId")
                    .WithParameter("@username", username)
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                if (!results.Any())
                    return 0;

                var messageCounts = results.Select(r => 
                    r.TryGetProperty("messageCount", out var countProp)
                        ? countProp.GetInt32()
                        : 0).ToList();
                return messageCounts.Average();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average session length");
                return 0;
            }
        }

        private async Task<int> ExecuteCountQueryAsync(QueryDefinition query, CancellationToken cancellationToken)
        {
            try
            {
                // Use Dictionary<string, object> to handle the result object
                var results = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                if (results.Any())
                {
                    var first = results.First();
                    // Query returns { "count": value } object
                    if (first.TryGetValue("count", out var countValue))
                    {
                        if (countValue is long longCount)
                        {
                            return (int)longCount;
                        }
                        if (countValue is int intCount)
                        {
                            return intCount;
                        }
                        if (countValue is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            return jsonElement.GetInt32();
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing count query");
                return 0;
            }
        }

        private async Task<long> ExecuteSumQueryAsync(QueryDefinition query, CancellationToken cancellationToken)
        {
            try
            {
                // Use Dictionary<string, object> to handle the result object
                var results = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                _logger.LogDebug("Sum query returned {Count} results", results.Count);

                if (results.Any())
                {
                    var first = results.First();
                    _logger.LogDebug("Sum query result dictionary keys: {Keys}", string.Join(", ", first.Keys));
                    
                    // Query returns { "sum": value } object
                    if (first.TryGetValue("sum", out var sumValue))
                    {
                        _logger.LogDebug("Sum value type: {Type}, value: {Value}", sumValue?.GetType().Name, sumValue);
                        
                        // SUM returns null when there are no matching rows
                        if (sumValue == null)
                        {
                            return 0;
                        }
                        if (sumValue is long longSum)
                        {
                            return longSum;
                        }
                        if (sumValue is int intSum)
                        {
                            return intSum;
                        }
                        if (sumValue is System.Text.Json.JsonElement jsonElement)
                        {
                            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Null)
                            {
                                return 0;
                            }
                            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                            {
                                return jsonElement.GetInt64();
                            }
                        }
                        // Try to convert if it's a boxed numeric type
                        if (sumValue is IConvertible convertible)
                        {
                            return convertible.ToInt64(System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Sum query result does not contain 'sum' key. Available keys: {Keys}", string.Join(", ", first.Keys));
                    }
                }
                else
                {
                    _logger.LogWarning("Sum query returned no results");
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing sum query");
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DailyMessageCount>> GetDailyMessageCountsPerAgentAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query messages grouped by date and agent
                // Cosmos DB doesn't support DATE functions directly, so we'll need to group by _ts and process in memory
                var query = new QueryDefinition(@"
                    SELECT 
                        c._ts as timestamp,
                        c.senderDisplayName as agentName
                    FROM c 
                    WHERE c.type = @type 
                        AND c.sender = @sender 
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.senderDisplayName) 
                        AND c.senderDisplayName != null")
                    .WithParameter("@type", "Message")
                    .WithParameter("@sender", "Agent")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                // Group by date and agent
                var dailyCounts = new Dictionary<DateTime, Dictionary<string, int>>();
                
                foreach (var result in results)
                {
                    if (result.TryGetValue("timestamp", out var tsValue) && result.TryGetValue("agentName", out var agentValue))
                    {
                        if (tsValue is long timestamp && agentValue is string agentName)
                        {
                            var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).Date;
                            
                            if (!dailyCounts.ContainsKey(date))
                            {
                                dailyCounts[date] = new Dictionary<string, int>();
                            }
                            
                            if (!dailyCounts[date].ContainsKey(agentName))
                            {
                                dailyCounts[date][agentName] = 0;
                            }
                            
                            dailyCounts[date][agentName]++;
                        }
                    }
                }

                // Convert to list and fill in missing dates
                var resultList = new List<DailyMessageCount>();
                var currentDate = start.Date;
                var allAgents = dailyCounts.Values
                    .SelectMany(d => d.Keys)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                while (currentDate <= end.Date)
                {
                    var dailyCount = new DailyMessageCount
                    {
                        Date = currentDate,
                        AgentCounts = new Dictionary<string, int>()
                    };

                    if (dailyCounts.ContainsKey(currentDate))
                    {
                        foreach (var agent in allAgents)
                        {
                            dailyCount.AgentCounts[agent] = dailyCounts[currentDate].GetValueOrDefault(agent, 0);
                        }
                    }
                    else
                    {
                        // No messages for this date, set all agents to 0
                        foreach (var agent in allAgents)
                        {
                            dailyCount.AgentCounts[agent] = 0;
                        }
                    }

                    resultList.Add(dailyCount);
                    currentDate = currentDate.AddDays(1);
                }

                return resultList.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily message counts per agent");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<DailyUserCount>> GetDailyUserCountsPerAgentAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query agent messages grouped by date, agent, and user
                var query = new QueryDefinition(@"
                    SELECT 
                        c._ts as timestamp,
                        c.senderDisplayName as agentName,
                        c.upn as username
                    FROM c 
                    WHERE c.type = @type 
                        AND c.sender = @sender 
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.senderDisplayName) 
                        AND c.senderDisplayName != null
                        AND IS_DEFINED(c.upn)
                        AND c.upn != null")
                    .WithParameter("@type", "Message")
                    .WithParameter("@sender", "Agent")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                // Group by date and agent, then count distinct users per day per agent
                var dailyCounts = new Dictionary<DateTime, Dictionary<string, HashSet<string>>>();

                foreach (var result in results)
                {
                    if (result.TryGetValue("timestamp", out var tsValue) && 
                        result.TryGetValue("agentName", out var agentValue) &&
                        result.TryGetValue("username", out var userValue))
                    {
                        if (tsValue is long timestamp && 
                            agentValue is string agentName && 
                            userValue is string username)
                        {
                            var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).Date;

                            if (!dailyCounts.ContainsKey(date))
                            {
                                dailyCounts[date] = new Dictionary<string, HashSet<string>>();
                            }

                            if (!dailyCounts[date].ContainsKey(agentName))
                            {
                                dailyCounts[date][agentName] = new HashSet<string>();
                            }

                            dailyCounts[date][agentName].Add(username);
                        }
                    }
                }

                // Convert to list and fill in missing dates
                var resultList = new List<DailyUserCount>();
                var currentDate = start.Date;
                var allAgents = dailyCounts.Values
                    .SelectMany(d => d.Keys)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                while (currentDate <= end.Date)
                {
                    var dailyCount = new DailyUserCount
                    {
                        Date = currentDate,
                        AgentCounts = new Dictionary<string, int>()
                    };

                    if (dailyCounts.ContainsKey(currentDate))
                    {
                        foreach (var agent in allAgents)
                        {
                            dailyCount.AgentCounts[agent] = dailyCounts[currentDate].GetValueOrDefault(agent, new HashSet<string>()).Count;
                        }
                    }
                    else
                    {
                        // No users for this date, set all agents to 0
                        foreach (var agent in allAgents)
                        {
                            dailyCount.AgentCounts[agent] = 0;
                        }
                    }

                    resultList.Add(dailyCount);
                    currentDate = currentDate.AddDays(1);
                }

                return resultList.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily user counts per agent");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<UserAnalyticsSummarySimple>> GetAllUsersAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Get all messages (both User and Agent) with their details
                var messagesQuery = new QueryDefinition(@"
                    SELECT 
                        c.upn as username,
                        c.sessionId,
                        c.tokens
                    FROM c 
                    WHERE c.type = @type 
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.upn) 
                        AND c.upn != null")
                    .WithParameter("@type", "Message")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var messages = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    messagesQuery,
                    cancellationToken);

                // Group by user
                var userData = new Dictionary<string, UserMetrics>();

                foreach (var message in messages)
                {
                    if (message == null || !message.TryGetValue("username", out var usernameObj) || usernameObj is not string username || string.IsNullOrEmpty(username))
                        continue;

                    if (!userData.ContainsKey(username))
                    {
                        userData[username] = new UserMetrics
                        {
                            Username = username,
                            Sessions = new HashSet<string>(),
                            TotalTokens = 0,
                            TotalMessages = 0
                        };
                    }

                    var metrics = userData[username];

                    // Track unique sessions
                    if (message.TryGetValue("sessionId", out var sessionIdObj) && sessionIdObj is string sessionId && !string.IsNullOrEmpty(sessionId))
                    {
                        metrics.Sessions.Add(sessionId);
                    }

                    // Sum tokens
                    if (message.TryGetValue("tokens", out var tokensObj))
                    {
                        if (tokensObj is long tokensLong)
                        {
                            metrics.TotalTokens += tokensLong;
                        }
                        else if (tokensObj is int tokensInt)
                        {
                            metrics.TotalTokens += tokensInt;
                        }
                    }

                    // Count messages
                    metrics.TotalMessages++;
                }

                // Convert to UserAnalyticsSummarySimple list
                var summaries = userData.Values.Select(metrics => new UserAnalyticsSummarySimple
                {
                    Username = metrics.Username,
                    TotalConversations = metrics.Sessions.Count,
                    TotalMessages = metrics.TotalMessages,
                    TotalTokens = metrics.TotalTokens
                }).OrderBy(u => u.Username).ToList();

                return summaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users analytics");
                throw;
            }
        }

        private class UserMetrics
        {
            public string Username { get; set; } = string.Empty;
            public HashSet<string> Sessions { get; set; } = new();
            public long TotalTokens { get; set; }
            public int TotalMessages { get; set; }
        }

        /// <inheritdoc/>
        public async Task<List<DailyActiveUserCount>> GetDailyActiveUserCountsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Query messages to get distinct users per day
                var query = new QueryDefinition(@"
                    SELECT 
                        c._ts as timestamp,
                        c.upn as username
                    FROM c 
                    WHERE c.type = @type 
                        AND c._ts >= @startTimestamp 
                        AND c._ts < @endTimestamp
                        AND IS_DEFINED(c.upn) 
                        AND c.upn != null")
                    .WithParameter("@type", "Message")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);

                var results = await _cosmosDBService.QueryItemsAsync<Dictionary<string, object>>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                // Group by date and count distinct users per day
                var dailyCounts = new Dictionary<DateTime, HashSet<string>>();

                foreach (var result in results)
                {
                    if (result.TryGetValue("timestamp", out var tsValue) && 
                        result.TryGetValue("username", out var userValue))
                    {
                        if (tsValue is long timestamp && userValue is string username && !string.IsNullOrEmpty(username))
                        {
                            var date = DateTimeOffset.FromUnixTimeSeconds(timestamp).Date;

                            if (!dailyCounts.ContainsKey(date))
                            {
                                dailyCounts[date] = new HashSet<string>();
                            }

                            dailyCounts[date].Add(username);
                        }
                    }
                }

                // Convert to list and fill in missing dates
                var resultList = new List<DailyActiveUserCount>();
                var currentDate = start.Date;

                while (currentDate <= end.Date)
                {
                    var dailyCount = new DailyActiveUserCount
                    {
                        Date = currentDate,
                        Count = dailyCounts.ContainsKey(currentDate) ? dailyCounts[currentDate].Count : 0
                    };

                    resultList.Add(dailyCount);
                    currentDate = currentDate.AddDays(1);
                }

                return resultList.OrderBy(d => d.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily active user counts");
                throw;
            }
        }
    }
}
