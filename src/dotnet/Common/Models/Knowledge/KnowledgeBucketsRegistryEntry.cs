using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents an entry in the registry for content items within a data pipeline state.
    /// </summary>
    public class KnowledgeBucketsRegistryEntry
    {
        /// <summary>
        /// Gets or sets the identifier of the knowledge bucket.
        /// </summary>
        [JsonPropertyName("bucket_id")]
        public required string BucketId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the knowledge bucket was last modified.
        /// </summary>
        [JsonPropertyName("last_modified_at")]
        public required DateTimeOffset LastModifiedAt { get; set; }
    }
}
