using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Quota
{
    /// <summary>
    /// Represents a request for quota usage history.
    /// </summary>
    public class QuotaHistoryRequest
    {
        /// <summary>
        /// Gets or sets the quota name to get history for.
        /// </summary>
        [JsonPropertyName("quota_name")]
        public required string QuotaName { get; set; }

        /// <summary>
        /// Gets or sets the start time for the history query.
        /// </summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for the history query.
        /// </summary>
        [JsonPropertyName("end_time")]
        public DateTimeOffset EndTime { get; set; }
    }
}
