using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements the base class for managing the in-memory state of a quota context.
    /// </summary>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public abstract class QuotaContextBase(
        QuotaDefinition quota,
        ILogger logger)
    {
        /// <summary>
        /// The object used to synchronize access to the quota context.
        /// </summary>
        protected readonly object _syncRoot = new();

        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger _logger = logger;

        /// <summary>
        /// The <see cref="QuotaDefinition"/> providing the quota configuration.
        /// </summary>
        private readonly QuotaDefinition _quota = quota;

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public QuotaDefinition Quota => _quota;

        /// <summary>
        /// Adds a new unit of the quota metric to the quota context and checks if the quota is exceeded or not.
        /// </summary>
        /// <param name="userIdentifier">The user identifier associated with the unit of the quota metric.</param>
        /// <param name="userPrincipalName">The user principal name associated with the unit of the quota metric.</param>
        /// <returns>The result of the quota evaluation.</returns>
        public abstract QuotaEvaluationResult AddMetricUnitAndEvaluateQuota(
            string userIdentifier,
            string userPrincipalName);

        /// <summary>
        /// Logs the result of the evaluation of a quota metric.
        /// </summary>
        /// <param name="metricResult">The <see cref="QuotaMetricEvaluationResult"/> providing the result of the evaluation.</param>
        /// <param name="userIdentifier">The user unique identifier.</param>
        /// <param name="userPrincipalName">The user principal name.</param>
        protected void LogMetricEvaluationResult(
            QuotaMetricEvaluationResult metricResult,
            string userIdentifier,
            string userPrincipalName) =>
            _logger.LogDebug("Quota context: {QuotaContext}, Metric count: {MetricCount}, Quota exceeded: {QuotaExceeded}, Remaining lockout: {RemainingLockout} seconds.",
                $"{Quota.Context}, {userIdentifier}, {userPrincipalName}",
                $"{metricResult.TotalMetricCount} (local = {metricResult.LocalMetricCount}, remote = {metricResult.RemoteMetricCount})",
                metricResult.LockedOut,
                metricResult.RemainingLockoutSeconds);
    }
}
