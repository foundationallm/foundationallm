using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements a quota context with a single partition.
    /// </summary>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class SinglePartitionQuotaContext(
        QuotaDefinition quota,
        ILogger logger) : QuotaContextBase(quota, logger)
    {
        private readonly QuotaMetricPartition _metricPartition = new(
            quota.Name,
            quota.Context,
            string.Empty,
            quota.MetricLimit,
            quota.MetricWindowSeconds,
            quota.LockoutDurationSeconds);

        /// <inheritdoc/>
        protected override QuotaMetricPartition GetQuotaMetricPartition(
            string userIdentifier,
            string userPrincipalName) =>
            _metricPartition;

        /// <inheritdoc/>
        protected override QuotaMetricPartition GetQuotaMetricPartition(
            string partitionId) =>
            partitionId switch
            {
                "" => _metricPartition,
                _ => throw new QuotaException($"The partition id {partitionId} is not valid in the quota context {Quota.Context}.")
            };
    }
}
