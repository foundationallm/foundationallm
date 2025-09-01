using FoundationaLLM.Common.Constants.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a registry for content items within the data pipeline engine.
    /// </summary>
    public class ContentItemsRegistry
    {
        /// <summary>
        /// Gets or sets the content items whose last content action is <see cref="ContentItemActions.AddOrUpdate"/>.
        /// </summary>
        [JsonPropertyName("to_add_or_update")]
        public SortedDictionary<string, ContentItemsRegistryEntry> ToAddOrUpdate { get; set; } = [];

        /// <summary>
        /// Gets or sets the content items whose last content action is <see cref="ContentItemActions.Remove"/>.
        /// </summary>
        [JsonPropertyName("to_remove")]
        public SortedDictionary<string, ContentItemsRegistryEntry> ToRemove { get; set; } = [];
    }
}
