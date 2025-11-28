using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the common properties of an item processed by a data pipeline.
    /// </summary>
    public class DataPipelineItemBase
    {
        /// <summary>
        /// Gets or sets the identifier of the index entry associated with this item.
        /// </summary>
        [JsonPropertyName("index_entry_id")]
        [JsonPropertyOrder(100)]
        public string? IndexEntryId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the item has been saved to an index.
        /// </summary>
        [JsonPropertyName("indexed")]
        [JsonPropertyOrder(101)]
        public bool Indexed { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the item has been shielded by a content safety mechanism.
        /// </summary>
        [JsonPropertyName("shielded")]
        [JsonPropertyOrder(102)]
        public bool Shielded { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the item.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonPropertyOrder(103)]
        public string? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run that last modified this item.
        /// </summary>
        [JsonPropertyName("last_changed_by")]
        [JsonPropertyOrder(104)]
        public string LastChangedBy { get; set; } = null!;
    }
}
