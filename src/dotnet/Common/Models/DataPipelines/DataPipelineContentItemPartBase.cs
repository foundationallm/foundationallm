using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the common properties of a content item part.
    /// </summary>
    public class DataPipelineContentItemPartBase : DataPipelineItemBase
    {
        /// <summary>
        /// Gets or sets the canonical identifier of the content item.
        /// </summary>
        [JsonPropertyName("content_item_canonical_id")]
        [JsonPropertyOrder(-50)]
        public string? ContentItemCanonicalId { get; set; }
    }
}
