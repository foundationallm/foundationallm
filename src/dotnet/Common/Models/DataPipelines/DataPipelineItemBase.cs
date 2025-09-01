using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the common properties of an item processed by a data pipeline.
    /// </summary>
    public class DataPipelineItemBase
    {
        /// <summary>
        /// Gets or sets the position of the item within its parent context.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonPropertyOrder(-100)]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the index entry associated with this item.
        /// </summary>
        [JsonPropertyName("index_entry_id")]
        [JsonPropertyOrder(100)]
        public string? IndexEntryId { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the item.
        /// </summary>
        [JsonPropertyName("metadata")]
        [JsonPropertyOrder(101)]
        public string? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the data pipeline run that last modified this item.
        /// </summary>
        [JsonPropertyName("last_changed_by")]
        [JsonPropertyOrder(102)]
        public string LastChangedBy { get; set; } = null!;
    }
}
