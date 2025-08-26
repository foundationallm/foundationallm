using FoundationaLLM.Common.Constants.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataPipelineEngine.Models.DataPipelineState
{
    /// <summary>
    /// Represents an entry in the registry for content items within a data pipeline state.
    /// </summary>
    public class ContentItemsRegistryEntry
    {
        /// <summary>
        /// Gets or sets the canonical identifier of the content item.
        /// </summary>
        [JsonPropertyName("content_item_canonical_id")]
        public required string ContentItemCanonicalId { get; set; }

        /// <summary>
        /// Gets or sets the action that was last performed on the content item.
        /// </summary>
        /// <remarks>
        /// The possible values are defined in <see cref="ContentItemActions"/>.
        /// </remarks>
        [JsonPropertyName("last_content_action")]
        public required string LastContentAction { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the content item was last modified.
        /// </summary>
        [JsonPropertyName("last_modified_at")]
        public required DateTimeOffset LastModifiedAt { get; set; }
    }
}
