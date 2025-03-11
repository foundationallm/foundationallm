namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Implements a non-partitioned quota context.
    /// </summary>
    public class PartitionlessQuotaContext: QuotaContextBase
    {
        private readonly QuotaMetricSequence _metric = new(Quota.MetricLimit, Quota.MetricWindowSeconds);

        /// <inheritdoc/>
        protected override QuotaEvaluationResult AddMetricUnit(string userIdentifier, string userPrincipalName) =>
            _metric.AddUnit();
    }
}
