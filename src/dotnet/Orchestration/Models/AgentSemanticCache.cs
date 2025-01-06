using Azure.AI.OpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using OpenAI.Embeddings;

namespace FoundationaLLM.Orchestration.Core.Models
{
    /// <summary>
    /// Provides all dependencies for an agent-related semantic cache.
    /// </summary>
    public class AgentSemanticCache
    {
        /// <summary>
        /// Gets or sets the agent's semantic cache settings.
        /// </summary>
        public required AgentSemanticCacheSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the Azure OpenAI client.
        /// </summary>
        public required EmbeddingClient EmbeddingClient { get; set; }
    }
}
