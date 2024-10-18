using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response.OpenAI
{
    /// <summary>
    /// An OpenAI image file message content item.
    /// </summary>
    public class OpenAIImageFileMessageContentItem : MessageContentItemBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The ID of the image file.
        /// </summary>
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        /// <summary>
        /// The URL of the image file.
        /// </summary>
        [JsonPropertyName("file_url")]
        public string? FileUrl { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public OpenAIImageFileMessageContentItem() =>
            Type = MessageContentItemTypes.ImageFile;
    }
}
