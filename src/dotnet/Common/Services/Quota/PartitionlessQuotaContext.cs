using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements a non-partitioned quota context.
    /// </summary>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class PartitionlessQuotaContext(
        QuotaDefinition quota,
        ILogger logger) : QuotaContextBase(quota, logger)
    {
        private readonly QuotaMetricSequence _metric = new(
            quota.MetricLimit,
            quota.MetricWindowSeconds,
            quota.LockoutDurationSeconds);

        /// <inheritdoc/>
        public override QuotaEvaluationResult AddMetricUnitAndEvaluateQuota(
            string userIdentifier,
            string userPrincipalName)
        {
            var metricResult = _metric.AddUnitAndEvaluateMetric();
            LogMetricEvaluationResult(metricResult, userIdentifier, userPrincipalName);

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
    }
}
