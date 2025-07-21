using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// The session properties object.
    /// </summary>
    public class ConversationProperties
    {
        /// <summary>
        /// The session name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the optional metadata associated with the conversation.
        /// </summary>
        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }
    }
}
