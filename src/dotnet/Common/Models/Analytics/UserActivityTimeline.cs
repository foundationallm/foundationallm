using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Timeline of user activity.
    /// </summary>
    public class UserActivityTimeline
    {
        /// <summary>
        /// The username/UPN of the user.
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Timeline entries.
        /// </summary>
        [JsonPropertyName("entries")]
        public List<UserActivityEntry> Entries { get; set; } = [];
    }

    /// <summary>
    /// A single entry in the user activity timeline.
    /// </summary>
    public class UserActivityEntry
    {
        /// <summary>
        /// The timestamp of the activity.
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// The operation ID.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// The number of tokens consumed.
        /// </summary>
        [JsonPropertyName("tokens")]
        public int Tokens { get; set; }

        /// <summary>
        /// The agent name used.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public string? AgentName { get; set; }
    }
}
