using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Context
{
    /// <summary>
    /// Represents a FoundationaLLM knowledge graph.
    /// </summary>
    public class KnowledgeGraph : ResourceBase
    {
        /// <summary>
        /// Gets or sets the embedding model used for the knowledge graph.
        /// </summary>
        [JsonPropertyName("embedding_model")]
        public required string EmbeddingModel { get; set; }

        /// <summary>
        /// Gets or sets the number of dimensions for the embeddings used in the knowledge graph.
        /// </summary>
        [JsonPropertyName("embedding_dimensions")]
        public required int EmbeddingDimensions { get; set; }
    }
}
