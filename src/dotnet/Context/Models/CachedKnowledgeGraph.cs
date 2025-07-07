using FoundationaLLM.Common.Models.Knowledge;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a knowledge graph with caching capabilities to optimize data retrieval and reduce redundant
    /// computations.
    /// </summary>
    public class CachedKnowledgeGraph
    {
        /// <summary>
        /// Gets or sets the collection of knowledge entities associated with the knowledge graph.
        /// </summary>
        public List<KnowledgeEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of relationships associated with the knowledge entity.
        /// </summary>
        public List<KnowledgeRelationship> Relationships { get; set; } = [];

        /// <summary>
        /// Gets or sets the knowledge graph index used for organizing and retrieving information.
        /// </summary>
        public KnowledgeGraphIndex Index { get; set; } = null!;
    }
}
