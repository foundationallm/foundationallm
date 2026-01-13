using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents filter criteria for querying quota events.
    /// </summary>
    public class QuotaEventFilter
    {
        /// <summary>
        /// Gets or sets the quota name to filter by. Optional.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public string? QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the partition ID to filter by. Optional.
        /// </summary>
        [JsonPropertyName("partition_id")]
        public string? PartitionId { get; set; }

        /// <summary>
        /// Gets or sets the event type to filter by. Optional.
        /// Values: "quota-exceeded" or "lockout-expired".
        /// </summary>
        [JsonPropertyName("event_type")]
        public string? EventType { get; set; }

        /// <summary>
        /// Gets or sets the start time for the time range filter. Optional.
        /// </summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the time range filter. Optional.
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset? EndTime { get; set; }
    }
}
