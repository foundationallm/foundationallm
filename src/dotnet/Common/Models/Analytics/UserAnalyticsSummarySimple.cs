using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Simple analytics summary for a user (for the users table).
    /// </summary>
    public class UserAnalyticsSummarySimple
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The total number of conversations (distinct sessions) for this user.
        /// </summary>
        [JsonPropertyName("total_conversations")]
        public int TotalConversations { get; set; }

        /// <summary>
        /// The total number of messages sent by this user.
        /// </summary>
        [JsonPropertyName("total_messages")]
        public int TotalMessages { get; set; }

        /// <summary>
        /// The total tokens consumed by this user.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }
    }
}

