using FoundationaLLM.Common.Models.Context.Knowledge;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a knowledge graph with indexing capabilities to optimize data retrieval and reduce redundant
    /// computations.
    /// </summary>
    public class IndexedKnowledgeGraph
    {
        /// <summary>
        /// Gets or sets the knowledge graph that contains the structured information.
        /// </summary>
        public KnowledgeGraph Graph { get; set; } = null!;

        /// <summary>
        /// Gets or sets the knowledge graph index used for organizing and retrieving information.
        /// </summary>
        public KnowledgeGraphIndex Index { get; set; } = null!;
    }
}
