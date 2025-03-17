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
        /// Gets the quota metric sequence that corresponds to the specified user identifier and/or user principal name.
        /// </summary>
        /// <param name="userIdentifier">The user identifier used to get the quota metric.</param>
        /// <param name="userPrincipalName">The user principal name used to get the quota metric.</param>
        /// <returns>A <see cref="QuotaMetricSequence"/> instance.</returns>
        protected abstract QuotaMetricSequence GetQuotaMetricSequence(
            string userIdentifier,
            string userPrincipalName);

        /// <summary>
        /// Adds a new unit of the quota metric to the quota context and checks if the quota is exceeded or not.
        /// </summary>
        /// <param name="userIdentifier">The user identifier associated with the unit of the quota metric.</param>
        /// <param name="userPrincipalName">The user principal name associated with the unit of the quota metric.</param>
        /// <returns>The result of the quota evaluation.</returns>
        public QuotaEvaluationResult AddLocalMetricUnitAndEvaluateQuota(
            string userIdentifier,
            string userPrincipalName)
        {
            var startTime = DateTimeOffset.UtcNow;
            var metricSequence = GetQuotaMetricSequence(userIdentifier, userPrincipalName);
            var metricResult = metricSequence.AddLocalUnitAndEvaluateMetric();

            LogMetricEvaluationResult(
                metricResult,
                userIdentifier,
                userPrincipalName,
                DateTimeOffset.UtcNow - startTime);

            return metricResult.LockedOut
                ? new QuotaEvaluationResult
                {
                    QuotaExceeded = true,
                    ExceededQuotaName = Quota.Name,
                    // Add a small buffer to the lockout duration to avoid race conditions at the limit.
                    TimeUntilRetrySeconds = metricResult.RemainingLockoutSeconds + 5
                }
                : new QuotaEvaluationResult();
        }

        private void LogMetricEvaluationResult(
            QuotaMetricEvaluationResult metricResult,
            string userIdentifier,
            string userPrincipalName,
            TimeSpan timeElapsed) =>
            _logger.LogDebug(string.Join(Environment.NewLine,
                [
                    "Time elapsed: {TimeElapsedMilliseconds} ms",
                    "Quota context: {QuotaContext}",
                    "Metric count: {MetricCount}, Quota exceeded: {QuotaExceeded}, Remaining lockout: {RemainingLockout} seconds.",
                    "----------------------------------------"
                ]),
                timeElapsed.TotalMilliseconds,
                $"{Quota.Context}, {userIdentifier}, {userPrincipalName}",
                $"{metricResult.TotalMetricCount} (local = {metricResult.LocalMetricCount}, remote = {metricResult.RemoteMetricCount})",
                metricResult.LockedOut,
                metricResult.RemainingLockoutSeconds);
    }
}
