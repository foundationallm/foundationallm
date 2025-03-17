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
        /// Gets or sets the number of units of the quota metric that have a local origin
        /// (the service instance hosting the quota metric sequence).
        /// </summary>
        public int LocalMetricCount { get; set; }

        /// <summary>
        /// Gets or sets the number of units of the quota metric that have a remote origin
        /// (other service instances than the one hosting the quota metric sequence).
        /// </summary>
        public int RemoteMetricCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of units of the quota metric, both local and remote.
        /// </summary>
        public int TotalMetricCount { get; set; }
    }
}
