using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// LLM orchestration request
    /// </summary>
    public class LLMCompletionRequest
    {
        /// <summary>
        /// The agent that will process the completion request.
        /// </summary>
        public required AgentBase Agent { get; set; }

        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Prompt entered by the user.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public string? UserPrompt { get; set; }

        /// <summary>
        /// The message history associated with the completion request.
        /// </summary>
        [JsonPropertyName("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; init; } = [];

        /// <summary>
        /// Collection of model settings to override with the orchestration request.
        /// </summary>
        [JsonPropertyName("settings")]
        public OrchestrationSettings? Settings { get; set; }

        /// <summary>
        /// List of attachment data lake URLs.
        /// </summary>
        [JsonPropertyName("attachment_data_lake_urls")]
        public List<string>? AttachmentDataLakeUrls { get; set; }
    }
}
