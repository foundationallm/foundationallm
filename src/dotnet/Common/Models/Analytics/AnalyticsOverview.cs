using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Overview of analytics across the platform.
    /// </summary>
    public class AnalyticsOverview
    {
        /// <summary>
        /// The total number of agents.
        /// </summary>
        [JsonPropertyName("total_agents")]
        public int TotalAgents { get; set; }

        /// <summary>
        /// The total number of conversations.
        /// </summary>
        [JsonPropertyName("total_conversations")]
        public int TotalConversations { get; set; }

        /// <summary>
        /// The total tokens consumed.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_response_time_ms")]
        public double AvgResponseTimeMs { get; set; }

        /// <summary>
        /// The total number of active users.
        /// </summary>
        [JsonPropertyName("active_users")]
        public int ActiveUsers { get; set; }

        /// <summary>
        /// The total number of tools.
        /// </summary>
        [JsonPropertyName("total_tools")]
        public int TotalTools { get; set; }

        /// <summary>
        /// The total number of models.
        /// </summary>
        [JsonPropertyName("total_models")]
        public int TotalModels { get; set; }
    }
}
