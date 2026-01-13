namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents the display state of a quota metric partition for reporting purposes.
    /// </summary>
    public class QuotaMetricPartitionDisplayState
    {
        /// <summary>
        /// Gets or sets the identifier of the quota metric partition.
        /// </summary>
        public required string QuotaMetricPartitionId { get; set; }

        /// <summary>
        /// Gets or sets the current metric value.
        /// </summary>
        public int MetricValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the partition is currently locked out.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the remaining seconds in the lockout period.
        /// </summary>
        public int LockoutRemainingSeconds { get; set; }
    }
}
