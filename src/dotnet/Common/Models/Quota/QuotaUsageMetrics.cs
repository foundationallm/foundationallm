using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents usage metrics for a quota.
    /// </summary>
    public class QuotaUsageMetrics
    {
        /// <summary>
        /// Gets or sets the name of the quota.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the context of the quota.
        /// </summary>
        [JsonPropertyName("quota_context")]
        public required string QuotaContext { get; set; }

        /// <summary>
        /// Gets or sets the partition identifier.
        /// </summary>
        [JsonPropertyName("partition_id")]
        public required string PartitionId { get; set; }

        /// <summary>
        /// Gets or sets the current count of requests.
        /// </summary>
        [JsonPropertyName("current_count")]
        public int CurrentCount { get; set; }

        /// <summary>
        /// Gets or sets the limit for the quota.
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the utilization percentage.
        /// </summary>
        [JsonPropertyName("utilization_percentage")]
        public double UtilizationPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a lockout is currently active.
        /// </summary>
        [JsonPropertyName("lockout_active")]
        public bool LockoutActive { get; set; }

        /// <summary>
        /// Gets or sets the remaining seconds for the lockout.
        /// </summary>
        [JsonPropertyName("lockout_remaining_seconds")]
        public int LockoutRemainingSeconds { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the metrics.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
