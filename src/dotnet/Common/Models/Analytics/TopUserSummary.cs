using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Summary of analytics for a top user.
    /// </summary>
    public class TopUserSummary
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The User Principal Name.
        /// </summary>
        [JsonPropertyName("upn")]
        public string UPN { get; set; } = string.Empty;

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
        /// The number of active sessions.
        /// </summary>
        [JsonPropertyName("active_sessions")]
        public int ActiveSessions { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_response_time_ms")]
        public double AverageResponseTimeMs { get; set; }

        /// <summary>
        /// The error rate as a percentage.
        /// </summary>
        [JsonPropertyName("error_rate")]
        public double ErrorRate { get; set; }

        /// <summary>
        /// The number of different agents used by this user.
        /// </summary>
        [JsonPropertyName("agents_used")]
        public int AgentsUsed { get; set; }

        /// <summary>
        /// The timestamp of the last activity.
        /// </summary>
        [JsonPropertyName("last_activity")]
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// The abuse risk score (0-100).
        /// </summary>
        [JsonPropertyName("abuse_risk_score")]
        public int AbuseRiskScore { get; set; }

        /// <summary>
        /// List of abuse indicators detected for this user.
        /// </summary>
        [JsonPropertyName("abuse_indicators")]
        public List<string> AbuseIndicators { get; set; } = [];
    }
}
