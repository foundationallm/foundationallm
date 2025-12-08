using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents the content of a file.
    /// </summary>
    public class ContextFileContent
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        [JsonPropertyName("file_name")]
        public required string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type of the file.
        /// </summary>
        [JsonPropertyName("content_type")]
        public required string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the the binary file content.
        /// </summary>
        [JsonPropertyName("file_content")]
        public BinaryData? FileContent { get; set; }
    }
}
