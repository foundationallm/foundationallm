using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a summary of quota events for a specific quota.
    /// </summary>
    public class QuotaEventSummary
    {
        /// <summary>
        /// Gets or sets the name of the quota.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the context where quota applies.
        /// </summary>
        [JsonPropertyName("quota_context")]
        public required string QuotaContext { get; set; }

        /// <summary>
        /// Gets or sets the total number of quota-exceeded events in the time period.
        /// </summary>
        [JsonPropertyName("exceeded_event_count")]
        public int ExceededEventCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of lockout-expired events in the time period.
        /// </summary>
        [JsonPropertyName("expired_event_count")]
        public int ExpiredEventCount { get; set; }

        /// <summary>
        /// Gets or sets the number of unique partitions that hit the quota.
        /// </summary>
        [JsonPropertyName("unique_partitions_affected")]
        public int UniquePartitionsAffected { get; set; }
    }
}
