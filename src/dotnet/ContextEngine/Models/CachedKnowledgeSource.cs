using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using OpenAI.Embeddings;

namespace FoundationaLLM.Context.Models
{
    /// <summary>
    /// Represents a knowledge source that caches data to improve retrieval performance.
    /// </summary>
    public class CachedKnowledgeSource
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
        /// Gets or sets the cached knowledge graph associated with this knowledge source.
        /// </summary>
        public CachedKnowledgeGraph? KnowledgeGraph { get; set; }
    }
}
