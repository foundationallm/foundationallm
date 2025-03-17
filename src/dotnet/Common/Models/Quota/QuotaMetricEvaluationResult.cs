namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents the result of evaluating a quota metric.
    /// </summary>
    public class QuotaMetricEvaluationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the quota metric is locked out.
        /// </summary>
        public bool LockedOut { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds remaining in the lockout period.
        /// </summary>
        public int RemainingLockoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of units of the quota metric.
        /// </summary>
        public int MetricCount { get; set; }
    }
}
