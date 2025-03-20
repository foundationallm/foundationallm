using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Quota;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Quota
{
    /// <summary>
    /// Implements a quota context partitioned by user identifier.
    /// </summary>
    /// <param name="quotaServiceIdentifier">The identifier of the QuotaService instance managing this quota context.</param>
    /// <param name="quota">The <see cref="QuotaDefinition"/> providing the quota configuration.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class UserIdentifierQuotaContext(
        string quotaServiceIdentifier,
        QuotaDefinition quota,
        ILogger logger) : QuotaContextBase(quotaServiceIdentifier, quota, logger)
    {
        /// <inheritdoc/>
        protected override QuotaMetricPartition GetQuotaMetricPartition(
            string userIdentifier,
            string userPrincipalName) =>
            EnsureQuotaMetricPartition(userIdentifier);
    }
}
