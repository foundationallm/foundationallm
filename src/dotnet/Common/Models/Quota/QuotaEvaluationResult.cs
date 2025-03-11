using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents the result of evaluating a quota.
    /// </summary>
    public class QuotaEvaluationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the quota has been exceeded.
        /// </summary>
        [JsonPropertyName("rate_limit_exceeded")]
        public bool QuotaExceeded { get; set; }

        /// <summary>
        /// Gets or sets the name of the quota that was exceeded.
        /// </summary>
        [JsonPropertyName("exceeded_quota_name")]
        public string? ExceededQuotaName { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds until the request can be retried.
        /// </summary>
        [JsonPropertyName("time_until_retry_seconds")]
        public int TimeUntilRetrySeconds { get; set; }
    }
}
