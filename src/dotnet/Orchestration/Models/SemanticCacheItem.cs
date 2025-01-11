using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Orchestration.Models
{
    /// <summary>
    /// Represents an item in the semantic cache.
    /// </summary>
    public class SemanticCacheItem
    {
        public string Id { get; set; }

        public string PartitionKey { get; set; }

        public string UserPrompt { get; set; }

        public int UserPromptTokens { get; set; }

        public ReadOnlyMemory<float> UserPromptEmbedding { get; set; }

        public CompletionResponse CompletionResponse { get; set; }
    }
}
