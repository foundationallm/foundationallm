using FoundationaLLM.Common.Models.Knowledge;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a node within a knowledge graph index.
    /// </summary>
    public class KnowledgeGraphIndexNode
    {
        /// <summary>
        /// Gets or sets the knowledge entity associated with the node.
        /// </summary>
        public required KnowledgeEntity Entity { get; set; }

        /// <summary>
        /// Gets or sets the collection of related nodes in the knowledge graph.
        /// </summary>
        public required List<KnowledgeGraphIndexRelatedNode> RelatedNodes { get; set; } = [];
    }
}
