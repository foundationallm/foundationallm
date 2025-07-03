using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a request to update a knowledge graph in the FoundationaLLM Context API.
    /// </summary>
    public class ContextKnowledgeGraphUpdateRequest
    {
        /// <summary>
        /// Gets or sets the path of the source file containing the entities for the knowledge graph.
        /// </summary>
        [JsonPropertyName("entities_source_file_path")]
        public required string EntitiesSourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the path of the source file containing the relationships for the knowledge graph.
        /// </summary>
        [JsonPropertyName("relationships_source_file_path")]
        public required string RelationshipsSourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector database associated with the knowledge graph.
        /// </summary>
        [JsonPropertyName("vector_database_object_id")]
        public required string VectorDatabaseObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector store associated with the knowledge graph.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public required string VectorStoreId { get; set; }

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
