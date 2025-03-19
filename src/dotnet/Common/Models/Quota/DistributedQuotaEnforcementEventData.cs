using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Provides the data associated with a distributed quota enforcement event.
    /// </summary>
    public class DistributedQuotaEnforcementEventData
    {
        /// <summary>
        /// Gets or sets the quota context.
        /// </summary>
        [JsonPropertyName("quota_context")]
        public required string QuotaContext { get; set; }

        /// <summary>
        /// Gets or sets the quota metric partition identifier.
        /// </summary>
        [JsonPropertyName("quota_metric_partition_id")]
        public required string QuotaMetricPartitionId { get; set; }

        /// <summary>
        /// Gets or sets the list of event timestamps.
        /// </summary>
        [JsonPropertyName("event_timestamps")]
        public List<DateTimeOffset> EventTimestamps { get; set; } = [];
    }
}
