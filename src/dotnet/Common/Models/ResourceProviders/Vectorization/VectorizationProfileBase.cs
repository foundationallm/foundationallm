using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Basic properties for vectorization profiles.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(TextPartitioningProfile), "text-partitioning-profile")]
    [JsonDerivedType(typeof(TextEmbeddingProfile), "text-embedding-profile")]
    [JsonDerivedType(typeof(IndexingProfile), "indexing-profile")]
    public class VectorizationProfileBase : ResourceBase
    {
        /// <summary>
        /// The type of the vectorization profile.
        /// </summary>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// The configuration associated with the vectorization profile.
        /// </summary>
        [JsonPropertyName("settings")]
        public Dictionary<string, string>? Settings { get; set; } = [];

        /// <summary>
        /// Configuration references associated with the vectorization profile.
        /// </summary>
        [JsonPropertyName("configuration_references")]
        public Dictionary<string, string>? ConfigurationReferences { get; set; } = [];
    }
}
