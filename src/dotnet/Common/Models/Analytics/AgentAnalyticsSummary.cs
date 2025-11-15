using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Summary of analytics for an agent.
    /// </summary>
    public class AgentAnalyticsSummary
    {
        /// <summary>
        /// The name of the agent.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public string AgentName { get; set; } = string.Empty;

        /// <summary>
        /// The number of unique users who have used this agent.
        /// </summary>
        [JsonPropertyName("unique_users")]
        public int UniqueUsers { get; set; }

        /// <summary>
        /// The total number of conversations/sessions for this agent.
        /// </summary>
        [JsonPropertyName("total_conversations")]
        public int TotalConversations { get; set; }

        /// <summary>
        /// The total number of tokens consumed by this agent.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }

        /// <summary>
        /// The average number of tokens per session.
        /// </summary>
        [JsonPropertyName("avg_tokens_per_session")]
        public double AvgTokensPerSession { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_response_time_ms")]
        public double AvgResponseTimeMs { get; set; }

        /// <summary>
        /// The 95th percentile response time in milliseconds.
        /// </summary>
        [JsonPropertyName("p95_response_time_ms")]
        public double P95ResponseTimeMs { get; set; }

        /// <summary>
        /// The error rate as a percentage.
        /// </summary>
        [JsonPropertyName("error_rate")]
        public double ErrorRate { get; set; }

        /// <summary>
        /// The most frequently used tools with this agent.
        /// </summary>
        [JsonPropertyName("most_used_tools")]
        public List<string> MostUsedTools { get; set; } = [];

        /// <summary>
        /// The average number of files uploaded per conversation.
        /// </summary>
        [JsonPropertyName("avg_files_per_conversation")]
        public double AvgFilesPerConversation { get; set; }

        /// <summary>
        /// The average number of rounds of interaction per conversation.
        /// </summary>
        [JsonPropertyName("avg_rounds_per_conversation")]
        public double AvgRoundsPerConversation { get; set; }
    }
}
