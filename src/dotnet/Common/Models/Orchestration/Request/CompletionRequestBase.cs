using FoundationaLLM.Common.Models.Conversation;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Request
{
    /// <summary>
    /// Base for completion requests
    /// </summary>
    public class CompletionRequestBase
    {
        /// <summary>
        /// Gets or sets the operation identifier of the completion request.
        /// </summary>
        [JsonPropertyName("operation_id")]
        public string? OperationId { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether this is a long-running operation.
        /// </summary>
        [JsonPropertyName("long_running_operation")]
        public bool LongRunningOperation { get; set; }

        /// <summary>
        /// Gets or sets the conversation identifier.
        /// </summary>
        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a new conversation.
        /// </summary>
        [JsonPropertyName("is_new_conversation")]
        public bool IsNewConversation { get; set; }

        /// <summary>
        /// Gets or sets the user prompt.
        /// </summary>
        [JsonPropertyName("user_prompt")]
        public required string UserPrompt { get; set; }

        /// <summary>
        /// Gets or sets the rewrite of the user prompt.
        /// </summary>
        [JsonPropertyName("user_prompt_rewrite")]
        public string? UserPromptRewrite { get; set; }

        /// <summary>
        /// Gets or sets the message history associated with the completion request.
        /// </summary>
        [JsonPropertyName("message_history")]
        public List<MessageHistoryItem>? MessageHistory { get; set; } = [];

        /// <summary>
        /// Gets or sets the file history associated with the completion request.
        /// </summary>
        [JsonPropertyName("file_history")]
        public List<FileHistoryItem> FileHistory { get; set; } = [];

        /// <summary>
        /// Gets or sets the metadata associated with the completion request.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
