using FoundationaLLM.Common.Interfaces;
using OpenAI.Embeddings;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a cached knowledge unit.
    /// </summary>
    public class CachedKnowledgeUnit
    {
        /// <summary>
        /// Gets or sets the Azure AI Search service used for performing search operations.
        /// </summary>
        public IAzureAISearchService SearchService { get; set; } = null!;

        /// <summary>
        /// Gets or sets the embedding client used for embedding-related operations.
        /// </summary>
        public EmbeddingClient EmbeddingClient { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Azure AI Search service used for performing knowledge graph search operations.
        /// </summary>
        public IAzureAISearchService? KnowledgeGraphSearchService { get; set; } = null!;

        /// <summary>
        /// Gets or sets the embedding client used for embedding-related knowledge graph operations.
        /// </summary>
        public EmbeddingClient? KnowledgeGraphEmbeddingClient { get; set; } = null!;

        /// <summary>
        /// Gets or sets the cached knowledge graph associated with this knowledge source.
        /// </summary>
        public IndexedKnowledgeGraph? KnowledgeGraph { get; set; }
    }
}
