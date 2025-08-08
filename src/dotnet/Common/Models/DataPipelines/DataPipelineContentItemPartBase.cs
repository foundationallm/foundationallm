using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the common properties of a content item part.
    /// </summary>
    public class DataPipelineContentItemPartBase
    {
        /// <summary>
        /// Gets or sets the canonical identifier of the content item.
        /// </summary>
        [JsonPropertyName("content_item_canonical_id")]
        public string? ContentItemCanonicalId { get; set; }

        /// <summary>
        /// Gets or sets the position of the part within the content item.
        /// </summary>
        [JsonPropertyName("position")]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the index entry associated with this content item part.
        /// </summary>
        [JsonPropertyName("index_entry_id")]
        public string? IndexEntryId { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the content item part.
        /// </summary>
        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run that last modified this content item part.
        /// </summary>
        [JsonPropertyName("last_changed_by")]
        public string LastChangedBy { get; set; } = null!;
    }
}
