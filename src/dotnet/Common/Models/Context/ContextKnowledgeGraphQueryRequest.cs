using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a request to query a knowledge graph.
    /// </summary>
    public class ContextKnowledgeGraphQueryRequest : ContextTextChunkQueryRequest
    {
        /// <summary>
        /// Gets or sets the maximum number of mapped entities included in the query result.
        /// </summary>
        [JsonPropertyName("mapped_entities_max_count")]
        public int MappedEntitiesMaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum similarity measure threshold used to identify relevant entities.
        /// </summary>
        [JsonPropertyName("mapped_entities_similarity_threshold")]
        public float MappedEntitiesSimilarityThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth allowed when navigating relationships.
        /// </summary>
        [JsonPropertyName("relationships_max_depth")]
        public int RelationshipsMaxDepth { get; set; }
    }
}
