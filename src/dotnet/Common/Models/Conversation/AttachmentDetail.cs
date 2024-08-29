using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Represents an attachment in a chat message or session.
    /// </summary>
    public class AttachmentDetail
    {
        /// <summary>
        /// The unique identifier of the attachment resource.
        /// </summary>
        [JsonPropertyName("objectId")]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The attachment file name.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The mime content type of the attachment.
        /// </summary>
        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }
    }
}
