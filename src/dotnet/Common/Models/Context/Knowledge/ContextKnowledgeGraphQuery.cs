using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to query a knowledge graph.
    /// </summary>
    public class ContextKnowledgeGraphQuery
    {
        /// <summary>
        /// Gets or sets the maximum number of mapped entities included in the query result.
        /// </summary>
        /// <remarks>
        /// These are entities directly mapped based on the similarity between their summary description
        /// an the user prompt.
        /// </remarks>
        [JsonPropertyName("mapped_entities_max_count")]
        public int MappedEntitiesMaxCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum threshold used to match entities based on the similarity between
        /// their summary description and the user prompt.
        /// </summary>
        [JsonPropertyName("mapped_entities_similarity_threshold")]
        public float MappedEntitiesSimilarityThreshold { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth allowed when navigating relationships.
        /// </summary>
        [JsonPropertyName("relationships_max_depth")]
        public int RelationshipsMaxDepth { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of entities returned in the response.
        /// </summary>
        /// <remarks>
        /// This includes both entities that were mapped directly and entities that were
        /// retrieved by navigating relationships.
        /// </remarks>
        [JsonPropertyName("all_entities_max_count")]
        public int AllEntitiesMaxCount { get; set; }
    }
}
