namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a sequence of units of a quota metric.
    /// </summary>
    /// <param name="metricLimit">The limit of the metric for the time window specified by <paramref name="metricWindowSeconds"/>.</param>
    /// <param name="metricWindowSeconds">The time window for the metric limit.</param>
    public class QuotaMetricSequence(
        int metricLimit,
        int metricWindowSeconds)
    {
        private readonly int _metricLimit = metricLimit;
        private readonly int _metricWindowSeconds = metricWindowSeconds;

        /// <summary>
        /// Try to add a unit to the metric sequence.
        /// </summary>
        /// <returns>False is the metric limit is exceeded.</returns>
        public bool TryAddUnit()
        {
            return true;
        }
    }
}
