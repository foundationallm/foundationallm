using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a quota event document stored in Cosmos DB.
    /// </summary>
    public class QuotaEventDocument
    {
        /// <summary>
        /// Gets or sets the unique document identifier.
        /// Format: {quotaName}_{partitionId}_{timestamp:yyyyMMddHHmmssfff}_{eventType}
        /// </summary>
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the document type identifier.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "quota-event";

        /// <summary>
        /// Gets or sets the event type: "quota-exceeded" or "lockout-expired".
        /// </summary>
        [JsonPropertyName("event_type")]
        public required string EventType { get; set; }

        /// <summary>
        /// Gets or sets the name of the quota definition.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the context where quota applies.
        /// </summary>
        [JsonPropertyName("quota_context")]
        public required string QuotaContext { get; set; }

        /// <summary>
        /// Gets or sets the partition identifier (e.g., user UPN, user ID).
        /// </summary>
        [JsonPropertyName("partition_id")]
        public required string PartitionId { get; set; }

        /// <summary>
        /// Gets or sets the quota limit at the time of the event.
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// Gets or sets the request count when the event occurred.
        /// </summary>
        [JsonPropertyName("count_at_event")]
        public int CountAtEvent { get; set; }

        /// <summary>
        /// Gets or sets the lockout duration configured for this quota (in seconds).
        /// </summary>
        [JsonPropertyName("lockout_duration_seconds")]
        public int LockoutDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the time-to-live in seconds (for automatic cleanup).
        /// Default: 2592000 (30 days).
        /// </summary>
        [JsonPropertyName("ttl")]
        public int Ttl { get; set; } = 2592000; // 30 days
    }
}
