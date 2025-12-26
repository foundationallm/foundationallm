using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents an file system item in a code session.
    /// </summary>
    public class CodeSessionFileStoreItem
    {
        /// <summary>
        /// Gets or sets the name of the file system item.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the file system item.
        /// </summary>
        [JsonPropertyName("type")]
        public required string Type { get; set; }

        /// <summary>
        /// Gets or sets the content of the file system item.
        /// </summary>
        [JsonPropertyName("contentType")]
        public required string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the size of the file system item in bytes.
        /// </summary>
        [JsonPropertyName("sizeInBytes")]
        public required long SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the last modified date and time of the file system item.
        /// </summary>
        [JsonPropertyName("lastModifiedAt")]
        public required DateTimeOffset LastModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the path of the file system item.
        /// </summary>
        [JsonIgnore]
        public required string ParentPath { get; set; }
    }
}
