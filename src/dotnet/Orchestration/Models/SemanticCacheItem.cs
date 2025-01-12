using FoundationaLLM.Common.Models.Orchestration.Response;

namespace FoundationaLLM.Orchestration.Models
{
    /// <summary>
    /// Represents an item in the semantic cache.
    /// </summary>
    public class SemanticCacheItem
    {
        public string Id { get; set; }

        public string OperationId { get; set; }

        public string UserPrompt { get; set; }

        public int UserPromptTokens { get; set; }

        public float[] UserPromptEmbedding { get; set; }

        public string SerializedItem { get; set; }
    }
}
