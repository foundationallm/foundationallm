using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides details about an indexing profile.
    /// </summary>
    public class IndexingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the indexer.
        /// </summary>
        [JsonPropertyName("indexer")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required IndexerType Indexer { get; set; }
    }
}
