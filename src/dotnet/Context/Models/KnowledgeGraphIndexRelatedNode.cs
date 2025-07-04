using FoundationaLLM.Common.Models.Knowledge;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a node related to another node within a knowledge graph index.
    /// </summary>
    public class KnowledgeGraphIndexRelatedNode
    {
        /// <summary>
        /// Gets or sets the knowledge entity associated with the node.
        /// </summary>
        public required KnowledgeEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the relationship between this node and the related node.
        /// </summary>
        public required KnowledgeRelationship Relationship { get; set; }
    }
}
