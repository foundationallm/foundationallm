using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Contains parts that compose the message content.
    /// </summary>
    public class MessageContent
    {
        /// <summary>
        /// The type of the message content. Could be text, image, etc.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// The file name related to the Value, if applicable.
        /// </summary>
        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        /// <summary>
        /// The value of the message content.
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
