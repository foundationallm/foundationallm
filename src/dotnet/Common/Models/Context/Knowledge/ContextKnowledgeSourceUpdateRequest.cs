using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to update a knowledge source in the FoundationaLLM Context API.
    /// </summary>
    public class ContextKnowledgeSourceUpdateRequest
    {
        /// <summary>
        /// Gets or sets the path of the source file containing the entities for the knowledge graph.
        /// </summary>
        [JsonPropertyName("entities_source_file_path")]
        public string? EntitiesSourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the path of the source file containing the relationships for the knowledge graph.
        /// </summary>
        [JsonPropertyName("relationships_source_file_path")]
        public string? RelationshipsSourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector database associated with the knowledge source.
        /// </summary>
        [JsonPropertyName("vector_database_object_id")]
        public required string VectorDatabaseObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector store associated with the knowledge source.
        /// </summary>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

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
    }
}
