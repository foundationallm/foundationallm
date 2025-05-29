using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a part of a content item.
    /// </summary>
    public class DataPipelineContentItemPart
    {
        /// <summary>
        /// Gets or sets the canonical identifier of the content item part.
        /// </summary>
        [JsonPropertyName("content_item_canonical_id")]
        public string? ContentItemCanonicalId { get; set; }

        /// <summary>
        /// Gets or sets the position of the part within the content item.
        /// </summary>
        [JsonPropertyName("position")]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the text content of the content item part.
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the size of the content item part in tokens.
        /// </summary>
        [JsonPropertyName("content_size_tokens")]
        public int ContentSizeTokens { get; set; }

        /// <summary>
        /// Gets or sets the embedding of the content item part.
        /// </summary>
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the index entry associated with this content item part.
        /// </summary>
        [JsonPropertyName("index_entry_id")]
        public string? IndexEntryId { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the content item part.
        /// </summary>
        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
