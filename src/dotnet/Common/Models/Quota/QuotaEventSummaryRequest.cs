using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a request for quota event summary.
    /// </summary>
    public class QuotaEventSummaryRequest
    {
        /// <summary>
        /// Gets or sets the optional quota name to filter by.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public string? QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the start time for the summary query.
        /// </summary>
        [JsonPropertyName("start_time")]
        public required DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the summary query.
        /// </summary>
        [JsonPropertyName("end_time")]
        public required DateTimeOffset EndTime { get; set; }
    }
}
