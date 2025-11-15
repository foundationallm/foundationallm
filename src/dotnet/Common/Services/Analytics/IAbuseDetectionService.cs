using FoundationaLLM.Common.Models.Analytics;

namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for detecting abuse patterns and calculating risk scores.
    /// </summary>
    public interface IAbuseDetectionService
    {
        /// <summary>
        /// Calculates the abuse risk score for a user.
        /// </summary>
        Task<AbuseRiskScore> CalculateAbuseRiskScoreAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Detects abuse indicators for a user.
        /// </summary>
        Task<UserAbuseIndicators> DetectAbuseIndicatorsAsync(string instanceId, string username, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Detects user anomalies across the platform.
        /// </summary>
        Task<List<UserAnomaly>> DetectUserAnomaliesAsync(string instanceId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents an abuse risk score with contributing factors.
    /// </summary>
    public class AbuseRiskScore
    {
        /// <summary>
        /// The calculated risk score (0-100).
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// The contributing factors and their weights.
        /// </summary>
        public Dictionary<string, double> Factors { get; set; } = new();
    }
}
