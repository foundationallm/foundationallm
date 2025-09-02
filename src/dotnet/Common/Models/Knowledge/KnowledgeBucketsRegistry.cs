using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents a registry for knowledge buckets within the data pipeline engine.
    /// </summary>
    public class KnowledgeBucketsRegistry
    {
        /// <summary>
        /// Gets or sets the knowledge buckets for entities.
        /// </summary>
        [JsonPropertyName("entities")]
        public SortedDictionary<string, KnowledgeBucketsRegistryEntry> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the knowledge buckets for relationships.
        /// </summary>
        [JsonPropertyName("relationships")]
        public SortedDictionary<string, KnowledgeBucketsRegistryEntry> Relationships { get; set; } = [];
    }
}
