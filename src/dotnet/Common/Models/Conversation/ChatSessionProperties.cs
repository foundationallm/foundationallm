using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// The session properties object.
    /// </summary>
    public class ChatSessionProperties
    {
        /// <summary>
        /// The session name.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}
