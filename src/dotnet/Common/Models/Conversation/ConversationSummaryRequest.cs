using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Request object for generating a conversation summary/title.
    /// </summary>
    public class ConversationSummaryRequest
    {
        /// <summary>
        /// The name of the agent to use for summarization.
        /// </summary>
        [JsonPropertyName("agent_name")]
        public required string AgentName { get; set; }
    }
}
