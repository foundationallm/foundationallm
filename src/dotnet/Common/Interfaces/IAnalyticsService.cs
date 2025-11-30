using FoundationaLLM.Common.Models.Analytics;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Service for retrieving analytics data.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Gets analytics overview for the platform.
        /// </summary>
        /// <param name="instanceId">The instance ID.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="agentName">Optional agent name to filter by.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<AnalyticsOverview> GetAnalyticsOverviewAsync(string instanceId, DateTime? startDate, DateTime? endDate, string? agentName = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets analytics summary for all agents.
        /// </summary>
        Task<List<AgentAnalyticsSummary>> GetAllAgentsAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets analytics summary for a specific agent.
        /// </summary>
        Task<AgentAnalyticsSummary> GetAgentAnalyticsSummaryAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets tool combinations used with an agent.
        /// </summary>
        Task<Dictionary<string, int>> GetAgentToolCombinationsAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets file analytics for an agent.
        /// </summary>
        Task<FileAnalyticsSummary> GetAgentFileAnalyticsAsync(string instanceId, string agentName, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets analytics summary for all tools.
        /// </summary>
        Task<List<ToolAnalyticsSummary>> GetToolAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets analytics summary for all models.
        /// </summary>
        Task<List<ModelAnalyticsSummary>> GetModelAnalyticsAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets top users by specified criteria.
        /// </summary>
        Task<List<TopUserSummary>> GetTopUsersAsync(string instanceId, int topCount, UserSortBy sortBy, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets detailed analytics summary for a user.
        /// </summary>
        Task<UserAnalyticsSummary> GetUserAnalyticsSummaryAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user activity timeline.
        /// </summary>
        Task<UserActivityTimeline> GetUserActivityTimelineAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets daily message counts per agent for the specified date range.
        /// </summary>
        Task<List<DailyMessageCount>> GetDailyMessageCountsPerAgentAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets daily user counts per agent for the specified date range.
        /// </summary>
        Task<List<DailyUserCount>> GetDailyUserCountsPerAgentAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    }
}
