using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the common properties of a content item part.
    /// </summary>
    public class DataPipelineContentItemPartBase : DataPipelineItemBase
    {
        /// <summary>
        /// Gets or sets the position of the item within its parent context.
        /// </summary>
        [JsonPropertyName("position")]
        [JsonPropertyOrder(-100)]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the canonical identifier of the content item.
        /// </summary>
        [JsonPropertyName("content_item_canonical_id")]
        [JsonPropertyOrder(-99)]
        public string? ContentItemCanonicalId { get; set; }
    }
}
