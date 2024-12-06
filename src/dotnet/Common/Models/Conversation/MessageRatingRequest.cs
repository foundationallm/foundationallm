using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Contains the request for rating a message.
    /// </summary>
    public class MessageRatingRequest
    {
        /// <summary>
        /// The rating to assign to the message.
        /// </summary>
        [JsonPropertyName("rating")]
        public bool? Rating { get; set; }

        /// <summary>
        /// Optional comments associated with the rating.
        /// </summary>
        [JsonPropertyName("comments")]
        public string? Comments { get; set; }
    }
}
