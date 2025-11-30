using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using FoundationaLLM.Common.Models.Configuration.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for detecting abuse patterns and calculating risk scores.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="analyticsSettings">The analytics settings.</param>
    /// <param name="logger">The logger.</param>
    public class AbuseDetectionService(
        IAnalyticsService analyticsService,
        IOptions<AnalyticsSettings> analyticsSettings,
        ILogger<AbuseDetectionService> logger) : IAbuseDetectionService
    {
        private readonly IAnalyticsService _analyticsService = analyticsService;
        private readonly AnalyticsSettings _analyticsSettings = analyticsSettings.Value;
        private readonly ILogger<AbuseDetectionService> _logger = logger;

        private int _continuousUsageHours = 20;
        private int _agentHoppingThreshold = 10;
        private double _highErrorRateThreshold = 30.0;

        /// <inheritdoc/>
        public async Task<AbuseRiskScore> CalculateAbuseRiskScoreAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var indicators = await DetectAbuseIndicatorsAsync(instanceId, username, startDate, endDate, cancellationToken);

                var factors = new Dictionary<string, double>();

                // Calculate volume factor (25% weight)
                var volumeScore = CalculateVolumeScore(indicators.VolumeIndicators);
                factors["Volume"] = volumeScore;

                // Calculate token consumption factor (20% weight)
                var tokenScore = CalculateTokenScore(indicators.VolumeIndicators);
                factors["TokenConsumption"] = tokenScore;

                // Calculate error rate factor (15% weight)
                var errorScore = CalculateErrorScore(indicators.BehavioralIndicators);
                factors["ErrorRate"] = errorScore;

                // Calculate temporal anomalies factor (15% weight)
                var temporalScore = CalculateTemporalScore(indicators.TemporalIndicators);
                factors["TemporalAnomalies"] = temporalScore;

                // Calculate behavioral anomalies factor (15% weight)
                var behavioralScore = CalculateBehavioralScore(indicators.BehavioralIndicators);
                factors["BehavioralAnomalies"] = behavioralScore;

                // Calculate resource exhaustion factor (10% weight)
                var resourceScore = CalculateResourceScore(indicators.ResourceIndicators);
                factors["ResourceExhaustion"] = resourceScore;

                // Calculate weighted total score
                var totalScore = (int)Math.Round(
                    (volumeScore * 0.25) +
                    (tokenScore * 0.20) +
                    (errorScore * 0.15) +
                    (temporalScore * 0.15) +
                    (behavioralScore * 0.15) +
                    (resourceScore * 0.10)
                );

                return new AbuseRiskScore
                {
                    Score = Math.Min(100, Math.Max(0, totalScore)),
                    Factors = factors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating abuse risk score for user {Username}", username);
                return new AbuseRiskScore { Score = 0 };
            }
        }

        /// <inheritdoc/>
        public async Task<UserAbuseIndicators> DetectAbuseIndicatorsAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var indicators = new UserAbuseIndicators
                {
                    Username = username,
                    VolumeIndicators = await DetectVolumeIndicatorsAsync(instanceId, username, start, end),
                    TemporalIndicators = await DetectTemporalIndicatorsAsync(instanceId, username, start, end),
                    BehavioralIndicators = await DetectBehavioralIndicatorsAsync(instanceId, username, start, end),
                    ResourceIndicators = await DetectResourceIndicatorsAsync(instanceId, username, start, end)
                };

                var riskScore = await CalculateAbuseRiskScoreAsync(instanceId, username, startDate, endDate);
                indicators.RiskScore = riskScore.Score;

                return indicators;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting abuse indicators for user {Username}", username);
                return new UserAbuseIndicators { Username = username };
            }
        }

        /// <inheritdoc/>
        public async Task<List<UserAnomaly>> DetectUserAnomaliesAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var anomalies = new List<UserAnomaly>();

                // Get top users to analyze
                var topUsers = await _analyticsService.GetTopUsersAsync(instanceId, 100, UserSortBy.Requests, start, end);

                foreach (var user in topUsers)
                {
                    var indicators = await DetectAbuseIndicatorsAsync(instanceId, user.Username, start, end);

                    // Check for high-risk users
                    if (indicators.RiskScore >= 61)
                    {
                        anomalies.Add(new UserAnomaly
                        {
                            Username = user.Username,
                            AnomalyType = "HighRiskUser",
                            Description = $"User has high abuse risk score: {indicators.RiskScore}",
                            Severity = indicators.RiskScore >= 81 ? "Critical" : "High",
                            DetectedAt = DateTime.UtcNow,
                            AnomalyData = new Dictionary<string, object>
                            {
                                ["riskScore"] = indicators.RiskScore,
                                ["totalRequests"] = user.TotalRequests,
                                ["totalTokens"] = user.TotalTokens
                            }
                        });
                    }

                    // Check for volume anomalies
                    if (indicators.VolumeIndicators.Any(i => i.Severity == "High" || i.Severity == "Critical"))
                    {
                        var highSeverityIndicators = indicators.VolumeIndicators.Where(i => i.Severity == "High" || i.Severity == "Critical").ToList();
                        anomalies.Add(new UserAnomaly
                        {
                            Username = user.Username,
                            AnomalyType = "VolumeAnomaly",
                            Description = $"User has {highSeverityIndicators.Count} high-severity volume indicators",
                            Severity = highSeverityIndicators.Any(i => i.Severity == "Critical") ? "Critical" : "High",
                            DetectedAt = DateTime.UtcNow,
                            AnomalyData = new Dictionary<string, object>
                            {
                                ["indicators"] = highSeverityIndicators.Select(i => i.Type).ToList()
                            }
                        });
                    }
                }

                return anomalies.OrderByDescending(a => a.Severity == "Critical" ? 3 : a.Severity == "High" ? 2 : 1).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting user anomalies");
                return [];
            }
        }

        private async Task<List<AbuseIndicator>> DetectVolumeIndicatorsAsync(string instanceId, string username, DateTime startDate, DateTime endDate)
        {
            var indicators = new List<AbuseIndicator>();

            try
            {
                var userAnalytics = await _analyticsService.GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate);

                // Check request rate
                var hours = (endDate - startDate).TotalHours;
                if (hours > 0)
                {
                    var requestsPerHour = userAnalytics.TotalRequests / hours;

                    if (requestsPerHour > _analyticsSettings.AbuseDetection.ExtremeRequestRateThreshold)
                    {
                        indicators.Add(new AbuseIndicator
                        {
                            Type = "ExtremeRequestRate",
                            Description = $"User has extreme request rate: {requestsPerHour:F1} requests/hour",
                            Severity = "High",
                            DetectedAt = DateTime.UtcNow,
                            Metrics = new Dictionary<string, object> { ["requestsPerHour"] = requestsPerHour }
                        });
                    }
                    else if (requestsPerHour > _analyticsSettings.AbuseDetection.HighRequestRateThreshold)
                    {
                        indicators.Add(new AbuseIndicator
                        {
                            Type = "HighRequestRate",
                            Description = $"User has high request rate: {requestsPerHour:F1} requests/hour",
                            Severity = "Medium",
                            DetectedAt = DateTime.UtcNow,
                            Metrics = new Dictionary<string, object> { ["requestsPerHour"] = requestsPerHour }
                        });
                    }
                }

                // Check token consumption spike (would need to compare to average)
                // This is simplified - actual implementation would compare to platform average
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting volume indicators");
            }

            return indicators;
        }

        private async Task<List<AbuseIndicator>> DetectTemporalIndicatorsAsync(string instanceId, string username, DateTime startDate, DateTime endDate)
        {
            var indicators = new List<AbuseIndicator>();

            try
            {
                var timeline = await _analyticsService.GetUserActivityTimelineAsync(instanceId, username, startDate, endDate);

                if (timeline.Entries.Count > _analyticsSettings.AbuseDetection.RapidFireThreshold)
                {
                    // Check for rapid-fire requests
                    var rapidRequests = 0;
                    for (int i = 1; i < timeline.Entries.Count; i++)
                    {
                        var timeDiff = timeline.Entries[i].Timestamp - timeline.Entries[i - 1].Timestamp;
                        if (timeDiff.TotalSeconds < 1)
                            rapidRequests++;
                    }

                    if (rapidRequests > _analyticsSettings.AbuseDetection.RapidFireThreshold)
                    {
                        indicators.Add(new AbuseIndicator
                        {
                            Type = "RapidFireRequests",
                            Description = $"User has {rapidRequests} rapid-fire requests (within 1 second)",
                            Severity = "High",
                            DetectedAt = DateTime.UtcNow,
                            Metrics = new Dictionary<string, object> { ["rapidRequestCount"] = rapidRequests }
                        });
                    }
                }

                // Check for continuous usage
                if (timeline.Entries.Any())
                {
                    var firstActivity = timeline.Entries.First().Timestamp;
                    var lastActivity = timeline.Entries.Last().Timestamp;
                    var duration = (lastActivity - firstActivity).TotalHours;

                    if (duration > _continuousUsageHours)
                    {
                        indicators.Add(new AbuseIndicator
                        {
                            Type = "ContinuousUsage",
                            Description = $"User has continuous activity spanning {duration:F1} hours",
                            Severity = "Medium",
                            DetectedAt = DateTime.UtcNow,
                            Metrics = new Dictionary<string, object> { ["durationHours"] = duration }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting temporal indicators");
            }

            return indicators;
        }

        private async Task<List<AbuseIndicator>> DetectBehavioralIndicatorsAsync(string instanceId, string username, DateTime startDate, DateTime endDate)
        {
            var indicators = new List<AbuseIndicator>();

            try
            {
                var userAnalytics = await _analyticsService.GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate);

                // Check error rate
                if (userAnalytics.ErrorRate > _highErrorRateThreshold)
                {
                    indicators.Add(new AbuseIndicator
                    {
                        Type = "HighErrorRate",
                        Description = $"User has high error rate: {userAnalytics.ErrorRate:F1}%",
                        Severity = "Medium",
                        DetectedAt = DateTime.UtcNow,
                        Metrics = new Dictionary<string, object> { ["errorRate"] = userAnalytics.ErrorRate }
                    });
                }

                // Agent hopping detection would require Application Insights data
                // For now, we'll note if user uses many agents
                if (userAnalytics.AgentsUsed > _agentHoppingThreshold)
                {
                    indicators.Add(new AbuseIndicator
                    {
                        Type = "AgentHopping",
                        Description = $"User accesses {userAnalytics.AgentsUsed} different agents",
                        Severity = "Medium",
                        DetectedAt = DateTime.UtcNow,
                        Metrics = new Dictionary<string, object> { ["agentsUsed"] = userAnalytics.AgentsUsed }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting behavioral indicators");
            }

            return indicators;
        }

        private async Task<List<AbuseIndicator>> DetectResourceIndicatorsAsync(string instanceId, string username, DateTime startDate, DateTime endDate)
        {
            var indicators = new List<AbuseIndicator>();

            // Resource exhaustion indicators would require quota violation tracking
            // This is a placeholder for future implementation

            return indicators;
        }

        private double CalculateVolumeScore(List<AbuseIndicator> indicators)
        {
            if (!indicators.Any()) return 0;

            var score = 0.0;
            foreach (var indicator in indicators)
            {
                score += indicator.Severity switch
                {
                    "Critical" => 30,
                    "High" => 20,
                    "Medium" => 10,
                    _ => 5
                };
            }

            return Math.Min(100, score);
        }

        private double CalculateTokenScore(List<AbuseIndicator> indicators) =>
            // Similar to volume score but focused on token consumption
            CalculateVolumeScore(indicators.Where(i => i.Type.Contains("Token")).ToList());

        private double CalculateErrorScore(List<AbuseIndicator> indicators) =>
            CalculateVolumeScore(indicators.Where(i => i.Type.Contains("Error")).ToList());

        private double CalculateTemporalScore(List<AbuseIndicator> indicators) =>
            CalculateVolumeScore(indicators);

        private double CalculateBehavioralScore(List<AbuseIndicator> indicators) =>
            CalculateVolumeScore(indicators);

        private double CalculateResourceScore(List<AbuseIndicator> indicators) =>
            CalculateVolumeScore(indicators);
    }
}
