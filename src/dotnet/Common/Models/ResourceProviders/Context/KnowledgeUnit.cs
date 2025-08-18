using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Context
{
    /// <summary>
    /// Represents a knowledge unit managed by the FoundationaLLM.Context resource provider.
    /// </summary>
    public class KnowledgeUnit : ResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeUnit"/> class and sets its type to <see
        /// cref="ContextTypes.KnowledgeUnit"/>.
        /// </summary>
        public KnowledgeUnit() =>
            Type = ContextTypes.KnowledgeUnit;

        /// <summary>
        /// Gets or sets the object identifier of the vector database associated with the knowledge unit.
        /// </summary>
        [JsonPropertyName("vector_database_object_id")]
        public required string VectorDatabaseObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector store associated with the knowledge unit.
        /// </summary>
        /// <remarks>
        /// If this value is null, the knowledge source queries must specify the vector store identifier explicitly.
        /// </remarks>
        [JsonPropertyName("vector_store_id")]
        public string? VectorStoreId { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates whether the knowledge unit also has a knowledge graph.
        /// </summary>
        [JsonPropertyName("has_knowledge_graph")]
        public bool HasKnowledgeGraph { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the vector database used to store knowledge graph embeddings.
        /// </summary>
        /// <remarks>The vector store identifier within the vector database will always be the name of
        /// the knowledge unit.</remarks>
        [JsonPropertyName("knowledge_graph_vector_database_object_id")]
        public string? KnowledgeGraphVectorDatabaseObjectId { get; set; }
    }
}
