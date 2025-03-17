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
        protected override QuotaMetricSequence GetQuotaMetricSequence(
            string userIdentifier,
            string userPrincipalName) =>
            _metric;
    }
}
