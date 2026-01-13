using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents historical usage data for a quota.
    /// </summary>
    public class QuotaUsageHistory
    {
        /// <summary>
        /// Gets or sets the name of the quota.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the partition identifier.
        /// </summary>
        [JsonPropertyName("partition_id")]
        public required string PartitionId { get; set; }

        /// <summary>
        /// Gets or sets the time bucket for the historical data.
        /// </summary>
        [JsonPropertyName("time_bucket")]
        public DateTimeOffset TimeBucket { get; set; }

        /// <summary>
        /// Gets or sets the request count for this time bucket.
        /// </summary>
        [JsonPropertyName("request_count")]
        public int RequestCount { get; set; }

        /// <summary>
        /// Gets or sets the number of times the quota was exceeded.
        /// </summary>
        [JsonPropertyName("exceeded_count")]
        public int ExceededCount { get; set; }

        /// <summary>
        /// Gets or sets the number of lockouts that occurred.
        /// </summary>
        [JsonPropertyName("lockout_count")]
        public int LockoutCount { get; set; }
    }
}
