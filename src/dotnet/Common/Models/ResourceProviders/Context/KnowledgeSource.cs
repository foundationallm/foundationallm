using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Context
{
    /// <summary>
    /// Represents a FoundationaLLM knowledge source.
    /// </summary>
    public class KnowledgeSource : ResourceBase
    {
        /// <summary>
        /// Gets or sets the object identifier of the vector database associated with the knowledge source.
        /// </summary>
        [JsonPropertyName("vector_database_object_id")]
        public required string VectorDatabaseObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector store associated with the knowledge source.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public required string VectorStoreId { get; set; }

        /// <summary>
        /// Gets or sets the embedding model used for the knowledge source.
        /// </summary>
        [JsonPropertyName("embedding_model")]
        public required string EmbeddingModel { get; set; }

        /// <summary>
        /// Gets or sets the number of dimensions for the embeddings used in the knowledge source.
        /// </summary>
        [JsonPropertyName("embedding_dimensions")]
        public required int EmbeddingDimensions { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether the knowledge source has a knowledge graph.
        /// </summary>
        [JsonPropertyName("has_knowledge_graph")]
        public bool HasKnowledgeGraph { get; set; }
    }
}
