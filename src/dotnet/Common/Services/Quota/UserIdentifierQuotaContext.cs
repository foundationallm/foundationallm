using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements a quota context partitioned by user identifier.
    /// </summary>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class UserIdentifierQuotaContext(
        QuotaDefinition quota,
        ILogger logger) : QuotaContextBase(quota, logger)
    {
        private readonly Dictionary<string, QuotaMetricSequence> _metrics = [];

        /// <inheritdoc/>
        protected override QuotaMetricSequence GetQuotaMetricSequence(
            string userIdentifier,
            string userPrincipalName)
        {
            if (!_metrics.ContainsKey(userIdentifier))
            {
                lock (_syncRoot)
                {
                    // Ensure that the key is still not present after acquiring the lock.
                    if (!_metrics.ContainsKey(userIdentifier))
                    {
                        _metrics[userIdentifier] = new(
                            Quota.MetricLimit,
                            Quota.MetricWindowSeconds,
                            Quota.LockoutDurationSeconds);
                    }
                }
            }

            return _metrics[userIdentifier];
        }
    }
}
