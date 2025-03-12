using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements a quota context partitioned by user principal name.
    /// </summary>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class UserPrincipalNameQuotaContext(
        QuotaDefinition quota,
        ILogger logger) : QuotaContextBase(quota, logger)
    {
        private readonly Dictionary<string, QuotaMetricSequence> _metrics = [];

        /// <inheritdoc/>
        public override QuotaEvaluationResult AddMetricUnitAndEvaluateQuota(
            string userIdentifier,
            string userPrincipalName)
        {
            if (!_metrics.ContainsKey(userPrincipalName))
            {
                lock (_syncRoot)
                {
                    // Ensure that the key is still not present after acquiring the lock.
                    if (!_metrics.ContainsKey(userPrincipalName))
                    {
                        _metrics[userPrincipalName] = new(
                            Quota.MetricLimit,
                            Quota.MetricWindowSeconds,
                            Quota.LockoutDurationSeconds);
                    }
                }
            }

            var metricResult = _metrics[userPrincipalName].AddUnitAndEvaluateMetric();
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
