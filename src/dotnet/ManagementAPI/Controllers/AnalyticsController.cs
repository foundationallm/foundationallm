using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using FoundationaLLM.Common.Services.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Management.API.Controllers
{
    /// <summary>
    /// Controller for analytics endpoints.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="abuseDetectionService">The abuse detection service.</param>
    /// <param name="callContext">The orchestration context.</param>
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AnalyticsController(
        IAnalyticsService analyticsService,
        IAbuseDetectionService abuseDetectionService,
        IOrchestrationContext callContext) : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly IAbuseDetectionService _abuseDetectionService = abuseDetectionService;
        private readonly IOrchestrationContext _callContext = callContext;

        /// <summary>
        /// Gets analytics overview for the platform.
        /// </summary>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Analytics overview.</returns>
        [HttpGet("overview")]
        public async Task<ActionResult<AnalyticsOverview>> GetAnalyticsOverviewAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var overview = await _analyticsService.GetAnalyticsOverviewAsync(instanceId, startDate, endDate, cancellationToken);
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets analytics summary for all agents.
        /// </summary>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of agent analytics summaries.</returns>
        [HttpGet("agents")]
        public async Task<ActionResult<List<AgentAnalyticsSummary>>> GetAllAgentsAnalyticsAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var agents = await _analyticsService.GetAllAgentsAnalyticsAsync(instanceId, startDate, endDate, cancellationToken);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets analytics summary for a specific agent.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Agent analytics summary.</returns>
        [HttpGet("agents/{agentName}")]
        public async Task<ActionResult<AgentAnalyticsSummary>> GetAgentAnalyticsSummaryAsync(
            string agentName,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var summary = await _analyticsService.GetAgentAnalyticsSummaryAsync(instanceId, agentName, startDate, endDate, cancellationToken);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets tool combinations used with an agent.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary of tool combinations and their frequencies.</returns>
        [HttpGet("agents/{agentName}/tool-combinations")]
        public async Task<ActionResult<Dictionary<string, int>>> GetAgentToolCombinationsAsync(
            string agentName,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var combinations = await _analyticsService.GetAgentToolCombinationsAsync(instanceId, agentName, startDate, endDate, cancellationToken);
                return Ok(combinations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets file analytics for an agent.
        /// </summary>
        /// <param name="agentName">The name of the agent.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>File analytics summary.</returns>
        [HttpGet("agents/{agentName}/files")]
        public async Task<ActionResult<FileAnalyticsSummary>> GetAgentFileAnalyticsAsync(
            string agentName,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var fileAnalytics = await _analyticsService.GetAgentFileAnalyticsAsync(instanceId, agentName, startDate, endDate, cancellationToken);
                return Ok(fileAnalytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets analytics summary for all tools.
        /// </summary>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of tool analytics summaries.</returns>
        [HttpGet("tools")]
        public async Task<ActionResult<List<ToolAnalyticsSummary>>> GetToolAnalyticsAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var tools = await _analyticsService.GetToolAnalyticsAsync(instanceId, startDate, endDate, cancellationToken);
                return Ok(tools);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets analytics summary for all models.
        /// </summary>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of model analytics summaries.</returns>
        [HttpGet("models")]
        public async Task<ActionResult<List<ModelAnalyticsSummary>>> GetModelAnalyticsAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var models = await _analyticsService.GetModelAnalyticsAsync(instanceId, startDate, endDate, cancellationToken);
                return Ok(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets top users by specified criteria.
        /// </summary>
        /// <param name="topCount">The number of top users to return.</param>
        /// <param name="sortBy">The criteria to sort by.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of top user summaries.</returns>
        [HttpGet("users/top")]
        public async Task<ActionResult<List<TopUserSummary>>> GetTopUsersAsync(
            [FromQuery] int topCount = 10,
            [FromQuery] UserSortBy sortBy = UserSortBy.Requests,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var topUsers = await _analyticsService.GetTopUsersAsync(instanceId, topCount, sortBy, startDate, endDate, cancellationToken);

                // Calculate abuse risk scores for each user
                foreach (var user in topUsers)
                {
                    var riskScore = await _abuseDetectionService.CalculateAbuseRiskScoreAsync(
                        instanceId, user.Username, startDate, endDate, cancellationToken);
                    user.AbuseRiskScore = riskScore.Score;

                    var indicators = await _abuseDetectionService.DetectAbuseIndicatorsAsync(
                        instanceId, user.Username, startDate, endDate, cancellationToken);
                    user.AbuseIndicators = indicators.VolumeIndicators
                        .Concat(indicators.TemporalIndicators)
                        .Concat(indicators.BehavioralIndicators)
                        .Concat(indicators.ResourceIndicators)
                        .Select(i => i.Type)
                        .ToList();
                }

                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets detailed analytics summary for a user.
        /// </summary>
        /// <param name="username">The username/UPN of the user.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User analytics summary.</returns>
        [HttpGet("users/{username}")]
        public async Task<ActionResult<UserAnalyticsSummary>> GetUserAnalyticsSummaryAsync(
            string username,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var summary = await _analyticsService.GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate, cancellationToken);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets user activity timeline.
        /// </summary>
        /// <param name="username">The username/UPN of the user.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User activity timeline.</returns>
        [HttpGet("users/{username}/timeline")]
        public async Task<ActionResult<UserActivityTimeline>> GetUserActivityTimelineAsync(
            string username,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var timeline = await _analyticsService.GetUserActivityTimelineAsync(instanceId, username, startDate, endDate, cancellationToken);
                return Ok(timeline);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets abuse indicators for a user.
        /// </summary>
        /// <param name="username">The username/UPN of the user.</param>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User abuse indicators.</returns>
        [HttpGet("users/{username}/abuse-indicators")]
        public async Task<ActionResult<UserAbuseIndicators>> GetUserAbuseIndicatorsAsync(
            string username,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var indicators = await _abuseDetectionService.DetectAbuseIndicatorsAsync(instanceId, username, startDate, endDate, cancellationToken);
                return Ok(indicators);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Detects user anomalies across the platform.
        /// </summary>
        /// <param name="startDate">Optional start date for the analytics period.</param>
        /// <param name="endDate">Optional end date for the analytics period.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of user anomalies.</returns>
        [HttpGet("anomalies")]
        public async Task<ActionResult<List<UserAnomaly>>> DetectUserAnomaliesAsync(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instanceId = _callContext.InstanceId;
                var anomalies = await _abuseDetectionService.DetectUserAnomaliesAsync(instanceId, startDate, endDate, cancellationToken);
                return Ok(anomalies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
