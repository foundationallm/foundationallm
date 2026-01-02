using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Detailed analytics summary for a user.
    /// </summary>
    public class UserAnalyticsSummary
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The total number of requests made by this user.
        /// </summary>
        [JsonPropertyName("total_requests")]
        public int TotalRequests { get; set; }

        /// <summary>
        /// The total tokens consumed by this user.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }

        /// <summary>
        /// The total number of sessions.
        /// </summary>
        [JsonPropertyName("total_sessions")]
        public int TotalSessions { get; set; }

        /// <summary>
        /// The number of active days.
        /// </summary>
        [JsonPropertyName("active_days")]
        public int ActiveDays { get; set; }

        /// <summary>
        /// The average session length (messages per conversation).
        /// </summary>
        [JsonPropertyName("avg_session_length")]
        public double AvgSessionLength { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_response_time_ms")]
        public double AvgResponseTimeMs { get; set; }

        /// <summary>
        /// The error rate as a percentage.
        /// </summary>
        [JsonPropertyName("error_rate")]
        public double ErrorRate { get; set; }

        /// <summary>
        /// The number of different agents used.
        /// </summary>
        [JsonPropertyName("agents_used")]
        public int AgentsUsed { get; set; }

        /// <summary>
        /// Peak usage hour (0-23).
        /// </summary>
        [JsonPropertyName("peak_usage_hour")]
        public int? PeakUsageHour { get; set; }

        /// <summary>
        /// Peak usage day of week (0-6, Sunday = 0).
        /// </summary>
        [JsonPropertyName("peak_usage_day")]
        public int? PeakUsageDay { get; set; }
    }
}
