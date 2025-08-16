using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a knowledge graph, which is a structured representation
    /// of knowledge entities and their relationships.
    /// </summary>
    public class KnowledgeGraph : ResourceBase
    {
        /// <summary>
        /// Gets or sets the collection of knowledge entities associated with the knowledge graph.
        /// </summary>
        public List<KnowledgeEntity> Entities { get; set; } = [];

        /// <summary>
        /// Gets or sets the collection of relationships associated with the knowledge entity.
        /// </summary>
        public List<KnowledgeRelationship> Relationships { get; set; } = [];
    }
}
