using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Holds the content of an OpenAI file.
    /// </summary>
    public class FileContent : ResourceBase
    {
        /// <summary>
        /// The original file name of the file.
        /// </summary>
        [JsonPropertyName("original_file_name")]
        public required string OriginalFileName { get; set; }

        /// <summary>
        /// The content type of the file.
        /// </summary>
        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }

        /// <summary>
        /// The binary content of the file.
        /// </summary>
        [JsonIgnore]
        public BinaryData? BinaryContent { get; set; }
    }
}
