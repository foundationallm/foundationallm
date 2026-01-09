using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Response object containing the generated conversation summary/title.
    /// </summary>
    public class ConversationSummaryResponse
    {
        /// <summary>
        /// The generated conversation summary/title.
        /// </summary>
        [JsonPropertyName("summary")]
        public required string Summary { get; set; }
    }
}
