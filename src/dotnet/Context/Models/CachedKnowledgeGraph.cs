using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using OpenAI.Embeddings;

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
        /// Gets or sets the Azure AI Search service used for performing search operations.
        /// </summary>
        public IAzureAISearchService SearchService { get; set; } = null!;

        /// <summary>
        /// Gets or sets the embedding client used for embedding-related operations.
        /// </summary>
        public EmbeddingClient EmbeddingClient { get; set; } = null!;

        /// <summary>
        /// Gets or sets the object with the knowledge graph configuration.
        /// </summary>
        public KnowledgeGraph KnowledgeGraph { get; set; } = null!;

        /// <summary>
        /// Gets or sets the vector database associated with the knowledge graph.
        /// </summary>
        public VectorDatabase VectorDatabase { get; set; } = null!;

        /// <summary>
        /// Gets or sets the knowledge graph index used for organizing and retrieving information.
        /// </summary>
        public KnowledgeGraphIndex Index { get; set; } = null!;
    }
}
