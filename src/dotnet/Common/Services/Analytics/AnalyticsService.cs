using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using FoundationaLLM.Common.Models.Conversation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
        public async Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;
            var startTimestamp = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endTimestamp = ((DateTimeOffset)end).ToUnixTimeSeconds();

            try
            {
                // Get total conversations
                var sessionsQuery = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp")
                    .WithParameter("@type", "Session")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);
                var totalConversations = await ExecuteCountQueryAsync(sessionsQuery, cancellationToken);

                // Get total tokens
                var tokensQuery = new QueryDefinition("SELECT VALUE SUM(c.tokensUsed) FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp")
                    .WithParameter("@type", "Session")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);
                var totalTokens = await ExecuteSumQueryAsync(tokensQuery, cancellationToken);

                // Get unique users - Cosmos DB doesn't support COUNT(DISTINCT) directly, so we'll get distinct UPNs and count
                var usersQuery = new QueryDefinition("SELECT DISTINCT c.upn FROM c WHERE c.type = @type AND c._ts >= @startTimestamp AND c._ts < @endTimestamp AND c.upn IS NOT NULL")
                    .WithParameter("@type", "Session")
                    .WithParameter("@startTimestamp", startTimestamp)
                    .WithParameter("@endTimestamp", endTimestamp);
                var distinctUsers = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    usersQuery,
                    cancellationToken);
                var activeUsers = distinctUsers.Count;

                return new AnalyticsOverview
                {
                    TotalConversations = totalConversations,
                    TotalTokens = totalTokens,
                    ActiveUsers = activeUsers,
                    TotalAgents = 0, // Will be populated from Application Insights
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
            // Note: Agent names need to be extracted from Application Insights telemetry
            // For now, return empty list - this will be implemented with Application Insights queries
            _logger.LogWarning("GetAllAgentsAnalyticsAsync: Agent analytics requires Application Insights integration");
            return new List<AgentAnalyticsSummary>();
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
                        AND c.upn IS NOT NULL
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
                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                if (results.Any())
                {
                    var first = results.First();
                    if (first.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        return first.GetInt32();
                    }
                    if (first.TryGetProperty("count", out var countElement))
                    {
                        return countElement.GetInt32();
                    }
                }

                return results.Count;
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
                var results = await _cosmosDBService.QueryItemsAsync<System.Text.Json.JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    query,
                    cancellationToken);

                if (results.Any())
                {
                    var first = results.First();
                    if (first.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        return first.GetInt64();
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing sum query");
                return 0;
            }
        }
    }
}
