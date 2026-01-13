using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents filter criteria for quota metrics.
    /// </summary>
    public class QuotaMetricsFilter
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
    }
}
