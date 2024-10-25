using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response.OpenAI
{
    /// <summary>
    /// File content item used to generate a message content item.
    /// </summary>
    public class OpenAIFilePathContentItem : MessageContentItemBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The text of the annotation.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// The starting index of the annotation.
        /// </summary>
        [JsonPropertyName("start_index")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// The ending index of the annotation.
        /// </summary>
        [JsonPropertyName("end_index")]
        public int? EndIndex { get; set; }

        /// <summary>
        /// The ID of the file referenced by the annotation.
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
        public OpenAIFilePathContentItem() =>
            Type = MessageContentItemTypes.FilePath;
    }
}
