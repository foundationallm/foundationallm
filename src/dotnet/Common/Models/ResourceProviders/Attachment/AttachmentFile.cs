using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Attachment
{
    /// <summary>
    /// Attachment resource.
    /// </summary>
    public class AttachmentFile : ResourceBase
    {
        [JsonIgnore] public override string? Type { get; set; } = nameof(AttachmentFile);

        /// <summary>
        /// File stream of the attachment contents.
        /// </summary>
        [JsonPropertyName("content")]
        public Stream? Content { get; set; }

        /// <summary>
        /// The mime content type of the attachment.
        /// </summary>
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        /// <summary>
        /// Path for the attachment, starting with the container name.
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = "";

    }
}
