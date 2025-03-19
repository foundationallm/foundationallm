using FoundationaLLM.Common.Exceptions;
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
        private readonly Dictionary<string, QuotaMetricPartition> _metricPartitions = [];

        /// <inheritdoc/>
        protected override QuotaMetricPartition GetQuotaMetricPartition(
            string userIdentifier,
            string userPrincipalName)
        {
            if (!_metricPartitions.ContainsKey(userPrincipalName))
            {
                lock (_syncRoot)
                {
                    // Ensure that the key is still not present after acquiring the lock.
                    if (!_metricPartitions.ContainsKey(userPrincipalName))
                    {
                        _metricPartitions[userPrincipalName] = new(
                            quota.Name,
                            quota.Context,
                            userPrincipalName,
                            Quota.MetricLimit,
                            Quota.MetricWindowSeconds,
                            Quota.LockoutDurationSeconds,
                            _logger);
                    }
                }
            }

            return _metricPartitions[userPrincipalName];
        }

        /// <inheritdoc/>
        protected override QuotaMetricPartition GetQuotaMetricPartition(
            string partitionId) =>
            _metricPartitions.GetValueOrDefault(partitionId)
                ?? throw new QuotaException($"The partition id {partitionId} is not valid in the quota context {Quota.Context}.");
    }
}
